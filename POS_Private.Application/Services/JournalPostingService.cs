using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace POS_Private.Application.Services;

public class JournalPostingService : IJournalPostingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JournalPostingService> _logger;

    public JournalPostingService(IUnitOfWork unitOfWork, ILogger<JournalPostingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<JournalEntry?> PostVendorBillAsync(VendorBill bill, Vendor vendor, int? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (bill.TotalAmount <= 0)
            {
                _logger.LogWarning("Vendor bill #{BillNumber} has non-positive amount {Amount}. Skipping GL journal entry.", bill.BillNumber, bill.TotalAmount);
                return null;
            }

            // Resolve target accounts
            var accInventory = await ResolveAccountAsync("1104", AccountType.Asset, AccountNature.Debit);
            var accInputVat = await ResolveAccountAsync("1106", AccountType.Asset, AccountNature.Debit);
            var accVendorPayable = await ResolveAccountAsync("2101", AccountType.Liability, AccountNature.Credit);

            if (accInventory == null || accVendorPayable == null)
            {
                _logger.LogError("Required financial accounts (1104 or 2101) could not be resolved for Vendor Bill #{BillNumber}.", bill.BillNumber);
                return null;
            }

            int? costCenterId = await ResolveCostCenterIdAsync(bill.BranchId);
            var entryNumber = await _unitOfWork.JournalEntries.GenerateNextEntryNumberAsync();

            decimal netPurchases = bill.SubTotal - bill.DiscountAmount;
            decimal taxAmount = bill.TaxAmount;
            decimal totalAmount = bill.TotalAmount;

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = bill.BillDate,
                PostingDate = DateTime.UtcNow,
                Status = JournalEntryStatus.Posted,
                SourceType = JournalSourceType.Vendor_Bill,
                ReferenceNumber = bill.BillNumber,
                NarrationEn = $"Purchase Bill #{bill.BillNumber} from vendor '{vendor.Name}'" + (!string.IsNullOrWhiteSpace(bill.VendorInvoiceNumber) ? $" (Ref: {bill.VendorInvoiceNumber})" : ""),
                NarrationAr = $"إثبات فاتورة مشتريات رقم {bill.BillNumber} من المورد '{vendor.Name}'" + (!string.IsNullOrWhiteSpace(bill.VendorInvoiceNumber) ? $" (مرجع: {bill.VendorInvoiceNumber})" : ""),
                TotalDebit = totalAmount,
                TotalCredit = totalAmount,
                CreatedByUserId = userId,
                ApprovedByUserId = userId,
                ApprovedAt = DateTime.UtcNow,
                ETASubmissionStatus = ETASubmissionStatus.NotApplicable,
                BranchTaxCode = "0"
            };

            // 1. Debit Inventory (Net Purchases)
            if (netPurchases > 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accInventory.Id,
                    CostCenterId = costCenterId,
                    Debit = netPurchases,
                    Credit = 0m,
                    Description = $"Merchandise Purchases - Bill #{bill.BillNumber}"
                });
            }

            // 2. Debit Input VAT 14%
            if (taxAmount > 0 && accInputVat != null)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accInputVat.Id,
                    CostCenterId = costCenterId,
                    Debit = taxAmount,
                    Credit = 0m,
                    Description = $"Input VAT 14% on Bill #{bill.BillNumber}",
                    ETATaxType = ETATaxType.T1_VAT,
                    TaxRate = 14m,
                    TaxAmount = taxAmount
                });
            }

            // 3. Credit Vendor Accounts Payable
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = accVendorPayable.Id,
                CostCenterId = costCenterId,
                Debit = 0m,
                Credit = totalAmount,
                Description = $"Accounts Payable - Vendor '{vendor.Name}' (#{bill.BillNumber})"
            });

            await _unitOfWork.JournalEntries.AddAsync(entry);
            await ApplyLineBalancesAsync(entry.Lines);

            _logger.LogInformation("Posted GL Journal Entry {EntryNumber} for Vendor Bill #{BillNumber} ({Amount:N2} EGP).", entryNumber, bill.BillNumber, totalAmount);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post automated GL journal entry for Vendor Bill #{BillNumber}.", bill.BillNumber);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<JournalEntry?> PostVendorPaymentAsync(VendorPayment payment, Vendor vendor, int? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (payment.Amount <= 0) return null;

            var accVendorPayable = await ResolveAccountAsync("2101", AccountType.Liability, AccountNature.Credit);
            var isCash = payment.PaymentMethod == ExpensePaymentMethod.Cash || payment.PaymentMethod == ExpensePaymentMethod.PettyCash || payment.PaymentMethod == ExpensePaymentMethod.DrawerCash;
            var accCredit = await ResolveAccountAsync(isCash ? "1101" : "1102", AccountType.Asset, AccountNature.Debit);

            if (accVendorPayable == null || accCredit == null)
            {
                _logger.LogError("Required financial accounts could not be resolved for Vendor Payment #{PaymentNumber}.", payment.PaymentNumber);
                return null;
            }

            var entryNumber = await _unitOfWork.JournalEntries.GenerateNextEntryNumberAsync();

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = payment.PaymentDate,
                PostingDate = DateTime.UtcNow,
                Status = JournalEntryStatus.Posted,
                SourceType = JournalSourceType.Vendor_Payment,
                ReferenceNumber = payment.PaymentNumber,
                NarrationEn = $"Payment #{payment.PaymentNumber} to vendor '{vendor.Name}' via {payment.PaymentMethod}" + (!string.IsNullOrWhiteSpace(payment.ReferenceNo) ? $" (Ref: {payment.ReferenceNo})" : ""),
                NarrationAr = $"سند صرف وسداد للمورد '{vendor.Name}' رقم {payment.PaymentNumber} بطريقة {payment.PaymentMethod}" + (!string.IsNullOrWhiteSpace(payment.ReferenceNo) ? $" (مرجع: {payment.ReferenceNo})" : ""),
                TotalDebit = payment.Amount,
                TotalCredit = payment.Amount,
                CreatedByUserId = userId,
                ApprovedByUserId = userId,
                ApprovedAt = DateTime.UtcNow,
                ETASubmissionStatus = ETASubmissionStatus.NotApplicable,
                BranchTaxCode = "0"
            };

            // Dr. Accounts Payable
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = accVendorPayable.Id,
                Debit = payment.Amount,
                Credit = 0m,
                Description = $"Settlement of Vendor Balance - '{vendor.Name}' ({payment.PaymentNumber})"
            });

            // Cr. Cash on Hand or Bank
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = accCredit.Id,
                Debit = 0m,
                Credit = payment.Amount,
                Description = isCash ? $"Cash disbursement via drawer for payment #{payment.PaymentNumber}" : $"Bank disbursement for payment #{payment.PaymentNumber}"
            });

            await _unitOfWork.JournalEntries.AddAsync(entry);
            await ApplyLineBalancesAsync(entry.Lines);

            _logger.LogInformation("Posted GL Journal Entry {EntryNumber} for Vendor Payment #{PaymentNumber} ({Amount:N2} EGP).", entryNumber, payment.PaymentNumber, payment.Amount);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post automated GL journal entry for Vendor Payment #{PaymentNumber}.", payment.PaymentNumber);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<JournalEntry?> PostShiftSalesSummaryAsync(CashierShift shift, List<Domain.Entities.Invoice> completedInvoices, decimal actualCash, decimal diff, int? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (completedInvoices == null || (!completedInvoices.Any() && Math.Abs(diff) < 0.01m))
            {
                _logger.LogInformation("Shift #{ShiftId} has no completed invoices or cash variance. Skipping GL journal entry.", shift.Id);
                return null;
            }

            // Resolve accounts
            var accCash = await ResolveAccountAsync("1101", AccountType.Asset, AccountNature.Debit);
            var accBank = await ResolveAccountAsync("1102", AccountType.Asset, AccountNature.Debit);
            var accSales = await ResolveAccountAsync("4101", AccountType.Revenue, AccountNature.Credit);
            var accVatOutput = await ResolveAccountAsync("2102", AccountType.Liability, AccountNature.Credit);
            var accReturns = await ResolveAccountAsync("4103", AccountType.Revenue, AccountNature.Debit);
            var accShortage = await ResolveAccountAsync("5302", AccountType.Expense, AccountNature.Debit);

            if (accCash == null || accSales == null || accVatOutput == null)
            {
                _logger.LogError("Required financial accounts (1101, 4101, 2102) could not be resolved for Shift #{ShiftId}.", shift.Id);
                return null;
            }

            int? costCenterId = await ResolveCostCenterIdAsync(shift.Register?.BranchId);

            var saleInvoices = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).ToList();
            var returnInvoices = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Return).ToList();

            decimal grossSales = saleInvoices.Sum(i => i.SubTotal - i.DiscountAmount);
            decimal salesVat = saleInvoices.Sum(i => i.TaxAmount);

            decimal grossReturns = returnInvoices.Sum(i => i.SubTotal - i.DiscountAmount);
            decimal returnsVat = returnInvoices.Sum(i => i.TaxAmount);

            var salePayments = saleInvoices.SelectMany(i => i.Payments).ToList();
            decimal cashFromSales = salePayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
            decimal electronicFromSales = salePayments.Where(p => p.PaymentMethod != PaymentMethod.Cash).Sum(p => p.Amount);

            var returnPayments = returnInvoices.SelectMany(i => i.Payments).ToList();
            decimal cashRefunded = returnPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
            decimal electronicRefunded = returnPayments.Where(p => p.PaymentMethod != PaymentMethod.Cash).Sum(p => p.Amount);

            decimal netCashCollected = cashFromSales - cashRefunded;
            decimal netElectronicCollected = electronicFromSales - electronicRefunded;

            var entryNumber = await _unitOfWork.JournalEntries.GenerateNextEntryNumberAsync();
            var registerName = shift.Register?.Name ?? $"Register #{shift.RegisterId}";
            var branchName = shift.Register?.Branch?.NameEn ?? "Store";

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = DateTime.UtcNow,
                PostingDate = DateTime.UtcNow,
                Status = JournalEntryStatus.Posted,
                SourceType = JournalSourceType.POS_Sale,
                ReferenceNumber = $"SHIFT-Z-{shift.Id:D5}",
                NarrationEn = $"POS Shift #{shift.Id} Daily Sales Closing (Register: {registerName}, {branchName})",
                NarrationAr = $"قيد إقفال مبيعات وردية الكاشير اليومية رقم {shift.Id} (نقطة البيع: {registerName})",
                CreatedByUserId = userId,
                ApprovedByUserId = userId,
                ApprovedAt = DateTime.UtcNow,
                ETASubmissionStatus = ETASubmissionStatus.NotApplicable,
                BranchTaxCode = "0"
            };

            // 1. Debit Cash on Hand (Net cash received)
            if (netCashCollected > 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accCash.Id,
                    CostCenterId = costCenterId,
                    Debit = netCashCollected,
                    Credit = 0m,
                    Description = $"Cash collected in POS register drawer for Shift #{shift.Id}"
                });
            }
            else if (netCashCollected < 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accCash.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = Math.Abs(netCashCollected),
                    Description = $"Net cash paid out for customer returns in Shift #{shift.Id}"
                });
            }

            // 2. Debit Bank & Electronic Clearing (Visa, InstaPay, Wallets)
            if (netElectronicCollected > 0 && accBank != null)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accBank.Id,
                    CostCenterId = costCenterId,
                    Debit = netElectronicCollected,
                    Credit = 0m,
                    Description = $"Card & electronic payments settlement for Shift #{shift.Id}"
                });
            }
            else if (netElectronicCollected < 0 && accBank != null)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accBank.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = Math.Abs(netElectronicCollected),
                    Description = $"Net electronic card refund for Shift #{shift.Id}"
                });
            }

            // 3. Debit Sales Discounts & Returns (if returns occurred)
            if (grossReturns > 0 && accReturns != null)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accReturns.Id,
                    CostCenterId = costCenterId,
                    Debit = grossReturns,
                    Credit = 0m,
                    Description = $"Apparel merchandise returns during Shift #{shift.Id}"
                });
            }

            // 4. Debit Reversal of Output VAT on Returns
            if (returnsVat > 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accVatOutput.Id,
                    CostCenterId = costCenterId,
                    Debit = returnsVat,
                    Credit = 0m,
                    Description = $"VAT reduction on apparel returns for Shift #{shift.Id}",
                    ETATaxType = ETATaxType.T1_VAT,
                    TaxRate = 14m,
                    TaxAmount = returnsVat
                });
            }

            // 5. Credit Retail Sales Revenue
            if (grossSales > 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accSales.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = grossSales,
                    Description = $"Retail apparel sales revenue for Shift #{shift.Id}"
                });
            }

            // 6. Credit Output VAT Payable 14%
            if (salesVat > 0)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accVatOutput.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = salesVat,
                    Description = $"Output VAT 14% payable on retail sales for Shift #{shift.Id}",
                    ETATaxType = ETATaxType.T1_VAT,
                    TaxRate = 14m,
                    TaxAmount = salesVat
                });
            }

            // 7. Cash Drawer Discrepancy Adjustment (if any)
            if (diff < -0.01m && accShortage != null)
            {
                decimal shortage = Math.Abs(diff);
                // Dr. Cash Shortage Expense
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accShortage.Id,
                    CostCenterId = costCenterId,
                    Debit = shortage,
                    Credit = 0m,
                    Description = $"Cash drawer shortage discrepancy for Shift #{shift.Id}"
                });
                // Cr. Cash on Hand
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accCash.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = shortage,
                    Description = $"Reduction of cash on hand due to drawer shortage in Shift #{shift.Id}"
                });
            }
            else if (diff > 0.01m)
            {
                // Dr. Cash on Hand
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accCash.Id,
                    CostCenterId = costCenterId,
                    Debit = diff,
                    Credit = 0m,
                    Description = $"Cash on hand addition due to drawer overage in Shift #{shift.Id}"
                });
                // Cr. Miscellaneous Revenue
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = accSales.Id,
                    CostCenterId = costCenterId,
                    Debit = 0m,
                    Credit = diff,
                    Description = $"Unexplained cash drawer overage surplus for Shift #{shift.Id}"
                });
            }

            if (!entry.Lines.Any())
            {
                _logger.LogInformation("No journal lines generated for Shift #{ShiftId}.", shift.Id);
                return null;
            }

            entry.TotalDebit = entry.Lines.Sum(l => l.Debit);
            entry.TotalCredit = entry.Lines.Sum(l => l.Credit);

            // Safety verification: Balance must match
            if (Math.Abs(entry.TotalDebit - entry.TotalCredit) > 0.01m)
            {
                _logger.LogError("Shift closing journal entry {EntryNumber} is unbalanced: Debit={Debit}, Credit={Credit}. Aborting auto-posting.", entryNumber, entry.TotalDebit, entry.TotalCredit);
                return null;
            }

            await _unitOfWork.JournalEntries.AddAsync(entry);
            await ApplyLineBalancesAsync(entry.Lines);

            _logger.LogInformation("Posted consolidated Z-Report GL Journal Entry {EntryNumber} for Shift #{ShiftId} (Debit/Credit: {Total:N2} EGP).", entryNumber, shift.Id, entry.TotalDebit);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post automated GL journal entry for Shift #{ShiftId}.", shift.Id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<JournalEntry?> PostExpenseAsync(Domain.Entities.Expense expense, ExpenseCategory category, int? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (expense.Amount <= 0) return null;

            // Map expense account based on category code/nature
            string accountCode = "5302"; // Default: General & Admin Expenses
            var catCode = category.Code.ToUpperInvariant();
            if (catCode.Contains("RENT")) accountCode = "5201";
            else if (catCode.Contains("UTIL") || catCode.Contains("POWER") || catCode.Contains("ELEC")) accountCode = "5203";
            else if (catCode.Contains("MARKET") || catCode.Contains("ADVERT")) accountCode = "5202";

            var accExpense = await ResolveAccountAsync(accountCode, AccountType.Expense, AccountNature.Debit);
            var isCash = expense.IsDrawerExpense || expense.PaymentMethod == ExpensePaymentMethod.Cash || expense.PaymentMethod == ExpensePaymentMethod.DrawerCash || expense.PaymentMethod == ExpensePaymentMethod.PettyCash;
            var accCredit = await ResolveAccountAsync(isCash ? "1101" : "1102", AccountType.Asset, AccountNature.Debit);

            if (accExpense == null || accCredit == null)
            {
                _logger.LogError("Required financial accounts could not be resolved for Expense #{ExpenseNumber}.", expense.ExpenseNumber);
                return null;
            }

            int? costCenterId = await ResolveCostCenterIdAsync(expense.BranchId);
            var entryNumber = await _unitOfWork.JournalEntries.GenerateNextEntryNumberAsync();

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = expense.ExpenseDate,
                PostingDate = DateTime.UtcNow,
                Status = JournalEntryStatus.Posted,
                SourceType = JournalSourceType.OperationalExpense,
                ReferenceNumber = expense.ExpenseNumber,
                NarrationEn = $"Operational Expense: {category.NameEn} - {expense.Description} (#{expense.ExpenseNumber})",
                NarrationAr = $"إثبات مصروف تشغيلي: {category.NameAr} - {expense.Description} (#{expense.ExpenseNumber})",
                TotalDebit = expense.Amount,
                TotalCredit = expense.Amount,
                CreatedByUserId = userId,
                ApprovedByUserId = userId,
                ApprovedAt = DateTime.UtcNow,
                ETASubmissionStatus = ETASubmissionStatus.NotApplicable,
                BranchTaxCode = "0"
            };

            // Dr. Expense Account
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = accExpense.Id,
                CostCenterId = costCenterId,
                Debit = expense.Amount,
                Credit = 0m,
                Description = $"{category.NameEn}: {expense.Description}"
            });

            // Cr. Cash on Hand or Bank
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = accCredit.Id,
                CostCenterId = costCenterId,
                Debit = 0m,
                Credit = expense.Amount,
                Description = isCash ? $"Disbursed from cash drawer for expense #{expense.ExpenseNumber}" : $"Disbursed from bank for expense #{expense.ExpenseNumber}"
            });

            await _unitOfWork.JournalEntries.AddAsync(entry);
            await ApplyLineBalancesAsync(entry.Lines);

            _logger.LogInformation("Posted GL Journal Entry {EntryNumber} for Expense #{ExpenseNumber} ({Amount:N2} EGP).", entryNumber, expense.ExpenseNumber, expense.Amount);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post automated GL journal entry for Expense #{ExpenseNumber}.", expense.ExpenseNumber);
            return null;
        }
    }

    #region Helpers

    private async Task<Account?> ResolveAccountAsync(string preferredCode, AccountType fallbackType, AccountNature fallbackNature)
    {
        var acc = await _unitOfWork.Accounts.GetByCodeAsync(preferredCode);
        if (acc != null && acc.IsPostable) return acc;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync(isActive: true, type: fallbackType);
        return allAccounts.FirstOrDefault(a => a.IsPostable && a.Nature == fallbackNature) ?? allAccounts.FirstOrDefault(a => a.IsPostable);
    }

    private async Task<int?> ResolveCostCenterIdAsync(int? branchId)
    {
        if (!branchId.HasValue) return null;
        var costCenters = await _unitOfWork.CostCenters.GetAllAsync(isActive: true, branchId: branchId.Value);
        return costCenters.FirstOrDefault()?.Id;
    }

    private async Task ApplyLineBalancesAsync(IEnumerable<JournalEntryLine> lines)
    {
        foreach (var line in lines)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
            if (account != null)
            {
                decimal delta = account.Nature == AccountNature.Debit
                    ? line.Debit - line.Credit
                    : line.Credit - line.Debit;

                account.CurrentBalance += delta;
                account.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);
            }
        }
    }

    #endregion
}

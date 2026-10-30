using POS_Private.Domain.Entities;

namespace POS_Private.Application.Services;

/// <summary>
/// Domain Accounting Posting Service implementing modern Egyptian ERP & retail standards.
/// Handles automated General Ledger (GL) reflection for Vendor Bills, Payments, Shift End Sales (Z-Report), and Expenses.
/// </summary>
public interface IJournalPostingService
{
    /// <summary>
    /// Egyptian B2B Standard: Real-time per Purchase Bill posting.
    /// Dr. Apparel Finished Goods Inventory (1104) [Net purchase amount]
    /// Dr. Input VAT Receivable 14% (1106) [VAT amount]
    /// Cr. Accounts Payable / Vendors (2101) [Total amount]
    /// </summary>
    Task<JournalEntry?> PostVendorBillAsync(VendorBill bill, Vendor vendor, int? userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Egyptian B2B Standard: Real-time per Payment voucher posting.
    /// Dr. Accounts Payable / Vendors (2101)
    /// Cr. Cash (1101) or Bank (1102)
    /// </summary>
    Task<JournalEntry?> PostVendorPaymentAsync(VendorPayment payment, Vendor vendor, int? userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Egyptian Retail POS (B2C) Standard: Shift-End Z-Report Batch Summary Posting.
    /// Dr. Cash on Hand (1101) [Total cash collected from sales]
    /// Dr. Bank & Electronic Cards (1102) [Visa, Mastercard, InstaPay, Mobile Wallets]
    /// Dr. Sales Discounts & Returns (4103) [If any returns in shift]
    /// Dr. Cash Shortage (5302) [If drawer cash discrepancy < 0]
    /// Cr. Retail Clothing Sales (4101) [Gross / Net sales]
    /// Cr. Output VAT Payable 14% (2102) [VAT output]
    /// Cr. Cash Overage (4101) [If drawer cash discrepancy > 0]
    /// </summary>
    Task<JournalEntry?> PostShiftSalesSummaryAsync(CashierShift shift, List<Domain.Entities.Invoice> completedInvoices, decimal actualCash, decimal diff, int? userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Operational / Petty Cash Drawer Expense Posting.
    /// Dr. Operating Expense (5201/5203/5302 based on category)
    /// Cr. Cash (1101) or Bank (1102)
    /// </summary>
    Task<JournalEntry?> PostExpenseAsync(Domain.Entities.Expense expense, ExpenseCategory category, int? userId, CancellationToken cancellationToken = default);
}

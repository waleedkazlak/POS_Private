namespace POS_Private.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IColorRepository Colors { get; }
    ISizeRepository Sizes { get; }
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IBranchRepository Branches { get; }
    IInventoryRepository Inventories { get; }
    IPosRegisterRepository PosRegisters { get; }
    ICashierShiftRepository CashierShifts { get; }
    IInvoiceRepository Invoices { get; }
    IInvoiceReturnRepository InvoiceReturns { get; }
    ICustomerRepository Customers { get; }
    ICouponRepository Coupons { get; }
    IBranchStockRepository BranchStocks { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IRolePermissionRepository RolePermissions { get; }
    IScreenRepository Screens { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IVendorRepository Vendors { get; }
    IVendorTransactionRepository VendorTransactions { get; }
    IVendorBillRepository VendorBills { get; }
    IVendorPaymentRepository VendorPayments { get; }
    IExpenseCategoryRepository ExpenseCategories { get; }
    IExpenseRepository Expenses { get; }
    IAccountRepository Accounts { get; }
    ICostCenterRepository CostCenters { get; }
    IJournalEntryRepository JournalEntries { get; }
    IETACompanySettingRepository ETASettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

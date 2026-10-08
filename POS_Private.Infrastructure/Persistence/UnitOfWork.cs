using POS_Private.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace POS_Private.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly POS_PrivateDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public ICategoryRepository Categories { get; }
    public IColorRepository Colors { get; }
    public ISizeRepository Sizes { get; }
    public IProductRepository Products { get; }
    public IProductVariantRepository ProductVariants { get; }
    public IBranchRepository Branches { get; }
    public IInventoryRepository Inventories { get; }
    public IPosRegisterRepository PosRegisters { get; }
    public ICashierShiftRepository CashierShifts { get; }
    public IInvoiceRepository Invoices { get; }
    public IInvoiceReturnRepository InvoiceReturns { get; }
    public ICustomerRepository Customers { get; }
    public ICouponRepository Coupons { get; }
    public IBranchStockRepository BranchStocks { get; }
    public IUserRepository Users { get; }
    public IRoleRepository Roles { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public IScreenRepository Screens { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IVendorRepository Vendors { get; }
    public IVendorTransactionRepository VendorTransactions { get; }
    public IVendorBillRepository VendorBills { get; }
    public IVendorPaymentRepository VendorPayments { get; }
    public IExpenseCategoryRepository ExpenseCategories { get; }
    public IExpenseRepository Expenses { get; }
    public IAccountRepository Accounts { get; }
    public ICostCenterRepository CostCenters { get; }
    public IJournalEntryRepository JournalEntries { get; }
    public IETACompanySettingRepository ETASettings { get; }

    public UnitOfWork(POS_PrivateDbContext context)
    {
        _context = context;

        Categories = new CategoryRepository(_context);
        Colors = new ColorRepository(_context);
        Sizes = new SizeRepository(_context);
        Products = new ProductRepository(_context);
        ProductVariants = new ProductVariantRepository(_context);
        Branches = new BranchRepository(_context);
        Inventories = new InventoryRepository(_context);
        PosRegisters = new PosRegisterRepository(_context);
        CashierShifts = new CashierShiftRepository(_context);
        Invoices = new InvoiceRepository(_context);
        InvoiceReturns = new InvoiceReturnRepository(_context);
        Customers = new CustomerRepository(_context);
        Coupons = new CouponRepository(_context);
        BranchStocks = new BranchStockRepository(_context);
        Users = new UserRepository(_context);
        Roles = new RoleRepository(_context);
        RolePermissions = new RolePermissionRepository(_context);
        Screens = new ScreenRepository(_context);
        RefreshTokens = new RefreshTokenRepository(_context);
        Vendors = new VendorRepository(_context);
        VendorTransactions = new VendorTransactionRepository(_context);
        VendorBills = new VendorBillRepository(_context);
        VendorPayments = new VendorPaymentRepository(_context);
        ExpenseCategories = new ExpenseCategoryRepository(_context);
        Expenses = new ExpenseRepository(_context);
        Accounts = new AccountRepository(_context);
        CostCenters = new CostCenterRepository(_context);
        JournalEntries = new JournalEntryRepository(_context);
        ETASettings = new ETACompanySettingRepository(_context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}

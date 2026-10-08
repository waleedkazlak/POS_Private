using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<int> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}

public interface IColorRepository
{
    Task<Color?> GetByIdAsync(int id);
    Task<IEnumerable<Color>> GetAllAsync();
    Task<int> AddAsync(Color color);
    Task UpdateAsync(Color color);
    Task DeleteAsync(int id);
}

public interface ISizeRepository
{
    Task<Size?> GetByIdAsync(int id);
    Task<IEnumerable<Size>> GetAllAsync();
    Task<int> AddAsync(Size size);
    Task UpdateAsync(Size size);
    Task DeleteAsync(int id);
}

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithDetailsAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByBarcodeOrSkuAsync(string barcodeOrSku);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? categoryId,
        bool? isActive,
        int pageNumber,
        int pageSize);
    Task<int> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

public interface IProductVariantRepository
{
    Task<ProductVariant?> GetByIdAsync(int id);
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
    Task<int> AddAsync(ProductVariant variant);
    Task UpdateAsync(ProductVariant variant);
    Task DeleteAsync(int id);
}

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch?> GetByCodeAsync(string code);
    Task<IEnumerable<Branch>> GetAllAsync();
    Task<int> AddAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
}

public interface IPosRegisterRepository
{
    Task<PosRegister?> GetByIdAsync(int id);
    Task<IEnumerable<PosRegister>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<PosRegister>> GetAllAsync();
    Task<int> AddAsync(PosRegister register);
    Task UpdateAsync(PosRegister register);
    Task DeleteAsync(int id);
}

public interface ICashierShiftRepository
{
    Task<CashierShift?> GetByIdAsync(int id);
    Task<CashierShift?> GetActiveShiftForUserAsync(int userId);
    Task<CashierShift?> GetActiveShiftForRegisterAsync(int registerId);
    Task<IEnumerable<CashierShift>> GetShiftsAsync(int? branchId, int? registerId, int? cashierUserId, DateTime? fromDate, DateTime? toDate);
    Task<int> AddAsync(CashierShift shift);
    Task UpdateAsync(CashierShift shift);
    Task AddTransactionAsync(ShiftTransaction transaction);
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByIdWithDetailsAsync(int id);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<(IEnumerable<Invoice> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? branchId,
        int? registerId,
        int? shiftId,
        int? cashierUserId,
        int? customerId,
        InvoiceType? type,
        InvoiceStatus? status,
        PaymentStatus? paymentStatus,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
    Task<int> AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task<string> GenerateNextInvoiceNumberAsync();
}

public interface IInvoiceReturnRepository
{
    Task<InvoiceReturn?> GetByIdAsync(int id);
    Task<InvoiceReturn?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<InvoiceReturn>> GetByOriginalInvoiceIdAsync(int originalInvoiceId);
    Task<int> AddAsync(InvoiceReturn invoiceReturn);
    Task<string> GenerateNextReturnNumberAsync();
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);
    Task<Customer?> GetByCodeAsync(string code);
    Task<(IEnumerable<Customer> Items, int TotalCount)> SearchAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<int> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(int id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetAllAsync();
    Task<int> AddAsync(Coupon coupon);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(int id);
}

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<Inventory?> GetByCodeAsync(string code);
    Task<IEnumerable<Inventory>> GetAllAsync(bool? includeInactive = false, int? branchId = null);
    Task<(IEnumerable<Inventory> Items, int TotalCount)> SearchAsync(string? searchTerm, bool? includeInactive, int? branchId, int pageNumber, int pageSize);
    Task<int> AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(int id);
}

public interface IBranchStockRepository
{
    Task<BranchStock?> GetStockAsync(int branchId, int productId, int? variantId);
    Task<BranchStock?> GetInventoryStockAsync(int inventoryId, int productId, int? variantId);
    Task<IEnumerable<BranchStock>> GetBranchStocksAsync(int branchId);
    Task<IEnumerable<BranchStock>> GetInventoryStocksAsync(int inventoryId);
    Task<IEnumerable<BranchStock>> GetAllStocksAsync(int? inventoryId = null, int? branchId = null, int? productId = null);
    Task<(IEnumerable<BranchStock> Items, int TotalCount)> SearchStocksAsync(int? inventoryId, int? branchId, int? productId, string? searchTerm, bool? isLowStock, int pageNumber, int pageSize);
    Task<IEnumerable<BranchStock>> GetProductStockAvailabilityAsync(int productId, int? variantId = null);
    Task<IEnumerable<BranchStock>> GetLowStockAlertsAsync(int? branchId = null, int? inventoryId = null);
    Task<(IEnumerable<BranchStock> Items, int TotalCount)> SearchLowStockAlertsAsync(int? branchId, int? inventoryId, int pageNumber, int pageSize);
    Task AddOrUpdateStockAsync(BranchStock stock);
    Task<IEnumerable<StockMovement>> GetMovementsAsync(int? branchId, int? productId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? inventoryId = null);
    Task<(IEnumerable<StockMovement> Items, int TotalCount)> SearchMovementsAsync(int? branchId, int? productId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? inventoryId = null);
    Task AddStockMovementAsync(StockMovement movement);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<int> AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(int id);
}

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId);
    Task UpdatePermissionsAsync(int roleId, IEnumerable<RolePermission> permissions);
}

public interface IScreenRepository
{
    Task<IEnumerable<Screen>> GetAllAsync();
    Task<Screen?> GetByCodeAsync(string code);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<int> AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeUserTokensAsync(int userId);
}

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(int id);
    Task<Vendor?> GetByCodeAsync(string code);
    Task<IEnumerable<Vendor>> GetAllAsync(bool? isActive = null);
    Task<(IEnumerable<Vendor> Items, int TotalCount)> SearchAsync(string? searchTerm, bool? isActive, int pageNumber, int pageSize);
    Task<int> AddAsync(Vendor vendor);
    Task UpdateAsync(Vendor vendor);
    Task DeleteAsync(int id);
    Task<string> GenerateNextVendorCodeAsync();
}

public interface IVendorTransactionRepository
{
    Task<IEnumerable<VendorTransaction>> GetByVendorIdAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> AddAsync(VendorTransaction transaction);
    Task<decimal> GetBalanceBeforeDateAsync(int vendorId, DateTime date);
}

public interface IVendorBillRepository
{
    Task<VendorBill?> GetByIdAsync(int id);
    Task<VendorBill?> GetByIdWithDetailsAsync(int id);
    Task<VendorBill?> GetByBillNumberAsync(string billNumber);
    Task<(IEnumerable<VendorBill> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? vendorId,
        int? branchId,
        VendorBillStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
    Task<int> AddAsync(VendorBill bill);
    Task UpdateAsync(VendorBill bill);
    Task<string> GenerateNextBillNumberAsync();
}

public interface IVendorPaymentRepository
{
    Task<VendorPayment?> GetByIdAsync(int id);
    Task<IEnumerable<VendorPayment>> GetByVendorIdAsync(int vendorId);
    Task<IEnumerable<VendorPayment>> GetByBillIdAsync(int billId);
    Task<int> AddAsync(VendorPayment payment);
    Task<string> GenerateNextPaymentNumberAsync();
}

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(int id);
    Task<ExpenseCategory?> GetByCodeAsync(string code);
    Task<IEnumerable<ExpenseCategory>> GetAllAsync(bool? isActive = null);
    Task<int> AddAsync(ExpenseCategory category);
    Task UpdateAsync(ExpenseCategory category);
    Task DeleteAsync(int id);
}

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense?> GetByIdWithDetailsAsync(int id);
    Task<(IEnumerable<Expense> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? categoryId,
        int? branchId,
        int? shiftId,
        ExpensePaymentMethod? paymentMethod,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
    Task<int> AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
    Task<string> GenerateNextExpenseNumberAsync();
    Task<decimal> GetTotalExpensesAsync(int? branchId, DateTime? fromDate, DateTime? toDate);
}

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByCodeAsync(string code);
    Task<IEnumerable<Account>> GetAllAsync(bool? isActive = null, AccountType? type = null);
    Task<IEnumerable<Account>> GetHierarchyAsync();
    Task<int> AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id);
    Task UpdateBalanceAsync(int accountId, decimal delta);
}

public interface ICostCenterRepository
{
    Task<CostCenter?> GetByIdAsync(int id);
    Task<CostCenter?> GetByCodeAsync(string code);
    Task<IEnumerable<CostCenter>> GetAllAsync(bool? isActive = null, int? branchId = null);
    Task<int> AddAsync(CostCenter costCenter);
    Task UpdateAsync(CostCenter costCenter);
    Task DeleteAsync(int id);
}

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(int id);
    Task<JournalEntry?> GetByIdWithLinesAsync(int id);
    Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber);
    Task<(IEnumerable<JournalEntry> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        JournalEntryStatus? status,
        JournalSourceType? sourceType,
        DateTime? fromDate,
        DateTime? toDate,
        int? accountId,
        int? costCenterId,
        int pageNumber,
        int pageSize);
    Task<int> AddAsync(JournalEntry entry);
    Task UpdateAsync(JournalEntry entry);
    Task<string> GenerateNextEntryNumberAsync();
    Task<IEnumerable<JournalEntryLine>> GetLinesWithEntryAsync(
        JournalEntryStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? accountId = null,
        int? costCenterId = null);
}

public interface IETACompanySettingRepository
{
    Task<ETACompanySetting> GetSettingAsync();
    Task UpdateSettingAsync(ETACompanySetting setting);
}


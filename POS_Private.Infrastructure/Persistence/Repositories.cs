using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Infrastructure.Persistence;

public class CategoryRepository : ICategoryRepository
{
    private readonly POS_PrivateDbContext _context;

    public CategoryRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id) =>
        await _context.Categories.Include(c => c.ParentCategory).Include(c => c.SubCategories).Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category?> GetBySlugAsync(string slug) =>
        await _context.Categories.Include(c => c.ParentCategory).Include(c => c.SubCategories).Include(c => c.Products).FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _context.Categories.Include(c => c.ParentCategory).Include(c => c.SubCategories).Include(c => c.Products).ToListAsync();

    public async Task<int> AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        return category.Id;
    }

    public Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null) _context.Categories.Remove(category);
    }
}

public class ColorRepository : IColorRepository
{
    private readonly POS_PrivateDbContext _context;

    public ColorRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Color?> GetByIdAsync(int id) =>
        await _context.Colors.FindAsync(id);

    public async Task<IEnumerable<Color>> GetAllAsync() =>
        await _context.Colors.OrderBy(c => c.DisplayOrder).ThenBy(c => c.NameEn).ToListAsync();

    public async Task<int> AddAsync(Color color)
    {
        await _context.Colors.AddAsync(color);
        return color.Id;
    }

    public Task UpdateAsync(Color color)
    {
        _context.Colors.Update(color);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var color = await _context.Colors.FindAsync(id);
        if (color != null) _context.Colors.Remove(color);
    }
}

public class SizeRepository : ISizeRepository
{
    private readonly POS_PrivateDbContext _context;

    public SizeRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Size?> GetByIdAsync(int id) =>
        await _context.Sizes.FindAsync(id);

    public async Task<IEnumerable<Size>> GetAllAsync() =>
        await _context.Sizes.OrderBy(s => s.DisplayOrder).ThenBy(s => s.NameEn).ToListAsync();

    public async Task<int> AddAsync(Size size)
    {
        await _context.Sizes.AddAsync(size);
        return size.Id;
    }

    public Task UpdateAsync(Size size)
    {
        _context.Sizes.Update(size);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var size = await _context.Sizes.FindAsync(id);
        if (size != null) _context.Sizes.Remove(size);
    }
}

public class ProductRepository : IProductRepository
{
    private readonly POS_PrivateDbContext _context;

    public ProductRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await _context.Products.FindAsync(id);

    public async Task<Product?> GetByIdWithDetailsAsync(int id) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.Images)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Inventory)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Branch)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.Images)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Inventory)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Branch)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<Product?> GetByBarcodeOrSkuAsync(string barcodeOrSku)
    {
        var clean = barcodeOrSku.Trim();
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.Images)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Inventory)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Branch)
            .FirstOrDefaultAsync(p => p.Barcode == clean || p.SKU == clean);

        if (product == null)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Color)
                .Include(v => v.Size)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Category)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Variants).ThenInclude(v => v.Color)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Variants).ThenInclude(v => v.Size)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.BranchStocks).ThenInclude(bs => bs.Inventory)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.BranchStocks).ThenInclude(bs => bs.Branch)
                .FirstOrDefaultAsync(v => v.Barcode == clean || v.SKU == clean);

            if (variant?.Product != null)
                product = variant.Product;
        }

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Inventory)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Branch)
            .ToListAsync();

    public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? categoryId,
        bool? isActive,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Inventory)
            .Include(p => p.BranchStocks).ThenInclude(bs => bs.Branch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.NameEn.ToLower().Contains(term) ||
                                     (p.NameAr != null && p.NameAr.ToLower().Contains(term)) ||
                                     (p.SKU != null && p.SKU.ToLower().Contains(term)) ||
                                     (p.Barcode != null && p.Barcode.ToLower().Contains(term)) ||
                                     p.Variants.Any(v => (v.Color != null && (v.Color.NameEn.ToLower().Contains(term) || v.Color.NameAr.ToLower().Contains(term))) ||
                                                         (v.Size != null && (v.Size.NameEn.ToLower().Contains(term) || v.Size.Code.ToLower().Contains(term))) ||
                                                         (v.SKU != null && v.SKU.ToLower().Contains(term))));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        return product.Id;
    }

    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null) _context.Products.Remove(product);
    }
}

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly POS_PrivateDbContext _context;

    public ProductVariantRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<ProductVariant?> GetByIdAsync(int id) =>
        await _context.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.Color)
            .Include(v => v.Size)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId) =>
        await _context.ProductVariants
            .Include(v => v.Color)
            .Include(v => v.Size)
            .Where(v => v.ProductId == productId)
            .ToListAsync();

    public async Task<int> AddAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
        return variant.Id;
    }

    public Task UpdateAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant != null) _context.ProductVariants.Remove(variant);
    }
}

public class BranchRepository : IBranchRepository
{
    private readonly POS_PrivateDbContext _context;

    public BranchRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Branch?> GetByIdAsync(int id) =>
        await _context.Branches.Include(b => b.Registers).FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Branch?> GetByCodeAsync(string code) =>
        await _context.Branches.Include(b => b.Registers).FirstOrDefaultAsync(b => b.Code == code);

    public async Task<IEnumerable<Branch>> GetAllAsync() =>
        await _context.Branches.Include(b => b.Registers).ToListAsync();

    public async Task<int> AddAsync(Branch branch)
    {
        await _context.Branches.AddAsync(branch);
        return branch.Id;
    }

    public Task UpdateAsync(Branch branch)
    {
        _context.Branches.Update(branch);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null) _context.Branches.Remove(branch);
    }
}

public class PosRegisterRepository : IPosRegisterRepository
{
    private readonly POS_PrivateDbContext _context;

    public PosRegisterRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<PosRegister?> GetByIdAsync(int id) =>
        await _context.PosRegisters.Include(r => r.Branch).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<PosRegister>> GetByBranchIdAsync(int branchId) =>
        await _context.PosRegisters.Where(r => r.BranchId == branchId).Include(r => r.Branch).ToListAsync();

    public async Task<IEnumerable<PosRegister>> GetAllAsync() =>
        await _context.PosRegisters.Include(r => r.Branch).ToListAsync();

    public async Task<int> AddAsync(PosRegister register)
    {
        await _context.PosRegisters.AddAsync(register);
        return register.Id;
    }

    public Task UpdateAsync(PosRegister register)
    {
        _context.PosRegisters.Update(register);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var register = await _context.PosRegisters.FindAsync(id);
        if (register != null) _context.PosRegisters.Remove(register);
    }
}

public class CashierShiftRepository : ICashierShiftRepository
{
    private readonly POS_PrivateDbContext _context;

    public CashierShiftRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<CashierShift?> GetByIdAsync(int id) =>
        await _context.CashierShifts
            .Include(s => s.Register)
                .ThenInclude(r => r!.Branch)
            .Include(s => s.CashierUser)
            .Include(s => s.Transactions)
            .Include(s => s.Invoices)
                .ThenInclude(i => i.Payments)
            .Include(s => s.Invoices)
                .ThenInclude(i => i.Lines)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<CashierShift?> GetActiveShiftForUserAsync(int userId) =>
        await _context.CashierShifts
            .Include(s => s.Register)
                .ThenInclude(r => r!.Branch)
            .Include(s => s.CashierUser)
            .Include(s => s.Transactions)
            .Include(s => s.Invoices)
                .ThenInclude(i => i.Payments)
            .FirstOrDefaultAsync(s => s.CashierUserId == userId && s.Status == ShiftStatus.Open);

    public async Task<CashierShift?> GetActiveShiftForRegisterAsync(int registerId) =>
        await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.RegisterId == registerId && s.Status == ShiftStatus.Open);

    public async Task<IEnumerable<CashierShift>> GetShiftsAsync(int? branchId, int? registerId, int? cashierUserId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.CashierShifts
            .Include(s => s.Register)
                .ThenInclude(r => r!.Branch)
            .Include(s => s.CashierUser)
            .Include(s => s.Invoices)
                .ThenInclude(i => i.Payments)
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(s => s.Register!.BranchId == branchId.Value);
        if (registerId.HasValue) query = query.Where(s => s.RegisterId == registerId.Value);
        if (cashierUserId.HasValue) query = query.Where(s => s.CashierUserId == cashierUserId.Value);
        if (fromDate.HasValue) query = query.Where(s => s.OpenedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(s => s.OpenedAt <= toDate.Value);

        return await query.OrderByDescending(s => s.OpenedAt).ToListAsync();
    }

    public async Task<int> AddAsync(CashierShift shift)
    {
        await _context.CashierShifts.AddAsync(shift);
        return shift.Id;
    }

    public Task UpdateAsync(CashierShift shift)
    {
        _context.CashierShifts.Update(shift);
        return Task.CompletedTask;
    }

    public async Task AddTransactionAsync(ShiftTransaction transaction)
    {
        await _context.ShiftTransactions.AddAsync(transaction);
    }
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly POS_PrivateDbContext _context;

    public InvoiceRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int id) =>
        await _context.Invoices.FindAsync(id);

    public async Task<Invoice?> GetByIdWithDetailsAsync(int id) =>
        await _context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.Register)
            .Include(i => i.CashierUser)
            .Include(i => i.Customer)
            .Include(i => i.Coupon)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Include(i => i.Payments)
            .Include(i => i.Returns)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber) =>
        await _context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.Register)
            .Include(i => i.CashierUser)
            .Include(i => i.Customer)
            .Include(i => i.Coupon)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

    public async Task<(IEnumerable<Invoice> Items, int TotalCount)> SearchAsync(
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
        int pageSize)
    {
        var query = _context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.Register)
            .Include(i => i.CashierUser)
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(i => i.InvoiceNumber.ToLower().Contains(term) ||
                                     (i.Customer != null && (i.Customer.FullName.ToLower().Contains(term) || i.Customer.PhoneNumber.Contains(term))));
        }

        if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);
        if (registerId.HasValue) query = query.Where(i => i.RegisterId == registerId.Value);
        if (shiftId.HasValue) query = query.Where(i => i.ShiftId == shiftId.Value);
        if (cashierUserId.HasValue) query = query.Where(i => i.CashierUserId == cashierUserId.Value);
        if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId.Value);
        if (type.HasValue) query = query.Where(i => i.InvoiceType == type.Value);
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        if (paymentStatus.HasValue) query = query.Where(i => i.PaymentStatus == paymentStatus.Value);
        if (fromDate.HasValue) query = query.Where(i => i.InvoiceDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.InvoiceDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.InvoiceDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        return invoice.Id;
    }

    public Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task<string> GenerateNextInvoiceNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMM");
        var count = await _context.Invoices.CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{today}-"));
        return $"INV-{today}-{(count + 1):D5}";
    }
}

public class InvoiceReturnRepository : IInvoiceReturnRepository
{
    private readonly POS_PrivateDbContext _context;

    public InvoiceReturnRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceReturn?> GetByIdAsync(int id) =>
        await _context.InvoiceReturns.FindAsync(id);

    public async Task<InvoiceReturn?> GetByIdWithDetailsAsync(int id) =>
        await _context.InvoiceReturns
            .Include(r => r.OriginalInvoice)
                .ThenInclude(i => i!.Lines)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<InvoiceReturn>> GetByOriginalInvoiceIdAsync(int originalInvoiceId) =>
        await _context.InvoiceReturns
            .Where(r => r.OriginalInvoiceId == originalInvoiceId)
            .Include(r => r.Lines)
            .ToListAsync();

    public async Task<int> AddAsync(InvoiceReturn invoiceReturn)
    {
        await _context.InvoiceReturns.AddAsync(invoiceReturn);
        return invoiceReturn.Id;
    }

    public async Task<string> GenerateNextReturnNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMM");
        var count = await _context.InvoiceReturns.CountAsync(r => r.ReturnNumber.StartsWith($"RET-{today}-"));
        return $"RET-{today}-{(count + 1):D5}";
    }
}

public class CustomerRepository : ICustomerRepository
{
    private readonly POS_PrivateDbContext _context;

    public CustomerRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _context.Customers.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

    public async Task<Customer?> GetByCodeAsync(string code) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> SearchAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Customers.Include(c => c.Invoices).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(term) || c.PhoneNumber.Contains(term) || (c.Email != null && c.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        return customer.Id;
    }

    public Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null) _context.Customers.Remove(customer);
    }
}

public class CouponRepository : ICouponRepository
{
    private readonly POS_PrivateDbContext _context;

    public CouponRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByIdAsync(int id) =>
        await _context.Coupons.FindAsync(id);

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<IEnumerable<Coupon>> GetAllAsync() =>
        await _context.Coupons.ToListAsync();

    public async Task<int> AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
        return coupon.Id;
    }

    public Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null) _context.Coupons.Remove(coupon);
    }
}

public class InventoryRepository : IInventoryRepository
{
    private readonly POS_PrivateDbContext _context;

    public InventoryRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(int id) =>
        await _context.Inventories
            .Include(i => i.Branch)
            .Include(i => i.Stocks)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Inventory?> GetByCodeAsync(string code) =>
        await _context.Inventories
            .Include(i => i.Branch)
            .FirstOrDefaultAsync(i => i.Code == code);

    public async Task<IEnumerable<Inventory>> GetAllAsync(bool? includeInactive = false, int? branchId = null)
    {
        var query = _context.Inventories.Include(i => i.Branch).AsQueryable();
        if (includeInactive != true) query = query.Where(i => i.IsActive);
        if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);
        return await query.OrderBy(i => i.Id).ToListAsync();
    }

    public async Task<(IEnumerable<Inventory> Items, int TotalCount)> SearchAsync(string? searchTerm, bool? includeInactive, int? branchId, int pageNumber, int pageSize)
    {
        var query = _context.Inventories
            .Include(i => i.Branch)
            .Include(i => i.Stocks)
            .AsQueryable();

        if (includeInactive != true) query = query.Where(i => i.IsActive);
        if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(i => i.Code.ToLower().Contains(term) ||
                                     i.NameEn.ToLower().Contains(term) ||
                                     (i.NameAr != null && i.NameAr.Contains(term)) ||
                                     (i.Address != null && i.Address.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(i => i.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Inventory inventory)
    {
        await _context.Inventories.AddAsync(inventory);
        return inventory.Id;
    }

    public Task UpdateAsync(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
            _context.Inventories.Remove(inventory);
    }
}

public class BranchStockRepository : IBranchStockRepository
{
    private readonly POS_PrivateDbContext _context;

    public BranchStockRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<BranchStock?> GetStockAsync(int branchId, int productId, int? variantId) =>
        await _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .FirstOrDefaultAsync(s => (s.BranchId == branchId || (s.Inventory != null && s.Inventory.BranchId == branchId)) && s.ProductId == productId && s.VariantId == variantId);

    public async Task<BranchStock?> GetInventoryStockAsync(int inventoryId, int productId, int? variantId) =>
        await _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .FirstOrDefaultAsync(s => s.InventoryId == inventoryId && s.ProductId == productId && s.VariantId == variantId);

    public async Task<IEnumerable<BranchStock>> GetBranchStocksAsync(int branchId) =>
        await _context.BranchStocks
            .Where(s => s.BranchId == branchId || (s.Inventory != null && s.Inventory.BranchId == branchId))
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .ToListAsync();

    public async Task<IEnumerable<BranchStock>> GetInventoryStocksAsync(int inventoryId) =>
        await _context.BranchStocks
            .Where(s => s.InventoryId == inventoryId)
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .ToListAsync();

    public async Task<IEnumerable<BranchStock>> GetAllStocksAsync(int? inventoryId = null, int? branchId = null, int? productId = null)
    {
        var query = _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .AsQueryable();

        if (inventoryId.HasValue) query = query.Where(s => s.InventoryId == inventoryId.Value);
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value || (s.Inventory != null && s.Inventory.BranchId == branchId.Value));
        if (productId.HasValue) query = query.Where(s => s.ProductId == productId.Value);

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<BranchStock> Items, int TotalCount)> SearchStocksAsync(int? inventoryId, int? branchId, int? productId, string? searchTerm, bool? isLowStock, int pageNumber, int pageSize)
    {
        var query = _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .AsQueryable();

        if (inventoryId.HasValue) query = query.Where(s => s.InventoryId == inventoryId.Value);
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value || (s.Inventory != null && s.Inventory.BranchId == branchId.Value));
        if (productId.HasValue) query = query.Where(s => s.ProductId == productId.Value);
        if (isLowStock == true) query = query.Where(s => s.QuantityOnHand <= s.MinAlertLevel);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(s => (s.Product != null && (s.Product.NameEn.ToLower().Contains(term) || (s.Product.NameAr != null && s.Product.NameAr.Contains(term)) || (s.Product.SKU != null && s.Product.SKU.ToLower().Contains(term)) || (s.Product.Barcode != null && s.Product.Barcode.Contains(term)))) ||
                                     (s.Variant != null && ((s.Variant.SKU != null && s.Variant.SKU.ToLower().Contains(term)) || (s.Variant.Barcode != null && s.Variant.Barcode.Contains(term)))) ||
                                     (s.Inventory != null && (s.Inventory.NameEn.ToLower().Contains(term) || s.Inventory.Code.ToLower().Contains(term))));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(s => s.Product != null ? s.Product.NameEn : "")
            .ThenBy(s => s.InventoryId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<BranchStock>> GetProductStockAvailabilityAsync(int productId, int? variantId = null)
    {
        var query = _context.BranchStocks
            .Where(s => s.ProductId == productId)
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .AsQueryable();

        if (variantId.HasValue)
            query = query.Where(s => s.VariantId == variantId.Value);

        return await query.OrderBy(s => s.InventoryId).ToListAsync();
    }

    public async Task<IEnumerable<BranchStock>> GetLowStockAlertsAsync(int? branchId = null, int? inventoryId = null)
    {
        var query = _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .Where(s => s.QuantityOnHand <= s.MinAlertLevel);

        if (inventoryId.HasValue)
            query = query.Where(s => s.InventoryId == inventoryId.Value);
        else if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value || (s.Inventory != null && s.Inventory.BranchId == branchId.Value));

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<BranchStock> Items, int TotalCount)> SearchLowStockAlertsAsync(int? branchId, int? inventoryId, int pageNumber, int pageSize)
    {
        var query = _context.BranchStocks
            .Include(s => s.Inventory)
            .Include(s => s.Branch)
            .Include(s => s.Product)
            .Include(s => s.Variant).ThenInclude(v => v!.Color)
            .Include(s => s.Variant).ThenInclude(v => v!.Size)
            .Where(s => s.QuantityOnHand <= s.MinAlertLevel);

        if (inventoryId.HasValue)
            query = query.Where(s => s.InventoryId == inventoryId.Value);
        else if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value || (s.Inventory != null && s.Inventory.BranchId == branchId.Value));

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(s => s.QuantityOnHand)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddOrUpdateStockAsync(BranchStock stock)
    {
        if (stock.Id == 0)
            await _context.BranchStocks.AddAsync(stock);
        else
            _context.BranchStocks.Update(stock);
    }

    public async Task<IEnumerable<StockMovement>> GetMovementsAsync(int? branchId, int? productId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? inventoryId = null)
    {
        var (items, _) = await SearchMovementsAsync(branchId, productId, fromDate, toDate, pageNumber, pageSize, inventoryId);
        return items;
    }

    public async Task<(IEnumerable<StockMovement> Items, int TotalCount)> SearchMovementsAsync(int? branchId, int? productId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? inventoryId = null)
    {
        var query = _context.StockMovements
            .Include(m => m.Inventory)
            .Include(m => m.ToInventory)
            .Include(m => m.Branch)
            .Include(m => m.Product)
            .Include(m => m.Variant).ThenInclude(v => v!.Color)
            .Include(m => m.Variant).ThenInclude(v => v!.Size)
            .Include(m => m.CreatedByUser)
            .AsQueryable();

        if (inventoryId.HasValue) query = query.Where(m => m.InventoryId == inventoryId.Value || m.ToInventoryId == inventoryId.Value);
        if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value || (m.Inventory != null && m.Inventory.BranchId == branchId.Value));
        if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
        if (fromDate.HasValue) query = query.Where(m => m.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(m => m.CreatedAt <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddStockMovementAsync(StockMovement movement)
    {
        await _context.StockMovements.AddAsync(movement);
    }
}

public class UserRepository : IUserRepository
{
    private readonly POS_PrivateDbContext _context;

    public UserRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.Include(u => u.Role).Include(u => u.Branch).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.Include(u => u.Role).Include(u => u.Branch).FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.Include(u => u.Role).Include(u => u.Branch).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.Include(u => u.Role).Include(u => u.Branch).ToListAsync();

    public async Task<int> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user.Id;
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null) _context.Users.Remove(user);
    }
}

public class RoleRepository : IRoleRepository
{
    private readonly POS_PrivateDbContext _context;

    public RoleRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int id) =>
        await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Screen).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Role?> GetByNameAsync(string name) =>
        await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Screen).FirstOrDefaultAsync(r => r.Name == name);

    public async Task<IEnumerable<Role>> GetAllAsync() =>
        await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Screen).ToListAsync();

    public async Task<int> AddAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        return role.Id;
    }

    public Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role != null) _context.Roles.Remove(role);
    }
}

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly POS_PrivateDbContext _context;

    public RolePermissionRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId) =>
        await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Include(rp => rp.Screen).ToListAsync();

    public async Task UpdatePermissionsAsync(int roleId, IEnumerable<RolePermission> permissions)
    {
        var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
        _context.RolePermissions.RemoveRange(existing);
        await _context.RolePermissions.AddRangeAsync(permissions);
    }
}

public class ScreenRepository : IScreenRepository
{
    private readonly POS_PrivateDbContext _context;

    public ScreenRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Screen>> GetAllAsync() =>
        await _context.Screens.OrderBy(s => s.DisplayOrder).ToListAsync();

    public async Task<Screen?> GetByCodeAsync(string code) =>
        await _context.Screens.FirstOrDefaultAsync(s => s.Code == code);
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly POS_PrivateDbContext _context;

    public RefreshTokenRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _context.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == token);

    public async Task<int> AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        return refreshToken.Id;
    }

    public Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeUserTokensAsync(int userId)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
        foreach (var t in tokens) t.IsRevoked = true;
    }
}

public class VendorRepository : IVendorRepository
{
    private readonly POS_PrivateDbContext _context;

    public VendorRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Vendor?> GetByIdAsync(int id) =>
        await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vendor?> GetByCodeAsync(string code) =>
        await _context.Vendors.FirstOrDefaultAsync(v => v.Code == code);

    public async Task<IEnumerable<Vendor>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.Vendors.AsQueryable();
        if (isActive.HasValue) query = query.Where(v => v.IsActive == isActive.Value);
        return await query.OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<(IEnumerable<Vendor> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Vendors.AsQueryable();

        if (isActive.HasValue) query = query.Where(v => v.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(v => v.Name.ToLower().Contains(term) ||
                                     v.Code.ToLower().Contains(term) ||
                                     (v.PhoneNumber != null && v.PhoneNumber.Contains(term)) ||
                                     (v.TaxNumber != null && v.TaxNumber.Contains(term)) ||
                                     (v.ContactPerson != null && v.ContactPerson.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(v => v.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Vendor vendor)
    {
        await _context.Vendors.AddAsync(vendor);
        return vendor.Id;
    }

    public Task UpdateAsync(Vendor vendor)
    {
        _context.Vendors.Update(vendor);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var v = await GetByIdAsync(id);
        if (v != null) _context.Vendors.Remove(v);
    }

    public async Task<string> GenerateNextVendorCodeAsync()
    {
        var count = await _context.Vendors.CountAsync();
        return $"VND-{(count + 1):D4}";
    }
}

public class VendorTransactionRepository : IVendorTransactionRepository
{
    private readonly POS_PrivateDbContext _context;

    public VendorTransactionRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorTransaction>> GetByVendorIdAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.VendorTransactions
            .Include(t => t.Branch)
            .Include(t => t.CreatedByUser)
            .Where(t => t.VendorId == vendorId);

        if (fromDate.HasValue) query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(t => t.TransactionDate <= toDate.Value);

        return await query.OrderBy(t => t.TransactionDate).ThenBy(t => t.Id).ToListAsync();
    }

    public async Task<int> AddAsync(VendorTransaction transaction)
    {
        await _context.VendorTransactions.AddAsync(transaction);
        return transaction.Id;
    }

    public async Task<decimal> GetBalanceBeforeDateAsync(int vendorId, DateTime date)
    {
        var txs = await _context.VendorTransactions
            .Where(t => t.VendorId == vendorId && t.TransactionDate < date)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        if (!txs.Any())
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId);
            return vendor?.OpeningBalance ?? 0m;
        }

        return txs.Last().BalanceAfter;
    }
}

public class VendorBillRepository : IVendorBillRepository
{
    private readonly POS_PrivateDbContext _context;

    public VendorBillRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<VendorBill?> GetByIdAsync(int id) =>
        await _context.VendorBills.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<VendorBill?> GetByIdWithDetailsAsync(int id) =>
        await _context.VendorBills
            .Include(b => b.Vendor)
            .Include(b => b.Branch)
            .Include(b => b.Inventory)
            .Include(b => b.CreatedByUser)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Variant)
                    .ThenInclude(v => v.Color)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Variant)
                    .ThenInclude(v => v.Size)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<VendorBill?> GetByBillNumberAsync(string billNumber) =>
        await _context.VendorBills.FirstOrDefaultAsync(b => b.BillNumber == billNumber);

    public async Task<(IEnumerable<VendorBill> Items, int TotalCount)> SearchAsync(
        string? searchTerm, int? vendorId, int? branchId, VendorBillStatus? status,
        DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.VendorBills
            .Include(b => b.Vendor)
            .Include(b => b.Branch)
            .Include(b => b.Inventory)
            .Include(b => b.CreatedByUser)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .AsQueryable();

        if (vendorId.HasValue) query = query.Where(b => b.VendorId == vendorId.Value);
        if (branchId.HasValue) query = query.Where(b => b.BranchId == branchId.Value);
        if (status.HasValue) query = query.Where(b => b.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(b => b.BillDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(b => b.BillDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(b => b.BillNumber.ToLower().Contains(term) ||
                                     (b.VendorInvoiceNumber != null && b.VendorInvoiceNumber.ToLower().Contains(term)) ||
                                     (b.Vendor != null && b.Vendor.Name.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.BillDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(VendorBill bill)
    {
        await _context.VendorBills.AddAsync(bill);
        return bill.Id;
    }

    public Task UpdateAsync(VendorBill bill)
    {
        _context.VendorBills.Update(bill);
        return Task.CompletedTask;
    }

    public async Task<string> GenerateNextBillNumberAsync()
    {
        var count = await _context.VendorBills.CountAsync();
        var datePrefix = DateTime.UtcNow.ToString("yyyyMM");
        return $"BILL-{datePrefix}-{(count + 1):D5}";
    }
}

public class VendorPaymentRepository : IVendorPaymentRepository
{
    private readonly POS_PrivateDbContext _context;

    public VendorPaymentRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<VendorPayment?> GetByIdAsync(int id) =>
        await _context.VendorPayments.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<VendorPayment>> GetByVendorIdAsync(int vendorId) =>
        await _context.VendorPayments
            .Include(p => p.Vendor)
            .Include(p => p.VendorBill)
            .Include(p => p.Branch)
            .Include(p => p.CreatedByUser)
            .Where(p => p.VendorId == vendorId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<IEnumerable<VendorPayment>> GetByBillIdAsync(int billId) =>
        await _context.VendorPayments
            .Where(p => p.VendorBillId == billId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<int> AddAsync(VendorPayment payment)
    {
        await _context.VendorPayments.AddAsync(payment);
        return payment.Id;
    }

    public async Task<string> GenerateNextPaymentNumberAsync()
    {
        var count = await _context.VendorPayments.CountAsync();
        var datePrefix = DateTime.UtcNow.ToString("yyyyMM");
        return $"VPAY-{datePrefix}-{(count + 1):D5}";
    }
}

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly POS_PrivateDbContext _context;

    public ExpenseCategoryRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseCategory?> GetByIdAsync(int id) =>
        await _context.ExpenseCategories.Include(c => c.Expenses).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ExpenseCategory?> GetByCodeAsync(string code) =>
        await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.ExpenseCategories.Include(c => c.Expenses).AsQueryable();
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);
        return await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.NameEn).ToListAsync();
    }

    public async Task<int> AddAsync(ExpenseCategory category)
    {
        await _context.ExpenseCategories.AddAsync(category);
        return category.Id;
    }

    public Task UpdateAsync(ExpenseCategory category)
    {
        _context.ExpenseCategories.Update(category);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await GetByIdAsync(id);
        if (cat != null) _context.ExpenseCategories.Remove(cat);
    }
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly POS_PrivateDbContext _context;

    public ExpenseRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(int id) =>
        await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Expense?> GetByIdWithDetailsAsync(int id) =>
        await _context.Expenses
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .Include(e => e.Shift)
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<(IEnumerable<Expense> Items, int TotalCount)> SearchAsync(
        string? searchTerm, int? categoryId, int? branchId, int? shiftId,
        ExpensePaymentMethod? paymentMethod, DateTime? fromDate, DateTime? toDate,
        int pageNumber, int pageSize)
    {
        var query = _context.Expenses
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .Include(e => e.CreatedByUser)
            .AsQueryable();

        if (categoryId.HasValue) query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);
        if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);
        if (shiftId.HasValue) query = query.Where(e => e.ShiftId == shiftId.Value);
        if (paymentMethod.HasValue) query = query.Where(e => e.PaymentMethod == paymentMethod.Value);
        if (fromDate.HasValue) query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.ExpenseDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(e => e.ExpenseNumber.ToLower().Contains(term) ||
                                     e.Description.ToLower().Contains(term) ||
                                     (e.PaidTo != null && e.PaidTo.ToLower().Contains(term)) ||
                                     (e.ReferenceNumber != null && e.ReferenceNumber.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(e => e.ExpenseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
        return expense.Id;
    }

    public Task UpdateAsync(Expense expense)
    {
        _context.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var e = await GetByIdAsync(id);
        if (e != null) _context.Expenses.Remove(e);
    }

    public async Task<string> GenerateNextExpenseNumberAsync()
    {
        var count = await _context.Expenses.CountAsync();
        var datePrefix = DateTime.UtcNow.ToString("yyyyMM");
        return $"EXP-{datePrefix}-{(count + 1):D5}";
    }

    public async Task<decimal> GetTotalExpensesAsync(int? branchId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Expenses.AsQueryable();
        if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);
        if (fromDate.HasValue) query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.ExpenseDate <= toDate.Value);
        return await query.SumAsync(e => e.Amount);
    }
}

public class AccountRepository : IAccountRepository
{
    private readonly POS_PrivateDbContext _context;

    public AccountRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int id) =>
        await _context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Account?> GetByCodeAsync(string code) =>
        await _context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Code == code);

    public async Task<IEnumerable<Account>> GetAllAsync(bool? isActive = null, AccountType? type = null)
    {
        var query = _context.Accounts.AsQueryable();
        if (isActive.HasValue) query = query.Where(a => a.IsActive == isActive.Value);
        if (type.HasValue) query = query.Where(a => a.Type == type.Value);
        return await query.OrderBy(a => a.Code).ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetHierarchyAsync() =>
        await _context.Accounts
            .Include(a => a.ChildrenAccounts)
            .Where(a => a.ParentAccountId == null)
            .OrderBy(a => a.Code)
            .ToListAsync();

    public async Task<int> AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        return account.Id;
    }

    public Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc != null) _context.Accounts.Remove(acc);
    }

    public async Task UpdateBalanceAsync(int accountId, decimal delta)
    {
        var acc = await _context.Accounts.FindAsync(accountId);
        if (acc != null)
        {
            acc.CurrentBalance += delta;
            acc.UpdatedAt = DateTime.UtcNow;
        }
    }
}

public class CostCenterRepository : ICostCenterRepository
{
    private readonly POS_PrivateDbContext _context;

    public CostCenterRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<CostCenter?> GetByIdAsync(int id) =>
        await _context.CostCenters
            .Include(c => c.Branch)
            .Include(c => c.ParentCostCenter)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<CostCenter?> GetByCodeAsync(string code) =>
        await _context.CostCenters
            .Include(c => c.Branch)
            .Include(c => c.ParentCostCenter)
            .FirstOrDefaultAsync(c => c.Code == code);

    public async Task<IEnumerable<CostCenter>> GetAllAsync(bool? isActive = null, int? branchId = null)
    {
        var query = _context.CostCenters.Include(c => c.Branch).Include(c => c.ParentCostCenter).AsQueryable();
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);
        if (branchId.HasValue) query = query.Where(c => c.BranchId == branchId.Value);
        return await query.OrderBy(c => c.Code).ToListAsync();
    }

    public async Task<int> AddAsync(CostCenter costCenter)
    {
        await _context.CostCenters.AddAsync(costCenter);
        return costCenter.Id;
    }

    public Task UpdateAsync(CostCenter costCenter)
    {
        _context.CostCenters.Update(costCenter);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var cc = await _context.CostCenters.FindAsync(id);
        if (cc != null) _context.CostCenters.Remove(cc);
    }
}

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly POS_PrivateDbContext _context;

    public JournalEntryRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetByIdAsync(int id) =>
        await _context.JournalEntries
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .Include(j => j.Lines).ThenInclude(l => l.CostCenter)
            .Include(j => j.CreatedByUser)
            .Include(j => j.ApprovedByUser)
            .Include(j => j.VoidedByUser)
            .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<JournalEntry?> GetByIdWithLinesAsync(int id) => await GetByIdAsync(id);

    public async Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber) =>
        await _context.JournalEntries
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .Include(j => j.Lines).ThenInclude(l => l.CostCenter)
            .FirstOrDefaultAsync(j => j.EntryNumber == entryNumber);

    public async Task<(IEnumerable<JournalEntry> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        JournalEntryStatus? status,
        JournalSourceType? sourceType,
        DateTime? fromDate,
        DateTime? toDate,
        int? accountId,
        int? costCenterId,
        int pageNumber,
        int pageSize)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .Include(j => j.Lines).ThenInclude(l => l.CostCenter)
            .Include(j => j.CreatedByUser)
            .Include(j => j.ApprovedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(j =>
                j.EntryNumber.ToLower().Contains(term) ||
                j.NarrationEn.ToLower().Contains(term) ||
                j.NarrationAr.ToLower().Contains(term) ||
                (j.ReferenceNumber != null && j.ReferenceNumber.ToLower().Contains(term)) ||
                (j.ETAUUID != null && j.ETAUUID.ToLower().Contains(term)));
        }

        if (status.HasValue) query = query.Where(j => j.Status == status.Value);
        if (sourceType.HasValue) query = query.Where(j => j.SourceType == sourceType.Value);
        if (fromDate.HasValue) query = query.Where(j => j.EntryDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(j => j.EntryDate <= toDate.Value);
        if (accountId.HasValue) query = query.Where(j => j.Lines.Any(l => l.AccountId == accountId.Value));
        if (costCenterId.HasValue) query = query.Where(j => j.Lines.Any(l => l.CostCenterId == costCenterId.Value));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> AddAsync(JournalEntry entry)
    {
        await _context.JournalEntries.AddAsync(entry);
        return entry.Id;
    }

    public Task UpdateAsync(JournalEntry entry)
    {
        _context.JournalEntries.Update(entry);
        return Task.CompletedTask;
    }

    public async Task<string> GenerateNextEntryNumberAsync()
    {
        var count = await _context.JournalEntries.CountAsync();
        var datePrefix = DateTime.UtcNow.ToString("yyyyMM");
        return $"JE-{datePrefix}-{(count + 1):D5}";
    }

    public async Task<IEnumerable<JournalEntryLine>> GetLinesWithEntryAsync(
        JournalEntryStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? accountId = null,
        int? costCenterId = null)
    {
        var query = _context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Include(l => l.CostCenter)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.JournalEntry.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate <= toDate.Value);

        if (accountId.HasValue)
            query = query.Where(l => l.AccountId == accountId.Value);

        if (costCenterId.HasValue)
            query = query.Where(l => l.CostCenterId == costCenterId.Value);

        return await query.ToListAsync();
    }
}

public class ETACompanySettingRepository : IETACompanySettingRepository
{
    private readonly POS_PrivateDbContext _context;

    public ETACompanySettingRepository(POS_PrivateDbContext context)
    {
        _context = context;
    }

    public async Task<ETACompanySetting> GetSettingAsync()
    {
        var setting = await _context.ETACompanySettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new ETACompanySetting
            {
                TaxRegistrationNumber = "100-200-300",
                ActivityCode = "4771",
                CompanyName = "POS Private Co.",
                BranchTaxCode = "0",
                DefaultVatRate = 14.0m,
                DefaultWhtRate = 1.0m,
                IsProduction = false,
                AutoSubmitInvoices = false
            };
            await _context.ETACompanySettings.AddAsync(setting);
            await _context.SaveChangesAsync();
        }
        return setting;
    }

    public Task UpdateSettingAsync(ETACompanySetting setting)
    {
        _context.ETACompanySettings.Update(setting);
        return Task.CompletedTask;
    }
}



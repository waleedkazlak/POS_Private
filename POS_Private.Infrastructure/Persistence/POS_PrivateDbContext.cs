using POS_Private.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Infrastructure.Persistence;

public class POS_PrivateDbContext : DbContext
{
    public POS_PrivateDbContext(DbContextOptions<POS_PrivateDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<PosRegister> PosRegisters => Set<PosRegister>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<ShiftTransaction> ShiftTransactions => Set<ShiftTransaction>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceReturn> InvoiceReturns => Set<InvoiceReturn>();
    public DbSet<InvoiceReturnLine> InvoiceReturnLines => Set<InvoiceReturnLine>();
    public DbSet<BranchStock> BranchStocks => Set<BranchStock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorTransaction> VendorTransactions => Set<VendorTransaction>();
    public DbSet<VendorBill> VendorBills => Set<VendorBill>();
    public DbSet<VendorBillLine> VendorBillLines => Set<VendorBillLine>();
    public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<ETACompanySetting> ETACompanySettings => Set<ETACompanySetting>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Color
        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HexCode).HasMaxLength(20);
            entity.HasIndex(e => e.NameEn);
        });

        // Size
        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Name);
            entity.Ignore(e => e.Description);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(150);
            entity.Property(e => e.NameAr).HasMaxLength(150);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.ParentCategory)
                .WithMany(p => p.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Name);
            entity.Ignore(e => e.Description);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(250);
            entity.Property(e => e.NameAr).HasMaxLength(250);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.CompareAtPrice).HasPrecision(18, 2);

            entity.HasIndex(e => e.Slug);
            entity.HasIndex(e => e.SKU);
            entity.HasIndex(e => e.Barcode);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductVariant
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.PriceOverride).HasPrecision(18, 2);
            entity.Property(e => e.CostPriceOverride).HasPrecision(18, 2);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);

            entity.HasIndex(e => e.SKU);
            entity.HasIndex(e => e.Barcode);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Color)
                .WithMany(c => c.Variants)
                .HasForeignKey(e => e.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Size)
                .WithMany(s => s.Variants)
                .HasForeignKey(e => e.SizeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(1000);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Branch
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Inventory
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Name);
            entity.Ignore(e => e.Description);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(150);
            entity.Property(e => e.NameAr).HasMaxLength(150);
            entity.Property(e => e.DescriptionEn).HasMaxLength(500);
            entity.Property(e => e.DescriptionAr).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Inventories)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PosRegister
        modelBuilder.Entity<PosRegister>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RegisterCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Registers)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.PosRegisters)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CashierShift
        modelBuilder.Entity<CashierShift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartingCash).HasPrecision(18, 2);
            entity.Property(e => e.ExpectedCash).HasPrecision(18, 2);
            entity.Property(e => e.ActualCash).HasPrecision(18, 2);
            entity.Property(e => e.Difference).HasPrecision(18, 2);

            entity.HasOne(e => e.Register)
                .WithMany(r => r.Shifts)
                .HasForeignKey(e => e.RegisterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashierUser)
                .WithMany(u => u.Shifts)
                .HasForeignKey(e => e.CashierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Shifts)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ShiftTransaction
        modelBuilder.Entity<ShiftTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(250);

            entity.HasOne(e => e.Shift)
                .WithMany(s => s.Transactions)
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
            entity.HasIndex(e => e.PhoneNumber);
        });

        // Coupon
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercentage).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.ChangeAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Invoices)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Invoices)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Register)
                .WithMany(r => r.Invoices)
                .HasForeignKey(e => e.RegisterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Shift)
                .WithMany(s => s.Invoices)
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashierUser)
                .WithMany(u => u.Invoices)
                .HasForeignKey(e => e.CashierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceLine
        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(250);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalLineAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.InvoiceLines)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.InvoiceLines)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // InvoiceReturn
        modelBuilder.Entity<InvoiceReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReturnNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.ReturnNumber).IsUnique();

            entity.HasOne(e => e.OriginalInvoice)
                .WithMany(i => i.Returns)
                .HasForeignKey(e => e.OriginalInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceReturnLine
        modelBuilder.Entity<InvoiceReturnLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.InvoiceReturn)
                .WithMany(r => r.Lines)
                .HasForeignKey(e => e.InvoiceReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvoiceLine)
                .WithMany(l => l.ReturnLines)
                .HasForeignKey(e => e.InvoiceLineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BranchStock
        modelBuilder.Entity<BranchStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InventoryId, e.ProductId, e.VariantId }).IsUnique();

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Stocks)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.BranchStocks)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.BranchStocks)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockMovement
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.StockMovements)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToInventory)
                .WithMany()
                .HasForeignKey(e => e.ToInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.StockMovements)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // User & Role
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(1000);
            entity.Property(e => e.PreferredLanguage).IsRequired().HasMaxLength(10).HasDefaultValue("en");
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Users)
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Role & Permissions
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.ScreenId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Screen)
                .WithMany(s => s.RolePermissions)
                .HasForeignKey(e => e.ScreenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Vendor
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.ContactPerson).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.CommercialRegister).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // VendorTransaction
        modelBuilder.Entity<VendorTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DebitAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasIndex(e => new { e.VendorId, e.TransactionDate });

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.Transactions)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // VendorBill
        modelBuilder.Entity<VendorBill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BillNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.VendorInvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.BillNumber).IsUnique();
            entity.HasIndex(e => new { e.VendorId, e.BillDate });

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.Bills)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Inventory)
                .WithMany()
                .HasForeignKey(e => e.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // VendorBillLine
        modelBuilder.Entity<VendorBillLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.ReceivedQuantity).HasPrecision(18, 2);

            entity.HasOne(e => e.VendorBill)
                .WithMany(b => b.Lines)
                .HasForeignKey(e => e.VendorBillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Variant)
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // VendorPayment
        modelBuilder.Entity<VendorPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ReferenceNo).HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.Payments)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.VendorBill)
                .WithMany(b => b.Payments)
                .HasForeignKey(e => e.VendorBillId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ExpenseCategory
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(150);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExpenseNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PaidTo).HasMaxLength(150);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(1000);
            entity.HasIndex(e => e.ExpenseNumber).IsUnique();
            entity.HasIndex(e => new { e.ExpenseCategoryId, e.ExpenseDate });

            entity.HasOne(e => e.ExpenseCategory)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Shift)
                .WithMany()
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Type);

            entity.HasOne(e => e.ParentAccount)
                .WithMany(p => p.ChildrenAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CostCenter
        modelBuilder.Entity<CostCenter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.ParentCostCenter)
                .WithMany(p => p.SubCostCenters)
                .HasForeignKey(e => e.ParentCostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntryNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NarrationEn).IsRequired().HasMaxLength(500);
            entity.Property(e => e.NarrationAr).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
            entity.Property(e => e.VoidReason).HasMaxLength(500);
            entity.Property(e => e.ETAUUID).HasMaxLength(100);
            entity.Property(e => e.ETALongId).HasMaxLength(150);
            entity.Property(e => e.ETARejectionReason).HasMaxLength(1000);
            entity.Property(e => e.BranchTaxCode).HasMaxLength(50);

            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => e.EntryDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ETAUUID);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.VoidedByUser)
                .WithMany()
                .HasForeignKey(e => e.VoidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntryLine
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.JournalEntry)
                .WithMany(je => je.Lines)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany(a => a.JournalLines)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CostCenter)
                .WithMany(cc => cc.JournalLines)
                .HasForeignKey(e => e.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FiscalPeriod
        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.Year, e.PeriodNumber }).IsUnique();
        });

        // ETACompanySetting
        modelBuilder.Entity<ETACompanySetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaxRegistrationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ActivityCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchTaxCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ClientId).HasMaxLength(200);
            entity.Property(e => e.ClientSecret).HasMaxLength(200);
            entity.Property(e => e.TokenPin).HasMaxLength(100);
            entity.Property(e => e.DefaultVatRate).HasPrecision(5, 2);
            entity.Property(e => e.DefaultWhtRate).HasPrecision(5, 2);
        });
    }
}


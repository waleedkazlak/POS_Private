using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(POS_PrivateDbContext context)
    {
        // 1. Seed Screens
        if (!await context.Screens.AnyAsync())
        {
            var screens = new List<Screen>
            {
                new() { Name = "POS Register", Code = "POS_REGISTER", Description = "Point of Sale Checkout Terminal", DisplayOrder = 1 },
                new() { Name = "Invoices & Receipts", Code = "INVOICES", Description = "Sales Invoices and Returns Management", DisplayOrder = 2 },
                new() { Name = "Products & Catalog", Code = "PRODUCTS", Description = "Clothing Brand Catalog & Variant Matrix", DisplayOrder = 3 },
                new() { Name = "Inventory & Stocks", Code = "INVENTORY", Description = "Branch Stock Levels and Movements", DisplayOrder = 4 },
                new() { Name = "Cashier Shifts", Code = "SHIFTS", Description = "Shift Open/Close and Drawer Reconciliation", DisplayOrder = 5 },
                new() { Name = "Customers & Loyalty", Code = "CUSTOMERS", Description = "Customer CRM and Points Management", DisplayOrder = 6 },
                new() { Name = "Discounts & Coupons", Code = "COUPONS", Description = "Promotional Codes and Discounts", DisplayOrder = 7 },
                new() { Name = "Reports & Analytics", Code = "REPORTS", Description = "Dashboard, Sales and Inventory Reports", DisplayOrder = 8 },
                new() { Name = "Branches & Terminals", Code = "BRANCHES", Description = "Multi-Branch and Terminal Management", DisplayOrder = 9 },
                new() { Name = "User Management", Code = "USERS", Description = "Staff Accounts and Role Permissions", DisplayOrder = 10 },
                new() { Name = "Vendors & Accounts", Code = "VENDORS", Description = "Vendor Master, Bills, Payments & Statements", DisplayOrder = 11 },
                new() { Name = "Expenses & Petty Cash", Code = "EXPENSES", Description = "Store Operational Expenses & Drawer Petty Cash", DisplayOrder = 12 },
                new() { Name = "Financial & General Ledger", Code = "FINANCIAL", Description = "General Ledger, Cost Centers, Trial Balance & ETA Tax Compliance", DisplayOrder = 13 }
            };

            await context.Screens.AddRangeAsync(screens);
            await context.SaveChangesAsync();
        }

        // 2. Seed Roles & Permissions
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role { Name = "Admin", Description = "Full system access and master data administration" };
            var managerRole = new Role { Name = "StoreManager", Description = "Store management, inventory, reports, and shift overrides" };
            var cashierRole = new Role { Name = "Cashier", Description = "POS terminal operator, sales issuance, and customer lookups" };
            var inventoryRole = new Role { Name = "InventoryManager", Description = "Stock adjustments, warehouse receiving, and catalog updates" };

            await context.Roles.AddRangeAsync(adminRole, managerRole, cashierRole, inventoryRole);
            await context.SaveChangesAsync();

            var screens = await context.Screens.ToListAsync();

            // Admin gets all permissions
            foreach (var s in screens)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    ScreenId = s.Id,
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = true
                });
            }

            // Manager gets most permissions
            foreach (var s in screens)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    ScreenId = s.Id,
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = s.Code != "USERS"
                });
            }

            // Cashier gets POS, Invoices, Customers, Shifts
            foreach (var s in screens)
            {
                bool isCashierScreen = s.Code is "POS_REGISTER" or "INVOICES" or "CUSTOMERS" or "SHIFTS" or "PRODUCTS";
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = cashierRole.Id,
                    ScreenId = s.Id,
                    CanView = isCashierScreen,
                    CanAdd = isCashierScreen,
                    CanEdit = s.Code is "POS_REGISTER" or "CUSTOMERS",
                    CanDelete = false
                });
            }

            await context.SaveChangesAsync();
        }

        // 3. Seed Branches & Inventories
        if (!await context.Branches.AnyAsync())
        {
            var branchFlagship = new Branch
            {
                Code = "BR-NC-01",
                NameEn = "New Cairo Flagship - 5A Mall",
                NameAr = "فرع التجمع الخامس الرئيسي - مول 5A",
                Address = "5A by The Waterway, Ring Road, New Cairo",
                City = "Cairo",
                Phone = "+20 2 2700 1100",
                IsActive = true
            };

            var branchZamalek = new Branch
            {
                Code = "BR-ZAM-02",
                NameEn = "Zamalek Boutique - Brazil St",
                NameAr = "بوتيك الزمالك - شارع البرازيل",
                Address = "22 Brazil Street, Zamalek, Cairo",
                City = "Cairo",
                Phone = "+20 2 2736 2200",
                IsActive = true
            };

            var branchAlex = new Branch
            {
                Code = "BR-ALX-03",
                NameEn = "Alexandria - Design District",
                NameAr = "فرع الإسكندرية - حي كفر عبده",
                Address = "18 Kafr Abdou Street, Roushdy, Alexandria",
                City = "Alexandria",
                Phone = "+20 3 5400 3300",
                IsActive = true
            };

            await context.Branches.AddRangeAsync(branchFlagship, branchZamalek, branchAlex);
            await context.SaveChangesAsync();

            // Inventories
            var invCentral = new Inventory
            {
                Code = "WH-CENTRAL",
                NameEn = "Central Logistics Hub (Obour)",
                NameAr = "المستودع الرئيسي - العبور",
                DescriptionEn = "Main distribution warehouse and master stock reserve",
                DescriptionAr = "المستودع المركزي لفرز وتوزيع منتجات البراند",
                Address = "Block 12, Industrial Area 1, Obour City",
                BranchId = branchFlagship.Id,
                IsActive = true,
                IsDefault = false
            };

            var invFlagship = new Inventory
            {
                Code = "INV-NC-5A",
                NameEn = "New Cairo Store Inventory",
                NameAr = "مخزن فرع التجمع الخامس",
                DescriptionEn = "Retail floor and back-store stock",
                DescriptionAr = "مخزن صالة العرض والمخزن الداخلي لفرع 5A",
                Address = "5A Mall, Unit G-14, New Cairo",
                BranchId = branchFlagship.Id,
                IsActive = true,
                IsDefault = true
            };

            var invZamalek = new Inventory
            {
                Code = "INV-ZAM",
                NameEn = "Zamalek Store Inventory",
                NameAr = "مخزن بوتيك الزمالك",
                DescriptionEn = "Boutique floor inventory",
                DescriptionAr = "مخزن بوتيك الزمالك",
                Address = "22 Brazil Street, Zamalek, Cairo",
                BranchId = branchZamalek.Id,
                IsActive = true,
                IsDefault = true
            };

            var invAlex = new Inventory
            {
                Code = "INV-ALX",
                NameEn = "Alexandria Store Inventory",
                NameAr = "مخزن فرع الإسكندرية",
                DescriptionEn = "Alexandria retail inventory",
                DescriptionAr = "مخزن فرع كفر عبده بالإسكندرية",
                Address = "18 Kafr Abdou Street, Alexandria",
                BranchId = branchAlex.Id,
                IsActive = true,
                IsDefault = true
            };

            await context.Inventories.AddRangeAsync(invCentral, invFlagship, invZamalek, invAlex);
            await context.SaveChangesAsync();

            // POS Registers
            var reg1 = new PosRegister { BranchId = branchFlagship.Id, InventoryId = invFlagship.Id, RegisterCode = "REG-NC-01", Name = "Main Counter Pos 1", IsActive = true };
            var reg2 = new PosRegister { BranchId = branchFlagship.Id, InventoryId = invFlagship.Id, RegisterCode = "REG-NC-02", Name = "Express Fitting Checkout", IsActive = true };
            var reg3 = new PosRegister { BranchId = branchZamalek.Id, InventoryId = invZamalek.Id, RegisterCode = "REG-ZAM-01", Name = "Zamalek Main Cashier", IsActive = true };
            var reg4 = new PosRegister { BranchId = branchAlex.Id, InventoryId = invAlex.Id, RegisterCode = "REG-ALX-01", Name = "Alex Counter 1", IsActive = true };

            await context.PosRegisters.AddRangeAsync(reg1, reg2, reg3, reg4);
            await context.SaveChangesAsync();
        }

        // 4. Seed Staff Users
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var managerRole = await context.Roles.FirstAsync(r => r.Name == "StoreManager");
            var cashierRole = await context.Roles.FirstAsync(r => r.Name == "Cashier");
            var branch = await context.Branches.FirstAsync();

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@posprivate.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "Brand Admin",
                PhoneNumber = "+20 100 000 0001",
                RoleId = adminRole.Id,
                BranchId = branch.Id,
                PreferredLanguage = "en",
                IsActive = true
            };

            var managerUser = new User
            {
                Username = "manager",
                Email = "manager@posprivate.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                FullName = "Tarek El-Sayed (Store Manager)",
                PhoneNumber = "+20 100 000 0002",
                RoleId = managerRole.Id,
                BranchId = branch.Id,
                PreferredLanguage = "ar",
                IsActive = true
            };

            var cashierUser = new User
            {
                Username = "cashier",
                Email = "cashier@posprivate.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123"),
                FullName = "Salma Nabil (Lead Cashier)",
                PhoneNumber = "+20 100 000 0003",
                RoleId = cashierRole.Id,
                BranchId = branch.Id,
                PreferredLanguage = "ar",
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, managerUser, cashierUser);
            await context.SaveChangesAsync();
        }

        // 5. Seed Apparel Master Colors
        var apparelColors = new List<Color>
        {
            new() { NameEn = "Pitch Black", NameAr = "أسود كربوني", HexCode = "#121212", DisplayOrder = 1, IsActive = true },
            new() { NameEn = "Chalk White", NameAr = "أبيض طباشيري", HexCode = "#F4F4F5", DisplayOrder = 2, IsActive = true },
            new() { NameEn = "Washed Charcoal", NameAr = "رمادي شاركول مغسول", HexCode = "#334155", DisplayOrder = 3, IsActive = true },
            new() { NameEn = "Heather Grey", NameAr = "رمادي ميلانج", HexCode = "#94A3B8", DisplayOrder = 4, IsActive = true },
            new() { NameEn = "Olive Drab", NameAr = "زيتي عسكري", HexCode = "#4D5940", DisplayOrder = 5, IsActive = true },
            new() { NameEn = "Washed Indigo Blue", NameAr = "أزرق جينز مغسول", HexCode = "#3B82F6", DisplayOrder = 6, IsActive = true },
            new() { NameEn = "Raw Denim Navy", NameAr = "كحلي إنديجو خام", HexCode = "#1E293B", DisplayOrder = 7, IsActive = true },
            new() { NameEn = "Desert Sand", NameAr = "بيج صحراوي", HexCode = "#D4C5B9", DisplayOrder = 8, IsActive = true },
            new() { NameEn = "Earth Mocca", NameAr = "بني موكا", HexCode = "#5C4033", DisplayOrder = 9, IsActive = true },
            new() { NameEn = "Terracotta", NameAr = "طوبي تيراكوتا", HexCode = "#B45309", DisplayOrder = 10, IsActive = true }
        };

        foreach (var c in apparelColors)
        {
            var existingColor = await context.Colors.FirstOrDefaultAsync(x => x.NameEn == c.NameEn);
            if (existingColor == null)
            {
                await context.Colors.AddAsync(c);
            }
        }
        await context.SaveChangesAsync();

        // 6. Seed Apparel Master Sizes (Tops + Pants/Denim)
        var apparelSizes = new List<Size>
        {
            // Standard Tops Sizes
            new() { NameEn = "Extra Small", NameAr = "XS صغير جداً", Code = "XS", DisplayOrder = 1, IsActive = true },
            new() { NameEn = "Small", NameAr = "S صغير", Code = "S", DisplayOrder = 2, IsActive = true },
            new() { NameEn = "Medium", NameAr = "M وسط", Code = "M", DisplayOrder = 3, IsActive = true },
            new() { NameEn = "Large", NameAr = "L كبير", Code = "L", DisplayOrder = 4, IsActive = true },
            new() { NameEn = "Extra Large", NameAr = "XL كبير جداً", Code = "XL", DisplayOrder = 5, IsActive = true },
            new() { NameEn = "Double Extra Large", NameAr = "XXL كبير جداً 2", Code = "XXL", DisplayOrder = 6, IsActive = true },
            new() { NameEn = "Oversized Fit", NameAr = "مقاس أوفر سايز موحد", Code = "OS", DisplayOrder = 7, IsActive = true },
            // Pants & Jeans Waist Sizes
            new() { NameEn = "Waist 30", NameAr = "مقاس بنطلون 30", Code = "30", DisplayOrder = 8, IsActive = true },
            new() { NameEn = "Waist 32", NameAr = "مقاس بنطلون 32", Code = "32", DisplayOrder = 9, IsActive = true },
            new() { NameEn = "Waist 34", NameAr = "مقاس بنطلون 34", Code = "34", DisplayOrder = 10, IsActive = true },
            new() { NameEn = "Waist 36", NameAr = "مقاس بنطلون 36", Code = "36", DisplayOrder = 11, IsActive = true },
            new() { NameEn = "Waist 38", NameAr = "مقاس بنطلون 38", Code = "38", DisplayOrder = 12, IsActive = true }
        };

        foreach (var s in apparelSizes)
        {
            var existingSize = await context.Sizes.FirstOrDefaultAsync(x => x.Code == s.Code);
            if (existingSize == null)
            {
                await context.Sizes.AddAsync(s);
            }
        }
        await context.SaveChangesAsync();

        // 7. Seed Clothing Categories & Products
        if (!await context.Categories.AnyAsync())
        {
            var catTees = new Category
            {
                NameEn = "T-Shirts & Polos",
                NameAr = "تيشيرتات وبولو",
                Slug = "tshirts-polos",
                DescriptionEn = "Heavyweight cotton boxy tees, vintage graphic tees, and luxury knit polos",
                DescriptionAr = "تيشيرتات قطن مصري فاخر ثقيل بقصات بوكسي، بولو حياكة راقية وتيشيرتات جرافيك عصرية",
                PictureUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 1,
                IsActive = true
            };

            var catJeans = new Category
            {
                NameEn = "Jeans & Denimwear",
                NameAr = "جينز ودنيم",
                Slug = "jeans-denim",
                DescriptionEn = "Raw selvedge denim, relaxed carpenter jeans, and vintage wash straight cuts",
                DescriptionAr = "بناطيل جينز دنيم ياباني خام، قصات واسعة كاربنتر وجينز ستريت مغسول يدوي",
                PictureUrl = "https://images.unsplash.com/photo-1542272604-780c96856592?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 2,
                IsActive = true
            };

            var catTrousers = new Category
            {
                NameEn = "Trousers & Chinos",
                NameAr = "بناطيل قماش وتشينو",
                Slug = "trousers-chinos",
                DescriptionEn = "Tailored pleated trousers, relaxed utility cargo pants, and cotton stretch chinos",
                DescriptionAr = "بناطيل قماش بليسيه راقية، بناطيل كارجو بجيوب جانبية عصرية، وتشينو قطن عملي",
                PictureUrl = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 3,
                IsActive = true
            };

            var catShirts = new Category
            {
                NameEn = "Shirts & Overshirts",
                NameAr = "قمصان وأوفر شيرت",
                Slug = "shirts-overshirts",
                DescriptionEn = "Relaxed linen resort shirts, heavy check flannel overshirts, and Oxford button-downs",
                DescriptionAr = "قمصان كتان كاجوال واسعة، أوفر شيرت شتوي كاروهات صوف، وقمصان أوكسفورد قطن كلاسيك",
                PictureUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 4,
                IsActive = true
            };

            var catHoodies = new Category
            {
                NameEn = "Hoodies & Sweatshirts",
                NameAr = "هوديز وسويت شيرت",
                Slug = "hoodies-sweatshirts",
                DescriptionEn = "Heavy 450 GSM French Terry hoodies, zip-ups, and minimal relaxed crewneck pullovers",
                DescriptionAr = "هوديز قطن فرينش تيري ثقيل 450 جرام، سويت شيرت كرو نيك واسع وسوستة هاف زيب",
                PictureUrl = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 5,
                IsActive = true
            };

            var catJackets = new Category
            {
                NameEn = "Jackets & Outerwear",
                NameAr = "جاكيتات وملابس خارجية",
                Slug = "jackets-outerwear",
                DescriptionEn = "Canvas utility chore coats, minimalist bomber jackets, and tech windbreakers",
                DescriptionAr = "جاكيتات كانفاس عملية، بومبر جاكيت عصري مبطن، وجاكيتات وتربروف عصرية",
                PictureUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=600&q=80",
                DisplayOrder = 6,
                IsActive = true
            };

            await context.Categories.AddRangeAsync(catTees, catJeans, catTrousers, catShirts, catHoodies, catJackets);
            await context.SaveChangesAsync();

            // Load colors
            var colorBlack = await context.Colors.FirstAsync(c => c.NameEn == "Pitch Black");
            var colorWhite = await context.Colors.FirstAsync(c => c.NameEn == "Chalk White");
            var colorCharcoal = await context.Colors.FirstAsync(c => c.NameEn == "Washed Charcoal");
            var colorGrey = await context.Colors.FirstAsync(c => c.NameEn == "Heather Grey");
            var colorOlive = await context.Colors.FirstAsync(c => c.NameEn == "Olive Drab");
            var colorBlueDenim = await context.Colors.FirstAsync(c => c.NameEn == "Washed Indigo Blue");
            var colorRawIndigo = await context.Colors.FirstAsync(c => c.NameEn == "Raw Denim Navy");
            var colorSand = await context.Colors.FirstAsync(c => c.NameEn == "Desert Sand");

            // Load sizes
            var sizeS = await context.Sizes.FirstAsync(s => s.Code == "S");
            var sizeM = await context.Sizes.FirstAsync(s => s.Code == "M");
            var sizeL = await context.Sizes.FirstAsync(s => s.Code == "L");
            var sizeXL = await context.Sizes.FirstAsync(s => s.Code == "XL");
            var sizeOS = await context.Sizes.FirstAsync(s => s.Code == "OS");

            var size30 = await context.Sizes.FirstAsync(s => s.Code == "30");
            var size32 = await context.Sizes.FirstAsync(s => s.Code == "32");
            var size34 = await context.Sizes.FirstAsync(s => s.Code == "34");
            var size36 = await context.Sizes.FirstAsync(s => s.Code == "36");

            var products = new List<Product>
            {
                // 1. Heavyweight Boxy T-Shirt
                new()
                {
                    CategoryId = catTees.Id,
                    NameEn = "Heavyweight Boxy Tee 280 GSM",
                    NameAr = "تيشيرت بوكسي قطن ثقيل 280 جرام",
                    Slug = "heavyweight-boxy-tee-280gsm",
                    DescriptionEn = "Custom 280 GSM 100% Egyptian combed cotton with a relaxed drop-shoulder boxy cut and thick ribbed collar.",
                    DescriptionAr = "تيشيرت مصنوع من قطن مصري ممشط فاخر 280 جرام بياقة مضلعة عريضة وقصة بوكسي دروب شولدر مريحة للغاية.",
                    SKU = "TEE-BXY-001",
                    Barcode = "6224001001011",
                    BasePrice = 690.00m,
                    CostPrice = 320.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=600&q=80",
                    Tags = "tshirt, boxy, heavyweight, cotton, streetwear, bestseller",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorBlack.Id, SizeId = sizeS.Id, SKU = "TEE-BXY-001-BLK-S", Barcode = "6224001001028", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = sizeM.Id, SKU = "TEE-BXY-001-BLK-M", Barcode = "6224001001035", StockQuantity = 40, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = sizeL.Id, SKU = "TEE-BXY-001-BLK-L", Barcode = "6224001001042", StockQuantity = 35, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = sizeXL.Id, SKU = "TEE-BXY-001-BLK-XL", Barcode = "6224001001059", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorWhite.Id, SizeId = sizeM.Id, SKU = "TEE-BXY-001-WHT-M", Barcode = "6224001001066", StockQuantity = 30, IsActive = true },
                        new() { ColorId = colorWhite.Id, SizeId = sizeL.Id, SKU = "TEE-BXY-001-WHT-L", Barcode = "6224001001073", StockQuantity = 30, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeL.Id, SKU = "TEE-BXY-001-CHR-L", Barcode = "6224001001080", StockQuantity = 25, IsActive = true }
                    }
                },

                // 2. Vintage Acid Wash Tee
                new()
                {
                    CategoryId = catTees.Id,
                    NameEn = "Acid Wash Graphic Tee",
                    NameAr = "تيشيرت أسيد ووش جرافيك عتيق",
                    Slug = "acid-wash-graphic-tee",
                    DescriptionEn = "Vintage garment-dyed and stone-washed oversized t-shirt with high-density distressed typography.",
                    DescriptionAr = "تيشيرت قطن مغسول بتقنية ستون ووش مع طبعة جرافيك حصرية ذات ملمس عتيق مميز.",
                    SKU = "TEE-ACD-002",
                    Barcode = "6224001002018",
                    BasePrice = 750.00m,
                    CostPrice = 360.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?auto=format&fit=crop&w=600&q=80",
                    Tags = "acid-wash, vintage, oversized, graphic",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeM.Id, SKU = "TEE-ACD-002-CHR-M", Barcode = "6224001002025", StockQuantity = 30, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeL.Id, SKU = "TEE-ACD-002-CHR-L", Barcode = "6224001002032", StockQuantity = 35, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeXL.Id, SKU = "TEE-ACD-002-CHR-XL", Barcode = "6224001002049", StockQuantity = 20, IsActive = true }
                    }
                },

                // 3. Relaxed Carpenter Jeans
                new()
                {
                    CategoryId = catJeans.Id,
                    NameEn = "Relaxed Carpenter Denim Jeans",
                    NameAr = "بنطلون جينز كاربنتر واسع مع جيوب عمالية",
                    Slug = "relaxed-carpenter-denim-jeans",
                    DescriptionEn = "13.5 oz durable cotton denim with utility tool pockets, hammer loop, and relaxed straight drape.",
                    DescriptionAr = "جينز قطن 13.5 أونصة متين بتفاصيل الكاربنتر العملية وجيوب أدوات وقصة واسعة عصرية مستقيمة.",
                    SKU = "JNS-CRP-003",
                    Barcode = "6224002003014",
                    BasePrice = 1350.00m,
                    CostPrice = 650.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1542272604-780c96856592?auto=format&fit=crop&w=600&q=80",
                    Tags = "jeans, denim, carpenter, streetwear, durable",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorBlueDenim.Id, SizeId = size30.Id, SKU = "JNS-CRP-003-BLU-30", Barcode = "6224002003021", StockQuantity = 15, IsActive = true },
                        new() { ColorId = colorBlueDenim.Id, SizeId = size32.Id, SKU = "JNS-CRP-003-BLU-32", Barcode = "6224002003038", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorBlueDenim.Id, SizeId = size34.Id, SKU = "JNS-CRP-003-BLU-34", Barcode = "6224002003045", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorBlueDenim.Id, SizeId = size36.Id, SKU = "JNS-CRP-003-BLU-36", Barcode = "6224002003052", StockQuantity = 15, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = size32.Id, SKU = "JNS-CRP-003-CHR-32", Barcode = "6224002003069", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = size34.Id, SKU = "JNS-CRP-003-CHR-34", Barcode = "6224002003076", StockQuantity = 20, IsActive = true }
                    }
                },

                // 4. Vintage Straight Selvedge Jeans
                new()
                {
                    CategoryId = catJeans.Id,
                    NameEn = "Vintage Straight Leg Raw Indigo Jeans",
                    NameAr = "بنطلون جينز ستريت كلاسيك إنديجو خام",
                    Slug = "vintage-straight-raw-indigo-jeans",
                    DescriptionEn = "Classic 5-pocket straight leg jeans in raw deep indigo denim with clean copper rivet accents.",
                    DescriptionAr = "جينز كلاسيكي بخمسة جيوب من الدنيم الأزرق الغامق الخام مع أزرار ومسامير نحاسية متينة.",
                    SKU = "JNS-RAW-004",
                    Barcode = "6224002004011",
                    BasePrice = 1450.00m,
                    CostPrice = 700.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?auto=format&fit=crop&w=600&q=80",
                    Tags = "jeans, straight, selvedge, raw, indigo",
                    TrackStock = true,
                    IsActive = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorRawIndigo.Id, SizeId = size30.Id, SKU = "JNS-RAW-004-IND-30", Barcode = "6224002004028", StockQuantity = 12, IsActive = true },
                        new() { ColorId = colorRawIndigo.Id, SizeId = size32.Id, SKU = "JNS-RAW-004-IND-32", Barcode = "6224002004035", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorRawIndigo.Id, SizeId = size34.Id, SKU = "JNS-RAW-004-IND-34", Barcode = "6224002004042", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorRawIndigo.Id, SizeId = size36.Id, SKU = "JNS-RAW-004-IND-36", Barcode = "6224002004059", StockQuantity = 10, IsActive = true }
                    }
                },

                // 5. Utility Cargo Trousers
                new()
                {
                    CategoryId = catTrousers.Id,
                    NameEn = "Tactical Utility Cargo Trousers",
                    NameAr = "بنطلون كارجو عسكري بجيوب جانبية متعددة",
                    Slug = "tactical-utility-cargo-trousers",
                    DescriptionEn = "Heavy ripstop cotton trousers equipped with deep gusseted 3D cargo pockets and adjustable ankle toggles.",
                    DescriptionAr = "بنطلون قماش ريب ستوب مقاوم للتمزق مع جيوب كارجو مجسمة وأربطة سفلية للتحكم في وسع الكاحل.",
                    SKU = "TRS-CRG-005",
                    Barcode = "6224003005018",
                    BasePrice = 1250.00m,
                    CostPrice = 580.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?auto=format&fit=crop&w=600&q=80",
                    Tags = "cargo, trousers, ripstop, tactical, utility",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorOlive.Id, SizeId = size32.Id, SKU = "TRS-CRG-005-OLV-32", Barcode = "6224003005025", StockQuantity = 22, IsActive = true },
                        new() { ColorId = colorOlive.Id, SizeId = size34.Id, SKU = "TRS-CRG-005-OLV-34", Barcode = "6224003005032", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = size32.Id, SKU = "TRS-CRG-005-BLK-32", Barcode = "6224003005049", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = size34.Id, SKU = "TRS-CRG-005-BLK-34", Barcode = "6224003005056", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorSand.Id, SizeId = size32.Id, SKU = "TRS-CRG-005-SND-32", Barcode = "6224003005063", StockQuantity = 15, IsActive = true }
                    }
                },

                // 6. Pleated Smart Trousers
                new()
                {
                    CategoryId = catTrousers.Id,
                    NameEn = "Pleated Relaxed Smart Trousers",
                    NameAr = "بنطلون قماش بليسيه سمارت كاجوال واسع",
                    Slug = "pleated-relaxed-smart-trousers",
                    DescriptionEn = "Contemporary double-pleated front trousers with an elegant drape, elastic waistband back, and clean tailored cuffs.",
                    DescriptionAr = "بنطلون قماش فاخر بكسرات بليسيه مزدوجة أمامية، حزام خصر مرن خلفي، وتصميم أنيق يجمع بين الراحة والفخامة.",
                    SKU = "TRS-PLT-006",
                    Barcode = "6224003006015",
                    BasePrice = 1190.00m,
                    CostPrice = 540.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1473966968600-fa801b869a1a?auto=format&fit=crop&w=600&q=80",
                    Tags = "pleated, trousers, smart, elegant, modern",
                    TrackStock = true,
                    IsActive = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorCharcoal.Id, SizeId = size32.Id, SKU = "TRS-PLT-006-CHR-32", Barcode = "6224003006022", StockQuantity = 18, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = size34.Id, SKU = "TRS-PLT-006-CHR-34", Barcode = "6224003006039", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorSand.Id, SizeId = size32.Id, SKU = "TRS-PLT-006-SND-32", Barcode = "6224003006046", StockQuantity = 15, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = size34.Id, SKU = "TRS-PLT-006-BLK-34", Barcode = "6224003006053", StockQuantity = 22, IsActive = true }
                    }
                },

                // 7. Oversized Linen Resort Shirt
                new()
                {
                    CategoryId = catShirts.Id,
                    NameEn = "Oversized Pure Linen Camp Shirt",
                    NameAr = "قميص كتان طبيعي أوفر سايز ياقة كوبية",
                    Slug = "oversized-pure-linen-camp-shirt",
                    DescriptionEn = "100% breathable natural flax linen with an open camp collar, mother-of-pearl buttons, and loose summer silhouette.",
                    DescriptionAr = "قميص كتان خالص 100% ناعم ومسامي بياقة كامب مفتوحة وأزرار صدف طبيعي وتصميم مريح.",
                    SKU = "SHT-LIN-007",
                    Barcode = "6224004007019",
                    BasePrice = 1150.00m,
                    CostPrice = 520.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?auto=format&fit=crop&w=600&q=80",
                    Tags = "shirt, linen, summer, camp-collar, resort",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorWhite.Id, SizeId = sizeM.Id, SKU = "SHT-LIN-007-WHT-M", Barcode = "6224004007026", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorWhite.Id, SizeId = sizeL.Id, SKU = "SHT-LIN-007-WHT-L", Barcode = "6224004007033", StockQuantity = 30, IsActive = true },
                        new() { ColorId = colorSand.Id, SizeId = sizeM.Id, SKU = "SHT-LIN-007-SND-M", Barcode = "6224004007040", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorOlive.Id, SizeId = sizeL.Id, SKU = "SHT-LIN-007-OLV-L", Barcode = "6224004007057", StockQuantity = 20, IsActive = true }
                    }
                },

                // 8. Heavy Flannel Overshirt
                new()
                {
                    CategoryId = catShirts.Id,
                    NameEn = "Heavyweight Plaid Flannel Overshirt",
                    NameAr = "أوفر شيرت صوف ثقيل كاروهات شتوي",
                    Slug = "heavyweight-plaid-flannel-overshirt",
                    DescriptionEn = "350 GSM brushed cotton flannel jacket-shirt with dual flap chest pockets and heavy horn buttons.",
                    DescriptionAr = "أوفر شيرت قطن فلانيل مبرد سميك 350 جرام مع جيوب صدر مزدوجة مناسب للارتداء كجاكيت خفيف.",
                    SKU = "SHT-FLN-008",
                    Barcode = "6224004008016",
                    BasePrice = 1450.00m,
                    CostPrice = 680.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1578932750294-f5075e85f44a?auto=format&fit=crop&w=600&q=80",
                    Tags = "flannel, overshirt, plaid, winter, warm",
                    TrackStock = true,
                    IsActive = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeM.Id, SKU = "SHT-FLN-008-CHR-M", Barcode = "6224004008023", StockQuantity = 15, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeL.Id, SKU = "SHT-FLN-008-CHR-L", Barcode = "6224004008030", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorCharcoal.Id, SizeId = sizeXL.Id, SKU = "TEE-FLN-008-CHR-XL", Barcode = "6224004008047", StockQuantity = 15, IsActive = true }
                    }
                },

                // 9. Heavy French Terry Hoodie
                new()
                {
                    CategoryId = catHoodies.Id,
                    NameEn = "Heavy French Terry Hoodie 450 GSM",
                    NameAr = "هودي قطن فرينش تيري فائق الثقل 450 جرام",
                    Slug = "heavy-french-terry-hoodie-450gsm",
                    DescriptionEn = "Ultra-dense 450 GSM French Terry hoodie with double-layered hood, hidden side seam pockets, and no drawstrings for a clean minimal aesthetic.",
                    DescriptionAr = "هودي فاخر قطن خالص 450 جرام بغطاء رأس مزدوج البطانة، جيوب جانبية مخفية، وتصميم مينيمال أنيق بدون حبال.",
                    SKU = "HOD-FTR-009",
                    Barcode = "6224005009013",
                    BasePrice = 1650.00m,
                    CostPrice = 780.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?auto=format&fit=crop&w=600&q=80",
                    Tags = "hoodie, heavyweight, french-terry, streetwear, cozy, bestseller",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorBlack.Id, SizeId = sizeM.Id, SKU = "HOD-FTR-009-BLK-M", Barcode = "6224005009020", StockQuantity = 25, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = sizeL.Id, SKU = "HOD-FTR-009-BLK-L", Barcode = "6224005009037", StockQuantity = 30, IsActive = true },
                        new() { ColorId = colorBlack.Id, SizeId = sizeXL.Id, SKU = "HOD-FTR-009-BLK-XL", Barcode = "6224005009044", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorGrey.Id, SizeId = sizeM.Id, SKU = "HOD-FTR-009-GRY-M", Barcode = "6224005009051", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorGrey.Id, SizeId = sizeL.Id, SKU = "HOD-FTR-009-GRY-L", Barcode = "6224005009068", StockQuantity = 25, IsActive = true }
                    }
                },

                // 10. Canvas Chore Worker Jacket
                new()
                {
                    CategoryId = catJackets.Id,
                    NameEn = "Heritage Canvas Work Chore Jacket",
                    NameAr = "جاكيت كانفاس عملي كلاسيك بياقة كوردروي",
                    Slug = "heritage-canvas-work-chore-jacket",
                    DescriptionEn = "Heavy 12 oz cotton duck canvas with vintage corduroy collar, brushed plaid lining, and reinforced triple-needle stitching.",
                    DescriptionAr = "جاكيت كانفاس قطني متين بياقة مضلعة كوردروي ناعمة وبطانة داخلية دافئة وخياطة ثلاثية معززة.",
                    SKU = "JKT-CAN-010",
                    Barcode = "6224006010019",
                    BasePrice = 2250.00m,
                    CostPrice = 1100.00m,
                    PictureUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=600&q=80",
                    Tags = "jacket, chore, canvas, heritage, corduroy, outerwear",
                    TrackStock = true,
                    IsActive = true,
                    IsFeatured = true,
                    Variants = new List<ProductVariant>
                    {
                        new() { ColorId = colorSand.Id, SizeId = sizeM.Id, SKU = "JKT-CAN-010-SND-M", Barcode = "6224006010026", StockQuantity = 15, IsActive = true },
                        new() { ColorId = colorSand.Id, SizeId = sizeL.Id, SKU = "JKT-CAN-010-SND-L", Barcode = "6224006010033", StockQuantity = 20, IsActive = true },
                        new() { ColorId = colorOlive.Id, SizeId = sizeL.Id, SKU = "JKT-CAN-010-OLV-L", Barcode = "6224006010040", StockQuantity = 15, IsActive = true }
                    }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Seed Initial Stock across Branches
            var branches = await context.Branches.ToListAsync();
            var adminUser = await context.Users.FirstAsync();

            foreach (var b in branches)
            {
                foreach (var p in products)
                {
                    if (p.Variants.Any())
                    {
                        foreach (var v in p.Variants)
                        {
                            var stock = new BranchStock
                            {
                                BranchId = b.Id,
                                ProductId = p.Id,
                                VariantId = v.Id,
                                QuantityOnHand = 35,
                                MinAlertLevel = 5
                            };
                            context.BranchStocks.Add(stock);

                            context.StockMovements.Add(new StockMovement
                            {
                                BranchId = b.Id,
                                ProductId = p.Id,
                                VariantId = v.Id,
                                MovementType = StockMovementType.InitialSeed,
                                QuantityChange = 35,
                                BalanceAfter = 35,
                                ReferenceNumber = "SEED-POS-PRIVATE",
                                Notes = "Initial Branch Stock Allocation",
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // 8. Seed Customers & Loyalty
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new()
                {
                    Code = "CUST-0001",
                    FullName = "Omar Yassin",
                    PhoneNumber = "01001234567",
                    Email = "omar.yassin@gmail.com",
                    Address = "New Cairo, 5th Settlement",
                    City = "Cairo",
                    LoyaltyPoints = 250,
                    TotalSpent = 8900.00m,
                    Notes = "Regular customer - prefers oversized tees and cargo pants",
                    IsActive = true
                },
                new()
                {
                    Code = "CUST-0002",
                    FullName = "Layla El-Sayed",
                    PhoneNumber = "01119876543",
                    Email = "layla.elsayed@outlook.com",
                    Address = "Zamalek, Cairo",
                    City = "Cairo",
                    LoyaltyPoints = 180,
                    TotalSpent = 6400.00m,
                    Notes = "Loves heavyweight hoodies and linen shirts",
                    IsActive = true
                },
                new()
                {
                    Code = "CUST-0003",
                    FullName = "Ziad Mansour",
                    PhoneNumber = "01223344556",
                    Email = "ziad.mansour@yahoo.com",
                    Address = "Kafr Abdou, Alexandria",
                    City = "Alexandria",
                    LoyaltyPoints = 95,
                    TotalSpent = 3850.00m,
                    Notes = "Denim collector",
                    IsActive = true
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }

        // 9. Seed Promotional Coupons
        if (!await context.Coupons.AnyAsync())
        {
            var coupons = new List<Coupon>
            {
                new()
                {
                    Code = "PRIVATE10",
                    Description = "10% First Order Welcome Discount",
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 10m,
                    MinOrderAmount = 1000m,
                    MaxDiscountAmount = 500m,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    UsageLimit = 1000,
                    UsageCount = 12,
                    IsActive = true
                },
                new()
                {
                    Code = "STREETWEAR20",
                    Description = "20% Season Launch Promo",
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 20m,
                    MinOrderAmount = 2500m,
                    MaxDiscountAmount = 1200m,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    UsageLimit = 500,
                    UsageCount = 34,
                    IsActive = true
                },
                new()
                {
                    Code = "SAVE400",
                    Description = "400 EGP Instant Order Discount",
                    DiscountType = DiscountType.FixedAmount,
                    DiscountValue = 400m,
                    MinOrderAmount = 2000m,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    UsageLimit = 200,
                    UsageCount = 8,
                    IsActive = true
                }
            };

            await context.Coupons.AddRangeAsync(coupons);
            await context.SaveChangesAsync();
        }

        // 10. Seed Expense Categories
        if (!await context.ExpenseCategories.AnyAsync())
        {
            var expenseCats = new List<ExpenseCategory>
            {
                new() { Code = "EXP-RENT", NameEn = "Store Rent & Facilities", NameAr = "إيجار المحلات والمستودعات", Description = "Monthly store lease and mall service charges", DisplayOrder = 1, IsActive = true },
                new() { Code = "EXP-UTIL", NameEn = "Electricity, Water & Utilities", NameAr = "الكهرباء والمياه والمرافق", Description = "Power bills, air-conditioning, internet & phone", DisplayOrder = 2, IsActive = true },
                new() { Code = "EXP-PKG", NameEn = "Packaging, Bags & Labels", NameAr = "أكياس التسوق ومستلزمات التغليف والشنط", Description = "Luxury brand shopping bags, boxes, garment tags", DisplayOrder = 3, IsActive = true },
                new() { Code = "EXP-SHP", NameEn = "Logistics & Courier Delivery", NameAr = "الشحن والنقل الداخلي والمناديب", Description = "Inter-branch transport and customer couriers", DisplayOrder = 4, IsActive = true },
                new() { Code = "EXP-MNT", NameEn = "Store Maintenance & Supplies", NameAr = "صيانة المتجر ومستلزمات النظافة", Description = "Hangers, cash register rolls, cleaning supplies", DisplayOrder = 5, IsActive = true },
                new() { Code = "EXP-MKT", NameEn = "Marketing & Influencer Ads", NameAr = "التسويق والدعاية والإعلانات", Description = "Social media campaigns, photo shoots, sponsorships", DisplayOrder = 6, IsActive = true },
                new() { Code = "EXP-PETTY", NameEn = "Staff Hospitality & Petty Cash", NameAr = "بوفيه وضيافة ونثريات يومية", Description = "Staff tea, coffee, emergency register expenses", DisplayOrder = 7, IsActive = true }
            };

            await context.ExpenseCategories.AddRangeAsync(expenseCats);
            await context.SaveChangesAsync();

            // Sample Expenses
            var branch = await context.Branches.FirstOrDefaultAsync();
            var adminUser = await context.Users.FirstOrDefaultAsync();
            var catPkg = expenseCats.First(c => c.Code == "EXP-PKG");
            var catPetty = expenseCats.First(c => c.Code == "EXP-PETTY");
            var catUtil = expenseCats.First(c => c.Code == "EXP-UTIL");

            var sampleExpenses = new List<Expense>
            {
                new()
                {
                    ExpenseNumber = "EXP-202610-00001",
                    ExpenseCategoryId = catPkg.Id,
                    BranchId = branch?.Id,
                    Amount = 3500.00m,
                    PaymentMethod = ExpensePaymentMethod.BankTransfer,
                    ExpenseDate = DateTime.UtcNow.AddDays(-12),
                    PaidTo = "Delta Modern Packaging Factory",
                    Description = "5000 Matte Black Luxury Shopping Bags with Silver Foil Logo",
                    ReferenceNumber = "INV-DLT-8821",
                    IsDrawerExpense = false,
                    CreatedByUserId = adminUser?.Id
                },
                new()
                {
                    ExpenseNumber = "EXP-202610-00002",
                    ExpenseCategoryId = catPetty.Id,
                    BranchId = branch?.Id,
                    Amount = 250.00m,
                    PaymentMethod = ExpensePaymentMethod.Cash,
                    ExpenseDate = DateTime.UtcNow.AddDays(-2),
                    PaidTo = "Metro Supermarket",
                    Description = "Store coffee, mineral water, and cleaning essentials",
                    ReferenceNumber = "RCP-29103",
                    IsDrawerExpense = true, // Cashier drawer petty cash
                    CreatedByUserId = adminUser?.Id
                },
                new()
                {
                    ExpenseNumber = "EXP-202610-00003",
                    ExpenseCategoryId = catUtil.Id,
                    BranchId = branch?.Id,
                    Amount = 1850.00m,
                    PaymentMethod = ExpensePaymentMethod.CreditCard,
                    ExpenseDate = DateTime.UtcNow.AddDays(-5),
                    PaidTo = "Vodafone Business Internet",
                    Description = "Monthly high-speed optical fiber store internet & cloud POS link",
                    ReferenceNumber = "VOD-FBR-4412",
                    IsDrawerExpense = false,
                    CreatedByUserId = adminUser?.Id
                }
            };

            await context.Expenses.AddRangeAsync(sampleExpenses);
            await context.SaveChangesAsync();
        }

        // 11. Seed Apparel & Fabric Vendors, Bills, and Payments
        if (!await context.Vendors.AnyAsync())
        {
            var branch = await context.Branches.FirstOrDefaultAsync();
            var adminUser = await context.Users.FirstOrDefaultAsync();
            var products = await context.Products.Include(p => p.Variants).ToListAsync();

            var vendorCotton = new Vendor
            {
                Code = "VND-0001",
                Name = "Misr Spinning & Weaving Co (Ghazl El Mahalla)",
                ContactPerson = "Eng. Mohamed Radi",
                PhoneNumber = "+20 40 223 4500",
                Email = "sales@misrspinning.com.eg",
                TaxNumber = "100-245-891",
                CommercialRegister = "CR-44190-GH",
                Address = "Industrial Zone, El Mahalla El Kubra",
                City = "Gharbia",
                OpeningBalance = 0m,
                CurrentBalance = 0m,
                CreditLimit = 500000m,
                PaymentTermsDays = 30,
                Notes = "Primary supplier for 100% Egyptian combed cotton (280 GSM) and French Terry fabrics",
                IsActive = true
            };

            var vendorDenim = new Vendor
            {
                Code = "VND-0002",
                Name = "Turkish Denim & Selvedge Mill",
                ContactPerson = "Baris Demir",
                PhoneNumber = "+20 122 889 9110",
                Email = "baris@turkishdenimmill.com",
                TaxNumber = "302-887-124",
                CommercialRegister = "CR-99214-CAI",
                Address = "Free Trade Zone, Nasr City",
                City = "Cairo",
                OpeningBalance = 15000m, // Existing balance carried over
                CurrentBalance = 15000m,
                CreditLimit = 350000m,
                PaymentTermsDays = 45,
                Notes = "Heavy 13.5 oz denim and Japanese raw indigo fabrics",
                IsActive = true
            };

            var vendorPackaging = new Vendor
            {
                Code = "VND-0003",
                Name = "Delta Packaging & Printing Industries",
                ContactPerson = "Mostafa El-Naggar",
                PhoneNumber = "+20 100 445 6789",
                Email = "info@deltapack.com.eg",
                TaxNumber = "209-661-304",
                CommercialRegister = "CR-12048-OBR",
                Address = "Block 19, Industrial Zone B, Obour City",
                City = "Cairo",
                OpeningBalance = 0m,
                CurrentBalance = 0m,
                CreditLimit = 100000m,
                PaymentTermsDays = 15,
                Notes = "Luxury apparel shopping bags, woven collar tags, and garment hangtags",
                IsActive = true
            };

            await context.Vendors.AddRangeAsync(vendorCotton, vendorDenim, vendorPackaging);
            await context.SaveChangesAsync();

            // Record Turkish Denim opening balance in ledger
            context.VendorTransactions.Add(new VendorTransaction
            {
                VendorId = vendorDenim.Id,
                TransactionType = VendorTransactionType.OpeningBalance,
                TransactionDate = DateTime.UtcNow.AddDays(-60),
                ReferenceNumber = $"OB-{vendorDenim.Code}",
                DebitAmount = 0m,
                CreditAmount = 15000m,
                BalanceAfter = 15000m,
                Notes = "Carried-over balance from previous season",
                CreatedByUserId = adminUser?.Id
            });

            // 11.1 Create Sample Purchase Bill for Misr Spinning & Weaving Co
            var firstProduct = products.FirstOrDefault();
            var billDate = DateTime.UtcNow.AddDays(-20);
            var billNumber = "BILL-202609-00001";

            var billLines = new List<VendorBillLine>
            {
                new()
                {
                    ProductId = firstProduct?.Id ?? 1,
                    VariantId = firstProduct?.Variants.FirstOrDefault()?.Id,
                    Quantity = 100,
                    UnitPrice = 240.00m,
                    TaxRate = 14m,
                    TaxAmount = Math.Round(100 * 240.00m * 0.14m, 2),
                    LineTotal = (100 * 240.00m) + Math.Round(100 * 240.00m * 0.14m, 2),
                    ReceivedQuantity = 100,
                    Notes = "Pre-cut cotton fabric & garment blanks"
                }
            };

            var billSubTotal = billLines.Sum(l => l.Quantity * l.UnitPrice);
            var billTax = billLines.Sum(l => l.TaxAmount);
            var billTotal = billSubTotal + billTax;

            var bill = new VendorBill
            {
                BillNumber = billNumber,
                VendorId = vendorCotton.Id,
                BranchId = branch?.Id,
                BillDate = billDate,
                DueDate = billDate.AddDays(30),
                SubTotal = billSubTotal,
                TaxAmount = billTax,
                DiscountAmount = 0m,
                TotalAmount = billTotal,
                PaidAmount = 15000.00m,
                RemainingAmount = billTotal - 15000.00m,
                Status = VendorBillStatus.PartiallyPaid,
                VendorInvoiceNumber = "FAC-MSW-9921",
                Notes = "Season launch production run lot #1",
                CreatedByUserId = adminUser?.Id,
                Lines = billLines
            };

            context.VendorBills.Add(bill);

            // Update vendor balance: credit bill
            vendorCotton.CurrentBalance += billTotal;

            // Transaction 1: Bill
            context.VendorTransactions.Add(new VendorTransaction
            {
                VendorId = vendorCotton.Id,
                TransactionType = VendorTransactionType.PurchaseBill,
                TransactionDate = billDate,
                ReferenceNumber = billNumber,
                DebitAmount = 0m,
                CreditAmount = billTotal,
                BalanceAfter = vendorCotton.CurrentBalance,
                Notes = $"Purchase Bill: {billNumber} (Inv #FAC-MSW-9921)",
                BranchId = branch?.Id,
                CreatedByUserId = adminUser?.Id
            });

            // 11.2 Record Partial Payment to Misr Spinning
            var payDate = DateTime.UtcNow.AddDays(-10);
            var payNumber = "VPAY-202609-00001";
            var payment = new VendorPayment
            {
                PaymentNumber = payNumber,
                VendorId = vendorCotton.Id,
                BranchId = branch?.Id,
                VendorBillId = bill.Id,
                Amount = 15000.00m,
                PaymentMethod = ExpensePaymentMethod.BankTransfer,
                PaymentDate = payDate,
                ReferenceNo = "CIB-TRF-90281",
                Notes = "Advance partial bank transfer payment for lot #1",
                CreatedByUserId = adminUser?.Id
            };

            context.VendorPayments.Add(payment);

            // Update vendor balance: debit payment
            vendorCotton.CurrentBalance -= 15000.00m;

            // Transaction 2: Payment
            context.VendorTransactions.Add(new VendorTransaction
            {
                VendorId = vendorCotton.Id,
                TransactionType = VendorTransactionType.Payment,
                TransactionDate = payDate,
                ReferenceNumber = payNumber,
                DebitAmount = 15000.00m,
                CreditAmount = 0m,
                BalanceAfter = vendorCotton.CurrentBalance,
                Notes = $"Bank Transfer Payment: {payNumber} (Ref: CIB-TRF-90281)",
                BranchId = branch?.Id,
                CreatedByUserId = adminUser?.Id
            });

            await context.SaveChangesAsync();
        }

        // 12. Seed Financial Module (Chart of Accounts, Cost Centers, ETA Company Settings & Journal Entries)
        // Ensure FINANCIAL screen permission exists for admin
        var financialScreen = await context.Screens.FirstOrDefaultAsync(s => s.Code == "FINANCIAL");
        if (financialScreen == null)
        {
            financialScreen = new Screen
            {
                Name = "Financial & General Ledger",
                Code = "FINANCIAL",
                Description = "General Ledger, Cost Centers, Trial Balance & ETA Tax Compliance",
                DisplayOrder = 13
            };
            context.Screens.Add(financialScreen);
            await context.SaveChangesAsync();
        }

        var adminRoleObj = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRoleObj != null && !await context.RolePermissions.AnyAsync(rp => rp.RoleId == adminRoleObj.Id && rp.ScreenId == financialScreen.Id))
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRoleObj.Id,
                ScreenId = financialScreen.Id,
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true
            });
            await context.SaveChangesAsync();
        }

        if (!await context.CostCenters.AnyAsync())
        {
            var branch1 = await context.Branches.FirstOrDefaultAsync(b => b.Code == "BR01");
            var branch2 = await context.Branches.FirstOrDefaultAsync(b => b.Code == "BR02");

            var costCenters = new List<CostCenter>
            {
                new() { Code = "CC-01", NameEn = "Main Branch - Nasr City", NameAr = "فرع مدينة نصر - تجزئة", Type = CostCenterType.ProfitCenter, BranchId = branch1?.Id, Description = "Retail sales and local branch costs" },
                new() { Code = "CC-02", NameEn = "Mall of Egypt Branch", NameAr = "فرع مول مصر - تجزئة", Type = CostCenterType.ProfitCenter, BranchId = branch2?.Id, Description = "Retail sales in western Cairo" },
                new() { Code = "CC-03", NameEn = "E-Commerce & Online Store", NameAr = "المتجر الإلكتروني والشحن", Type = CostCenterType.ProfitCenter, Description = "Online apparel sales, courier delivery, and packaging" },
                new() { Code = "CC-04", NameEn = "Central Warehouse & Cutting", NameAr = "المخزن الرئيسي ومصنع القص", Type = CostCenterType.CostCenter, Description = "Fabric storage, cutting atelier, and quality assurance" },
                new() { Code = "CC-05", NameEn = "Headquarters & Management", NameAr = "الإدارة العامة والمالية", Type = CostCenterType.CostCenter, Description = "Executive management, finance, HR, and IT infrastructure" }
            };

            await context.CostCenters.AddRangeAsync(costCenters);
            await context.SaveChangesAsync();
        }

        if (!await context.Accounts.AnyAsync())
        {
            // Level 1: Main Control Accounts
            var accAssets = new Account { Code = "1", NameEn = "Assets", NameAr = "الأصول", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 1, IsPostable = false };
            var accLiabilities = new Account { Code = "2", NameEn = "Liabilities", NameAr = "الالتزامات", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 1, IsPostable = false };
            var accEquity = new Account { Code = "3", NameEn = "Equity", NameAr = "حقوق الملكية", Type = AccountType.Equity, Nature = AccountNature.Credit, Level = 1, IsPostable = false };
            var accRevenue = new Account { Code = "4", NameEn = "Revenues", NameAr = "الإيرادات", Type = AccountType.Revenue, Nature = AccountNature.Credit, Level = 1, IsPostable = false };
            var accExpenses = new Account { Code = "5", NameEn = "Expenses & Costs", NameAr = "المصروفات والتكاليف", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 1, IsPostable = false };

            await context.Accounts.AddRangeAsync(accAssets, accLiabilities, accEquity, accRevenue, accExpenses);
            await context.SaveChangesAsync();

            // Level 2: Sub-control Groups
            var accCurrentAssets = new Account { Code = "11", NameEn = "Current Assets", NameAr = "الأصول المتداولة", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 2, IsPostable = false, ParentAccountId = accAssets.Id };
            var accFixedAssets = new Account { Code = "12", NameEn = "Fixed Assets", NameAr = "الأصول الثابتة والمعدات", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 2, IsPostable = false, ParentAccountId = accAssets.Id };

            var accCurrentLiabilities = new Account { Code = "21", NameEn = "Current Liabilities", NameAr = "الالتزامات المتداولة", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 2, IsPostable = false, ParentAccountId = accLiabilities.Id };
            
            var accCapitalReserves = new Account { Code = "31", NameEn = "Capital & Retained Earnings", NameAr = "رأس المال والأرباح المرحلة", Type = AccountType.Equity, Nature = AccountNature.Credit, Level = 2, IsPostable = false, ParentAccountId = accEquity.Id };

            var accOperatingRevenue = new Account { Code = "41", NameEn = "Sales & Operating Revenues", NameAr = "إيرادات المبيعات والنشاط", Type = AccountType.Revenue, Nature = AccountNature.Credit, Level = 2, IsPostable = false, ParentAccountId = accRevenue.Id };

            var accCogs = new Account { Code = "51", NameEn = "Cost of Goods Sold (COGS)", NameAr = "تكلفة البضاعة المباعة", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 2, IsPostable = false, ParentAccountId = accExpenses.Id };
            var accSellingExp = new Account { Code = "52", NameEn = "Selling & Marketing Expenses", NameAr = "مصروفات البيع والتسويق", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 2, IsPostable = false, ParentAccountId = accExpenses.Id };
            var accAdminExp = new Account { Code = "53", NameEn = "General & Administrative Expenses", NameAr = "مصروفات عمومية وإدارية", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 2, IsPostable = false, ParentAccountId = accExpenses.Id };

            await context.Accounts.AddRangeAsync(accCurrentAssets, accFixedAssets, accCurrentLiabilities, accCapitalReserves, accOperatingRevenue, accCogs, accSellingExp, accAdminExp);
            await context.SaveChangesAsync();

            // Level 3/4: Postable Detail Accounts
            var detailAccounts = new List<Account>
            {
                // Current Assets
                new() { Code = "1101", NameEn = "Cash on Hand & Petty Cash", NameAr = "النقدية بالصندوق والخزينة", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, CurrentBalance = 75000.00m },
                new() { Code = "1102", NameEn = "Cash in Banks (CIB / Banque Misr)", NameAr = "البنوك والحسابات الجارية", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, CurrentBalance = 245000.00m },
                new() { Code = "1103", NameEn = "Accounts Receivable (Trade Debtors)", NameAr = "العملاء والمدينون", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, CurrentBalance = 18500.00m },
                new() { Code = "1104", NameEn = "Apparel Finished Goods Inventory", NameAr = "مخزون الملابس الجاهزة والأقمشة", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, CurrentBalance = 320000.00m },
                new() { Code = "1105", NameEn = "Prepaid Expenses & Advances", NameAr = "مصروفات مدفوعة مقدماً وتأمينات", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, CurrentBalance = 12000.00m },
                new() { Code = "1106", NameEn = "Input VAT Receivable (14%)", NameAr = "ضريبة القيمة المضافة على المدخلات (14%)", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, ETATaxType = ETATaxType.T1_VAT, CurrentBalance = 16800.00m },
                new() { Code = "1107", NameEn = "Withholding Tax Receivable (1%)", NameAr = "ضريبة الخصم والتحصيل لصالحنا (1%)", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCurrentAssets.Id, ETATaxType = ETATaxType.T4_WithholdingTax, CurrentBalance = 2100.00m },

                // Fixed Assets
                new() { Code = "1201", NameEn = "POS Hardware & Store Fixtures", NameAr = "أجهزة الكاشير وتجهيزات المعارض", Type = AccountType.Asset, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accFixedAssets.Id, CurrentBalance = 85000.00m },

                // Current Liabilities
                new() { Code = "2101", NameEn = "Accounts Payable (Vendors)", NameAr = "الموردون والدائنون التجاريون", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCurrentLiabilities.Id, CurrentBalance = 160000.00m },
                new() { Code = "2102", NameEn = "Output VAT Payable (14%)", NameAr = "ضريبة القيمة المضافة المستحقة (14%)", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCurrentLiabilities.Id, ETATaxType = ETATaxType.T1_VAT, CurrentBalance = 34500.00m },
                new() { Code = "2103", NameEn = "Withholding Tax Payable (1%)", NameAr = "ضريبة الخصم واجبة التوريد (1%)", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCurrentLiabilities.Id, ETATaxType = ETATaxType.T4_WithholdingTax, CurrentBalance = 3200.00m },
                new() { Code = "2104", NameEn = "Accrued Salaries & Expenses", NameAr = "رواتب ومصروفات مستحقة", Type = AccountType.Liability, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCurrentLiabilities.Id, CurrentBalance = 25000.00m },

                // Equity
                new() { Code = "3101", NameEn = "Paid-in Share Capital", NameAr = "رأس المال المدفوع", Type = AccountType.Equity, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCapitalReserves.Id, CurrentBalance = 450000.00m },
                new() { Code = "3102", NameEn = "Retained Earnings", NameAr = "الأرباح / الخسائر المرحلة", Type = AccountType.Equity, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accCapitalReserves.Id, CurrentBalance = 101700.00m },

                // Revenues
                new() { Code = "4101", NameEn = "Retail Clothing Sales", NameAr = "إيرادات مبيعات الملابس الجاهزة", Type = AccountType.Revenue, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accOperatingRevenue.Id, CurrentBalance = 0m },
                new() { Code = "4102", NameEn = "Online Store Sales", NameAr = "إيرادات مبيعات المتجر الإلكتروني", Type = AccountType.Revenue, Nature = AccountNature.Credit, Level = 3, IsPostable = true, ParentAccountId = accOperatingRevenue.Id, CurrentBalance = 0m },
                new() { Code = "4103", NameEn = "Sales Discounts & Returns", NameAr = "خصومات ومردودات المبيعات", Type = AccountType.Revenue, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accOperatingRevenue.Id, CurrentBalance = 0m },

                // Costs & Expenses
                new() { Code = "5101", NameEn = "Cost of Goods Sold - Apparel", NameAr = "تكلفة مبيعات الملابس الجاهزة", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accCogs.Id, CurrentBalance = 0m },
                new() { Code = "5201", NameEn = "Branch Rent Expense", NameAr = "إيجارات الفروع والمعارض", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accSellingExp.Id, CurrentBalance = 0m },
                new() { Code = "5202", NameEn = "Digital Advertising & Campaigns", NameAr = "إعلانات وحملات تسويقية", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accSellingExp.Id, CurrentBalance = 0m },
                new() { Code = "5203", NameEn = "Store Utilities & Power", NameAr = "كهرباء ومرافق الفروع", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accSellingExp.Id, CurrentBalance = 0m },
                new() { Code = "5301", NameEn = "Salaries & Administrative Wages", NameAr = "رواتب ومستحقات الموظفين", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accAdminExp.Id, CurrentBalance = 0m },
                new() { Code = "5302", NameEn = "Office, Audit & Legal Fees", NameAr = "مصروفات إدارية وأتعاب مراجعة", Type = AccountType.Expense, Nature = AccountNature.Debit, Level = 3, IsPostable = true, ParentAccountId = accAdminExp.Id, CurrentBalance = 0m }
            };

            await context.Accounts.AddRangeAsync(detailAccounts);
            await context.SaveChangesAsync();
        }

        if (!await context.ETACompanySettings.AnyAsync())
        {
            context.ETACompanySettings.Add(new ETACompanySetting
            {
                TaxRegistrationNumber = "499-823-115",
                ActivityCode = "4771", // تجارة التجزئة في الملابس
                CompanyName = "POS Private Garments & Fashion LLC",
                BranchTaxCode = "0",
                DefaultVatRate = 14.0m,
                DefaultWhtRate = 1.0m,
                IsProduction = false,
                AutoSubmitInvoices = false
            });
            await context.SaveChangesAsync();
        }

        if (!await context.FiscalPeriods.AnyAsync())
        {
            var year = DateTime.UtcNow.Year;
            var periods = new List<FiscalPeriod>();
            for (int m = 1; m <= 12; m++)
            {
                var start = new DateTime(year, m, 1);
                var end = start.AddMonths(1).AddDays(-1);
                periods.Add(new FiscalPeriod
                {
                    Year = year,
                    PeriodNumber = m,
                    NameEn = start.ToString("MMMM yyyy"),
                    NameAr = $"الفترة المحاسبية {m} - {year}",
                    StartDate = start,
                    EndDate = end,
                    IsClosed = m < DateTime.UtcNow.Month
                });
            }
            await context.FiscalPeriods.AddRangeAsync(periods);
            await context.SaveChangesAsync();
        }

        // Seed Sample Journal Entries (1 Posted, 1 Draft for review)
        if (!await context.JournalEntries.AnyAsync())
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            var ccNasr = await context.CostCenters.FirstOrDefaultAsync(c => c.Code == "CC-01");
            var ccMall = await context.CostCenters.FirstOrDefaultAsync(c => c.Code == "CC-02");
            var ccHQ = await context.CostCenters.FirstOrDefaultAsync(c => c.Code == "CC-05");

            var accCash = await context.Accounts.FirstOrDefaultAsync(a => a.Code == "1101");
            var accBank = await context.Accounts.FirstOrDefaultAsync(a => a.Code == "1102");
            var accSales = await context.Accounts.FirstOrDefaultAsync(a => a.Code == "4101");
            var accVatPayable = await context.Accounts.FirstOrDefaultAsync(a => a.Code == "2102");
            var accRent = await context.Accounts.FirstOrDefaultAsync(a => a.Code == "5201");

            if (accCash != null && accSales != null && accVatPayable != null)
            {
                // Entry 1: POSTED - Daily POS Cash Sales with 14% VAT
                var entry1 = new JournalEntry
                {
                    EntryNumber = "JE-202609-00001",
                    EntryDate = DateTime.UtcNow.AddDays(-2),
                    PostingDate = DateTime.UtcNow.AddDays(-2),
                    Status = JournalEntryStatus.Posted,
                    SourceType = JournalSourceType.POS_Sale,
                    ReferenceNumber = "POS-BATCH-0928",
                    NarrationEn = "Daily POS Retail Sales summary for Nasr City branch incl. 14% VAT",
                    NarrationAr = "إثبات مبيعات نقاط البيع اليومية لفرع مدينة نصر متضمنة 14% ضريبة قيمة مضافة",
                    TotalDebit = 22800.00m,
                    TotalCredit = 22800.00m,
                    CreatedByUserId = adminUser?.Id,
                    ApprovedByUserId = adminUser?.Id,
                    ApprovedAt = DateTime.UtcNow.AddDays(-2),
                    ETASubmissionStatus = ETASubmissionStatus.Valid,
                    ETAUUID = "ETA-EINV-2026-9918231",
                    BranchTaxCode = "0",
                    Lines = new List<JournalEntryLine>
                    {
                        new() { AccountId = accCash.Id, CostCenterId = ccNasr?.Id, Debit = 22800.00m, Credit = 0m, Description = "Cash collected in POS register drawer" },
                        new() { AccountId = accSales.Id, CostCenterId = ccNasr?.Id, Debit = 0m, Credit = 20000.00m, Description = "Net clothing sales revenue" },
                        new() { AccountId = accVatPayable.Id, CostCenterId = ccNasr?.Id, Debit = 0m, Credit = 2800.00m, Description = "Output VAT 14% payable to ETA", ETATaxType = ETATaxType.T1_VAT, TaxRate = 14.0m, TaxAmount = 2800.00m }
                    }
                };

                // Entry 2: DRAFT - Monthly Store Rent (Pending Accountant Review)
                var entry2 = new JournalEntry
                {
                    EntryNumber = "JE-202609-00002",
                    EntryDate = DateTime.UtcNow.AddDays(-1),
                    Status = JournalEntryStatus.Draft,
                    SourceType = JournalSourceType.OperationalExpense,
                    ReferenceNumber = "RENT-OCT-2026",
                    NarrationEn = "Monthly store lease rental payment for Mall of Egypt Branch via Bank Transfer",
                    NarrationAr = "إثبات سداد إيجار فرع مول مصر لشهر أكتوبر عبر تحويل بنكي للمراجعة والترحيل",
                    TotalDebit = 35000.00m,
                    TotalCredit = 35000.00m,
                    CreatedByUserId = adminUser?.Id,
                    ETASubmissionStatus = ETASubmissionStatus.NotApplicable,
                    BranchTaxCode = "0",
                    Lines = new List<JournalEntryLine>
                    {
                        new() { AccountId = accRent?.Id ?? accCash.Id, CostCenterId = ccMall?.Id, Debit = 35000.00m, Credit = 0m, Description = "Mall branch lease rental expense" },
                        new() { AccountId = accBank?.Id ?? accCash.Id, CostCenterId = ccHQ?.Id, Debit = 0m, Credit = 35000.00m, Description = "Payment made via CIB corporate account" }
                    }
                };

                context.JournalEntries.AddRange(entry1, entry2);
                await context.SaveChangesAsync();
            }
        }
    }
}


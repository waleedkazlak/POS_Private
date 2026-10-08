namespace POS_Private.Domain.Enums;

public enum AccountType
{
    Asset = 1,       // أصول
    Liability = 2,   // التزامات
    Equity = 3,      // حقوق ملكية
    Revenue = 4,     // إيرادات
    Expense = 5      // مصروفات
}

public enum AccountNature
{
    Debit = 1,  // مدين طبيعته
    Credit = 2  // دائن طبيعته
}

public enum JournalEntryStatus
{
    Draft = 1,    // مسودة - قيد للمراجعة
    Posted = 2,   // مرحل - معتمد ومؤثر في الحسابات
    Voided = 3    // ملغي - قيد تم إلغاؤه مع ذكر السبب
}

public enum JournalSourceType
{
    Manual = 1,              // قيد يومية يدوي
    POS_Sale = 2,            // مبيعات نقاط البيع
    POS_Return = 3,          // مرتجع مبيعات
    Vendor_Bill = 4,         // فاتورة مورد
    Vendor_Payment = 5,      // سداد مورد
    OperationalExpense = 6,  // مصروف تشغيلي
    StockAdjustment = 7,     // تسوية مخزنية
    TaxSettlement = 8,       // تسوية ضريبية
    ClosingEntry = 9         // قيد إقفال فترات
}

public enum CostCenterType
{
    CostCenter = 1,    // مركز تكلفة
    ProfitCenter = 2,  // مركز ربحية
    RevenueCenter = 3  // مركز إيرادات
}

public enum ETATaxType
{
    None = 0,
    T1_VAT = 1,        // ضريبة القيمة المضافة 14%
    T2_TableTax = 2,   // ضريبة جدول
    T3_TableTaxPercentage = 3,
    T4_WithholdingTax = 4, // خصم وتحصيل 1%
    T5_StampingTax = 5,
    T6_EntertainmentTax = 6,
    T7_ResourceDevelopmentTax = 7,
    T8_CustomsDuty = 8,
    T12_StampTax = 12
}

public enum ETASubmissionStatus
{
    NotApplicable = 0, // غير خاضع للإرسال المباشر
    Pending = 1,       // بانتظار الإرسال
    Submitted = 2,     // تم الإرسال لمنظومة الضرائب
    Valid = 3,         // صالح ومعتمد رسمياً
    Rejected = 4,      // مرفوض من مصلحة الضرائب
    Cancelled = 5      // ملغي
}

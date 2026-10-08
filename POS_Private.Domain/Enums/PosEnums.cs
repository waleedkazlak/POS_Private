namespace POS_Private.Domain.Enums;

public enum PaymentMethod
{
    Cash = 1,
    Visa = 2,
    MasterCard = 3,
    InstaPay = 4,
    VodafoneCash = 5,
    OrangeMoney = 6,
    EtisalatCash = 7,
    LoyaltyPoints = 8,
    Voucher = 9,
    Split = 10
}

public enum InvoiceType
{
    Sale = 1,
    Return = 2,
    Quotation = 3
}

public enum InvoiceStatus
{
    Completed = 1,
    Draft = 2,
    Cancelled = 3,
    Refunded = 4
}

public enum PaymentStatus
{
    Paid = 1,
    PartiallyPaid = 2,
    Unpaid = 3
}

public enum ShiftStatus
{
    Open = 1,
    Closed = 2
}

public enum ShiftTransactionType
{
    CashIn = 1,
    CashOut = 2,
    Drop = 3
}

public enum StockMovementType
{
    InitialSeed = 1,
    Sale = 2,
    Return = 3,
    Adjustment = 4,
    TransferIn = 5,
    TransferOut = 6,
    Damaged = 7,
    VendorPurchase = 8,
    VendorReturn = 9
}

public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}

public enum VendorTransactionType
{
    OpeningBalance = 1,
    PurchaseBill = 2,
    PurchaseReturn = 3,
    Payment = 4,
    DebitAdjustment = 5,
    CreditAdjustment = 6
}

public enum VendorBillStatus
{
    Draft = 1,
    Approved = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Cancelled = 5
}

public enum ExpensePaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    CreditCard = 3,
    Cheque = 4,
    PettyCash = 5,
    DrawerCash = 6
}

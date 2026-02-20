namespace BookKeeping.Models;

/// <summary>
/// Represents the type of an account.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Cash account (現金).
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Bank account (銀行).
    /// </summary>
    Bank = 1,

    /// <summary>
    /// Credit card account (信用卡).
    /// </summary>
    CreditCard = 2,

    /// <summary>
    /// Electronic payment account (電子支付).
    /// </summary>
    EPayment = 3
}

using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Services;

/// <summary>
/// Result of creating a payment invoice
/// </summary>
public class CreateInvoiceResult
{
    /// <summary>
    /// Unique quote/payment identifier
    /// </summary>
    public required string QuoteId { get; set; }
    
    /// <summary>
    /// Payment request (Lightning invoice, payment URL, etc.)
    /// </summary>
    public required string PaymentRequest { get; set; }
    
    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment unit
    /// </summary>
    public required string Unit { get; set; }
    
    /// <summary>
    /// When the payment request expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Status of a payment
/// </summary>
public enum PaymentStatus
{
    Pending,
    Paid,
    Expired,
    Failed
}

/// <summary>
/// Result of checking payment status
/// </summary>
public class PaymentStatusResult
{
    /// <summary>
    /// Current payment status
    /// </summary>
    public PaymentStatus Status { get; set; }
    
    /// <summary>
    /// Payment amount (when paid)
    /// </summary>
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Additional data from payment provider
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Result of claiming a paid token/proof
/// </summary>
public class ClaimTokenResult
{
    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Claimed token/proof data (for internal storage)
    /// </summary>
    public string? TokenData { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Interface for payment providers (Cashu, Lightning, Stripe, etc.)
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Provider name (e.g., "cashu", "lightning", "stripe")
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Create a payment invoice/request
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="unit">Payment unit</param>
    /// <param name="description">Optional description</param>
    /// <returns>Invoice result</returns>
    Task<CreateInvoiceResult> CreateInvoiceAsync(decimal amount, string unit, string? description = null);
    
    /// <summary>
    /// Check payment status
    /// </summary>
    /// <param name="quoteId">Quote/payment identifier</param>
    /// <returns>Payment status</returns>
    Task<PaymentStatusResult> CheckPaymentAsync(string quoteId);
    
    /// <summary>
    /// Claim the token/proof after successful payment (optional)
    /// </summary>
    /// <param name="quoteId">Quote/payment identifier</param>
    /// <param name="amount">Payment amount</param>
    /// <returns>Claim result</returns>
    Task<ClaimTokenResult?> ClaimTokenAsync(string quoteId, decimal amount);
}
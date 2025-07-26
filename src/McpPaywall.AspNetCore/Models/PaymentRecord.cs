using System.ComponentModel.DataAnnotations;

namespace McpPaywall.AspNetCore.Models;

/// <summary>
/// Represents a payment record for MCP server access
/// </summary>
public class PaymentRecord
{
    /// <summary>
    /// Unique quote identifier from payment provider
    /// </summary>
    [Key]
    public string QuoteId { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token for MCP server authentication
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// When the payment record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether the payment has been completed
    /// </summary>
    public bool IsPaid { get; set; }
    
    /// <summary>
    /// The claimed token/proof from payment provider (optional, for internal use)
    /// </summary>
    public string? ClaimedToken { get; set; }
    
    /// <summary>
    /// User identifier (IP address, user ID, etc.)
    /// </summary>
    public string? UserIdentifier { get; set; }
    
    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment unit (usd, sat, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment provider used (cashu, lightning, stripe, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Check if the payment record is currently active
    /// </summary>
    public bool IsActive => DateTime.UtcNow < ExpiresAt && IsPaid;
}
namespace McpPaywall.AspNetCore.Models;

/// <summary>
/// Response for invoice creation request
/// </summary>
public class CreateInvoiceResponse
{
    /// <summary>
    /// Unique quote identifier
    /// </summary>
    public required string Quote { get; set; }
    
    /// <summary>
    /// Payment request (Lightning invoice, payment URL, etc.)
    /// </summary>
    public required string Request { get; set; }
    
    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment unit
    /// </summary>
    public required string Unit { get; set; }
    
    /// <summary>
    /// Payment provider
    /// </summary>
    public required string Provider { get; set; }
    
    /// <summary>
    /// When the payment request expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response for payment status check
/// </summary>
public class CheckPaymentResponse
{
    /// <summary>
    /// Payment state (PENDING, PAID, EXPIRED, etc.)
    /// </summary>
    public required string State { get; set; }
    
    /// <summary>
    /// Whether payment is completed
    /// </summary>
    public bool Paid { get; set; }
    
    /// <summary>
    /// Access token for MCP server (only when paid)
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// MCP server link with access token (only when paid)
    /// </summary>
    public string? McpLink { get; set; }
    
    /// <summary>
    /// When the access expires (only when paid)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request for creating an invoice
/// </summary>
public class CreateInvoiceRequest
{
    /// <summary>
    /// Payment amount (optional, can be configured in options)
    /// </summary>
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Payment unit (optional, can be configured in options)
    /// </summary>
    public string? Unit { get; set; }
    
    /// <summary>
    /// Payment description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// User identifier (optional, will use IP if not provided)
    /// </summary>
    public string? UserIdentifier { get; set; }
}
using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Services;

/// <summary>
/// Service for managing paywall operations
/// </summary>
public interface IPaywallService
{
    /// <summary>
    /// Create a payment invoice
    /// </summary>
    /// <param name="request">Invoice creation request</param>
    /// <param name="userIdentifier">User identifier (IP, etc.)</param>
    /// <returns>Invoice response</returns>
    Task<CreateInvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, string? userIdentifier = null);
    
    /// <summary>
    /// Check payment status and return access details if paid
    /// </summary>
    /// <param name="quoteId">Quote identifier</param>
    /// <param name="baseUrl">Base URL for generating MCP link</param>
    /// <returns>Payment status response</returns>
    Task<CheckPaymentResponse> CheckPaymentAsync(string quoteId, string baseUrl);
    
    /// <summary>
    /// Validate an access token
    /// </summary>
    /// <param name="accessToken">Access token to validate</param>
    /// <returns>Payment record if valid, null otherwise</returns>
    Task<PaymentRecord?> ValidateAccessTokenAsync(string accessToken);
    
    /// <summary>
    /// Get payment statistics
    /// </summary>
    /// <returns>Statistics</returns>
    Task<object> GetStatisticsAsync();
    
    /// <summary>
    /// Clean up expired payment records
    /// </summary>
    /// <returns>Number of records cleaned</returns>
    Task<int> CleanupExpiredRecordsAsync();
}
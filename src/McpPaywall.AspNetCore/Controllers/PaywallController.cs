using Microsoft.AspNetCore.Mvc;
using McpPaywall.AspNetCore.Models;
using McpPaywall.AspNetCore.Services;

namespace McpPaywall.AspNetCore.Controllers;

/// <summary>
/// API controller for paywall operations
/// </summary>
[ApiController]
[Route("paywall")]
public class PaywallController : ControllerBase
{
    private readonly IPaywallService _paywallService;
    private readonly ILogger<PaywallController> _logger;

    public PaywallController(IPaywallService paywallService, ILogger<PaywallController> logger)
    {
        _paywallService = paywallService;
        _logger = logger;
    }

    /// <summary>
    /// Create a payment invoice
    /// </summary>
    /// <param name="request">Invoice creation request</param>
    /// <returns>Invoice response with payment details</returns>
    [HttpPost("create-invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var userIdentifier = GetUserIdentifier();
            var response = await _paywallService.CreateInvoiceAsync(request, userIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create invoice");
            return BadRequest(new { error = "Failed to create invoice", details = ex.Message });
        }
    }

    /// <summary>
    /// Check payment status and get access details
    /// </summary>
    /// <param name="quoteId">Quote identifier</param>
    /// <returns>Payment status and access details</returns>
    [HttpGet("check-payment/{quoteId}")]
    public async Task<IActionResult> CheckPayment(string quoteId)
    {
        try
        {
            var baseUrl = GetBaseUrl();
            var response = await _paywallService.CheckPaymentAsync(quoteId, baseUrl);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check payment for quote {QuoteId}", quoteId);
            return BadRequest(new { error = "Failed to check payment", details = ex.Message });
        }
    }

    /// <summary>
    /// Get paywall statistics (admin endpoint)
    /// </summary>
    /// <returns>Statistics</returns>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _paywallService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get statistics");
            return BadRequest(new { error = "Failed to get statistics", details = ex.Message });
        }
    }

    /// <summary>
    /// Clean up expired payment records (admin endpoint)
    /// </summary>
    /// <returns>Number of cleaned records</returns>
    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup()
    {
        try
        {
            var cleanedCount = await _paywallService.CleanupExpiredRecordsAsync();
            return Ok(new { cleaned = cleanedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired records");
            return BadRequest(new { error = "Failed to cleanup", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate access token (utility endpoint)
    /// </summary>
    /// <param name="token">Access token</param>
    /// <returns>Token validation result</returns>
    [HttpGet("validate-token")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        try
        {
            var paymentRecord = await _paywallService.ValidateAccessTokenAsync(token);

            if (paymentRecord == null)
            {
                return Ok(new { valid = false, message = "Invalid or expired token" });
            }

            return Ok(new
            {
                valid = true,
                expiresAt = paymentRecord.ExpiresAt,
                provider = paymentRecord.Provider,
                amount = paymentRecord.Amount,
                unit = paymentRecord.Unit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate token");
            return BadRequest(new { error = "Failed to validate token", details = ex.Message });
        }
    }

    private string GetUserIdentifier()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetBaseUrl()
    {
        var request = HttpContext.Request;
        var scheme = request.Headers.ContainsKey("X-Forwarded-Proto") 
            ? request.Headers["X-Forwarded-Proto"].ToString()
            : request.Scheme;
        
        // Force HTTPS in production
        if (scheme == "http" && !request.Host.Host.Contains("localhost"))
        {
            scheme = "https";
        }
        
        return $"{scheme}://{request.Host}";
    }
}
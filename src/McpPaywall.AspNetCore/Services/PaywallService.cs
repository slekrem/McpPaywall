using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using McpPaywall.AspNetCore.Data;
using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Services;

/// <summary>
/// Default implementation of paywall service
/// </summary>
public class PaywallService : IPaywallService
{
    private readonly PaywallDbContext _dbContext;
    private readonly IPaymentProvider _paymentProvider;
    private readonly McpPaywallOptions _options;
    private readonly ILogger<PaywallService> _logger;

    public PaywallService(
        PaywallDbContext dbContext,
        IPaymentProvider paymentProvider,
        IOptions<McpPaywallOptions> options,
        ILogger<PaywallService> logger)
    {
        _dbContext = dbContext;
        _paymentProvider = paymentProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CreateInvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, string? userIdentifier = null)
    {
        var amount = request.Amount ?? _options.DefaultAmount;
        var unit = request.Unit ?? _options.DefaultUnit;
        var description = request.Description ?? $"MCP Server Access ({amount} {unit})";

        // Create invoice with payment provider
        var invoiceResult = await _paymentProvider.CreateInvoiceAsync(amount, unit, description);

        // Generate access token
        var accessToken = GenerateAccessToken();

        // Save payment record
        var paymentRecord = new PaymentRecord
        {
            QuoteId = invoiceResult.QuoteId,
            AccessToken = accessToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.TokenValidityDays),
            IsPaid = false,
            Amount = amount,
            Unit = unit,
            Provider = _paymentProvider.ProviderName,
            UserIdentifier = userIdentifier ?? request.UserIdentifier
        };

        _dbContext.PaymentRecords.Add(paymentRecord);
        await _dbContext.SaveChangesAsync();

        if (_options.EnableLogging)
        {
            _logger.LogInformation(
                "Created invoice {QuoteId} for {Amount} {Unit} using {Provider} (user: {UserIdentifier})",
                invoiceResult.QuoteId,
                amount,
                unit,
                _paymentProvider.ProviderName,
                userIdentifier ?? "unknown");
        }

        return new CreateInvoiceResponse
        {
            Quote = invoiceResult.QuoteId,
            Request = invoiceResult.PaymentRequest,
            Amount = amount,
            Unit = unit,
            Provider = _paymentProvider.ProviderName,
            ExpiresAt = invoiceResult.ExpiresAt
        };
    }

    public async Task<CheckPaymentResponse> CheckPaymentAsync(string quoteId, string baseUrl)
    {
        // Find payment record
        var paymentRecord = await _dbContext.PaymentRecords
            .FirstOrDefaultAsync(p => p.QuoteId == quoteId);

        if (paymentRecord == null)
        {
            return new CheckPaymentResponse
            {
                State = "NOT_FOUND",
                Paid = false
            };
        }

        // If already paid, return cached result
        if (paymentRecord.IsPaid && !string.IsNullOrEmpty(paymentRecord.ClaimedToken))
        {
            var mcpLink = GenerateMcpLink(baseUrl, paymentRecord.AccessToken);
            
            return new CheckPaymentResponse
            {
                State = "PAID",
                Paid = true,
                AccessToken = paymentRecord.AccessToken,
                McpLink = mcpLink,
                ExpiresAt = paymentRecord.ExpiresAt
            };
        }

        // Check payment status with provider
        var statusResult = await _paymentProvider.CheckPaymentAsync(quoteId);

        if (statusResult.Status == PaymentStatus.Paid)
        {
            // Claim token if provider supports it
            var claimResult = await _paymentProvider.ClaimTokenAsync(quoteId, paymentRecord.Amount);

            // Update payment record
            paymentRecord.IsPaid = true;
            paymentRecord.ClaimedToken = claimResult?.TokenData;
            await _dbContext.SaveChangesAsync();

            var mcpLink = GenerateMcpLink(baseUrl, paymentRecord.AccessToken);

            if (_options.EnableLogging)
            {
                _logger.LogInformation(
                    "Payment completed for quote {QuoteId} (user: {UserIdentifier})",
                    quoteId,
                    paymentRecord.UserIdentifier ?? "unknown");
            }

            return new CheckPaymentResponse
            {
                State = "PAID",
                Paid = true,
                AccessToken = paymentRecord.AccessToken,
                McpLink = mcpLink,
                ExpiresAt = paymentRecord.ExpiresAt
            };
        }

        return new CheckPaymentResponse
        {
            State = statusResult.Status.ToString().ToUpper(),
            Paid = false
        };
    }

    public async Task<PaymentRecord?> ValidateAccessTokenAsync(string accessToken)
    {
        var paymentRecord = await _dbContext.PaymentRecords
            .FirstOrDefaultAsync(p => p.AccessToken == accessToken);

        return paymentRecord?.IsActive == true ? paymentRecord : null;
    }

    public async Task<object> GetStatisticsAsync()
    {
        var totalPayments = await _dbContext.PaymentRecords.CountAsync();
        var paidPayments = await _dbContext.PaymentRecords.CountAsync(p => p.IsPaid);
        var activeTokens = await _dbContext.PaymentRecords.CountAsync(p => p.IsActive);
        var expiredTokens = await _dbContext.PaymentRecords.CountAsync(p => p.IsPaid && DateTime.UtcNow >= p.ExpiresAt);

        var totalRevenue = await _dbContext.PaymentRecords
            .Where(p => p.IsPaid)
            .GroupBy(p => p.Unit)
            .Select(g => new { Unit = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        return new
        {
            TotalPayments = totalPayments,
            PaidPayments = paidPayments,
            ActiveTokens = activeTokens,
            ExpiredTokens = expiredTokens,
            ConversionRate = totalPayments > 0 ? (double)paidPayments / totalPayments : 0,
            Revenue = totalRevenue
        };
    }

    public async Task<int> CleanupExpiredRecordsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30); // Keep records for 30 days after expiry
        var expiredRecords = await _dbContext.PaymentRecords
            .Where(p => p.ExpiresAt < cutoffDate)
            .ToListAsync();

        if (expiredRecords.Any())
        {
            _dbContext.PaymentRecords.RemoveRange(expiredRecords);
            await _dbContext.SaveChangesAsync();

            if (_options.EnableLogging)
            {
                _logger.LogInformation("Cleaned up {Count} expired payment records", expiredRecords.Count);
            }
        }

        return expiredRecords.Count;
    }

    private static string GenerateAccessToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
    }

    private string GenerateMcpLink(string baseUrl, string accessToken)
    {
        var mcpPath = _options.McpPath.TrimStart('/');
        return $"{baseUrl.TrimEnd('/')}/{mcpPath}/sse?accessToken={accessToken}";
    }
}
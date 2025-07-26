using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using McpPaywall.AspNetCore.Data;
using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Middleware;

/// <summary>
/// Middleware to authenticate MCP server requests using access tokens
/// </summary>
public class McpAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly McpPaywallOptions _options;

    public McpAuthenticationMiddleware(
        RequestDelegate next, 
        IServiceScopeFactory scopeFactory,
        IOptions<McpPaywallOptions> options)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate for MCP endpoints
        if (context.Request.Path.StartsWithSegments(_options.McpPath))
        {
            var accessToken = context.Request.Query["accessToken"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                await WriteUnauthorizedResponse(context, "Access token required");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaywallDbContext>();
            
            var paymentRecord = await dbContext.PaymentRecords
                .FirstOrDefaultAsync(p => p.AccessToken == accessToken);

            if (paymentRecord == null)
            {
                await WriteUnauthorizedResponse(context, "Invalid access token");
                return;
            }

            if (!paymentRecord.IsActive)
            {
                await WriteUnauthorizedResponse(context, "Access token expired or inactive");
                return;
            }

            // Add user info to context for potential logging
            context.Items["McpPaywall.UserId"] = paymentRecord.QuoteId;
            context.Items["McpPaywall.UserIdentifier"] = paymentRecord.UserIdentifier;
            context.Items["McpPaywall.Provider"] = paymentRecord.Provider;
            context.Items["McpPaywall.ExpiresAt"] = paymentRecord.ExpiresAt;

            if (_options.EnableLogging)
            {
                var logger = scope.ServiceProvider.GetService<ILogger<McpAuthenticationMiddleware>>();
                logger?.LogInformation(
                    "[McpPaywall] User {UserId} ({UserIdentifier}) accessing MCP endpoint {Path}",
                    paymentRecord.QuoteId, 
                    paymentRecord.UserIdentifier ?? "unknown",
                    context.Request.Path);
            }
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        
        var response = System.Text.Json.JsonSerializer.Serialize(new { 
            error = "Unauthorized", 
            message = message,
            timestamp = DateTime.UtcNow
        });
        
        await context.Response.WriteAsync(response);
    }
}
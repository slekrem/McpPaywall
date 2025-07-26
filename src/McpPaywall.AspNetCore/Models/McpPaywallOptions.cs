namespace McpPaywall.AspNetCore.Models;

/// <summary>
/// Configuration options for MCP Paywall
/// </summary>
public class McpPaywallOptions
{
    /// <summary>
    /// Database connection string (default: SQLite in memory)
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=mcppaywall.db";
    
    /// <summary>
    /// How many days access tokens are valid (default: 7)
    /// </summary>
    public int TokenValidityDays { get; set; } = 7;
    
    /// <summary>
    /// Default payment amount (default: 99)
    /// </summary>
    public decimal DefaultAmount { get; set; } = 99;
    
    /// <summary>
    /// Default payment unit (default: "usd")
    /// </summary>
    public string DefaultUnit { get; set; } = "usd";
    
    /// <summary>
    /// Default payment provider (default: "cashu")
    /// </summary>
    public string DefaultProvider { get; set; } = "cashu";
    
    /// <summary>
    /// Base path for paywall endpoints (default: "/paywall")
    /// </summary>
    public string BasePath { get; set; } = "/paywall";
    
    /// <summary>
    /// MCP endpoint path that requires authentication (default: "/mcp")
    /// </summary>
    public string McpPath { get; set; } = "/mcp";
    
    /// <summary>
    /// Whether to enable detailed logging (default: true)
    /// </summary>
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Whether to automatically create database (default: true)
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; } = true;
    
    /// <summary>
    /// Custom CSS for paywall UI (optional)
    /// </summary>
    public string? CustomCss { get; set; }
    
    /// <summary>
    /// Custom logo URL for paywall UI (optional)
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Custom title for paywall UI (default: "MCP Server Access")
    /// </summary>
    public string Title { get; set; } = "MCP Server Access";
    
    /// <summary>
    /// Custom description for paywall UI (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Cashu-specific configuration options
/// </summary>
public class CashuPaymentOptions
{
    /// <summary>
    /// Cashu mint URL (required for Cashu provider)
    /// </summary>
    public string MintUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to store claimed Cashu tokens (default: true)
    /// </summary>
    public bool StoreTokens { get; set; } = true;
}
using ModelContextProtocol.Server;
using McpPaywall.AspNetCore.Extensions;
using McpPaywallDemo;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add HTTP context accessor for user tracking
builder.Services.AddHttpContextAccessor();

// Add MCP Paywall with Cashu integration
builder.Services.AddMcpPaywallWithCashu(
    options =>
    {
        // Database configuration
        options.ConnectionString = "Data Source=demo-paywall.db";
        options.EnsureDatabaseCreated = true;

        // Payment configuration
        options.DefaultAmount = 10;           // 10 satoshis for demo
        options.DefaultUnit = "sat";
        options.TokenValidityDays = 1;        // Shorter validity for demo

        // Endpoint configuration
        options.McpPath = "/mcp/sse";        // MCP endpoint path
        options.BasePath = "/paywall";       // Paywall API base path

        // UI customization
        options.Title = "McpPaywall Demo - Premium MCP Tools";
        options.Description = "Pay 10 sats to access premium Cashu mint tools and weather information.";

        // Logging
        options.EnableLogging = true;
    },
    cashuOptions =>
    {
        // Use Minibits.cash mint for demo
        cashuOptions.MintUrl = "https://stablenut.cashu.network";
        cashuOptions.StoreTokens = true;
    });

// Add MCP Server with both HTTP and STDIO transports
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    //.WithStdioServerTransport()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly()
    .WithToolsFromAssembly();

// Add HTTP client for external API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure middleware pipeline
app.UsePathBase("/paywall")
   .UseDefaultFiles()
   .UseStaticFiles()
   .UseMcpPaywall()
   .UseRouting();

// Map endpoints
app.MapControllers();
app.MapMcp("/mcp");

await app.RunAsync();


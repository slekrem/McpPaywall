using ModelContextProtocol.Server;
using System.ComponentModel;
using DotNut.Api;
using DotNut.ApiModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using McpPaywall.AspNetCore.Extensions;

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
        options.BasePath = "/demo/paywall";   // Paywall API base path

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
    .WithStdioServerTransport()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly()
    .WithToolsFromAssembly();

// Add HTTP client for external API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure middleware pipeline
app.UsePathBase("/demo")
   .UseDefaultFiles()
   .UseStaticFiles()
   .UseMcpPaywall()
   .UseRouting();

// Map endpoints
app.MapControllers();
app.MapMcp("/mcp");

// Start the application
Console.WriteLine("=== McpPaywall Demo Server ===");
Console.WriteLine("Paywall UI: http://localhost:5000/demo/paywall");
Console.WriteLine("MCP Endpoint: http://localhost:5000/demo/mcp (requires payment)");
Console.WriteLine("Payment: 10 sats via Cashu eCash");
Console.WriteLine();

await app.RunAsync();

/// <summary>
/// Demo MCP tools that require payment to access
/// </summary>
[McpServerToolType]
public static class DemoTools
{
    [McpServerTool, Description("Get comprehensive information about a Cashu mint including supported features, contact info, and technical details.")]
    public static async Task<string> GetCashuMintInfo(string mintUrl,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        var userIp = context?.Items["McpPaywall.UserIdentifier"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} ({userIp}) requesting mint info for: {mintUrl}");

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(mintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);
            var info = await cashuClient.GetInfo();

            return JsonSerializer.Serialize(new
            {
                mint_url = mintUrl,
                name = info.Name,
                version = info.Version,
                description = info.Description,
                description_long = info.DescriptionLong,
                pubkey = info.Pubkey,
                contact = info.Contact,
                motd = info.Motd,
                icon_url = info.IconUrl,
                tos_url = info.TosUrl,
                supported_nuts = info.Nuts?.Keys.OrderBy(x => x).ToArray(),
                server_time = info.Time,
                accessed_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEMO] Error fetching mint info for user {userId}: {ex.Message}");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to fetch mint information",
                details = ex.Message,
                mint_url = mintUrl
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get all available keysets from a Cashu mint with detailed information about fees and supported denominations.")]
    public static async Task<string> GetCashuKeysets(string mintUrl,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} requesting keysets for: {mintUrl}");

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(mintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);
            var keysets = await cashuClient.GetKeysets();

            return JsonSerializer.Serialize(new
            {
                mint_url = mintUrl,
                total_keysets = keysets.Keysets.Count(),
                active_keysets = keysets.Keysets.Count(k => k.Active),
                keysets = keysets.Keysets.Select(k => new
                {
                    id = k.Id.ToString(),
                    unit = k.Unit,
                    active = k.Active,
                    input_fee_ppk = k.InputFee,
                    fee_description = $"{k.InputFee} parts per thousand"
                }).ToArray(),
                retrieved_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEMO] Error fetching keysets for user {userId}: {ex.Message}");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to fetch keysets",
                details = ex.Message,
                mint_url = mintUrl
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a mint quote to get a Lightning invoice for minting new Cashu tokens.")]
    public static async Task<string> CreateMintQuote(string mintUrl, ulong amount, string unit = "sat", string? description = null,
        [FromServices] IHttpContextAccessor httpContextAccessor = null!)
    {
        var context = httpContextAccessor?.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} creating mint quote: {amount} {unit} at {mintUrl}");

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(mintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);

            var request = new PostMintQuoteBolt11Request
            {
                Amount = amount,
                Unit = unit,
                Description = description ?? $"Mint {amount} {unit} via McpPaywall Demo"
            };

            var response = await cashuClient.CreateMintQuote<PostMintQuoteBolt11Response, PostMintQuoteBolt11Request>("bolt11", request);

            return JsonSerializer.Serialize(new
            {
                mint_url = mintUrl,
                quote_id = response.Quote,
                lightning_invoice = response.Request,
                amount = response.Amount,
                unit = response.Unit,
                state = response.State,
                expiry = response.Expiry,
                expiry_datetime = response.Expiry.HasValue ?
                    DateTimeOffset.FromUnixTimeSeconds((long)response.Expiry.Value).ToString("yyyy-MM-dd HH:mm:ss UTC") : null,
                description = description,
                created_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                instructions = "Pay the Lightning invoice to mint Cashu tokens. Use GetMintQuoteState to check payment status."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEMO] Error creating mint quote for user {userId}: {ex.Message}");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to create mint quote",
                details = ex.Message,
                mint_url = mintUrl,
                requested_amount = amount,
                requested_unit = unit
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get current weather information for any city worldwide.")]
    public static async Task<string> GetWeather(string city,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IHttpClientFactory httpClientFactory)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        var provider = context?.Items["McpPaywall.Provider"]?.ToString() ?? "unknown";
        var expiresAt = context?.Items["McpPaywall.ExpiresAt"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} (provider: {provider}, expires: {expiresAt}) requesting weather for: {city}");

        try
        {
            // Using a free weather API (OpenWeatherMap would require API key)
            // For demo purposes, we'll simulate weather data
            await Task.Delay(100); // Simulate API call delay
            var random = new Random();
            var temperature = random.Next(-10, 35);
            var conditions = new[] { "sunny", "cloudy", "rainy", "snowy", "windy", "foggy" };
            var condition = conditions[random.Next(conditions.Length)];
            var humidity = random.Next(30, 90);
            var windSpeed = random.Next(0, 25);

            return JsonSerializer.Serialize(new
            {
                city = city,
                weather = new
                {
                    temperature_celsius = temperature,
                    condition = condition,
                    humidity_percent = humidity,
                    wind_speed_kmh = windSpeed,
                    description = $"Currently {condition} with {temperature}°C"
                },
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                access_info = new
                {
                    user_id = userId,
                    payment_provider = provider,
                    access_expires_at = expiresAt
                },
                note = "This is simulated weather data for demo purposes. In production, this would use a real weather API."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEMO] Error fetching weather for user {userId}: {ex.Message}");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to fetch weather data",
                details = ex.Message,
                city = city
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Generate a random inspirational quote with author attribution.")]
    public static async Task<string> GetInspirationalQuote(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} requesting inspirational quote");

        await Task.Delay(100); // Simulate async operation

        var quotes = new[]
        {
            new { quote = "The future belongs to those who believe in the beauty of their dreams.", author = "Eleanor Roosevelt" },
            new { quote = "In the midst of winter, I found there was, within me, an invincible summer.", author = "Albert Camus" },
            new { quote = "The only way to do great work is to love what you do.", author = "Steve Jobs" },
            new { quote = "Life is what happens to you while you're busy making other plans.", author = "John Lennon" },
            new { quote = "The way to get started is to quit talking and begin doing.", author = "Walt Disney" },
            new { quote = "Innovation distinguishes between a leader and a follower.", author = "Steve Jobs" },
            new { quote = "Stay hungry. Stay foolish.", author = "Stewart Brand" },
            new { quote = "Be yourself; everyone else is already taken.", author = "Oscar Wilde" }
        };

        var random = new Random();
        var selectedQuote = quotes[random.Next(quotes.Length)];

        return JsonSerializer.Serialize(new
        {
            quote = selectedQuote.quote,
            author = selectedQuote.author,
            category = "inspirational",
            delivered_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_info = new
            {
                user_id = userId,
                access_type = "premium_paid"
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
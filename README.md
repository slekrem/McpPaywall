# McpPaywall

ASP.NET Core middleware for implementing paywalls on Model Context Protocol (MCP) servers with Cashu eCash integration.

## Overview

McpPaywall enables you to monetize MCP server tools through micropayments using the Cashu eCash protocol. Users pay small amounts (via Lightning Network) to access premium MCP tools, providing a privacy-preserving payment solution for AI/ML services.

## Features

- 🔐 **Automatic MCP Protection** - Middleware protects MCP endpoints seamlessly
- ⚡ **Lightning Payments** - Cashu eCash integration for instant micropayments  
- 🛡️ **Token-based Access** - Secure access tokens with configurable expiration
- 🎯 **Plug-and-Play** - Easy integration with existing ASP.NET Core applications
- 📊 **Usage Tracking** - Built-in analytics and user session management
- 🔧 **Extensible** - Support for custom payment providers

## Quick Demo

Try the live demonstration:

```bash
cd demo
./run-demo.sh
```

Visit http://localhost:5000/demo to see McpPaywall in action with real Cashu payments.

## Installation

```bash
dotnet add package McpPaywall.AspNetCore
```

## Basic Usage

### 1. Configure Services

```csharp
builder.Services.AddMcpPaywallWithCashu(
    options =>
    {
        options.DefaultAmount = 10;
        options.DefaultUnit = "sat";
        options.TokenValidityDays = 7;
        options.McpPath = "/mcp";
    },
    cashuOptions =>
    {
        cashuOptions.MintUrl = "https://mint.minibits.cash/Bitcoin";
    });
```

### 2. Add Middleware

```csharp
app.UseMcpPaywall();
```

### 3. Create Protected MCP Tools

```csharp
[McpServerToolType]
public static class PremiumTools
{
    [McpServerTool, Description("Premium weather data")]
    public static async Task<string> GetWeather(string city)
    {
        // Your premium tool implementation
        return JsonSerializer.Serialize(weatherData);
    }
}
```

## Payment Flow

1. **User Access** - Client attempts to use MCP tools
2. **Payment Required** - Middleware returns 401 with payment info
3. **Invoice Creation** - Client requests Lightning invoice via paywall API
4. **Payment** - User pays with Cashu wallet (Lightning Network)
5. **Token Issued** - System generates access token after payment verification
6. **Tool Access** - Client uses token to access protected MCP tools

## API Endpoints

### Paywall API (Public)
- `POST /paywall/create-invoice` - Create payment invoice
- `GET /paywall/check-payment/{quoteId}` - Check payment status
- `GET /paywall/validate-token?token={token}` - Validate access token

### Protected MCP Tools
- `GET /mcp?accessToken={token}` - MCP server endpoint (requires payment)

## Configuration Options

```csharp
public class McpPaywallOptions
{
    public decimal DefaultAmount { get; set; } = 99;
    public string DefaultUnit { get; set; } = "sat";
    public int TokenValidityDays { get; set; } = 7;
    public string McpPath { get; set; } = "/mcp";
    public string BasePath { get; set; } = "/paywall";
    public string Title { get; set; } = "MCP Server Access";
    public bool EnableLogging { get; set; } = true;
}
```

## Use Cases

- **Premium AI Tools** - Charge for advanced model access
- **API Rate Limiting** - Monetize high-value API calls
- **Data Services** - Pay-per-query specialized data feeds
- **Research Tools** - Subscription-free academic API access
- **Computational Resources** - Charge for CPU-intensive operations

## Demo Project

The `demo/` directory contains a complete working example:

- **Real Payments** - Uses actual Cashu eCash (10 sats)
- **Multiple Tools** - Weather, quotes, Cashu mint tools
- **Full Documentation** - Step-by-step usage guide
- **Test Scripts** - Automated payment flow testing

### Demo Features
- Web interface at `/demo`
- Protected MCP tools requiring payment
- Real Lightning Network integration
- User session tracking
- Payment verification

## Security Features

- **Token Expiration** - Configurable access token lifetime
- **Payment Verification** - Cryptographic proof via Cashu protocol
- **User Tracking** - IP-based session management
- **Database Security** - Encrypted token storage
- **Middleware Protection** - Automatic endpoint security

## Architecture

```
Client Request → McpAuthenticationMiddleware → PaywallService → CashuPaymentProvider
                        ↓                           ↓                    ↓
                 Token Validation            Payment Verification    Lightning Invoice
                        ↓                           ↓                    ↓
                  MCP Tools Access           Database Storage        Cashu Mint
```

## Requirements

- .NET 9.0+
- ASP.NET Core
- SQLite (default) or SQL Server
- Cashu wallet for payments

## Contributing

1. Fork the repository
2. Create feature branch
3. Add tests for new functionality
4. Submit pull request

## License

MIT License - see LICENSE file for details.

## Support

- 📖 **Documentation** - Complete guides in `/demo`
- 🐛 **Issues** - GitHub issue tracker
- 💬 **Discussions** - GitHub discussions
- 📧 **Contact** - [Your contact information]

---

**Try it now:** `cd demo && ./run-demo.sh`
# McpPaywall Demo

This demo project showcases the McpPaywall middleware in action, demonstrating how to protect MCP (Model Context Protocol) server tools with Cashu eCash micropayments.

## Overview

The demo creates an MCP server with premium tools that require a small payment (10 satoshis) to access. Users pay via Lightning Network using Cashu eCash for privacy-preserving payments.

## Features Demonstrated

### 🔐 Paywall Protection
- ASP.NET Core middleware automatically protects MCP endpoints
- Payment verification through Cashu eCash protocol
- Access token generation and validation
- Session management with configurable expiration

### 🛠️ Premium MCP Tools
The demo includes several protected MCP tools:

1. **GetCashuMintInfo(mintUrl)** - Comprehensive Cashu mint information
2. **GetCashuKeysets(mintUrl)** - Available keysets and fee structures  
3. **CreateMintQuote(mintUrl, amount, unit)** - Generate Lightning invoices
4. **GetWeather(city)** - Current weather information (simulated)
5. **GetInspirationalQuote()** - Random motivational quotes

### 💰 Payment Flow
1. User attempts to access MCP tools without payment
2. Middleware returns 401 with paywall information
3. User creates payment invoice via paywall API
4. Payment is made through Cashu wallet (Lightning Network)
5. System verifies payment and generates access token
6. User accesses protected tools using token

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- A Cashu wallet (e.g., [Minibits](https://www.minibits.cash/))
- Some satoshis for testing payments

### Running the Demo

1. **Clone and build:**
   ```bash
   cd /path/to/McpPaywall/demo
   dotnet restore
   dotnet build
   ```

2. **Start the server:**
   ```bash
   dotnet run
   ```

3. **Access the demo:**
   - Web Interface: http://localhost:5000/demo
   - Paywall API: http://localhost:5000/demo/paywall
   - MCP Endpoint: http://localhost:5000/demo/mcp (requires payment)

### Testing the Payment Flow

1. **Open the paywall interface:**
   ```
   http://localhost:5000/demo/paywall
   ```

2. **Create an invoice:**
   ```bash
   curl -X POST http://localhost:5000/demo/paywall/create-invoice \
     -H "Content-Type: application/json" \
     -d '{"amount": 10, "unit": "sat", "provider": "cashu"}'
   ```

3. **Check payment status:**
   ```bash
   curl http://localhost:5000/demo/paywall/check-payment/{quoteId}
   ```

4. **Use MCP tools with access token:**
   ```bash
   curl "http://localhost:5000/demo/mcp?accessToken={your-token}"
   ```

## Configuration

The demo is configured in `Program.cs` with the following settings:

```csharp
// Payment Configuration
options.DefaultAmount = 10;           // 10 satoshis
options.DefaultUnit = "sat";
options.TokenValidityDays = 1;        // 24 hour access

// Endpoints
options.McpPath = "/demo/mcp";        // Protected MCP endpoint
options.BasePath = "/demo/paywall";   // Payment API

// Cashu Integration
cashuOptions.MintUrl = "https://mint.minibits.cash/Bitcoin";
cashuOptions.StoreTokens = true;
```

## API Endpoints

### Paywall API (Public)
- `POST /demo/paywall/create-invoice` - Create payment invoice
- `GET /demo/paywall/check-payment/{quoteId}` - Check payment status  
- `GET /demo/paywall/validate-token?token={token}` - Validate access token
- `GET /demo/paywall/statistics` - Payment statistics (admin)
- `POST /demo/paywall/cleanup` - Clean expired records (admin)

### MCP Tools (Protected)
- `GET /demo/mcp?accessToken={token}` - MCP server endpoint

All MCP tools require a valid access token obtained through payment.

## Database

The demo uses SQLite with the database file `demo-paywall.db` created automatically. The database tracks:

- Payment records and quotes
- Access tokens and expiration
- User sessions and usage

## Development

### Adding New Tools

Create new MCP tools by adding methods to the `DemoTools` class:

```csharp
[McpServerTool, Description("Your tool description")]
public static async Task<string> YourNewTool(string parameter,
    [FromServices] IHttpContextAccessor httpContextAccessor)
{
    var context = httpContextAccessor.HttpContext;
    var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
    
    // Your tool implementation
    return JsonSerializer.Serialize(result);
}
```

### Customizing Payment Settings

Modify the paywall configuration in `Program.cs`:

```csharp
builder.Services.AddMcpPaywallWithCashu(
    options =>
    {
        options.DefaultAmount = 50;        // Change amount
        options.DefaultUnit = "sat";       // Change unit
        options.TokenValidityDays = 7;     // Change validity
        options.Title = "Your Title";      // Customize UI
        // ... other options
    },
    cashuOptions =>
    {
        cashuOptions.MintUrl = "your-mint-url";  // Use different mint
        // ... other Cashu options
    });
```

## Testing with MCP Clients

The demo works with any MCP client. Example using the MCP SDK:

```csharp
var client = new McpClient();
await client.ConnectAsync("http://localhost:5000/demo/mcp?accessToken=your-token");

var result = await client.CallToolAsync("GetWeather", new { city = "Berlin" });
Console.WriteLine(result);
```

## Security Notes

- Access tokens expire after the configured period
- Payment verification happens through Cashu mint
- User sessions are tracked by IP address
- Database automatically cleans up expired records

## Troubleshooting

### Common Issues

1. **Payment not confirming:**
   - Check Lightning invoice is actually paid
   - Verify Cashu mint is accessible
   - Check logs for payment processing errors

2. **Access token invalid:**
   - Verify token hasn't expired
   - Check database for payment record
   - Ensure token is passed correctly in query string

3. **MCP tools not working:**
   - Confirm access token is valid
   - Check middleware is properly configured
   - Verify MCP endpoint path is correct

### Logs

The demo provides detailed logging. Check console output for:
- Payment processing steps
- Token validation results
- Tool access attempts
- Error messages

## Production Considerations

When using McpPaywall in production:

1. **Security:**
   - Use HTTPS for all endpoints
   - Implement proper error handling
   - Add rate limiting
   - Secure database access

2. **Monitoring:**
   - Track payment success rates
   - Monitor token usage patterns
   - Log security events
   - Set up alerts for failures

3. **Performance:**
   - Use connection pooling
   - Implement caching where appropriate
   - Optimize database queries
   - Consider horizontal scaling

4. **Compliance:**
   - Ensure payment processing compliance
   - Implement proper data retention
   - Consider privacy regulations
   - Document audit trails

## Contributing

This demo is part of the McpPaywall project. Contributions welcome:

1. Fork the repository
2. Create feature branch
3. Make changes
4. Add tests
5. Submit pull request

## License

This demo is licensed under the same terms as the McpPaywall project (MIT License).
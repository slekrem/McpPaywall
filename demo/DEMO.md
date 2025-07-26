# McpPaywall Live Demo Guide

This guide walks you through the complete McpPaywall demonstration, from setup to testing the payment flow.

## 🚀 Quick Start

### 1. Start the Demo Server

```bash
cd demo
./run-demo.sh
```

The server will start on `http://localhost:5000` with the following endpoints:
- **Web Interface:** http://localhost:5000/demo  
- **Paywall API:** http://localhost:5000/demo/paywall
- **MCP Endpoint:** http://localhost:5000/demo/mcp *(requires payment)*

### 2. Open the Demo Interface

Navigate to http://localhost:5000/demo in your browser to see the demo homepage.

## 💳 Payment Flow Walkthrough

### Step 1: Try Accessing Protected Tools (Will Fail)

First, let's see what happens when you try to access MCP tools without payment:

```bash
curl http://localhost:5000/demo/mcp
```

**Expected Response:** 401 Unauthorized with paywall information

### Step 2: Create a Payment Invoice

Request a Lightning invoice for 10 satoshis:

```bash
curl -X POST http://localhost:5000/demo/paywall/create-invoice \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 10,
    "unit": "sat", 
    "provider": "cashu",
    "description": "Demo access payment"
  }'
```

**Response includes:**
- `quoteId` - Payment identifier
- `paymentRequest` - Lightning invoice to pay
- `amount` and `unit` - Payment details
- `expiresAt` - Invoice expiration time

### Step 3: Pay the Lightning Invoice

Use a Cashu wallet (like [Minibits](https://www.minibits.cash/)) to pay the Lightning invoice from Step 2.

**Recommended Wallets:**
- Minibits (Mobile/Web)
- Cashu.me (Web)
- eNuts (Mobile)

### Step 4: Check Payment Status

Monitor the payment using the quote ID from Step 2:

```bash
curl http://localhost:5000/demo/paywall/check-payment/{QUOTE_ID}
```

**Before Payment:** Status shows "pending"  
**After Payment:** Status shows "paid" with access token

### Step 5: Access Premium Tools

Use the access token to call protected MCP tools:

```bash
# Get Cashu mint information
curl "http://localhost:5000/demo/mcp?accessToken={YOUR_TOKEN}" \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "GetCashuMintInfo",
      "arguments": {
        "mintUrl": "https://mint.minibits.cash/Bitcoin"
      }
    }
  }'
```

## 🧪 Automated Testing

Run the complete test suite:

```bash
./test-api.sh
```

This script:
1. ✅ Tests server connectivity
2. ❌ Attempts unauthorized MCP access (fails as expected)
3. 💳 Creates payment invoice
4. 🔍 Checks payment status
5. 🔐 Tests token validation
6. 📋 Shows MCP endpoint structure

## 🛠️ Available Demo Tools

### Cashu Tools
- **GetCashuMintInfo(mintUrl)** - Detailed mint information
- **GetCashuKeysets(mintUrl)** - Available keysets and fees
- **CreateMintQuote(mintUrl, amount, unit)** - Generate Lightning invoices

### Utility Tools  
- **GetWeather(city)** - Weather information (simulated)
- **GetInspirationalQuote()** - Random motivational quotes

### Tool Usage Examples

```bash
# Weather information
curl "http://localhost:5000/demo/mcp?accessToken={TOKEN}" \
  -X POST -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0", "id": 1,
    "method": "tools/call",
    "params": {
      "name": "GetWeather",
      "arguments": {"city": "Berlin"}
    }
  }'

# Inspirational quote
curl "http://localhost:5000/demo/mcp?accessToken={TOKEN}" \
  -X POST -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0", "id": 1,
    "method": "tools/call", 
    "params": {"name": "GetInspirationalQuote", "arguments": {}}
  }'
```

## 🔧 Configuration Details

The demo uses these settings (configured in `Program.cs`):

```csharp
// Payment Settings
DefaultAmount = 10 satoshis
TokenValidityDays = 1 day
DefaultUnit = "sat"

// Endpoints
McpPath = "/demo/mcp"
BasePath = "/demo/paywall"

// Cashu Integration
MintUrl = "https://mint.minibits.cash/Bitcoin"
StoreTokens = true
```

## 💾 Database Inspection

The demo creates a SQLite database `demo-paywall.db`. You can inspect it:

```bash
# Install sqlite3 if needed
# macOS: brew install sqlite
# Ubuntu: sudo apt install sqlite3

# View payment records
sqlite3 demo-paywall.db "SELECT * FROM PaymentRecords;"

# Check active sessions
sqlite3 demo-paywall.db "
  SELECT QuoteId, IsPaid, ExpiresAt, Amount, Unit, Provider 
  FROM PaymentRecords 
  WHERE datetime(ExpiresAt) > datetime('now');
"
```

## 🔍 Monitoring and Logs

The demo provides detailed console logging:

```
[MCP] User abc123 (192.168.1.100) called GetCashuMintInfo for https://mint.minibits.cash/Bitcoin
[McpPaywall] User abc123 (192.168.1.100) accessing MCP endpoint /demo/mcp
[DEMO] Paid user abc123 (192.168.1.100) requesting mint info for: https://mint.minibits.cash/Bitcoin
```

**Log Categories:**
- `[McpPaywall]` - Middleware authentication events
- `[MCP]` - MCP tool invocations  
- `[DEMO]` - Demo-specific logging

## 🚫 Testing Error Scenarios

### Expired Tokens
Wait 24 hours or manually expire tokens in database to test expiration handling.

### Invalid Tokens
```bash
curl "http://localhost:5000/demo/mcp?accessToken=invalid-token-123"
# Expected: 401 Unauthorized
```

### Payment Failures
Try creating invoices with invalid amounts or units to test error handling.

## 📊 Admin Endpoints

Monitor system health and usage:

```bash
# Payment statistics
curl http://localhost:5000/demo/paywall/statistics

# Clean up expired records
curl -X POST http://localhost:5000/demo/paywall/cleanup

# Validate specific token
curl "http://localhost:5000/demo/paywall/validate-token?token={TOKEN}"
```

## 🔧 Customization Examples

### Change Payment Amount

Edit `Program.cs`:
```csharp
options.DefaultAmount = 100; // Change to 100 sats
```

### Add Custom Tools

Add to `DemoTools` class:
```csharp
[McpServerTool, Description("Your custom tool")]
public static async Task<string> YourCustomTool(string parameter,
    [FromServices] IHttpContextAccessor httpContextAccessor)
{
    // Implementation
}
```

### Use Different Mint

Change mint in configuration:
```csharp
cashuOptions.MintUrl = "https://your-preferred-mint.com";
```

## 🌐 Integration with MCP Clients

The demo works with standard MCP clients. Example using cURL to mimic MCP client:

```bash
# List available tools
curl "http://localhost:5000/demo/mcp?accessToken={TOKEN}" \
  -X POST -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'

# Get tool schema  
curl "http://localhost:5000/demo/mcp?accessToken={TOKEN}" \
  -X POST -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0","id":1,
    "method":"tools/get",
    "params":{"name":"GetWeather"}
  }'
```

## 🏁 Demo Completion Checklist

- [ ] Server starts successfully
- [ ] Web interface loads at /demo
- [ ] Unauthorized access properly blocked
- [ ] Payment invoice created successfully
- [ ] Lightning invoice paid with Cashu wallet
- [ ] Access token received after payment
- [ ] MCP tools accessible with token
- [ ] Tools return expected data
- [ ] Token validation works
- [ ] Logs show user activity

## 🆘 Troubleshooting

**Server won't start:**
- Check .NET 9.0 SDK is installed
- Verify all dependencies restored
- Check port 5000 isn't in use

**Payment not confirming:**
- Verify Lightning invoice was actually paid
- Check Cashu mint is accessible
- Wait a few seconds for confirmation

**Tools not accessible:**
- Confirm token hasn't expired (24 hours)
- Check token is passed in query string correctly
- Verify database contains payment record

**Database errors:**
- Delete `demo-paywall.db` to reset
- Check file permissions
- Ensure SQLite is available

This demo showcases the complete McpPaywall functionality in a realistic scenario with real payments and actual MCP tool protection.
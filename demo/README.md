# 🎮 McpPaywall Interactive Demo

**Live Demo**: [sup3r.cool/paywall](https://sup3r.cool/paywall)

Experience the complete McpPaywall payment flow with real Lightning Network payments and Cashu eCash privacy.

## 🌟 What This Demo Shows

### 4-Step Interactive Experience:

1. **🚫 Access Attempt** - Try accessing MCP tools without payment
2. **💳 Create Invoice** - Generate Lightning payment invoice (10 sats)
3. **⚡ Pay with Cashu** - Use eCash wallet for private payment
4. **🛠️ Use Premium Tools** - Access paid MCP resources

### Demo Features:

- **Real Lightning Payments** (10 satoshis ≈ $0.003)
- **Cashu eCash Privacy** (anonymous, unlinkable payments)
- **QR Code Support** (scan with mobile wallets)
- **Auto Payment Detection** (5-second polling)
- **MCP Connection URL** (copy for AI clients)
- **Interactive Tool Testing** (weather, passwords, etc.)

## 🛠️ Available MCP Resources

After payment, explore these premium tools:

### Tools
- **GetWeather** - Simulated weather data for any city
- **GeneratePassword** - Secure password generation (customizable length)
- **CalculateHash** - SHA256 hash calculation for text input

### Prompts  
- **EmailTemplate** - Professional email template generation
- **MeetingAgenda** - Structured meeting agenda creation

### Resources
- **ProductivityTips** - Curated productivity advice
- **DevTools** - Essential development tool recommendations

## 💳 Supported Wallets

Pay the Lightning invoice with any Cashu-compatible wallet:

- **[Minibits](https://minibits.cash)** - Mobile Cashu wallet (iOS/Android)
- **[Cashu.me](https://cashu.me)** - Web-based Cashu wallet
- **[eNuts](https://enuts.cash)** - Cross-platform Cashu wallet
- **Any Lightning wallet** - Standard Lightning Network support

## 🔗 AI Client Integration

After payment, you'll get an MCP connection URL like:

```
https://sup3r.cool/paywall/mcp/sse?accessToken=abc123...
```

### Use with:
- **Claude Desktop** - Add to MCP settings
- **Continue.dev** - Configure as MCP server
- **Any MCP Client** - Standard Model Context Protocol

## 🏃‍♂️ Run Demo Locally

### Prerequisites:
- .NET 8.0 or later
- Git

### Steps:

```bash
# Clone the repository
git clone https://github.com/slekrem/McpPaywall.git
cd McpPaywall/demo

# Run the demo
dotnet run

# Visit in browser
open http://localhost:5001/paywall
```

### Local Configuration:

The demo uses these settings:
- **Amount**: 10 satoshis (≈ $0.003)
- **Validity**: 24 hours
- **Cashu Mint**: `https://stablenut.cashu.network`
- **Database**: SQLite (`demo-paywall.db`)

## 🔧 Demo Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Web Browser   │───▶│   Demo Server    │───▶│   MCP Tools     │
│  (Interactive)  │    │ (ASP.NET Core)   │    │ (After Payment) │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌──────────────────┐
                       │  Cashu Mint      │
                       │  (stablenut)     │
                       └──────────────────┘
```

### Key Components:

- **Frontend**: Bootstrap 5 + Vanilla JavaScript
- **Backend**: ASP.NET Core with McpPaywall middleware
- **Payments**: Cashu eCash via DotNut library
- **Database**: SQLite for payment records
- **MCP Server**: Model Context Protocol implementation

## 📊 Demo Analytics

The demo tracks:
- Payment attempts & successes
- Tool usage statistics  
- User access patterns
- Performance metrics

## 🛡️ Security Features

- **Token Validation** - Secure access tokens with expiration
- **Rate Limiting** - Prevents abuse of payment endpoints
- **HTTPS Only** - All payments over encrypted connections
- **Privacy First** - Cashu eCash ensures payment anonymity

## 🎯 Business Model Examples

This demo showcases various monetization strategies:

### Pay-per-Use
- Weather API calls
- Password generation
- Hash calculations

### Time-based Access
- 24-hour tool access
- Daily/weekly subscriptions
- Premium feature unlocking

### Content Gating
- Productivity guides
- Developer resources
- Curated tool lists

## 🔄 Demo Flow Details

### Step 1: Access Attempt
```http
GET /paywall/mcp/sse
Response: 401 Unauthorized
```

### Step 2: Create Invoice
```http
POST /paywall/paywall/create-invoice
{
  "amount": 10,
  "unit": "sat",
  "description": "McpPaywall Demo Access"
}
```

### Step 3: Payment Check
```http
GET /paywall/paywall/check-payment/{quoteId}
Response: {
  "paid": true,
  "accessToken": "eyJ0eXAiOiJKV1QiLCJ...",
  "expiresAt": "2024-01-21T12:00:00Z"
}
```

### Step 4: MCP Access
```http
GET /paywall/mcp?accessToken=eyJ0eXAiOiJKV1Qi...
Response: MCP capabilities and tools
```

## 🆘 Troubleshooting

### Common Issues:

**Payment not detected?**
- Wait 10-15 seconds for confirmation
- Check wallet transaction history
- Refresh payment status

**MCP connection fails?**
- Verify access token is valid
- Check token expiration time
- Ensure correct MCP endpoint URL

**Demo won't load?**
- Check internet connection
- Verify .NET 8.0 is installed
- Try different browser

## 🤝 Contributing

Want to improve the demo?

1. Fork the repository
2. Create feature branch
3. Make your changes
4. Submit pull request

### Ideas for contributions:
- Additional MCP tools
- Payment provider integrations
- UI/UX improvements
- Performance optimizations

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/slekrem/McpPaywall/issues)
- **Discussions**: [GitHub Discussions](https://github.com/slekrem/McpPaywall/discussions)
- **Discord**: [Cashu Community](https://discord.gg/cashu)

---

**🎉 Enjoy exploring the future of AI tool monetization with privacy-preserving payments!**

[![Try Demo](https://img.shields.io/badge/🎮_Try_Live_Demo-sup3r.cool/paywall-blue?style=for-the-badge)](https://sup3r.cool/paywall)
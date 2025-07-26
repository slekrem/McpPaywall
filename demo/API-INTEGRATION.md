# API-Integration für McpPaywall Demo

## Korrigierte API-Endpunkte

Die index.html verwendet jetzt die korrekten API-Endpunkte:

### 1. Create Invoice
- **URL**: `/demo/Paywall/create-invoice`
- **Method**: `POST`
- **Request Body**:
```json
{
  "amount": 10,
  "unit": "sat", 
  "description": "McpPaywall Demo Access"
}
```
- **Response**:
```json
{
  "quote": "quote-id-string",
  "request": "lightning-invoice-string",
  "amount": 10,
  "unit": "sat",
  "provider": "cashu",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

### 2. Check Payment
- **URL**: `/demo/Paywall/check-payment/{quoteId}`
- **Method**: `GET`
- **Response**:
```json
{
  "state": "PAID",
  "paid": true,
  "accessToken": "access-token-string",
  "mcpLink": "http://localhost:5000/demo/mcp?accessToken=...",
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

## JavaScript-Änderungen

### ✅ Korrigierte URL-Pfade
- Von `/demo/paywall/` zu `/demo/Paywall/` (Controller-Name groß geschrieben)

### ✅ Korrigierte Request-Struktur  
- Entfernt: `provider` (wird automatisch auf "cashu" gesetzt)
- Beibehalten: `amount`, `unit`, `description`

### ✅ Korrigierte Response-Verarbeitung
- `data.quoteId` → `data.quote`
- `data.paymentRequest` → `data.request`
- `data.isPaid || data.paid` → `data.paid`

## Demo-Flow

1. **Schritt 1**: Zugang ohne Token testen → 401 Unauthorized
2. **Schritt 2**: Invoice erstellen → Lightning-Rechnung + QR-Code
3. **Schritt 3**: Zahlung durchführen → Auto-Polling alle 5 Sekunden
4. **Schritt 4**: Tools nutzen → MCP-Aufrufe mit Access Token

## Testen

```bash
cd demo
./run-demo.sh
```

Dann auf http://localhost:5000/demo gehen und den interaktiven Demo-Flow durchlaufen.

## API-Debugging

Falls Probleme auftreten, können Sie die APIs direkt testen:

```bash
# 1. Invoice erstellen
curl -X POST http://localhost:5000/demo/Paywall/create-invoice \
  -H "Content-Type: application/json" \
  -d '{"amount": 10, "unit": "sat", "description": "Test"}'

# 2. Payment status prüfen  
curl http://localhost:5000/demo/Paywall/check-payment/{QUOTE_ID}

# 3. MCP-Zugang ohne Token (sollte 401 geben)
curl http://localhost:5000/demo/mcp

# 4. MCP-Zugang mit Token
curl "http://localhost:5000/demo/mcp?accessToken={ACCESS_TOKEN}"
```
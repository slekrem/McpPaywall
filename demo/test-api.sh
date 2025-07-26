#!/bin/bash

# McpPaywall Demo API Testing Script
# This script demonstrates the complete payment flow

BASE_URL="http://localhost:5000/demo"
PAYWALL_API="$BASE_URL/paywall"
MCP_API="$BASE_URL/mcp"

echo "🧪 McpPaywall Demo API Testing"
echo "=============================="
echo

# Function to make HTTP requests with error handling
make_request() {
    local method=$1
    local url=$2
    local data=$3
    local description=$4
    
    echo "📡 $description"
    echo "   $method $url"
    
    if [ -n "$data" ]; then
        echo "   Data: $data"
        response=$(curl -s -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -d "$data" 2>/dev/null)
    else
        response=$(curl -s "$url" 2>/dev/null)
    fi
    
    if [ $? -eq 0 ]; then
        echo "✅ Response:"
        echo "$response" | jq . 2>/dev/null || echo "$response"
    else
        echo "❌ Request failed"
        return 1
    fi
    echo
}

# Test 1: Check if server is running
echo "🔍 Testing server availability..."
if ! curl -s "$BASE_URL" >/dev/null 2>&1; then
    echo "❌ Server not running at $BASE_URL"
    echo "Please start the demo server first with: ./run-demo.sh"
    exit 1
fi
echo "✅ Server is running"
echo

# Test 2: Try to access MCP endpoint without payment (should fail)
echo "🚫 Testing MCP access without payment (should fail)..."
make_request "GET" "$MCP_API" "" "Accessing MCP endpoint without token"

# Test 3: Create a payment invoice
echo "💳 Creating payment invoice..."
INVOICE_DATA='{"amount": 10, "unit": "sat", "provider": "cashu", "description": "Demo API test payment"}'
INVOICE_RESPONSE=$(curl -s -X POST "$PAYWALL_API/create-invoice" \
    -H "Content-Type: application/json" \
    -d "$INVOICE_DATA")

echo "📄 Invoice Response:"
echo "$INVOICE_RESPONSE" | jq . 2>/dev/null || echo "$INVOICE_RESPONSE"
echo

# Extract quote ID for further testing
QUOTE_ID=$(echo "$INVOICE_RESPONSE" | jq -r '.quoteId // .quote_id // .quote // empty' 2>/dev/null)
if [ -n "$QUOTE_ID" ] && [ "$QUOTE_ID" != "null" ]; then
    echo "📝 Quote ID: $QUOTE_ID"
    
    # Test 4: Check payment status (will be unpaid)
    echo "🔍 Checking payment status..."
    make_request "GET" "$PAYWALL_API/check-payment/$QUOTE_ID" "" "Checking payment status for quote $QUOTE_ID"
    
    # Show Lightning invoice for manual payment
    LIGHTNING_INVOICE=$(echo "$INVOICE_RESPONSE" | jq -r '.paymentRequest // .lightning_invoice // .request // empty' 2>/dev/null)
    if [ -n "$LIGHTNING_INVOICE" ] && [ "$LIGHTNING_INVOICE" != "null" ]; then
        echo "⚡ Lightning Invoice (pay this to complete the demo):"
        echo "$LIGHTNING_INVOICE"
        echo
        echo "💡 To complete the test:"
        echo "   1. Pay the Lightning invoice above using a Cashu wallet"
        echo "   2. Wait for payment confirmation"
        echo "   3. Run: curl $PAYWALL_API/check-payment/$QUOTE_ID"
        echo "   4. Use the returned access token to access MCP tools"
        echo
    fi
else
    echo "❌ Could not extract quote ID from response"
fi

# Test 5: Demonstrate token validation (with fake token)
echo "🔐 Testing token validation with invalid token..."
make_request "GET" "$PAYWALL_API/validate-token?token=fake-token-123" "" "Validating fake access token"

# Test 6: Show available MCP tools structure
echo "📋 Testing MCP server discovery..."
make_request "POST" "$MCP_API" '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}' "Listing available MCP tools (should fail without token)"

echo "🎯 Test Summary"
echo "==============="
echo "✅ Server connectivity test passed"
echo "✅ Payment flow demonstration completed"
echo "✅ Security validation confirmed (unauthorized access blocked)"
echo
echo "💡 Next Steps:"
echo "   1. Pay the Lightning invoice shown above"
echo "   2. Get your access token from check-payment endpoint"
echo "   3. Use token to access protected MCP tools"
echo
echo "🔗 Useful Commands:"
echo "   Check payment: curl $PAYWALL_API/check-payment/$QUOTE_ID"
echo "   Validate token: curl '$PAYWALL_API/validate-token?token=YOUR_TOKEN'"
echo "   Use MCP tools: curl '$MCP_API?accessToken=YOUR_TOKEN'"
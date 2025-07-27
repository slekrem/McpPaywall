using DotNut;
using DotNut.Api;
using DotNut.ApiModels;
using Microsoft.Extensions.Options;
using NBitcoin.Secp256k1;
using System.Security.Cryptography;
using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Services;

/// <summary>
/// Cashu eCash payment provider implementation
/// </summary>
public class CashuPaymentProvider : IPaymentProvider
{
    private readonly CashuPaymentOptions _options;
    private readonly ILogger<CashuPaymentProvider> _logger;

    public string ProviderName => "cashu";

    public CashuPaymentProvider(IOptions<CashuPaymentOptions> options, ILogger<CashuPaymentProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CreateInvoiceResult> CreateInvoiceAsync(decimal amount, string unit, string? description = null)
    {
        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(_options.MintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);

            var request = new PostMintQuoteBolt11Request
            {
                Amount = (ulong)amount,
                Unit = unit,
                Description = description
            };

            var response = await cashuClient.CreateMintQuote<PostMintQuoteBolt11Response, PostMintQuoteBolt11Request>("bolt11", request);

            return new CreateInvoiceResult
            {
                QuoteId = response.Quote,
                PaymentRequest = response.Request,
                Amount = amount,
                Unit = unit,
                ExpiresAt = response.Expiry.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)response.Expiry.Value).DateTime : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Cashu invoice for amount {Amount} {Unit}", amount, unit);
            throw;
        }
    }

    public async Task<PaymentStatusResult> CheckPaymentAsync(string quoteId)
    {
        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(_options.MintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);

            var response = await cashuClient.CheckMintQuote<PostMintQuoteBolt11Response>("bolt11", quoteId);

            var status = response.State switch
            {
                "PAID" => PaymentStatus.Paid,
                "PENDING" => PaymentStatus.Pending,
                "EXPIRED" => PaymentStatus.Expired,
                _ => PaymentStatus.Failed
            };

            return new PaymentStatusResult
            {
                Status = status,
                Amount = response.Amount,
                Metadata = new Dictionary<string, object>
                {
                    ["cashu_state"] = response.State ?? "unknown",
                    ["cashu_expiry"] = response.Expiry ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Cashu payment status for quote {QuoteId}", quoteId);
            return new PaymentStatusResult { Status = PaymentStatus.Failed };
        }
    }

    public async Task<ClaimTokenResult?> ClaimTokenAsync(string quoteId, decimal amount, string unit)
    {
        if (!_options.StoreTokens)
            return null;

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(_options.MintUrl) };
            var cashuClient = new CashuHttpClient(httpClient);

            // Get keysets and generate blinded outputs
            var keysets = await cashuClient.GetKeysets();
            var keyset = keysets.Keysets.FirstOrDefault(k => k.Active && k.Unit == unit)
                ?? keysets.Keysets.First(k => k.Active);
            var keys = await cashuClient.GetKeys(keyset.Id);
            var standardAmounts = keys.Keysets.First().Keys.Select(x => x.Key).ToArray();

            var outputs = new List<BlindedMessage>();
            var amounts = new List<ulong>();
            var remaining = (ulong)amount;

            // Split amount into standard denominations
            for (int i = standardAmounts.Length - 1; i >= 0; i--)
            {
                while (remaining >= standardAmounts[i])
                {
                    amounts.Add(standardAmounts[i]);
                    remaining -= standardAmounts[i];
                }
            }

            var blindingFactors = new List<ECPrivKey>();
            var secrets = new List<StringSecret>();

            foreach (var amt in amounts)
            {
                var secret = new StringSecret(Guid.NewGuid().ToString());
                var blindingFactor = ECPrivKey.Create(RandomNumberGenerator.GetBytes(32));
                blindingFactors.Add(blindingFactor);
                secrets.Add(secret);

                var Y = secret.ToCurve();
                var B_ = Cashu.ComputeB_(Y, blindingFactor);

                var blindedMessage = new BlindedMessage
                {
                    Amount = amt,
                    Id = keyset.Id,
                    B_ = new PubKey(Convert.ToHexString(B_.ToBytes(true))),
                    Witness = null
                };

                outputs.Add(blindedMessage);
            }

            // Mint the tokens
            var mintRequest = new PostMintBolt11Request
            {
                Quote = quoteId,
                Outputs = [.. outputs]
            };

            var mintResponse = await cashuClient.Mint<PostMintBolt11Request, PostMintBolt11Response>("bolt11", mintRequest);

            // Create proofs
            var proofs = new List<Proof>();
            for (int i = 0; i < mintResponse.Signatures.Length; i++)
            {
                var signature = mintResponse.Signatures[i];
                var mintPubKey = keys.Keysets.First().Keys[signature.Amount];
                var C = Cashu.ComputeC(signature.C_, blindingFactors[i], mintPubKey);

                if (signature.DLEQ != null)
                    signature.DLEQ.R = blindingFactors[i];

                var proof = new Proof
                {
                    Amount = signature.Amount,
                    Id = signature.Id,
                    Secret = secrets[i],
                    C = new PubKey(Convert.ToHexString(C.ToBytes(true))),
                    DLEQ = signature.DLEQ,
                    Witness = null
                };

                proofs.Add(proof);
            }

            // Encode token
            var token = new CashuToken.Token
            {
                Mint = _options.MintUrl,
                Proofs = proofs
            };

            var cashuToken = new CashuToken
            {
                Tokens = [token],
                Unit = unit,
                Memo = "MCP Paywall Token"
            };

            var encodedToken = CashuTokenHelper.Encode(cashuToken, "B", false);

            _logger.LogInformation("Successfully claimed Cashu token for quote {QuoteId}", quoteId);

            return new ClaimTokenResult
            {
                Success = true,
                TokenData = encodedToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to claim Cashu token for quote {QuoteId}", quoteId);
            return new ClaimTokenResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
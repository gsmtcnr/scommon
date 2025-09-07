using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using scommon.Utils.Helpers;

namespace scommon;

public class EuroMessageMailSender : IMailSender
{
    private static readonly string EURO_MESSAGE_TOKEN_CACHE_KEY = "EURO_MESSAGE_TOKEN_CACHE_KEY";

    private ICacheManager _cacheManager;
    private readonly EuroMessageOptions _options;

    public EuroMessageMailSender(IOptions<EuroMessageOptions> options)
    {
        _options = options.Value;
    }

    public void SetCacheManagerAsync(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public class EuroMessageMailOutput : IMailOutput
    {
        public bool IsSuccess { get; set; }
        public bool IsContainsWebhook { get; set; } = false;
        public string? Id { get; set; } // TODO : true geldi�inde ekleme yap�labilinir.
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
    }

    public async Task<IMailOutput> SendAsync(IMailInput input)
    {
        var tokenResponse =  CreateToken().GetAwaiter().GetResult();
        if (tokenResponse != null)
        {
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.TokenValue);

                var url = $"{_options.BaseUrl}/accounts/{tokenResponse.AccountId}/transactional-email";

                var request = new EuroMessageRequest(tokenResponse.AccountId!,
                    input.Subject,
                    input.Message,
                    input.Email,
                    Convert.ToInt32(_options.SenderProfileId));

                var serializedRequest = JsonSerializer.Serialize(request);
                var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, requestContent);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return new EuroMessageMailOutput
                    {
                        Id = Guid.NewGuid().ToString(), //TODO:Euromessage dökümantasyona bakılacak.
                        IsSuccess = true
                    };
                }

                var responseString = await response.Content.ReadAsStringAsync();
                return new EuroMessageMailOutput
                {
                    IsSuccess = false,
                    ErrorMessage = responseString
                };
            }
        }

        return new EuroMessageMailOutput
        {
            IsSuccess = false,
            ErrorCode = 0,
            ErrorMessage = "token not created"
        };
    }
    
    private async Task<EuroMessageTokenResponse> CreateToken()
    {
        if (_cacheManager != null)
        {
            var cachedResult = await _cacheManager.Get<EuroMessageTokenResponse>(EURO_MESSAGE_TOKEN_CACHE_KEY);
            if (cachedResult != null && !string.IsNullOrEmpty(cachedResult.AccountId))
            {
                var tokenExpiryDate = Convert.ToDateTime(cachedResult.ExpireTime);
                if (tokenExpiryDate > DateHelper.Now())
                {
                    return cachedResult;
                }
            }
        }

        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12
        };

        using (var client = new HttpClient(handler))
        {
            var request = new EuroMessageTokenRequest
            {
                Email = _options.Email,
                Password = _options.Password
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var serializedRequest = JsonSerializer.Serialize(request);
            var url = _options.BaseUrl + "/tokens";

            var httpContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            var responseString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var tokenResponse = JsonSerializer.Deserialize<EuroMessageTokenResponse>(responseString);
                if (tokenResponse != null)
                {
                    await _cacheManager.Set(EURO_MESSAGE_TOKEN_CACHE_KEY, tokenResponse, TimeSpan.FromMinutes(10));
                    return tokenResponse;
                }
            }

            Console.WriteLine($"Mail token error : {responseString}");
        }

        return default(EuroMessageTokenResponse)!;
    }
}

public class EuroMessageTokenRequest
{
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
}

public class EuroMessageTokenResponse
{
    [JsonPropertyName("accountId")] public string? AccountId { get; set; }
    [JsonPropertyName("tokenValue")] public string? TokenValue { get; set; }
    [JsonPropertyName("expireTime")] public string? ExpireTime { get; set; }
}

public class EuroMessageRequest
{
    public EuroMessageRequest(string accountId, string subject, string content, string receiverEmailAddress, int senderProfileId)
    {
        AccountId = accountId;
        Subject = subject;
        Content = content;
        ReceiverEmailAddress = receiverEmailAddress;
        SenderProfileId = senderProfileId;
    }

    [JsonPropertyName("accountId")] public string AccountId { get; private set; }
    [JsonPropertyName("subject")] public string Subject { get; private set; }
    [JsonPropertyName("content")] public string Content { get; private set; }

    [JsonPropertyName("receiverEmailAddress")]
    public string ReceiverEmailAddress { get; private set; }

    [JsonPropertyName("senderProfileId")] public int SenderProfileId { get; private set; }
}

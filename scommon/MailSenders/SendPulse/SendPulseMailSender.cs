using System.Globalization;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;
using scommon.Utils.Helpers;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using scommon.Utils.Constants;

namespace scommon.SendPulse;

public class SendPulseMailSender : IMailSender
{
    private static readonly string SEND_PULSE_TOKEN_CACHE_KEY = "SEND_PULSE_MAIL_TOKEN_CACHE_KEY";
    private static readonly string CLIENT_CREDENTIALS = "client_credentials";

    private ICacheManager _cacheManager;
    private readonly ILogger<SendPulseMailSender> _logger;
    private readonly SendPulseOptions _options;

    public SendPulseMailSender(IOptions<SendPulseOptions> options, ILogger<SendPulseMailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public void SetCacheManagerAsync(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public class SendPulseMailOutput : IMailOutput
    {
        public bool IsContainsWebhook { get; set; } = true;
        public string? Id { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
    }

    public class SendPulseTokenResponse
    {
        [JsonPropertyName("token_type")] public string? TokenType { get; set; }
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
    }

    public class SendPulseResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("result")] public bool Result { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("error_code")] public int ErrorCode { get; set; }
    }

    public class SendPulseTokenCacheObject
    {
        public string? Token { get; set; }
        public string? ExpireTime { get; set; }
    }

    public class SendPulseRequest
    {
        public SendPulseRequest()
        {
        }

        [JsonPropertyName("email")] public EmailBase Email { get; set; }

        public SendPulseRequest(string subject, string fromName, string fromEmail, string toEmail, string htmlContent)
        {
            Email = new EmailBase()
            {
                Subject = subject,
                From = new EmailBase.Sender
                {
                    Name = fromName,
                    Email = fromEmail
                },
                To = new[]
                {
                    new EmailBase.Recipient
                    {
                        Email = toEmail
                    }
                },
                Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlContent))
            };
        }

        public class EmailBase
        {
            [JsonPropertyName("subject")] public string? Subject { get; set; }
            [JsonPropertyName("from")] public Sender? From { get; set; }
            [JsonPropertyName("to")] public Recipient[]? To { get; set; }
            [JsonPropertyName("html")] public string? Html { get; set; }

            public class Sender
            {
                [JsonPropertyName("name")] public string? Name { get; set; }
                [JsonPropertyName("email")] public string? Email { get; set; }
            }

            public class Recipient
            {
                [JsonPropertyName("email")] public string? Email { get; set; }
            }
        }
    }

    public async Task<IMailOutput> SendAsync(IMailInput input)
    {
        var tokenResponse = await CreateToken();
        if (tokenResponse != null)
        {
            var requestBody = new SendPulseRequest(input.Subject, _options.FromName, _options.FromMail, input.Email, input.Message);
            var requestJsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(
                requestJsonBody,
                Encoding.UTF8,
                CommonMediaTypeConstants.APPLICATION_JSON_MEDIA_TYPE
            );
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
            var response = await client.PostAsync(_options.ApiUrl + "smtp/emails", content);
            var readAsString = await response.Content.ReadAsStringAsync();
            var sendPulseResponse = JsonSerializer.Deserialize<SendPulseResponse>(readAsString);
            if (sendPulseResponse is null)
            {
                _logger.LogError("[SendPulseMailSender] response is null - {readAsString}", readAsString);
                return new SendPulseMailOutput
                {
                    IsSuccess = false,
                    ErrorCode = 0,
                    ErrorMessage = "response is null"
                };
            }

            if (response.StatusCode == HttpStatusCode.OK && sendPulseResponse.Result)
            {
                return new SendPulseMailOutput
                {
                    Id = sendPulseResponse.Id,
                    IsSuccess = true
                };
            }

            _logger.LogError("[SendPulseMailSender] response return error - {readAsString}", readAsString);

            return new SendPulseMailOutput
            {
                IsSuccess = false,
                ErrorCode = sendPulseResponse?.ErrorCode,
                ErrorMessage = sendPulseResponse?.Message
            };
        }

        return new SendPulseMailOutput
        {
            IsSuccess = false,
            ErrorCode = 0,
            ErrorMessage = "token not created"
        };
    }


    private async Task<SendPulseTokenCacheObject> CreateToken()
    {
        if (_cacheManager != null)
        {
            var cachedResult = await _cacheManager.Get<SendPulseTokenCacheObject>(SEND_PULSE_TOKEN_CACHE_KEY);
            if (cachedResult != null)
            {
                var tokenExpiryDate = Convert.ToDateTime(cachedResult.ExpireTime);
                if (tokenExpiryDate > DateHelper.Now())
                {
                    return cachedResult;
                }
            }
        }

        using var client = new HttpClient();
        var requestBody = new
        {
            grant_type = CLIENT_CREDENTIALS,
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, CommonMediaTypeConstants.APPLICATION_JSON_MEDIA_TYPE);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CommonMediaTypeConstants.APPLICATION_JSON_MEDIA_TYPE));
        var response = await client.PostAsync(_options.ApiUrl + "oauth/access_token", requestContent);

        var responseString = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            try
            {
                var tokenResponse = JsonSerializer.Deserialize<SendPulseTokenResponse>(responseString);
                if (tokenResponse != null)
                {
                    var cacheObject = new SendPulseTokenCacheObject
                    {
                        ExpireTime = DateHelper.Now().AddMinutes(45).ToString(CultureInfo.InvariantCulture),
                        Token = tokenResponse.AccessToken
                    };

                    await _cacheManager!.Set(SEND_PULSE_TOKEN_CACHE_KEY, cacheObject, TimeSpan.FromMinutes(1));
                    return cacheObject;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        Console.WriteLine($"Mail token error : {responseString}");

        return default(SendPulseTokenCacheObject)!;
    }
}

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using scommon.Auths;

namespace scommon.Proxies;

public interface IServiceProxy
{
    Task<TResponse> GetAsync<TResponse>(string uri, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> PostAsync<TResponse>(string uri, object payload, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> PostAsync<TResponse>(string uri, string token, object payload, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> PutAsync<TResponse>(string uri, object payload, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> PutAsync<TResponse>(string uri, string token, object payload, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> DeleteAsync<TResponse>(string uri, CancellationToken ct) where TResponse : class, new();
    Task<TResponse> DeleteAsync<TResponse>(string uri, string token, CancellationToken ct) where TResponse : class, new();
}

public abstract class BaseServiceProxy : IServiceProxy
{
    private readonly ServiceProxyInformation _serviceProxyInformation;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICurrentUser _currentUser;
    protected abstract ServiceProxyInformation SetInformation(IConfiguration configuration);

    protected BaseServiceProxy(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ICurrentUser currentUser)
    {
        _serviceProxyInformation = SetInformation(configuration);
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    protected BaseServiceProxy(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProxyInformation = SetInformation(configuration);
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient(_serviceProxyInformation.Name);
        string authorizationValue = _httpContextAccessor?.HttpContext?.Request?.Headers?["Authorization"]!;
        if (!string.IsNullOrEmpty(authorizationValue) && _currentUser != null)
        {
            httpClient.DefaultRequestHeaders.Add("X-User", _currentUser.Id.ToString());
        }

        string traceId = _httpContextAccessor?.HttpContext?.Request?.Headers?["X-Trace-Id"]!;
        if (string.IsNullOrEmpty(traceId))
        {
            traceId = Guid.NewGuid().ToString();
        }

        httpClient.DefaultRequestHeaders.Add("X-Trace-Id", traceId);
        httpClient.BaseAddress = new Uri(_serviceProxyInformation.BaseAddress);
        httpClient.Timeout = TimeSpan.FromSeconds(_serviceProxyInformation.DefaultTimeout);
        return httpClient;
    }

    public virtual async Task<TResponse> GetAsync<TResponse>(string uri, CancellationToken ct) where TResponse : class, new()
    {
        var httpClient = CreateHttpClient();


        var response = await httpClient.GetAsync(uri, ct);
        //
        // if (!response.IsSuccessStatusCode) return new TResponse();

        var responseData = await response.Content.ReadAsStringAsync(ct);

        //httpClient.Dispose();

        return JsonSerializer.Deserialize<TResponse>(responseData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public virtual async Task<TResponse> PostAsync<TResponse>(string uri, object payload, CancellationToken ct) where TResponse : class, new()
    {
        return await PostAsync<TResponse>(uri, string.Empty, payload, ct);
    }

    public virtual async Task<TResponse> PostAsync<TResponse>(string uri, string token, object payload, CancellationToken ct) where TResponse : class, new()
    {
        var httpClient = CreateHttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var serializedPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(serializedPayload, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(uri, content, ct);

        var responseData = await response.Content.ReadAsStringAsync(ct);

        httpClient.Dispose();

        return JsonSerializer.Deserialize<TResponse>(responseData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 1024,
            WriteIndented = true
        })!;
    }

    public virtual async Task<TResponse> PutAsync<TResponse>(string uri, object payload, CancellationToken ct) where TResponse : class, new()
    {
        return await PutAsync<TResponse>(uri, string.Empty, payload, ct);
    }

    public virtual async Task<TResponse> PutAsync<TResponse>(string uri, string token, object payload, CancellationToken ct) where TResponse : class, new()
    {
        var httpClient = CreateHttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync(uri, content, ct);

        var responseData = await response.Content.ReadAsStringAsync(ct);

        httpClient.Dispose();

        return JsonSerializer.Deserialize<TResponse>(responseData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 1024,
            WriteIndented = true
        })!;
    }

    public virtual async Task<TResponse> DeleteAsync<TResponse>(string uri, CancellationToken ct) where TResponse : class, new()
    {
        return await DeleteAsync<TResponse>(uri, string.Empty, ct);
    }

    public virtual async Task<TResponse> DeleteAsync<TResponse>(string uri, string token, CancellationToken ct) where TResponse : class, new()
    {
        var httpClient = CreateHttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.DeleteAsync(uri, ct);

        var responseData = await response.Content.ReadAsStringAsync(ct);

        httpClient.Dispose();

        return JsonSerializer.Deserialize<TResponse>(responseData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 1024,
            WriteIndented = true
        })!;
    }
}

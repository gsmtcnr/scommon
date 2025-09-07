namespace scommon;

public interface IMailSender
{
    void SetCacheManagerAsync(ICacheManager cacheManager);
    Task<IMailOutput> SendAsync(IMailInput input);
}

public interface IMailInput
{
    public Guid TraceId { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}

public interface IMailOutput
{
    public bool IsContainsWebhook { get; set; }
    public string? Id { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ErrorCode { get; set; }
}

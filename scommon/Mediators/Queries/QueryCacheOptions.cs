namespace scommon;

public class QueryCacheOptions
    : IQueryCacheOptions
{
    public string? Key { get; set; }
    public TimeSpan ExpiryTime { get; set; }
}

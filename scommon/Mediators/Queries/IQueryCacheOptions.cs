namespace scommon;

public interface IQueryCacheOptions
{
    public string Key { get; set; }
    public TimeSpan ExpiryTime { get; set; }
}

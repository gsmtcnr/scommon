namespace scommon.Proxies;

public class ServiceProxyInformation
{
    public ServiceProxyInformation(string name, string baseAddress, double defaultTimeout)
    {
        Name = name;
        BaseAddress = baseAddress;
        DefaultTimeout = defaultTimeout;
    }

    public string Name { get; private set; }
    public  string BaseAddress { get; private set; }
    public double DefaultTimeout { get;private set; }
}

namespace scommon;

public class RabbitMqOptions
{
    public RabbitMqOptions(string prefix, string uri, string userName, string password)
    {
        Prefix = prefix;
        Uri = uri;
        UserName = userName;
        Password = password;
    }

    public string Prefix { get; private set; }
    public string Uri { get; private set; }
    public string UserName { get; private set; }
    public string Password { get; private set; }
}

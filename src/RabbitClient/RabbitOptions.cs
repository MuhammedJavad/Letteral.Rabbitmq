namespace RabbitClient;

public class RabbitOptions
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Vhost { get; set; } = "/";
    public bool AutoDelete { get; set; }
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; }
    public bool UseSecondaryConnectionForConsumers { get; set; }
}
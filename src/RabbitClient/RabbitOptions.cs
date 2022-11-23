namespace RabbitClient;

public class RabbitOptions
{
    public string RabbitConnection { get; set; }
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Vhost { get; set; }
    public bool AutoDelete { get; set; }
    public bool Durable { get; set; }
    public bool Exclusive { get; set; }
    public int RetryCount { get; set; }
    public bool UseSecondaryConnectionForConsumers { get; set; }
}
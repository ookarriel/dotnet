namespace fifo.Api.Configuration;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "fifoApi";
    public string Audience { get; set; } = "fifoClients";
    public int ExpirationMinutes { get; set; } = 60;
}

using System.Text;

namespace Server;

public class ServerConfig
{
    private readonly Dictionary<Types.Codes, int> _maxLength;
    public ServerConfig()
    {
        ReceiveTimeout = 1000000;
        _maxLength = new Dictionary<Types.Codes, int>();
    }
    
    public int MaxId { get; set; }
    public int MinId { get; set; }
    public int ReceiveTimeout { get; set; }
    public int RechargingTimeout { get; set; }
    public string MessageSuffix { get; set; }
    public int TestNumber { get; set; }
    public IReadOnlyDictionary<Types.Codes, int> MaxLength => _maxLength;

    public void SetMaxLenght(Types.Codes code, int maxLength)
    {
        _maxLength[code] = maxLength;
    }
}
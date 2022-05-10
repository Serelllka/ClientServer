using System.Text;

namespace Server;

public class ClientConfig
{
    public ClientConfig(int maxNameLen)
    {
        MaxNameLen = maxNameLen;
    }
    
    public int MaxNameLen { get; set; }
}
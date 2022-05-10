namespace Server;

public class User
{
    private string _userName;

    public User(string userName)
    {
        _userName = userName;
        ClientKeyId = -1;
    }

    public string Name => _userName;
    public int ClientKeyId { get; set; }
    public int Hash { get; set; }
}
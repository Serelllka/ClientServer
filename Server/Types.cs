namespace Server;

public class Types
{
    public static List<KeyValuePair<int, int>> Ids = new(){
        new KeyValuePair<int, int>(23019,32037),
        new KeyValuePair<int, int>(32037,29295),
        new KeyValuePair<int, int>(18789,13603),
        new KeyValuePair<int, int>(16443,29533),
        new KeyValuePair<int, int>(18189,21952)
    };
    public enum Codes
    {
        ServerConfirmation,
        ServerMove,
        ServerTurnLeft,
        ServerTurnRight,
        ServerPickUp,
        ServerLogout,
        ServerLoginFailed,
        ServerSyntaxError,
        ServerLogicError,
        ServerKeyOutOfRangeError,
        ClientUsername,
        ClientKeyId,
        ClientConfirmation,
        ClientOk,
        ClientRecharging,
        ClientFullPower,
        ClientMessage,
        ServerKeyRequest,
        ServerOk
    }
    
    public static bool IsValid(Codes code, string message, int maxMessageLen = 0)
    {
        return code switch
        {
            Codes.ClientUsername => message.EndsWith("\\a\\b") && message.Length <= maxMessageLen,
            Codes.ClientFullPower => message == GetRequest(Codes.ClientFullPower),
            Codes.ClientKeyId => message.Length <= maxMessageLen && int.TryParse(message, out _),
            Codes.ClientConfirmation => message.Length <= maxMessageLen && int.TryParse(message, out _),
            _ => throw new NotImplementedException("")
        };
    }

    public static string GetRequest(Codes code)
    {
        const string suffix = "\\a\\b";
        var returnValue = code switch
        {
            Codes.ServerKeyRequest => "107 KEY REQUEST",
            Codes.ServerSyntaxError => "301 SYNTAX ERROR",
            Codes.ServerLogicError => "302 LOGIC ERROR",
            Codes.ServerKeyOutOfRangeError => "303 KEY OUT OF RANGE",
            Codes.ClientFullPower => "FULL POWER",
            Codes.ClientRecharging => "RECHARGING",
            Codes.ServerLoginFailed => "300 LOGIN FAILED",
            Codes.ServerOk => "200 OK",
            _ => throw new NotImplementedException("")
        };
        returnValue += suffix;
        return returnValue;
    }

    public static string GetMessage(Codes code)
    {
        return ToMessage(GetRequest(code));
    }

    public static string ToMessage(string request)
    {
        return request.Replace("\\a\\b", "");
    }
    
    public static string ToRequest(string message)
    {
        return message + "\\a\\b";
    }
}
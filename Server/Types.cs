using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace Server;

public class Types
{
    public struct Position
    {
        public int X = 0, Y = 0;
        public Position() {}
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Position a, Position b) => !(a == b);
    }

    public enum Direction
    {
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }
    
    public static List<KeyValuePair<int, int>> Ids = new(){
        new KeyValuePair<int, int>(23019,32037),
        new KeyValuePair<int, int>(32037,29295),
        new KeyValuePair<int, int>(18789,13603),
        new KeyValuePair<int, int>(16443,29533),
        new KeyValuePair<int, int>(18189,21952)
    };
    public enum Codes
    {
        ValidMessage,
        
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

            
            Codes.ClientConfirmation => message.Length <= maxMessageLen && int.TryParse(message, out _),
            _ => throw new NotImplementedException("")
        };
    }

    public static string GetRequest(Codes code)
    {
        const string suffix = "\\a\\b";
        var returnValue = code switch
        {
            Codes.ServerMove => "102 MOVE",
            Codes.ServerTurnLeft => "103 TURN LEFT",
            Codes.ServerTurnRight => "104 TURN RIGHT",
            Codes.ServerPickUp => "105 GET MESSAGE",
            Codes.ServerLogout => "106 LOGOUT",
            Codes.ServerKeyRequest => "107 KEY REQUEST",
            Codes.ServerSyntaxError => "301 SYNTAX ERROR",
            Codes.ServerLogicError => "302 LOGIC ERROR",
            Codes.ServerKeyOutOfRangeError => "303 KEY OUT OF RANGE",
            Codes.ClientFullPower => "FULL POWER",
            Codes.ClientRecharging => "RECHARGING",
            Codes.ServerLoginFailed => "300 LOGIN FAILED",
            Codes.ServerOk => "200 OK",
            _ => throw new NotImplementedException("No type!")
        };
        returnValue += suffix;
        return returnValue;
    }

    public static string GetMessage(Codes code, ServerConfig config)
    {
        return ToMessage(GetRequest(code), config);
    }

    public static string ToMessage(string request, ServerConfig config)
    {
        return request.Replace(config.MessageSuffix, "");
    }
    
    public static string ToRequest(string message, ServerConfig config)
    {
        return message + config.MessageSuffix;
    }
}
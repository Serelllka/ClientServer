namespace Server;

public class Validator
{
    private ServerConfig _config;
    
    public Validator(ServerConfig config)
    {
        _config = config;
    }

    public Types.Codes Validate(Types.Codes code, string message)
    {
        var returnCode = ValidateLength(code, message);
        if (returnCode is Types.Codes.ServerSyntaxError)
            return returnCode;

        returnCode = ValidateSyntax(code, message);
        if (returnCode is Types.Codes.ServerSyntaxError)
            return returnCode;

        returnCode = ValidateRequest(code, message);
        
        return returnCode;
    }

    private Types.Codes ValidateLength(Types.Codes code, string message)
    {
        return message.Length <= _config.MaxLength[code] ? Types.Codes.ValidMessage : Types.Codes.ServerSyntaxError;
    }

    private Types.Codes ValidateSyntax(Types.Codes code, string message)
    {
        return message.EndsWith(_config.MessageSuffix) ? Types.Codes.ValidMessage : Types.Codes.ServerSyntaxError;
    }
    
    private Types.Codes ValidateRequest(Types.Codes code, string message)
    {
        if (code is not Types.Codes.ClientFullPower && message == Types.GetRequest(Types.Codes.ClientFullPower))
            return Types.Codes.ServerLogicError;
        switch (code)
        {
            case Types.Codes.ClientFullPower:
                return message == Types.GetRequest(Types.Codes.ClientFullPower)
                    ? Types.Codes.ValidMessage : Types.Codes.ServerSyntaxError;
            case Types.Codes.ClientUsername:
                return Types.Codes.ValidMessage;
            case Types.Codes.ClientKeyId:
                if (!int.TryParse(Types.ToMessage(message), out var id))
                {
                    return Types.Codes.ServerSyntaxError;
                }
                if (id > _config.MaxId || id < _config.MinId)
                {
                    return Types.Codes.ServerKeyOutOfRangeError;
                }
                else
                {
                    return Types.Codes.ValidMessage;
                }
            case Types.Codes.ClientConfirmation:
                return int.TryParse(Types.ToMessage(message), out _) 
                    ? Types.Codes.ValidMessage : Types.Codes.ServerSyntaxError;
            case Types.Codes.ClientOk:
                var list = Types.ToMessage(message).Split(" ").ToList();
                return list.Count == 3 && list[0] == "OK" && int.TryParse(list[1], out _) && int.TryParse(list[2], out _)
                    ? Types.Codes.ValidMessage : Types.Codes.ServerSyntaxError;
            case Types.Codes.ClientMessage:
                return Types.Codes.ValidMessage;
            default:
                throw new ArgumentOutOfRangeException(nameof(code), code, null);
        }
    }
}
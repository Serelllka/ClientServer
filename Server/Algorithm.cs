namespace Server;

public static class Algorithm
{
    public static Types.Direction GetMotionVector(Types.Position prev, Types.Position curr)
    {
        var x = curr.X - prev.X;
        var y = curr.Y - prev.Y;
        if (x == 1)
            return Types.Direction.Right;
        if (x == -1)
            return Types.Direction.Left;
        if (y == 1)
            return Types.Direction.Up;
        if (y == -1)
            return Types.Direction.Down;
        throw new NotImplementedException("GetDirection");
    }

    public static KeyValuePair<Types.Direction, Types.Direction> GetDirections(Types.Position position)
    {
        var vertDirection = position.X switch
        {
            >= 0 => Types.Direction.Left,
            < 0 => Types.Direction.Right,
        };
        
        var horDirection = position.Y switch
        {
            >= 0 => Types.Direction.Down,
            < 0 => Types.Direction.Up,
        };

        return new KeyValuePair<Types.Direction, Types.Direction>(vertDirection, horDirection);
    }
}
using Unity.Mathematics;

public static class MazeMethods
{
    public static readonly Direction[] AvailableActions = new[] {
        Direction.Left,
        Direction.Right,
        Direction.Down,
        Direction.Up
    };

    public static int2 DirectionToDisplacment(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new int2(0, 1),
            Direction.Down => new int2(0, -1),
            Direction.Right => new int2(1, 0),
            Direction.Left => new int2(-1, 0),
            _ => throw new System.Exception(),
        };
    }
}

using Unity.Entities;
using Unity.Mathematics;

public enum Direction
{
    Up,
    Left,
    Right,
    Down,
}

public struct MoveTo : IComponentData
{
    public int2 to;
}

public struct MoveProps : IComponentData
{
    public float speed;
    public float lossyDistance;
}
using Unity.Entities;

public struct MazeWall : IComponentData { }

public struct WallDisplacment : IComponentData
{
    public float speed;
}
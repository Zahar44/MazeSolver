using Unity.Entities;
using Unity.Mathematics;

public struct MazeGenerationProps : IComponentData
{
    public int size;
    public Entity wallPrefab;
    public float2 displacementSpeed;
}
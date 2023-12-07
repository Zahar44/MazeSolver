using Unity.Entities;

public struct MinMaxValue : IComponentData
{
    public float min;
    public float max;
}

public struct MazeGenerationProps : IComponentData
{
    public int size;
    public Entity wallPrefab;
    public MinMaxValue displacementSpeed;
}
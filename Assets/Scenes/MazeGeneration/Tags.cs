using Unity.Entities;

public struct GenerateMazeTag : IComponentData { }
public struct DestroyMazeTag : IComponentData { }
public struct GenerateMazeEndsTag : IComponentData { }

public struct MazeDestroyableTag : IComponentData { }

public struct MazeDisplacment : IComponentData
{
    public float speed;
}

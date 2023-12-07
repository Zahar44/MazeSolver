using Unity.Entities;
using UnityEngine;

public class WallAuthoring : MonoBehaviour
{

}

public class WallBaker : Baker<WallAuthoring>
{
    public override void Bake(WallAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent<MazeDestroyableTag>(entity);
    }
}
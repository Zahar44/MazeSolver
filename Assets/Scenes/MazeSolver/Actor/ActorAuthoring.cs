using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ActorAuthoring : MonoBehaviour
{
    public bool explore;
    public float scale;
}

public class ActorBaker : Baker<ActorAuthoring>
{
    public override void Bake(ActorAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent<MazeDestroyableTag>(entity);
        AddComponent<ActorTag>(entity);
        AddComponent(entity, new ActorScale { scale = authoring.scale });

        if (authoring.explore)
        {
            AddComponent<ExplorationTag>(entity);
        } else
        {
            AddComponent<ExploitationTag>(entity);
        }
    }
}

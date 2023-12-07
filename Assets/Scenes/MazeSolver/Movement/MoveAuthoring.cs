using Unity.Entities;
using UnityEngine;

public class MoveAuthoring : MonoBehaviour
{
    public float speed;
    public float lossyDistance;
}

public class MoveBaker : Baker<MoveAuthoring>
{
    public override void Bake(MoveAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new MoveProps
        {
            speed = authoring.speed,
            lossyDistance = authoring.lossyDistance,
        });
    }
}
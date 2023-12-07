using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CheckPointAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public Color startColor;
    public Color endColor;
    public float initialIntensity;
    public float finalIntensity;
    public float duration;
}

public class CheckPointBaker : Baker<CheckPointAuthoring>
{
    public override void Bake(CheckPointAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new CheckPointProps
        {
            prefab = GetEntity(authoring.prefab, TransformUsageFlags.Renderable),
            startColor = new float4(authoring.startColor.r, authoring.startColor.g, authoring.startColor.b, 1f),
            endColor = new float4(authoring.endColor.r, authoring.endColor.g, authoring.endColor.b, 1f),
            initialIntensity = authoring.initialIntensity,
            finalIntensity = authoring.finalIntensity,
            duration = authoring.duration,
        });
    }
}
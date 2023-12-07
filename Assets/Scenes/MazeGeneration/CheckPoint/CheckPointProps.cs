using Unity.Entities;
using Unity.Mathematics;

public struct CheckPointProps : IComponentData
{
    public Entity prefab;
    public float4 startColor;
    public float4 endColor;
    public float initialIntensity;
    public float finalIntensity;
    public float duration;
}
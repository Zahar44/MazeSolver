using Unity.Entities;
using Unity.Mathematics;

public struct FadeEmission : IComponentData
{
    public float4 color;
    public float initialIntensity;
    public float finalIntensity;
    public float duration;
}
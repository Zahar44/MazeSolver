using System;
using Unity.Entities;
using UnityEngine;

public class MazeGenerationAuthoring : MonoBehaviour
{
    [Range(5, 100)]
    public int size = 10;
    public GameObject wallPrefab;
    [Range(0, 10)]
    public float minDisplacementSpeed;
    [Range(0, 10)]
    public float maxDisplacementSpeed;
}

public class MazeGenerationBaker : Baker<MazeGenerationAuthoring>
{
    public override void Bake(MazeGenerationAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new MazeGenerationProps
        {
            size = authoring.size,
            wallPrefab = GetEntity(authoring.wallPrefab, TransformUsageFlags.Renderable),
            displacementSpeed = new MinMaxValue
            {
                min = authoring.minDisplacementSpeed,
                max = authoring.maxDisplacementSpeed,
            },
        });

        var regenerateTag = CreateAdditionalEntity(TransformUsageFlags.None);
        AddComponent<GenerateMazeTag>(regenerateTag);
    }
}
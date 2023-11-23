using System;
using Unity.Entities;
using UnityEngine;

public class MazeGenerationAuthoring : MonoBehaviour
{
    [Range(10, 100)]
    public int size = 10;
    public GameObject wallPrefab;
    public Vector2 displacementSpeed;
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
            displacementSpeed = authoring.displacementSpeed,
        });

        var regenerateTag = CreateAdditionalEntity(TransformUsageFlags.None);
        AddComponent<RegenerateMaze>(regenerateTag);

        Camera.main.transform.position = new Vector3(authoring.size, 10f, authoring.size);
        Camera.main.transform.LookAt(new Vector3());
    }
}
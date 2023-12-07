using Unity.Entities;
using UnityEngine;

public class RegenerateMazeButton : MonoBehaviour
{
    public void OnClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(DestroyMazeTag), typeof(GenerateMazeTag) },
        });

        if (!query.IsEmpty) return;

        var entity = entityManager.CreateEntity();
        entityManager.AddComponent<DestroyMazeTag>(entity);
        entityManager.AddComponent<GenerateMazeTag>(entity);
    }
}
using Unity.Entities;
using UnityEngine;

public class RegenerateMazeButton : MonoBehaviour
{
    public void OnClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = entityManager.CreateEntity();
        entityManager.AddComponent<RegenerateMaze>(entity);
    }
}
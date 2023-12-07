using Unity.Entities;
using UnityEngine;

public class ExplorationAuthoring : MonoBehaviour
{
    public float learningRate;
    public float discountFactor;
    public float wallReward;
    public float roadReward;
    public float winReward;
}

public class ExplorationBaker : Baker<ExplorationAuthoring>
{
    public override void Bake(ExplorationAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new ExplorationProps
        {
            learningRate = authoring.learningRate,
            discountFactor = authoring.discountFactor,
            wallReward = authoring.wallReward,
            roadReward = authoring.roadReward,
            winReward = authoring.winReward,
        });
    }
}
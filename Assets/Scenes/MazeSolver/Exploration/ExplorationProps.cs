using Unity.Entities;

public struct ExplorationProps : IComponentData
{
    public float duration;
    public float learningRate;
    public float discountFactor;
    public float wallReward;
    public float roadReward;
    public float winReward;
}
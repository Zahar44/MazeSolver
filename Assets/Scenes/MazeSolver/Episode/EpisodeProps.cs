using Unity.Entities;

public struct EpisodeProps : IComponentData
{
    public float duration;
    public Entity explorationPrefab;
    public Entity exploitPrefab;
    public int actorsPerEpisode;
    public float explorationProbability;
}
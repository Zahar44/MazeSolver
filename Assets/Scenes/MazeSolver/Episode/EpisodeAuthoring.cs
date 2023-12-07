using Unity.Entities;
using UnityEngine;

public class EpisodeAuthoring : MonoBehaviour
{
    public float duration;
    public GameObject explorationPrefab;
    public GameObject exploitPrefab;
    [Range(1, 100)]
    public int actorsPerEpisode;
    public float explorationProbability;
}

public class EpisodeBaker : Baker<EpisodeAuthoring>
{
    public override void Bake(EpisodeAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new EpisodeProps
        {
            duration = authoring.duration,
            explorationPrefab = GetEntity(authoring.explorationPrefab, TransformUsageFlags.Renderable),
            exploitPrefab = GetEntity(authoring.exploitPrefab, TransformUsageFlags.Renderable),
            actorsPerEpisode = authoring.actorsPerEpisode,
            explorationProbability = authoring.explorationProbability,
        });
    }
}

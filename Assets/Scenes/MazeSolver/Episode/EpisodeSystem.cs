using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class EpisodeSystem : SystemBase
{
    [BurstCompile]
    private partial struct SpawnJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter command;
        [ReadOnly]
        public NativeHashMap<int2, bool> maze;
        public Unity.Mathematics.Random random;
        public EpisodeProps props;
        public Entity episode;
        public int2 startPosition;
        [ReadOnly]
        public ComponentLookup<ActorScale> scaleLookup;

        [BurstCompile]
        public void Execute(int index)
        {
            var entity = Spawn(index, out var scale);
            command.AppendToBuffer(index, episode, new LinkedEntityGroup { Value = entity });
            command.AddComponent(index, entity, LocalTransform.FromPositionRotationScale(
                new float3(startPosition.x, 0f, startPosition.y), quaternion.identity, scale));
        }

        private Entity Spawn(int index, out float scale)
        {
            scale = 1f;
            if (random.NextFloat() < props.explorationProbability)
            {
                if (scaleLookup.TryGetComponent(props.explorationPrefab, out var actorScale))
                {
                    scale = actorScale.scale;
                }

                return command.Instantiate(index, props.explorationPrefab);
            }
            else
            {
                if (scaleLookup.TryGetComponent(props.exploitPrefab, out var actorScale))
                {
                    scale = actorScale.scale;
                }

                return command.Instantiate(index, props.exploitPrefab);
            }
        }
    }


    protected override void OnCreate()
    {
        RequireForUpdate<EpisodeProps>();
        RequireForUpdate<MazeEndTag>();
    }

    protected override void OnUpdate()
    {
        var command = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);
        var props = SystemAPI.GetSingleton<EpisodeProps>();

        if (!SystemAPI.TryGetSingletonEntity<Episode>(out var episode))
        {
            StartNewEpisode(ref command, props);
        }

        var actors = SystemAPI.GetBuffer<LinkedEntityGroup>(episode);
        for (var i = 0; i < actors.Length; i++)
        {
            if (!SystemAPI.Exists(actors[i].Value))
            {
                actors.RemoveAt(i);
            }
        }

        var episodeStartedAt = SystemAPI.GetSingleton<Episode>().startedAt;
        if (actors.IsEmpty || SystemAPI.Time.ElapsedTime - episodeStartedAt > props.duration)
        {
            command.DestroyEntity(episode);
            for (var i = 0; i < actors.Length; i++)
            {
                command.DestroyEntity(actors[i].Value);
            }

            StartNewEpisode(ref command, props);
        }
    }

    void StartNewEpisode(ref EntityCommandBuffer command, EpisodeProps props)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000) + 1);
        var episode = EntityManager.CreateEntity();
        EntityManager.AddComponentData(episode, new Episode
        {
            startedAt = SystemAPI.Time.ElapsedTime,
        });
        EntityManager.AddBuffer<LinkedEntityGroup>(episode);
        var startPosition = (int2)SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<MazeStartTag>()).Position.xz;

        Dependency.Complete();
        Dependency = new SpawnJob
        {
            command = command.AsParallelWriter(),
            maze = GenerateMazeSystem.MazeCells,
            random = random,
            props = props,
            episode = episode,
            startPosition = startPosition,
            scaleLookup = SystemAPI.GetComponentLookup<ActorScale>(true),
        }.Schedule(props.actorsPerEpisode, 32, Dependency);
    }
}
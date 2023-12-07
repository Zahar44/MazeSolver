using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct DestroyMazeSystem : ISystem
{
    [BurstCompile]
    [WithAll(typeof(MazeDestroyableTag))]
    private partial struct DestroyAllWalls : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter command;
        public Random random;
        [ReadOnly]
        public MazeGenerationProps props;

        [BurstCompile]
        public void Execute(
            [EntityIndexInQuery] int index,
            Entity entity)
        {
            var speed = random.NextFloat(props.displacementSpeed.min, props.displacementSpeed.max);
            command.AddComponent(index, entity, new MazeDisplacment { speed = -speed });
        }
    }


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MazeGenerationProps>();
        state.RequireForUpdate<DestroyMazeTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(state.WorldUnmanaged);
        var props = SystemAPI.GetSingleton<MazeGenerationProps>();
        var random = Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000) + 1);

        var destroyTag = SystemAPI.GetSingletonEntity<DestroyMazeTag>();
        command.RemoveComponent<DestroyMazeTag>(destroyTag);

        new DestroyAllWalls
        {
            command = command.AsParallelWriter(),
            random = random,
            props = props,
        }.ScheduleParallel();
    }
}
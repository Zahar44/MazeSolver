using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct WallDisplacementSystem : ISystem
{
    [BurstCompile]
    private partial struct DisplacementJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter command;
        [ReadOnly]
        public float delta;

        [BurstCompile]
        void Execute(
            [EntityIndexInQuery] int index,
            Entity entity,
            in WallDisplacment displacment,
            ref LocalTransform transform)
        {
            transform.Position.y += displacment.speed * delta;

            if (transform.Position.y <= -1f)
            {
                command.DestroyEntity(index, entity);
            } else if (transform.Position.y >= 0f)
            {
                transform.Position.y = 0f;
                command.RemoveComponent<WallDisplacment>(index, entity);
            }
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WallDisplacment>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(state.WorldUnmanaged);

        new DisplacementJob
        {
            command = command.AsParallelWriter(),
            delta = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
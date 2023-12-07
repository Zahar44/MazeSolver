using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[BurstCompile]
public partial struct FadeEmissionSystem : ISystem
{
    [BurstCompile]
    private partial struct FadeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter command;
        [ReadOnly]
        public float delta;

        [BurstCompile]
        void Execute(
            [EntityIndexInQuery] int index,
            Entity entity,
            ref FadeEmission emission)
        {
            emission.duration -= delta;
            if (emission.duration < 0f)
            {
                command.RemoveComponent<FadeEmission>(index, entity);
                command.AddComponent(index, entity, new URPMaterialPropertyEmissionColor
                {
                    Value = emission.color * emission.finalIntensity,
                });
                return;
            }

            var intensity = math.lerp(emission.finalIntensity, emission.initialIntensity, emission.duration);
            command.AddComponent(index, entity, new URPMaterialPropertyEmissionColor
            {
                Value = emission.color * intensity,
            });
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FadeEmission>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(state.WorldUnmanaged);

        new FadeJob
        {
            command = command.AsParallelWriter(),
            delta = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}
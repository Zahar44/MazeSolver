using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct MoveToSystem : ISystem
{
    [BurstCompile]
    private partial struct MoveJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter command;
        public float delta;
        public MoveProps props;
        [ReadOnly]
        public NativeHashMap<int2, bool> maze;


        [BurstCompile]
        void Execute(
            [EntityIndexInQuery] int index,
            Entity entity,
            in MoveTo move,
            ref LocalTransform transform)
        {
            var currentPos = transform.Position.xz;
            var displacment = move.to - currentPos;

            if (math.length(displacment) < props.lossyDistance)
            {
                command.RemoveComponent<MoveTo>(index, entity);
                var cell = math.round(transform.Position.xz);
                transform.Position = new float3(cell.x, 0f, cell.y);
                return;
            }

            var direction = math.normalizesafe(displacment);
            var deltaSpeed = delta * props.speed;
            transform.Position = new float3(
                transform.Position.x + direction.x * deltaSpeed,
                0f,
                transform.Position.z + direction.y * deltaSpeed);
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveTo>();
        state.RequireForUpdate<MoveProps>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(state.WorldUnmanaged);
        var moveProps = SystemAPI.GetSingleton<MoveProps>();

        new MoveJob
        {
            command = command.AsParallelWriter(),
            delta = SystemAPI.Time.DeltaTime,
            props = moveProps,
            maze = GenerateMazeSystem.MazeCells,
        }.ScheduleParallel();
    }
}
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial class ExplorationSystem : SystemBase
{
    public static NativeParallelMultiHashMap<int2, ActionAndQValue> MazeState;

    [BurstCompile]
    [WithAll(typeof(ActorTag), typeof(ExplorationTag))]
    [WithNone(typeof(MoveTo), typeof(MazeDisplacment))]
    private partial struct SolveJob : IJobEntity
    {
        public EntityCommandBuffer command;
        [ReadOnly]
        public NativeHashMap<int2, bool> maze;
        public NativeParallelMultiHashMap<int2, ActionAndQValue> state;
        public Unity.Mathematics.Random random;
        public ExplorationProps props;
        public int2 endCheckPointCell;

        [BurstCompile]
        void Execute(
            Entity entity,
            in LocalTransform transform)
        {
            var currentCell = (int2)transform.Position.xz;
            var nextCell = UpdateQValue(currentCell);

            if (IsWon(currentCell))
            {
                command.DestroyEntity(entity);
                return;
            }

            if (IsInWall(nextCell))
            {
                UpdateQValue(currentCell);
                return;
            }

            command.AddComponent(entity, new MoveTo
            {
                to = nextCell,
            });
        }

        [BurstCompile]
        int2 UpdateQValue(int2 currentCell)
        {
            var action = MazeMethods.AvailableActions[random.NextInt(0, MazeMethods.AvailableActions.Length)];
            var nextCell = currentCell + MazeMethods.DirectionToDisplacment(action);

            var reward = GetReward(nextCell);
            var currentQValue = GetQValue(currentCell, action);
            var nextBestQValue = GetBestQValueFromPosition(nextCell);

            var wightedCurrentQValue = currentQValue * (1 - props.learningRate);
            var temporalDifferentTarget = reward + props.discountFactor * nextBestQValue;
            var qValue = wightedCurrentQValue + props.learningRate * temporalDifferentTarget;

            InsertQValue(currentCell, action, qValue);
            return nextCell;
        }

        [BurstCompile]
        float GetReward(int2 position)
        {
            if (IsWon(position))
                return props.winReward;

            return IsInWall(position) ? props.wallReward : props.roadReward;
        }

        bool IsWon(int2 position)
        {
            return position.x == endCheckPointCell.x && position.y == endCheckPointCell.y;
        }

        bool IsInWall(int2 position)
        {
            var isRoad = maze.TryGetValue(position, out _);
            return !isRoad;
        }

        [BurstCompile]
        float GetBestQValueFromPosition(int2 position)
        {
            var exist = state.TryGetFirstValue(position, out var data, out var it);
            var maxQValue = data.qValue;

            while (exist)
            {
                if (data.qValue > maxQValue)
                {
                    maxQValue = data.qValue;
                };

                exist = state.TryGetNextValue(out data, ref it);
            }

            return maxQValue;
        }

        [BurstCompile]
        float GetQValue(int2 position, Direction action)
        {
            var exist = state.TryGetFirstValue(position, out var data, out var it);
            while (exist)
            {
                if (data.action == action) return data.qValue;

                exist = state.TryGetNextValue(out data, ref it);
            }

            return 0f;
        }

        [BurstCompile]
        void InsertQValue(int2 position, Direction action, float qValue)
        {
            var exist = state.TryGetFirstValue(position, out var data, out var it);
            while (exist)
            {
                if (data.action == action)
                {
                    data.qValue = qValue;
                    state.SetValue(data, it);
                    return;
                }

                exist = state.TryGetNextValue(out data, ref it);
            }

            state.Add(position, new ActionAndQValue
            {
                action = action,
                qValue = qValue,
            });
        }
    }

    protected override void OnCreate()
    {
        MazeState = new(0, Allocator.Persistent);
        RequireForUpdate<MazeEndTag>();
        RequireForUpdate<ExplorationTag>();
    }

    protected override void OnUpdate()
    {
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(World.Unmanaged);
        var endCheckPoint = SystemAPI.GetSingletonEntity<MazeEndTag>();
        var endCheckPointCell = (int2)SystemAPI.GetComponent<LocalTransform>(endCheckPoint).Position.xz;

        new SolveJob
        {
            command = command,
            maze = GenerateMazeSystem.MazeCells,
            state = MazeState,
            random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000) + 1),
            props = SystemAPI.GetSingleton<ExplorationProps>(),
            endCheckPointCell = endCheckPointCell,
        }.Schedule();
        CompleteDependency();
    }
}
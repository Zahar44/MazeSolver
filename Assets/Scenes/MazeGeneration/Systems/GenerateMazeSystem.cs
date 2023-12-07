using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class GenerateMazeSystem : SystemBase
{
    public static NativeHashMap<int2, bool> MazeCells;

    [WithAll(typeof(GenerateMazeTag))]
    [BurstCompile]
    private partial struct GenerationJob : IJobEntity
    {
        public EntityCommandBuffer command;
        public NativeHashMap<int2, bool> maze;
        public Unity.Mathematics.Random random;
        [ReadOnly]
        public MazeGenerationProps props;

        [BurstCompile]
        public void Execute(Entity entity)
        {
            command.RemoveComponent<GenerateMazeTag>(entity);
            command.AddComponent<GenerateMazeEndsTag>(entity);

            Generate(new int2(1, 1));
            SpawnEntities();
        }

        private void Generate(int2 startPosition)
        {
            maze[startPosition] = false;
            var directions = ShuffleDirection();

            foreach (var direction in directions)
            {
                var cellPosition = startPosition + direction;
                if (!IsValidCell(cellPosition) || maze.ContainsKey(cellPosition)) continue;

                maze[startPosition + direction / 2] = false;
                Generate(cellPosition);
            }
        }

        private NativeArray<int2> ShuffleDirection()
        {
            var directions = new NativeArray<int2>(4, Allocator.Temp);
            directions[0] = new int2(2, 0);
            directions[1] = new int2(-2, 0);
            directions[2] = new int2(0, 2);
            directions[3] = new int2(0, -2);

            for (var i = directions.Length - 1; i > 0; i--)
            {
                var j = random.NextInt(0, i);
                (directions[i], directions[j]) = (directions[j], directions[i]);
            }

            return directions;
        }

        private bool IsValidCell(int2 position)
        {
            return (position.x > 0 && position.x < props.size) && (position.y > 0 && position.y < props.size);
        }

        private void SpawnEntities()
        {
            for (var i = 0; i < props.size; i++)
            {
                for (var j = 0; j < props.size; j++)
                {
                    var position = new int2(i, j);

                    if (maze.ContainsKey(position)) 
                    {
                        continue;
                    };

                    InstantiateWall(position);
                }
            }
        }

        private void InstantiateWall(int2 position)
        {
            var wall = command.Instantiate(props.wallPrefab);
            command.AddComponent(wall, LocalTransform.FromPosition(new float3(position.x, -1f, position.y)));
            var speed = random.NextFloat(props.displacementSpeed.min, props.displacementSpeed.max);
            command.AddComponent(wall, new MazeDisplacment { speed = speed });
        }
    }

    [BurstCompile]
    protected override void OnCreate()
    {
        MazeCells = new(0, Allocator.Persistent);
        RequireForUpdate<MazeGenerationProps>();
        RequireForUpdate<GenerateMazeTag>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (SystemAPI.HasSingleton<DestroyMazeTag>()) return;

        var props = SystemAPI.GetSingleton<MazeGenerationProps>();
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(World.Unmanaged);
        var random = new Unity.Mathematics.Random((uint)(SystemAPI.Time.ElapsedTime * 1000) + 1);

        MazeCells.Clear();
        MazeCells.Capacity = math.max(MazeCells.Capacity, props.size);

        ExplorationSystem.MazeState.Clear();
        ExplorationSystem.MazeState.Capacity = math.max(ExplorationSystem.MazeState.Capacity, props.size);

        Camera.main.transform.position = new Vector3(props.size / 2f, props.size, props.size / 2f);
        Camera.main.transform.LookAt(new Vector3(props.size / 2f, 0f, props.size / 2f));

        new GenerationJob
        {
            command = command,
            maze = MazeCells,
            random = random,
            props = props,
        }.Schedule();
    }
}

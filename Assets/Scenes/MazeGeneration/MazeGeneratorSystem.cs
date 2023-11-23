using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class MazeGeneratorSystem : SystemBase
{
    public static NativeHashMap<int2, bool> MazeCells;

    [WithAll(typeof(MazeWall))]
    [BurstCompile]
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
            var speed = random.NextFloat(props.displacementSpeed.x, props.displacementSpeed.y);
            command.AddComponent(index, entity, new WallDisplacment { speed = -speed });
        }
    }

    [WithAll(typeof(RegenerateMaze))]
    [BurstCompile]
    private partial struct GenerationJob : IJobEntity
    {
        public EntityCommandBuffer command;
        public NativeHashMap<int2, bool> maze;
        public Random random;
        [ReadOnly]
        public MazeGenerationProps props;

        [BurstCompile]
        public void Execute(Entity entity)
        {
            command.DestroyEntity(entity);

            Generate(new int2(1, 1));

            for (var i = 0; i < props.size; i++)
            {
                for (var j = 0; j < props.size; j++)
                {
                    var position = new int2(i, j);
                    if (maze.ContainsKey(position)) continue;

                    var wall = command.Instantiate(props.wallPrefab);
                    command.AddComponent(wall, LocalTransform.FromPosition(new float3(position.x, -1f, position.y)));
                    var speed = random.NextFloat(props.displacementSpeed.x, props.displacementSpeed.y);
                    command.AddComponent(wall, new WallDisplacment { speed = speed });
                }
            }
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
    }

    [BurstCompile]
    protected override void OnCreate()
    {
        MazeCells = new(0, Allocator.Persistent);
        RequireForUpdate<MazeGenerationProps>();
        RequireForUpdate<RegenerateMaze>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var props = SystemAPI.GetSingleton<MazeGenerationProps>();
        var commandSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(World.Unmanaged);
        var random = new Random((uint)SystemAPI.Time.ElapsedTime + 1);

        MazeCells.Clear();
        MazeCells.Capacity = props.size * props.size;

        new DestroyAllWalls
        {
            command = command.AsParallelWriter(),
            random = random,
            props = props,
        }.ScheduleParallel();

        new GenerationJob
        {
            command = command,
            maze = MazeCells,
            random = random,
            props = props,
        }.Schedule();
    }
}

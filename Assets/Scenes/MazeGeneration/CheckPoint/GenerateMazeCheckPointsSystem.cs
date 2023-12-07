using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial class GenerateMazeCheckPointsSystem : SystemBase
{
    [WithAll(typeof(GenerateMazeEndsTag))]
    [BurstCompile]
    private partial struct GenerateJob : IJobEntity
    {
        public EntityCommandBuffer command;
        public Random random;
        [ReadOnly]
        public NativeHashMap<int2, bool> maze;
        [ReadOnly]
        public CheckPointProps props;
        [ReadOnly]
        public MinMaxValue displacementSpeed;

        [BurstCompile]
        void Execute(Entity entity)
        {
            command.RemoveComponent<GenerateMazeEndsTag>(entity);
            var mazeRoads = maze.GetKeyArray(Allocator.Temp);

            var startPosition = random.NextInt(0, mazeRoads.Length);
            var endPosition = random.NextInt(0, mazeRoads.Length);
            while (endPosition == startPosition)
            {
                endPosition = random.NextInt(0, mazeRoads.Length);
            }

            var start = InstantiateCheckPoint(mazeRoads[startPosition], props.startColor);
            command.AddComponent<MazeStartTag>(start);
            var end = InstantiateCheckPoint(mazeRoads[endPosition], props.endColor);
            command.AddComponent<MazeEndTag>(end);
        }

        private Entity InstantiateCheckPoint(int2 position, float4 color)
        {
            var checkPoint = command.Instantiate(props.prefab);
            command.AddComponent(checkPoint, LocalTransform.FromPositionRotationScale(
                new float3(position.x, -1f, position.y), quaternion.identity, 0.5f));

            var speed = random.NextFloat(displacementSpeed.min, displacementSpeed.max);
            command.AddComponent(checkPoint, new MazeDisplacment { speed = speed });
            command.AddComponent(checkPoint, new URPMaterialPropertyBaseColor { Value = color });
            command.AddComponent(checkPoint, new FadeEmission
            {
                color = color,
                initialIntensity = props.initialIntensity,
                finalIntensity = props.finalIntensity,
                duration = props.duration,
            });

            return checkPoint;
        }
    }

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<GenerateMazeEndsTag>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var props = SystemAPI.GetSingleton<CheckPointProps>();
        var mazeProps = SystemAPI.GetSingleton<MazeGenerationProps>();
        var commandSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var command = commandSystem.CreateCommandBuffer(World.Unmanaged);
        var random = Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000) + 1);

        new GenerateJob
        {
            command = command,
            random = random,
            maze = GenerateMazeSystem.MazeCells,
            props = props,
            displacementSpeed = mazeProps.displacementSpeed,
        }.Schedule();
    }
}
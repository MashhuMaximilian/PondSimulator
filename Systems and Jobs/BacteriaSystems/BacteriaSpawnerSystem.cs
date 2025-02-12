using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using LakeBacteria.Components;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class BacteriaSpawnerSystem : SystemBase
{
    private EntityQuery _bacteriaQuery;

    protected override void OnCreate()
    {
        _bacteriaQuery = GetEntityQuery(ComponentType.ReadOnly<BacteriaData>());
        RequireForUpdate<BacteriaSpawningConfig>();
        RequireForUpdate<BacteriaShapeConfig>();
    }

    protected override void OnUpdate()
    {
        // Get the spawn and shape configuration singletons.
        var spawnConfig = SystemAPI.GetSingleton<BacteriaSpawningConfig>();
        var shapeConfig = SystemAPI.GetSingleton<BacteriaShapeConfig>();

        using (var ecb = new EntityCommandBuffer(Allocator.TempJob))
        {
            int existingCount = _bacteriaQuery.CalculateEntityCount();
            int toSpawn = math.max(0, spawnConfig.SpawnCount - existingCount);
            if (toSpawn > 0)
            {
                uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000 + 1);
                var job = new SpawnBacteriaJob
                {
                    ECB = ecb.AsParallelWriter(),
                    ShapeConfig = shapeConfig,
                    SpawnArea = spawnConfig.SpawnArea,
                    Seed = seed
                };

                JobHandle handle = job.ScheduleParallel(toSpawn, 64, Dependency);
                handle.Complete();
            }
            ecb.Playback(EntityManager);
        }
    }

    [BurstCompile]
    private struct SpawnBacteriaJob : IJobFor
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public BacteriaShapeConfig ShapeConfig;
        public float3 SpawnArea;
        public uint Seed;

        public void Execute(int index)
        {
            // Create a random generator for this bacteria instance.
            var random = new Unity.Mathematics.Random(Seed + (uint)index);
            float3 position = random.NextFloat3(-SpawnArea / 2, SpawnArea / 2);

            // Generate random gene values.
            BacteriaData bacteriaData = new BacteriaData
            {
                Speed = random.NextFloat(0.0f, 1f),
                MetabolicEfficiency = random.NextFloat(0.0f, 0.9f),
                Sturdiness = random.NextFloat(0.0f, 1.0f),
                SensorRadius = random.NextFloat(0.0f, 1.0f),
                RadiationResistance = random.NextFloat(0.0f, 1.0f),
                ReproductiveCost = random.NextFloat(0.0f, 1.0f),
                AggressionBias = random.NextFloat(0.0f, 1.0f),
                ClusterPreference = random.NextFloat(0.0f, 1.0f),
                MutationRate = random.NextFloat(0.0f, 1.0f),
                shapeType = BacteriaData.ShapeType.Bacillus // default value; will be updated below.

            };


            // Determine the shape based on gene values using our helper function.
            BacteriaData.ShapeType chosenShape = BacteriaVisuals.DetermineShapeType(bacteriaData);
            bacteriaData.shapeType = chosenShape;

            // Retrieve the correct prefab based on the chosen shape.
            Entity chosenPrefab = BacteriaVisuals.GetPrefabForShape(ShapeConfig, chosenShape);

            BurstLogger.Log($"[Spawner] Bacterium gene values: " +
                                    $"Metabolic Efficiency: {bacteriaData.MetabolicEfficiency}, " +
                                    $"Aggression Bias: {bacteriaData.AggressionBias}, " +
                                    $"Speed: {bacteriaData.Speed}, " +
                                    $"Cluster Preference: {bacteriaData.ClusterPreference}, " +
                                    $"Sensor Radius: {bacteriaData.SensorRadius}, " +
                                    $"Determined shape: {chosenShape}");

            if (chosenPrefab == Entity.Null)
            {
                UnityEngine.Debug.LogError($"No prefab assigned for shape {chosenShape}. Check your BacteriaShapeConfig!");
                return;
            }


            // Instantiate the bacteria entity using the chosen prefab.
            Entity spawned = ECB.Instantiate(index, chosenPrefab);

            // Assign gene values and additional components.
            ECB.SetComponent(index, spawned, bacteriaData);
            ECB.SetComponent(index, spawned, new Energy { Value = 100f });
            ECB.SetComponent(index, spawned, new Health { Value = 100f });
            ECB.SetComponent(index, spawned, new LocalTransform
            {
                Position = position,
                Rotation = Unity.Mathematics.quaternion.identity,
                Scale = 1f
            });

            // Add a marker for the visual system to update the shape.
            ECB.AddComponent(index, spawned, new BacteriaVisualTag());
        }
    }
}

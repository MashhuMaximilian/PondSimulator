using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class FoodLifecycleSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationECBSystem;
    private EntityQuery _foodQuery;
    private bool _hasInitialSpawned;

    protected override void OnCreate()
    {
        // Get the managed ECB system.
        _endSimulationECBSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        _foodQuery = GetEntityQuery(ComponentType.ReadOnly<FoodTag>(), ComponentType.ReadOnly<FoodData>());
        RequireForUpdate<FoodSpawningConfig>();
        RequireForUpdate<FoodTypeConfig>();
        RequireForUpdate<SimulationSingleton>();
        _hasInitialSpawned = false;
    }

    protected override void OnUpdate()
    {
        if (!ValidateRequiredSingletons()) return;

        var configs = GetConfigurations();
        float scaledDeltaTime = SystemAPI.Time.DeltaTime * configs.simulation.tickSpeed;

        // Use the ECB from the EndSimulationEntityCommandBufferSystem.
        var commandBuffer = _endSimulationECBSystem.CreateCommandBuffer().AsParallelWriter();

        if (!_hasInitialSpawned)
        {
            HandleInitialSpawning(configs.spawn, configs.type, commandBuffer);
            _hasInitialSpawned = true;
        }

        HandleFoodLifecycle(scaledDeltaTime, commandBuffer);

        // Make sure the ECB system waits for our jobs.
        _endSimulationECBSystem.AddJobHandleForProducer(Dependency);
    }

    private bool ValidateRequiredSingletons()
    {
        if (!SystemAPI.HasSingleton<FoodSpawningConfig>() ||
            !SystemAPI.HasSingleton<FoodTypeConfig>() ||
            !SystemAPI.HasSingleton<SimulationSingleton>())
        {
            Debug.LogError("Required singletons not found!");
            return false;
        }
        return true;
    }

    private (FoodSpawningConfig spawn, FoodTypeConfig type, SimulationSingleton simulation) GetConfigurations()
    {
        return (
            SystemAPI.GetSingleton<FoodSpawningConfig>(),
            SystemAPI.GetSingleton<FoodTypeConfig>(),
            SystemAPI.GetSingleton<SimulationSingleton>()
        );
    }

    private void HandleInitialSpawning(FoodSpawningConfig spawnConfig, FoodTypeConfig typeConfig, EntityCommandBuffer.ParallelWriter commandBuffer)
    {
        int spawnCount = math.max(0, spawnConfig.SpawnedFoodUnits - _foodQuery.CalculateEntityCount());
        if (spawnCount > 0)
        {
            SpawnInitialFood(spawnCount, spawnConfig, typeConfig, commandBuffer);
        }
    }

    private void SpawnInitialFood(int spawnCount, FoodSpawningConfig spawnConfig, FoodTypeConfig typeConfig, EntityCommandBuffer.ParallelWriter commandBuffer)
    {
        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000 + 1);
        NativeArray<float3> hotspots = CreateHotspots(spawnConfig);
        float defaultRadius = spawnConfig.SpawnedFoodRadius;

        var spawnJob = new SpawnFoodJob
        {
            ECB = commandBuffer,
            SpawnArea = spawnConfig.SpawnArea,
            LifespanRange = spawnConfig.LifespanRange,
            Seed = seed,
            FoodPrefabEntity = typeConfig.SpawnedFoodPrefab,
            Hotspots = hotspots,
            HotspotRadius = spawnConfig.HotspotRadius,
            HotspotDensity = spawnConfig.HotspotDensity,
            ClusterDensity = spawnConfig.ClusterDensity,
            DefaultRadius = defaultRadius
        };

        Dependency = spawnJob.Schedule(spawnCount, 64, Dependency);
        Dependency = hotspots.Dispose(Dependency);
    }

    private NativeArray<float3> CreateHotspots(FoodSpawningConfig config)
    {
        int count = config.HotspotCount > 0 ? config.HotspotCount : 0;
        var hotspots = new NativeArray<float3>(count, Allocator.TempJob);
        if (count > 0)
        {
            var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime + 1);
            for (int i = 0; i < count; i++)
            {
                hotspots[i] = random.NextFloat3(-config.SpawnArea / 2, config.SpawnArea / 2);
            }
        }
        return hotspots;
    }

    private void HandleFoodLifecycle(float scaledDeltaTime, EntityCommandBuffer.ParallelWriter commandBuffer)
    {
        // Allocate a NativeCounter for decay counting.
        NativeCounter decayCounter = new NativeCounter(Allocator.TempJob);

        var decayJob = new DecayJob
        {
            DeltaTime = scaledDeltaTime,
            ECB = commandBuffer,
            DecayCounter = decayCounter.AsParallelWriter()
        };
        Dependency = decayJob.ScheduleParallel(Dependency);

        var movementJob = new MovementJob
        {
            DeltaTime = scaledDeltaTime,
            Time = (float)SystemAPI.Time.ElapsedTime,
            SpawnArea = SystemAPI.GetSingleton<FoodSpawningConfig>().SpawnArea
        };
        Dependency = movementJob.ScheduleParallel(Dependency);

        // Schedule disposal of the decay counter once jobs are complete.
        Dependency = decayCounter.Dispose(Dependency);
    }
}

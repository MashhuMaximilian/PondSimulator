using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;           // For IJobParallelFor scheduling
using Unity.Mathematics;
using UnityEngine;



// FoodLifecycleSystem.cs
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class FoodLifecycleSystem : SystemBase
{
    private EntityQuery _foodQuery;
    private bool _hasInitialSpawned;
    // private SimulationConfig _simulationConfig;

    protected override void OnCreate()
    {
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

        using (var ecb = new EntityCommandBuffer(Allocator.TempJob))
        {
            if (!_hasInitialSpawned)
            {
                HandleInitialSpawning(configs.spawn, configs.type, ecb);
            }

            HandleFoodLifecycle(scaledDeltaTime, ecb);
        }
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

    private void HandleInitialSpawning(FoodSpawningConfig spawnConfig, FoodTypeConfig typeConfig, EntityCommandBuffer ecb)
    {
        int spawnCount = math.max(0, spawnConfig.SpawnedFoodUnits - _foodQuery.CalculateEntityCount());
        if (spawnCount > 0)
        {
            SpawnInitialFood(spawnCount, spawnConfig, typeConfig, ecb);
            _hasInitialSpawned = true;
        }
    }

    private void SpawnInitialFood(int spawnCount, FoodSpawningConfig spawnConfig, FoodTypeConfig typeConfig, EntityCommandBuffer ecb)
    {
        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000 + 1);
        var hotspots = CreateHotspots(spawnConfig);

        var spawnJob = new SpawnFoodJob
        {
            ECB = ecb.AsParallelWriter(),
            SpawnArea = spawnConfig.SpawnArea,
            LifespanRange = spawnConfig.LifespanRange,
            Seed = seed,
            FoodPrefabEntity = typeConfig.SpawnedFoodPrefab,
            Hotspots = hotspots,
            HotspotRadius = spawnConfig.HotspotRadius,
            HotspotDensity = spawnConfig.HotspotDensity,
            ClusterDensity = spawnConfig.ClusterDensity
        };

        Dependency = spawnJob.Schedule(spawnCount, 64, Dependency);
        Dependency.Complete();

        if (hotspots.IsCreated) hotspots.Dispose();
    }

    private NativeArray<float3> CreateHotspots(FoodSpawningConfig config)
    {
        if (config.HotspotCount <= 0) return default;

        var hotspots = new NativeArray<float3>(config.HotspotCount, Allocator.TempJob);
        var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime + 1);

        for (int i = 0; i < config.HotspotCount; i++)
        {
            hotspots[i] = random.NextFloat3(-config.SpawnArea / 2, config.SpawnArea / 2);
        }

        return hotspots;
    }

    private void HandleFoodLifecycle(float scaledDeltaTime, EntityCommandBuffer ecb)
    {
        var decayCounter = new NativeCounter(Allocator.TempJob);
        try
        {
            ScheduleLifecycleJobs(scaledDeltaTime, ecb, decayCounter);
            Dependency.Complete();
            ecb.Playback(EntityManager);
        }
        finally
        {
            decayCounter.Dispose();
        }
    }

    private void ScheduleLifecycleJobs(float scaledDeltaTime, EntityCommandBuffer ecb, NativeCounter decayCounter)
    {
        var decayJob = new DecayJob
        {
            DeltaTime = scaledDeltaTime,
            ECB = ecb.AsParallelWriter(),
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
    }
}

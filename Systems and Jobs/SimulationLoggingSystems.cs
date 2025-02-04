using Unity.Entities;
using UnityEngine;
/// <summary>
/// An ISystem that periodically logs simulation statistics.
/// </summary>
public partial struct SimulationLoggingSystem : ISystem
{
    private float _logTimer;
    private const float LOG_INTERVAL = 3f;
    public void OnCreate(ref SystemState state)
    {
        // Initialize SimulationStatistics and SimulationSingleton
        var statsQuery = SystemAPI.QueryBuilder()
            .WithAll<SimulationStatistics, SimulationStatisticsSingletonTag>()
            .Build();
        if (statsQuery.CalculateEntityCount() == 0)
        {
            var statsEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(statsEntity, new SimulationStatistics
            {
                InitialSpawnedCount = 0,
                RegeneratedSpawnedCount = 0,
                FoodDecayedCount = 0,
                OnMapNow = 0,
                TotalEnergyInEnvironment = 0f,
                TotalAgeAtDecay = 0f
            });
            state.EntityManager.AddComponent<SimulationStatisticsSingletonTag>(statsEntity);
        }
        // Initialize SimulationSingleton
        var singletonQuery = SystemAPI.QueryBuilder()
            .WithAll<SimulationSingleton>()
            .Build();
        if (singletonQuery.CalculateEntityCount() == 0)
        {
            var singletonEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(singletonEntity, new SimulationSingleton { tickSpeed = 1f });
        }
    }
    public void OnUpdate(ref SystemState state)
    {
        // Calculate scaled delta time
        if (!SystemAPI.TryGetSingleton<SimulationSingleton>(out var singleton))
        {
            Debug.LogError("SimulationSingleton not found!");
            return;
        }
        float scaledDeltaTime = SystemAPI.Time.DeltaTime * singleton.tickSpeed;
        _logTimer += scaledDeltaTime;
        if (_logTimer < LOG_INTERVAL)
            return;
        if (!SystemAPI.TryGetSingleton<SimulationStatistics>(out var stats))
            return;
        var loggingSystem = state.World.GetExistingSystemManaged<SimulationLoggingSystemManaged>();
        loggingSystem?.LogStatistics(stats);
        if (SystemAPI.TryGetSingletonRW<SimulationStatistics>(out var statsRW))
        {
            statsRW.ValueRW.RegeneratedSpawnedCount = 0;
            statsRW.ValueRW.FoodDecayedCount = 0;
        }
        _logTimer = 0f;
    }
}
/// <summary>
/// A managed SystemBase that performs the actual Debug.Log.
/// </summary>
public partial class SimulationLoggingSystemManaged : SystemBase
{
    public void LogStatistics(SimulationStatistics stats)
    {
        Debug.Log(
            $"[Food] Spawned: {stats.InitialSpawnedCount} | " +
            $"Regenerated: {stats.RegeneratedSpawnedCount} | " +
            $"Decayed: {stats.FoodDecayedCount} | " +
            $"On map now: {stats.OnMapNow}"
        );
    }
    protected override void OnUpdate()
    {
        // No-op: This system is managed and only used for logging.
    }
}
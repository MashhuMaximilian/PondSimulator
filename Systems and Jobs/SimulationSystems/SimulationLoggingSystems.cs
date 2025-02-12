using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace LakeBacteria.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class SimulationLoggingSystem : SystemBase
    {
        private float _logTimer;
        private const float LOG_INTERVAL = 3f;

        protected override void OnCreate()
        {
            // Initialize SimulationStatistics singleton if missing
            if (!SystemAPI.HasSingleton<SimulationStatistics>())
            {
                var statsEntity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(statsEntity, new SimulationStatistics
                {
                    InitialSpawnedCount = 0,
                    RegeneratedSpawnedCount = 0,
                    FoodDecayedCount = 0,
                    OnMapNow = 0,
                    TotalEnergyInEnvironment = 0f,
                    TotalAgeAtDecay = 0f
                });
            }

            // Initialize SimulationSingleton if missing
            if (!SystemAPI.HasSingleton<SimulationSingleton>())
            {
                var singletonEntity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(singletonEntity, new SimulationSingleton { tickSpeed = 1f });
            }
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.HasSingleton<SimulationSingleton>())
            {
                Debug.LogError("SimulationSingleton not found!");
                return;
            }

            var singleton = SystemAPI.GetSingleton<SimulationSingleton>();
            float scaledDeltaTime = SystemAPI.Time.DeltaTime * singleton.tickSpeed;
            _logTimer += scaledDeltaTime;

            if (_logTimer < LOG_INTERVAL)
                return;

            if (!SystemAPI.HasSingleton<SimulationStatistics>())
            {
                Debug.LogError("SimulationStatistics not found!");
                return;
            }

            var stats = SystemAPI.GetSingleton<SimulationStatistics>();

            // Log statistics using the managed system
            var loggingSystem = World.GetExistingSystemManaged<SimulationLoggingSystemManaged>();
            loggingSystem?.LogStatistics(stats);

            // Reset counters
            if (SystemAPI.TryGetSingletonRW<SimulationStatistics>(out var statsRW))
            {
                statsRW.ValueRW.RegeneratedSpawnedCount = 0;
                statsRW.ValueRW.FoodDecayedCount = 0;
            }

            _logTimer = 0f;
        }
    }
}
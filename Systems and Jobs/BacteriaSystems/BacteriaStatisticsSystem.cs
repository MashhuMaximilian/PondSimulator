using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace LakeBacteria.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BacteriaStatisticsSystem : SystemBase
    {

        private float _logTimer;
        private const float LOG_INTERVAL = 3f;

        protected override void OnCreate()
        {
            // Initialize BacteriaStatistics singleton if missing
            if (!SystemAPI.HasSingleton<BacteriaStatistics>())
            {
                var statsEntity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(statsEntity, new BacteriaStatistics
                {
                    SpawnedBacteriaCount = 0,
                    AliveBacteriaCount = 0,
                    DecayedBacteriaCount = 0,
                    TotalEnergyInBacteria = 0f
                });
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

            if (_logTimer >= LOG_INTERVAL)
            {
                _logTimer = 0f;

                // Get spawn count from BacteriaSpawningConfig
                int spawnedCount = 0;
                foreach (var config in SystemAPI.Query<BacteriaSpawningConfig>())
                {
                    spawnedCount = config.SpawnCount;
                    break; // Only need the first one
                }

                // Calculate alive bacteria count and total energy
                int aliveCount = 0;
                float totalEnergy = 0f;

                Entities.ForEach((in Health health, in Energy energy) =>
                {
                    if (health.Value > 0)
                    {
                        aliveCount++;
                        totalEnergy += energy.Value;
                    }

                }).Run();

                int decayedCount = spawnedCount - aliveCount;

                // Update statistics
                var stats = SystemAPI.GetSingletonRW<BacteriaStatistics>().ValueRW;
                stats.SpawnedBacteriaCount = spawnedCount;
                stats.AliveBacteriaCount = aliveCount;
                stats.DecayedBacteriaCount = decayedCount;
                stats.TotalEnergyInBacteria = totalEnergy;

                // Log statistics
                Debug.Log($"[Bacteria] Spawned: {stats.SpawnedBacteriaCount}, " +
                          $"Alive: {stats.AliveBacteriaCount}, " +
                          $"Decayed: {stats.DecayedBacteriaCount}, " +
                          $"Total Energy: {stats.TotalEnergyInBacteria}");
            }
        }
    }
}
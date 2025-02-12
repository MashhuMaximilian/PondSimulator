using Unity.Entities;
using UnityEngine;

namespace LakeBacteria.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
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
}
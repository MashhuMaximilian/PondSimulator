using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using LakeBacteria.Components;


namespace LakeBacteria.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BacteriaEnergySystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<SimulationSingleton>();
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
            const float MIN_ENERGY = 0.01f;

            Entities.ForEach((ref Energy energy, ref Health health, in BacteriaData profile) =>
            {
                // Energy Consumption
                energy.Value -= profile.Speed * (1f - profile.MetabolicEfficiency * 0.1f) * scaledDeltaTime;

                // Mock Food Consumption (add energy periodically)
                if (energy.Value <= MIN_ENERGY)
                {
                    energy.Value = 0f;
                    return;
                }

                // Health Depletion from Starvation
                if (energy.Value <= 0)
                {
                    health.Value -= 1f * scaledDeltaTime;
                }

                // Clamp Health to Zero
                health.Value = math.max(health.Value, 0f);

            }).ScheduleParallel();
        }
    }
}
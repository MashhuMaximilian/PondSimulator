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
    public partial class RandomMovementSystem : SystemBase
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

            // Generate a base seed on the main thread.
            uint baseSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

            // Use the entityInQueryIndex to make each iteration's seed unique.
            Entities.ForEach((int entityInQueryIndex, ref LocalTransform transform, in BacteriaData profile, in Energy energy) =>
            {
                if (energy.Value <= 0) return;

                // Create a new random instance for this iteration.
                var random = new Unity.Mathematics.Random(baseSeed + (uint)entityInQueryIndex);
                var direction = random.NextFloat3(-5f, 5f);
                direction = math.normalize(direction);

                transform.Position += direction * profile.Speed * scaledDeltaTime;
                transform.Position = math.clamp(transform.Position, -50f, 50f);

            }).ScheduleParallel();
        }
    }
}

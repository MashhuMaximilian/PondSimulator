using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

[BurstCompile]

public partial struct BacteriaEnergySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<SimulationSingleton>(out var singleton))
        {
            UnityEngine.Debug.LogError("SimulationSingleton not found!");
            return;
        }

        float scaledDeltaTime = SystemAPI.Time.DeltaTime * singleton.tickSpeed;

        foreach (var (profile, energy, health) in
            SystemAPI.Query<RefRO<BacteriaData>, RefRW<Energy>, RefRW<Health>>())
        {
            // Calculate energy consumption based on speed and metabolic efficiency
            energy.ValueRW.Value -= profile.ValueRO.Speed * (1f - profile.ValueRO.MetabolicEfficiency) * scaledDeltaTime;

            // Reduce health if energy is depleted
            if (energy.ValueRO.Value <= 0)
            {
                health.ValueRW.Value -= 1f * scaledDeltaTime;
            }

            // Clamp health to zero if it goes below zero
            if (health.ValueRO.Value <= 0)
            {
                health.ValueRW.Value = 0;
            }
        }
    }
}


[BurstCompile]
public partial struct BacteriaCleanupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // No additional setup required
    }

    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;

        // Query Health and Energy components with Entity access
        foreach (var (health, energy, entity) in
            SystemAPI.Query<RefRO<Health>, RefRO<Energy>>().WithEntityAccess())
        {
            // Destroy entities with zero or negative health
            if (health.ValueRO.Value <= 0)
            {
                entityManager.DestroyEntity(entity);
            }
        }
    }
}

public struct Energy : IComponentData
{
    public float Value;
}

public struct Health : IComponentData
{
    public float Value;
}
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
public class SimulationControlPanel : MonoBehaviour
{
    public float tickSpeed = 1f; // Exposed in the Inspector
    void Update()
    {
        // Update the SimulationSingleton with the slider's value
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var system = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSingletonUpdater>();
            if (system != null)
            {
                system.tickSpeed = tickSpeed;
            }
        }
    }
}

[BurstCompile]
public partial class SimulationSingletonUpdater : SystemBase
{
    public float tickSpeed;
    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingletonRW<SimulationSingleton>(out var singletonRW))
        {
            Debug.LogError("SimulationSingleton not found!");
            return;
        }
        singletonRW.ValueRW.tickSpeed = tickSpeed;
    }
}
using Unity.Entities;

public struct SimulationSingleton : IComponentData
{
    public float tickSpeed; // This controls the simulation speed
}

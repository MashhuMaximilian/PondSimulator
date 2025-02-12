using Unity.Entities;
/// <summary>
/// Holds various counters and accumulators for the simulation.
/// </summary>
public struct SimulationStatistics : IComponentData
{
    public int SpawnedFoodUnits;
    public int InitialSpawnedCount;
    public int RegeneratedSpawnedCount;
    public int FoodDecayedCount;
    public float TotalEnergyInEnvironment;
    public float TotalAgeAtDecay;
    public int OnMapNow; // New field to track alive food entities
}
/// <summary>
/// A tag to mark the singleton entity that holds SimulationStatistics.
/// </summary>
public struct SimulationStatisticsSingletonTag : IComponentData { }
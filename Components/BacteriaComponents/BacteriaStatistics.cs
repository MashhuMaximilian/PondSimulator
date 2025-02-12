// BacteriaStatistics.cs
using Unity.Entities;

public struct BacteriaStatistics : IComponentData
{
    public int SpawnedBacteriaCount;
    public int AliveBacteriaCount;
    public int DecayedBacteriaCount;
    public float TotalEnergyInBacteria;
}


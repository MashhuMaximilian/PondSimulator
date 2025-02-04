
using Unity.Entities;
using Unity.Mathematics; // Added for float3

public struct FoodSpawningConfig : IComponentData
{
    public Entity FoodPrefab;
    public int MaxFoodUnits;
    public float3 SpawnArea;
    public int HotspotCount;       // Number of clusters
    public float HotspotRadius;    // Cluster size
    public float HotspotDensity;   // Cluster intensity (0-1)
    public float ClusterDensity;
    public int SpawnedFoodUnits;
    public int InitialSpawnedCount;
    public float2 LifespanRange;  // Min and Max lifespan for food entities

}

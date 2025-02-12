using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct FoodSpawningConfig : IComponentData
{
    public FixedString64Bytes FoodPrefabName; // informational only
    public int MaxFoodUnits; // (optional; may be used later)
    public float3 SpawnArea;
    public int HotspotCount;
    public float HotspotRadius;
    public float HotspotDensity;
    public float ClusterDensity;
    public int SpawnedFoodUnits;
    public int InitialSpawnedCount;
    public float2 LifespanRange;
    public Entity PrefabEntity; // legacy field; we use FoodTypeConfig for runtime
}


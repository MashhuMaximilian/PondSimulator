
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct Energy : IComponentData
{
    public float Value;
}

public struct Health : IComponentData
{
    public float Value;
}

public struct BacteriaSpawningConfig : IComponentData
{
    public FixedString64Bytes BacteriaPrefabName; // informational only
    public int SpawnCount;
    public float3 SpawnArea;
    // You can leave the PrefabEntity field unused (or remove it)
    public Entity PrefabEntity;
}


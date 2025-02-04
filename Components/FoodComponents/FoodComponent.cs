using Unity.Entities;
using Unity.Mathematics;

public struct FoodData : IComponentData
{
    public float Energy;
    public float Radius;
    public float Lifespan;
    public float Age;
    public float2 LifespanRange; // Min and Max lifespan range

}

// Tags
public struct FoodTag : IComponentData { }
public struct RegeneratedFoodTag : IComponentData { }
public struct InitialSpawnCompleteTag : IComponentData { } // Singleton
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public static class FoodSpawnHelper
{
    public static void SpawnFood(
        EntityCommandBuffer.ParallelWriter ecb,
        int sortKey,
        Entity foodPrefab,
        float3 position,
        float radius,
        float energy,
        float lifespan,
        float2 lifespanRange
    )
    {
        Entity food = ecb.Instantiate(sortKey, foodPrefab);
        ecb.SetComponent(sortKey, food, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = radius
        });
        ecb.SetComponent(sortKey, food, new FoodData
        {
            Energy = energy,
            Radius = radius,
            Lifespan = lifespan,
            LifespanRange = lifespanRange, // Pass the range
            Age = 0f
        });
    }
}
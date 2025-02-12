using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public static class FoodSpawnHelper
{

    public static void SpawnFood(
        EntityCommandBuffer.ParallelWriter ecb,
        int sortKey,
        Entity foodPrefabEntity, // already converted from the Subscene
        float3 position,
        float radius,
        float energy,
        float lifespan,
        float2 lifespanRange
    )
    {
        if (foodPrefabEntity == Entity.Null)
        {
            Debug.LogError("Food prefab entity is null!");
            return;
        }
        // Instantiate the food entity from the converted prefab.
        Entity food = ecb.Instantiate(sortKey, foodPrefabEntity);
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
            LifespanRange = lifespanRange,
            Age = 0f
        });
    }
}

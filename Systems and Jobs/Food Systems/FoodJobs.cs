
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;           // For IJobParallelFor scheduling
using Unity.Mathematics;
using Unity.Transforms;



[BurstCompile]
public struct SpawnFoodJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float3 SpawnArea;
    public float2 LifespanRange;
    public uint Seed;
    public Entity FoodPrefabEntity;
    [ReadOnly] public NativeArray<float3> Hotspots;
    public float HotspotRadius;
    public float HotspotDensity;
    public float ClusterDensity;
    public float DefaultRadius;

    public void Execute(int index)
    {
        var random = new Unity.Mathematics.Random(Seed + (uint)index);
        float3 position;
        if (Hotspots.Length > 0 && random.NextFloat() < HotspotDensity)
        {
            int hotspotIndex = random.NextInt(0, Hotspots.Length);
            float3 hotspotCenter = Hotspots[hotspotIndex];
            FoodGrouping.GetHotspotPosition(
                ref random,
                ref hotspotCenter,
                HotspotRadius,
                ClusterDensity,
                DefaultRadius,
                out position
            );
        }
        else
        {
            position = random.NextFloat3(-SpawnArea / 2, SpawnArea / 2);
        }

        float lifespan = random.NextFloat(LifespanRange.x, LifespanRange.y);
        FoodSpawnHelper.SpawnFood(ECB, index, FoodPrefabEntity, position, DefaultRadius, 10f, lifespan, LifespanRange);
    }
}

[BurstCompile]
public partial struct DecayJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;
    public NativeCounter.ParallelWriter DecayCounter;

    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref FoodData food, in FoodTag tag)
    {
        float newAge = food.Age + DeltaTime;
        if (newAge >= food.Lifespan)
        {
            ECB.DestroyEntity(chunkIndex, entity);
            DecayCounter.Increment();
        }
        else
        {
            food.Age = newAge;
        }
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float DeltaTime;
    public float Time;
    public float3 SpawnArea;

    public void Execute(ref LocalTransform transform, in FoodData food)
    {
        float3 noiseOffset = new float3(
            noise.snoise(new float2(Time, transform.Position.y)),
            noise.snoise(new float2(transform.Position.x, transform.Position.z)),
            noise.snoise(new float2(transform.Position.z, Time))
        ) * DeltaTime * 0.1f;

        transform.Position += noiseOffset;
        transform.Position = math.clamp(transform.Position, -SpawnArea / 2, SpawnArea / 2);
    }
}



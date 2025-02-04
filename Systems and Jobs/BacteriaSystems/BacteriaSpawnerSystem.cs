using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct BacteriaSpawnerSystem : ISystem
{
    private EntityQuery _bacteriaQuery;

    public void OnCreate(ref SystemState state)
    {
        _bacteriaQuery = SystemAPI.QueryBuilder()
            .WithAll<BacteriaData>()
            .Build();
        state.RequireForUpdate<BacteriaSpawningConfig>();
    }


    public void OnUpdate(ref SystemState state)
    {
        // Use tickSpeed of 1 for now (later you can incorporate a SimulationSingleton)
        float scaledDeltaTime = SystemAPI.Time.DeltaTime * 1f;
        var config = SystemAPI.GetSingleton<BacteriaSpawningConfig>();
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        int existingCount = _bacteriaQuery.CalculateEntityCount();
        int toSpawn = math.max(0, config.SpawnCount - existingCount);
        if (toSpawn > 0)
        {
            var randomSeed = (uint)(Time.realtimeSinceStartup * 1000 + 1);
            var spawnJob = new SpawnBacteriaJob
            {
                ECB = ecb.AsParallelWriter(),
                BacteriaPrefab = config.BacteriaPrefab,
                SpawnArea = config.SpawnArea,
                Seed = randomSeed
            };
            var handle = spawnJob.Schedule(toSpawn, 64, state.Dependency);
            handle.Complete();
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private struct SpawnBacteriaJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity BacteriaPrefab;
        public float3 SpawnArea;
        public uint Seed;

        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random(Seed + (uint)index);
            float3 position = random.NextFloat3(-SpawnArea / 2, SpawnArea / 2);
            var entity = ECB.Instantiate(index, BacteriaPrefab);
            ECB.SetComponent(index, entity, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f
            });
        }
    }
}

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[BurstCompile]
public partial struct FoodLifecycleSystem : ISystem
{
    private EntityQuery _foodQuery;
    private bool _hasInitialSpawned; // Flag to track initial spawn
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _foodQuery = SystemAPI.QueryBuilder().WithAll<FoodTag, FoodData>().Build();
        state.RequireForUpdate<FoodSpawningConfig>();
        _hasInitialSpawned = false; // Initialize the flag
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<FoodSpawningConfig>();
        // Get the scaled delta time using the tickSpeed
        if (!SystemAPI.TryGetSingleton<SimulationSingleton>(out var singleton))
        {
            Debug.LogError("SimulationSingleton not found!");
            return;
        }
        float scaledDeltaTime = SystemAPI.Time.DeltaTime * singleton.tickSpeed;
        var deltaTime = scaledDeltaTime; // Use scaledDeltaTime instead of raw SystemAPI.Time.DeltaTime
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        if (!SystemAPI.TryGetSingletonRW<SimulationStatistics>(out var statsRW))
        {
            Debug.LogWarning("SimulationStatistics singleton not found.");
            return;
        }
        // Food Spawning
        if (!_hasInitialSpawned)
        {
            var spawnCount = math.max(0, config.SpawnedFoodUnits - _foodQuery.CalculateEntityCount());
            if (spawnCount > 0)
            {
                HandleFoodSpawning(ref state, config, spawnCount, ecb, ref statsRW.ValueRW);
                _hasInitialSpawned = true; // Set the flag to true after initial spawn
            }
        }
        // Food Decay
        var decayCounter = new NativeCounter(Allocator.TempJob);
        var decayJob = new DecayJob
        {
            DeltaTime = deltaTime,
            ECB = ecb.AsParallelWriter(),
            DecayCounter = decayCounter.AsParallelWriter()
        };
        state.Dependency = decayJob.ScheduleParallel(state.Dependency);
        // Food Movement
        var movementJob = new MovementJob
        {
            DeltaTime = deltaTime,
            Time = (float)SystemAPI.Time.ElapsedTime
        };
        state.Dependency = movementJob.ScheduleParallel(state.Dependency);
        // Complete jobs and process ECB
        state.Dependency.Complete();
        // Update statistics
        statsRW.ValueRW.FoodDecayedCount += decayCounter.Count;
        statsRW.ValueRW.OnMapNow = _foodQuery.CalculateEntityCount(); // Update alive count
        statsRW.ValueRW.InitialSpawnedCount = config.SpawnedFoodUnits; // Set Initial to SpawnedFoodUnits (constant)
        decayCounter.Dispose();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private void HandleFoodSpawning(ref SystemState state, FoodSpawningConfig config, int spawnCount,
        EntityCommandBuffer ecb, ref SimulationStatistics stats)
    {
        var hotspots = GenerateHotspots(ref state, config.HotspotCount);
        var spawnCounter = new NativeCounter(Allocator.TempJob);
        new SpawnJob
        {
            FoodPrefab = config.FoodPrefab,
            Hotspots = hotspots,
            HotspotRadius = config.HotspotRadius,
            HotspotDensity = config.HotspotDensity,
            ClusterDensity = config.ClusterDensity,
            ECB = ecb.AsParallelWriter(),
            Seed = (uint)SystemAPI.Time.ElapsedTime + 1,
            FoodData = SystemAPI.GetComponent<FoodData>(config.FoodPrefab),
            SpawnCounter = spawnCounter.AsParallelWriter()
        }.Schedule(spawnCount, 64, state.Dependency).Complete();
        stats.InitialSpawnedCount += spawnCounter.Count;
        hotspots.Dispose();
        spawnCounter.Dispose();
    }
    private NativeArray<float3> GenerateHotspots(ref SystemState state, int count)
    {
        var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime + 1);
        var hotspots = new NativeArray<float3>(count, Allocator.TempJob);
        for (int i = 0; i < count; i++)
        {
            hotspots[i] = random.NextFloat3(-50, 50);
        }
        return hotspots;
    }
    [BurstCompile]
    partial struct DecayJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        public NativeCounter.ParallelWriter DecayCounter;
        public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref FoodData food, in FoodTag _)
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
    partial struct MovementJob : IJobEntity
    {
        public float DeltaTime;
        public float Time;
        public void Execute(ref LocalTransform transform, in FoodData food)
        {
            float2 noiseInput = new float2(transform.Position.x, transform.Position.z) * 0.1f;
            float noiseValue = noise.snoise(noiseInput);
            transform.Position.y += noiseValue * DeltaTime * 0.1f;
            transform.Position.x += noise.snoise(new float2(Time, transform.Position.y)) * DeltaTime * 0.1f;
            transform.Position.z += noise.snoise(new float2(Time, transform.Position.x)) * DeltaTime * 0.1f;
            transform.Position = math.clamp(transform.Position, -50, 50);
        }
    }
    [BurstCompile]
    struct SpawnJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Hotspots;
        public float HotspotRadius;
        public float HotspotDensity;
        public float ClusterDensity;
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity FoodPrefab;
        public uint Seed;
        public FoodData FoodData;
        public NativeCounter.ParallelWriter SpawnCounter;
        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random(Seed + (uint)index);
            float3 position;
            if (random.NextFloat() < HotspotDensity && Hotspots.Length > 0)
            {
                int hotspotIndex = random.NextInt(0, Hotspots.Length);
                float3 hotspotCenter = Hotspots[hotspotIndex];
                FoodGrouping.GetHotspotPosition(
                    ref random,
                    ref hotspotCenter,
                    HotspotRadius,
                    ClusterDensity,
                    FoodData.Radius,
                    out position
                );
            }
            else
            {
                position = random.NextFloat3(-50, 50);
            }
            // Create a new FoodData with randomized lifespan
            var foodData = new FoodData
            {
                Energy = FoodData.Energy,
                Radius = FoodData.Radius,
                Lifespan = random.NextFloat(FoodData.LifespanRange.x, FoodData.LifespanRange.y), // Use the range
                LifespanRange = FoodData.LifespanRange, // Pass the range
                Age = 0f // Reset age to 0
            };
            var entity = ECB.Instantiate(index, FoodPrefab);
            ECB.SetComponent(index, entity, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            ECB.SetComponent(index, entity, foodData); // Use the randomized FoodData
            SpawnCounter.Increment();
        }
    }
}
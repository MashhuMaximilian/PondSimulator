using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BacteriaConfigAuthoring : MonoBehaviour
{
    [Header("Bacteria Settings")]
    public GameObject BacteriaPrefab; // Prefab to spawn bacteria
    public int SpawnCount = 10; // Number of bacteria to spawn initially
    public Vector3 SpawnArea = new Vector3(50, 50, 50); // Size of the spawn area

    class Baker : Baker<BacteriaConfigAuthoring>
    {
        public override void Bake(BacteriaConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BacteriaSpawningConfig
            {
                BacteriaPrefab = GetEntity(authoring.BacteriaPrefab, TransformUsageFlags.Dynamic),
                SpawnCount = authoring.SpawnCount,
                SpawnArea = authoring.SpawnArea
            });
        }
    }
}

public struct BacteriaSpawningConfig : IComponentData
{
    public Entity BacteriaPrefab;
    public int SpawnCount;
    public float3 SpawnArea;
}

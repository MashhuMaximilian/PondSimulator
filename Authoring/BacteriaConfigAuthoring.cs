using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using LakeBacteria.Components;


public class BacteriaConfigAuthoring : MonoBehaviour
{
    [Header("Bacteria Spawn Settings")]
    public string BacteriaPrefabName = "Bacillus"; // Informational only
    public int SpawnCount = 10;
    public Vector3 SpawnArea = new Vector3(50, 50, 50);

    class Baker : Baker<BacteriaConfigAuthoring>
    {
        public override void Bake(BacteriaConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BacteriaSpawningConfig
            {
                BacteriaPrefabName = authoring.BacteriaPrefabName,
                SpawnCount = authoring.SpawnCount,
                SpawnArea = authoring.SpawnArea,
                PrefabEntity = Entity.Null // Not used now
            });
        }
    }
}

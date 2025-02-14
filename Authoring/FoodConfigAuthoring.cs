using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using LakeBacteria.Components;

public class FoodConfigAuthoring : MonoBehaviour
{
    [Header("Spawning")]
    public string FoodPrefabName; // informational only
    [Tooltip("Drag your converted food prefab (from the Subscene) here.")]
    public GameObject FoodPrefab; // (This is used only during baking)
    public int SpawnedFoodUnits = 1000;
    public Vector3 SpawnArea = new Vector3(100, 100, 100);

    [Header("Hotspot Settings")]
    [Tooltip("Number of hotspots in the environment")]
    public int HotspotCount = 5;
    [Tooltip("Base radius of each hotspot")]
    public float HotspotRadius = 10f;
    [Tooltip("Probability (0-1) of spawning in hotspots vs. randomly")]
    [Range(0, 1)] public float HotspotDensity = 0.7f;
    [Tooltip("How tightly food clumps in hotspots (lower = tighter)")]
    public float ClusterDensity = 100f;
    public Vector2 LifespanRange = new Vector2(20f, 40f);

    [Header("Food Settings")]
    [Tooltip("Override radius for spawned food prefab")]
    public float SpawnedFoodRadius = 0.01f;

    class Baker : Baker<FoodConfigAuthoring>
    {
        public override void Bake(FoodConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            Entity prefabEntity = Entity.Null;
            if (authoring.FoodPrefab != null)
            {
                var wrapper = authoring.FoodPrefab.GetComponent<EntityLinkWrapper>();
                if (wrapper != null)
                    prefabEntity = wrapper.Entity;
                else
                    Debug.LogError("FoodPrefab is missing the EntityLinkWrapper component.");
            }
            else
            {
                Debug.LogError("FoodPrefab is not assigned in FoodConfigAuthoring.");
            }

            // Include the override radius in the configuration component.
            AddComponent(entity, new FoodSpawningConfig
            {
                FoodPrefabName = authoring.FoodPrefabName,
                SpawnedFoodUnits = authoring.SpawnedFoodUnits,
                SpawnArea = authoring.SpawnArea,
                HotspotCount = authoring.HotspotCount,
                HotspotRadius = authoring.HotspotRadius,
                HotspotDensity = authoring.HotspotDensity,
                ClusterDensity = authoring.ClusterDensity,
                LifespanRange = new float2(authoring.LifespanRange.x, authoring.LifespanRange.y),
                SpawnedFoodRadius = authoring.SpawnedFoodRadius, // <-- New field!
                PrefabEntity = prefabEntity // For legacy use; new code uses FoodTypeConfig.
            });
        }
    }
}

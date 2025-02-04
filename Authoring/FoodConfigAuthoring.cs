using Unity.Entities;
using UnityEngine;

public class FoodConfigAuthoring : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject FoodPrefab;
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
    public float ClusterDensity = 100f; // Renamed for clarity

    class Baker : Baker<FoodConfigAuthoring>
    {
        public override void Bake(FoodConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new FoodSpawningConfig
            {
                FoodPrefab = GetEntity(authoring.FoodPrefab, TransformUsageFlags.Dynamic),
                SpawnedFoodUnits = authoring.SpawnedFoodUnits,
                SpawnArea = authoring.SpawnArea,
                HotspotCount = authoring.HotspotCount,
                HotspotRadius = authoring.HotspotRadius,
                HotspotDensity = authoring.HotspotDensity,
                ClusterDensity = authoring.ClusterDensity // Add this line
            });
        }
    }
}
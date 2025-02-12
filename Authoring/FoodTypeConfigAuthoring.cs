using Unity.Entities;
using UnityEngine;

public class FoodTypeConfigAuthoring : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject SpawnedFoodPrefab;
    public GameObject ExcretedFoodPrefab;
    public GameObject DecayedFoodPrefab;

    class Baker : Baker<FoodTypeConfigAuthoring>
    {

        public override void Bake(FoodTypeConfigAuthoring authoring)
        {
            // Create an entity for this configuration.
            var entity = GetEntity(TransformUsageFlags.None);

            // Convert the prefab GameObjects to entities.
            // (Make sure these prefabs are in a SubScene or converted appropriately.)
            var spawnedEntity = authoring.SpawnedFoodPrefab != null
                ? GetEntity(authoring.SpawnedFoodPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            var excretedEntity = authoring.ExcretedFoodPrefab != null
                ? GetEntity(authoring.ExcretedFoodPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            var decayedEntity = authoring.DecayedFoodPrefab != null
                ? GetEntity(authoring.DecayedFoodPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;

            // Add the configuration component.
            AddComponent(entity, new FoodTypeConfig
            {
                SpawnedFoodPrefab = spawnedEntity,
                ExcretedFoodPrefab = excretedEntity,
                DecayedFoodPrefab = decayedEntity
            });
        }
    }
}

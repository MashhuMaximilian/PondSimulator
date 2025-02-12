using Unity.Entities;

public struct FoodTypeConfig : IComponentData
{
    public Entity SpawnedFoodPrefab;   // The “normal” food prefab
    public Entity ExcretedFoodPrefab;  // (For future use: food that is excreted)
    public Entity DecayedFoodPrefab;   // (For future use: food produced when a bacteria dies)
}


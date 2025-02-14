using Unity.Entities;
using UnityEngine;
// using LakeBacteria.Components;


public class EntityLinkWrapper : MonoBehaviour
{
    public Entity Entity; // This will be assigned during baking

    public class Baker : Baker<EntityLinkWrapper>
    {
        public override void Bake(EntityLinkWrapper authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, authoring);
            // Debug.Log($"Prefab entities: {BacteriaData.ShapeType.Bacillus}, {BacteriaData.ShapeType.Coccus}, {BacteriaData.ShapeType.Spiral}, {BacteriaData.ShapeType.Vibrio}, {BacteriaData.ShapeType.YShape}");
            authoring.Entity = entity;
        }
    }
}


using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FoodAuthoring : MonoBehaviour
{
    public float Energy = 5f;
    public float Radius = 0.01f;
    public float Lifespan = 30f;
    public Vector2 LifespanRange = new Vector2(20f, 40f);

    class Baker : Baker<FoodAuthoring>
    {
        public override void Bake(FoodAuthoring authoring)
        {
            // Adjust the GameObject's scale before conversion.
            authoring.gameObject.transform.localScale = Vector3.one * authoring.Radius;

            // Create the entity.
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FoodPrefabComponent>(entity);
            AddComponent(entity, new Prefab());

            // Set up food data.
            AddComponent(entity, new FoodData
            {
                Energy = authoring.Energy,
                Radius = authoring.Radius,
                Lifespan = authoring.Lifespan,
                LifespanRange = new float2(authoring.LifespanRange.x, authoring.LifespanRange.y),
                Age = 0f
            });
            AddComponent<FoodTag>(entity);

            // Add rendering data.
            var mesh = authoring.GetComponent<MeshFilter>().sharedMesh;
            var material = authoring.GetComponent<MeshRenderer>().sharedMaterial;
            AddSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
        }
    }
}

public struct FoodPrefabComponent : IComponentData { }

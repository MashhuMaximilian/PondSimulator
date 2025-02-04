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
    public Vector2 LifespanRange = new Vector2(20f, 40f); // Min and Max lifespan range


    class Baker : Baker<FoodAuthoring>
    {
        public override void Bake(FoodAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LocalTransform
            {
                Position = authoring.transform.position,
                Rotation = authoring.transform.rotation,
                Scale = authoring.Radius
            });
            AddComponent(entity, new FoodData
            {
                Energy = authoring.Energy,
                Radius = authoring.Radius,
                Lifespan = authoring.Lifespan, // Use the base lifespan (randomization happens in SpawnJob)
                LifespanRange = new float2(authoring.LifespanRange.x, authoring.LifespanRange.y), // Pass the range


                Age = 0f
            });
            AddComponent<FoodTag>(entity);
            var mesh = authoring.GetComponent<MeshFilter>().sharedMesh;
            var material = authoring.GetComponent<MeshRenderer>().sharedMaterial;
            AddSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
        }
    }
}
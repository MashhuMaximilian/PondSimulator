/* The BacteriaAuthoring class in C# is used to define the properties and behaviors of bacteria
entities in an ECS system, including gene data, mesh rendering, and entity linking. */
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using LakeBacteria.Components;
using Unity.Rendering; // Now using RenderMeshArray from com.unity.entities.graphics@1.3
using UnityEngine.Rendering;


public class BacteriaAuthoring : MonoBehaviour
{
    [Header("Genes")]
    public float Speed = 0.5f;
    public float MetabolicEfficiency = 0.5f;
    public float Sturdiness = 0.5f;
    public float SensorRadius = 0.5f;
    public float RadiationResistance = 0.5f;
    public float ReproductiveCost = 0.5f;
    public float AggressionBias = 0.5f;
    public float ClusterPreference = 0.5f;
    public float MutationRate = 0.05f;

    class Baker : Baker<BacteriaAuthoring>
    {
        public override void Bake(BacteriaAuthoring authoring)
        {
            // Get the entity for this GameObject (using dynamic transform)
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            // Debug.Log($"Baking bacteria prefab for {authoring.gameObject.name}, Entity: {entity}");

            // Mark this as a bacteria prefab and add the ECS Prefab component.
            AddComponent<BacteriaPrefabComponent>(entity);
            AddComponent<Prefab>(entity);

            // Set the default bacteria gene data.
            AddComponent(entity, new BacteriaData
            {
                Speed = Mathf.Clamp(authoring.Speed, 0.35f, 0.75f),
                MetabolicEfficiency = Mathf.Clamp(authoring.MetabolicEfficiency, 0.0f, 0.9f),
                Sturdiness = Mathf.Clamp(authoring.Sturdiness, 0.0f, 1.0f),
                SensorRadius = Mathf.Clamp(authoring.SensorRadius, 0.0f, 1.0f),
                RadiationResistance = Mathf.Clamp(authoring.RadiationResistance, 0.0f, 1.0f),
                ReproductiveCost = Mathf.Clamp(authoring.ReproductiveCost, 0.0f, 1.0f),
                AggressionBias = Mathf.Clamp(authoring.AggressionBias, 0.0f, 1.0f),
                ClusterPreference = Mathf.Clamp(authoring.ClusterPreference, 0.0f, 1.0f),
                MutationRate = Mathf.Clamp(authoring.MutationRate, 0.0f, 1.0f),
                shapeType = BacteriaData.ShapeType.Coccus // Default shape
            });

            AddComponent(entity, new Energy { Value = 100f });
            AddComponent(entity, new Health { Value = 100f });

            // Retrieve mesh and material from the GameObject.
            var mesh = authoring.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                mesh.RecalculateBounds(); // Ensure correct bounds
                Bounds adjustedBounds = mesh.bounds;

                // Expand bounds to avoid culling issues
                adjustedBounds.Expand(100.0f); // Increase this factor if needed
                mesh.bounds = adjustedBounds;

                // Debug.Log($"{authoring.gameObject.name} recalculated bounds: {mesh.bounds}");
            }

            var material = authoring.GetComponent<MeshRenderer>()?.sharedMaterial;
            if (mesh != null && material != null)
            {
                // Create a RenderMeshArray with a single-element array.
                var renderMeshArray = new RenderMeshArray(
                    meshes: new Mesh[] { mesh },
                    materials: new Material[] { material }
                );
                AddSharedComponentManaged(entity, renderMeshArray);
                // Debug.Log($"RenderMeshArray assigned to {authoring.gameObject.name}, Entity: {entity}");

            }
            else
            {
                Debug.LogError($"Mesh or Material missing on bacteria prefab: {authoring.gameObject.name}");
            }

            // Link the converted prefab with its GameObject (for Hybrid ECS debugging).
            var linkWrapper = authoring.gameObject.GetComponent<EntityLinkWrapper>();
            if (linkWrapper != null)
            {
                linkWrapper.Entity = entity;
                // Debug.Log($"EntityLinkWrapper assigned to {authoring.gameObject.name}: Entity: {entity}");

            }
            else
            {
                Debug.LogError("EntityLinkWrapper missing on bacteria prefab!");
            }

            // Set a debug color based on gene values.
            Color color = new Color(authoring.AggressionBias, 0f, 0f, 1f);
            color = Color.Lerp(color, Color.black, 1f - authoring.RadiationResistance);
            AddComponent(entity, new DebugColor { Value = new float4(color.r, color.g, color.b, color.a) });
        }
    }
}

// A marker component to flag bacteria prefabs.
public struct BacteriaPrefabComponent : IComponentData { }

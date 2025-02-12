using Unity.Entities;
using UnityEngine;
using LakeBacteria.Components;

public class BacteriaShapeConfigAuthoring : MonoBehaviour
{
    [Header("Bacteria Shape Prefabs (Converted)")]
    public GameObject BacillusPrefab;
    public GameObject CoccusPrefab;
    public GameObject SpiralPrefab;
    public GameObject VibrioPrefab;
    public GameObject YShapePrefab;

    class Baker : Baker<BacteriaShapeConfigAuthoring>
    {
        public override void Bake(BacteriaShapeConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Retrieve the converted prefab entities from their GameObjects.
            Entity bacillus = authoring.BacillusPrefab != null
                ? GetEntity(authoring.BacillusPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            Entity coccus = authoring.CoccusPrefab != null
                ? GetEntity(authoring.CoccusPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            Entity spiral = authoring.SpiralPrefab != null
                ? GetEntity(authoring.SpiralPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            Entity vibrio = authoring.VibrioPrefab != null
                ? GetEntity(authoring.VibrioPrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;
            Entity yshape = authoring.YShapePrefab != null
                ? GetEntity(authoring.YShapePrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;

            // Log errors if any prefab is missing.
            if (coccus == Entity.Null)
                Debug.LogError("CoccusPrefab not assigned or conversion failed!");
            if (spiral == Entity.Null)
                Debug.LogError("SpiralPrefab not assigned or conversion failed!");
            if (vibrio == Entity.Null)
                Debug.LogError("VibrioPrefab not assigned or conversion failed!");
            if (yshape == Entity.Null)
                Debug.LogError("YShapePrefab not assigned or conversion failed!");

            AddComponent(entity, new BacteriaShapeConfig
            {
                BacillusPrefab = bacillus,
                CoccusPrefab = coccus,
                SpiralPrefab = spiral,
                VibrioPrefab = vibrio,
                YShapePrefab = yshape
            });
        }
    }
}

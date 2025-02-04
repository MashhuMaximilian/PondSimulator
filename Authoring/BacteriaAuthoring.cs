using Unity.Entities;
using UnityEngine;

public class BacteriaAuthoring : MonoBehaviour
{
    [Header("Genes")]
    public float Speed = 10f;
    public float MetabolicEfficiency = 10f;
    public float Sturdiness = 10f;
    public float SensorRadius = 10f;
    public float RadiationResistance = 10f;
    public float ReproductiveCost = 20f;
    public float AggressionBias = 10f;
    public float ClusterPreference = 10f;
    public float MutationRate = 10f;

    public class Baker : Baker<BacteriaAuthoring>
    {
        public override void Bake(BacteriaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add the gene component
            AddComponent(entity, new BacteriaData
            {
                Speed = authoring.Speed,
                MetabolicEfficiency = authoring.MetabolicEfficiency,
                Sturdiness = authoring.Sturdiness,
                SensorRadius = authoring.SensorRadius,
                RadiationResistance = authoring.RadiationResistance,
                ReproductiveCost = authoring.ReproductiveCost,
                AggressionBias = authoring.AggressionBias,
                ClusterPreference = authoring.ClusterPreference,
                MutationRate = authoring.MutationRate,
                shapeType = 0, // Placeholder for future use
                flagellaType = 0, // Placeholder for future use
                piliType = 0 // Placeholder for future use
            });

            // Add initial energy and health
            AddComponent(entity, new Energy { Value = 100f });
            AddComponent(entity, new Health { Value = 100f });
        }
    }
}
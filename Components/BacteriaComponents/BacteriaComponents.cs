using Unity.Entities;
using Unity.Mathematics;

namespace LakeBacteria.Components
{
    public struct DebugColor : IComponentData
    {
        public float4 Value;
    }


    public struct Energy : IComponentData
    {
        public float Value;
    }

    public struct Health : IComponentData
    {
        public float Value;
    }

    // Add these new components here:
    public struct BacteriaVisualTag : IComponentData
    {
        // Marks an entity as having a visual representation
    }

    public struct EntityLink : IComponentData
    {
        public Entity Entity; // Links the GameObject to the ECS entity
    }

    public struct BacteriaData : IComponentData
    {
        public float Speed;
        public float MetabolicEfficiency;
        public float Sturdiness;
        public float SensorRadius;
        public float RadiationResistance;
        public float ReproductiveCost;
        public float AggressionBias;
        public float ClusterPreference;
        public float MutationRate;
        public ShapeType shapeType;

        public enum ShapeType
        {
            Coccus,
            YShape,
            Bacillus,
            Vibrio,
            Spiral
        }
    }
}
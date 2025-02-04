using Unity.Entities;

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
    public int shapeType; // 0 = None; later can be an enum
    public int flagellaType; // 0 = None; later can be an enum
    public int piliType; // 0 = None; later can be an enum


}

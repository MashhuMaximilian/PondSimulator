using Unity.Entities;
using LakeBacteria.Components; // Adjust namespace as needed

public static class BacteriaVisuals
{
    /// <summary>
    /// Determines the shape type based on the gene values in BacteriaData.
    /// </summary>
    public static BacteriaData.ShapeType DetermineShapeType(in BacteriaData data)
    {
        if (data.MetabolicEfficiency >= 0.7f)
            return BacteriaData.ShapeType.Coccus;
        else if (data.AggressionBias < 0.6f && data.Speed < 0.5f)
            return BacteriaData.ShapeType.Spiral;
        else if (data.MetabolicEfficiency >= 0.4f && data.MetabolicEfficiency <= 0.7f && data.AggressionBias < 0.6f)
            return BacteriaData.ShapeType.Vibrio;
        else if (data.ClusterPreference >= 0.7f && data.SensorRadius >= 0.7f)
            return BacteriaData.ShapeType.YShape;
        else
            return BacteriaData.ShapeType.Bacillus;
    }

    /// <summary>
    /// Returns the prefab Entity corresponding to the given shape type using the provided config.
    /// </summary>
    public static Entity GetPrefabForShape(in BacteriaShapeConfig shapeConfig, BacteriaData.ShapeType shape)
    {
        return shape switch
        {
            BacteriaData.ShapeType.Bacillus => shapeConfig.BacillusPrefab,
            BacteriaData.ShapeType.Coccus => shapeConfig.CoccusPrefab,
            BacteriaData.ShapeType.Spiral => shapeConfig.SpiralPrefab,
            BacteriaData.ShapeType.Vibrio => shapeConfig.VibrioPrefab,
            BacteriaData.ShapeType.YShape => shapeConfig.YShapePrefab,
            _ => Entity.Null
        };
    }

}

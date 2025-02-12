using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;



[BurstCompile]
public static class FoodGrouping
{
    [BurstCompile]
    public static void GetHotspotPosition(
      ref Random random,
    ref float3 hotspotCenter,
    float hotspotRadius,
    float clusterDensity,
    float foodRadius, // Add foodRadius parameter
    out float3 result
    )
    {// Calculate effective radius with inversion (higher clusterDensity = sparser)
        float effectiveRadius = hotspotRadius * math.max(0.1f, clusterDensity * 0.1f);

        // Ensure minimal spacing between food units (2 * foodRadius to prevent overlap)
        float minSpacing = 2 * foodRadius;
        effectiveRadius = math.max(effectiveRadius, minSpacing);

        // Generate offset scaled by effectiveRadius
        float3 gaussianOffset = NextGaussianFloat3(ref random) * effectiveRadius;

        result = math.clamp(
            hotspotCenter + gaussianOffset,
            new float3(-50, -50, -50),
            new float3(50, 50, 50)
        );
    }

    private static float3 NextGaussianFloat3(ref Random random)
    {
        return new float3(
            math.sqrt(-2 * math.log(random.NextFloat())) * math.cos(2 * math.PI * random.NextFloat()),
            math.sqrt(-2 * math.log(random.NextFloat())) * math.cos(2 * math.PI * random.NextFloat()),
            math.sqrt(-2 * math.log(random.NextFloat())) * math.cos(2 * math.PI * random.NextFloat())
        );
    }
}
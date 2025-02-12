using Unity.Entities;
using UnityEngine;

/// <summary>
/// Monobehaviour for SimulationStatistics.  
/// This no longer handles singleton creation to prevent duplicate instances.
/// </summary>
public class SimulationStatisticsAuthoring : MonoBehaviour
{
    // If you need to expose fields in the inspector or add additional functionality,
    // you can keep this class. Otherwise, it can be removed entirely.


    // Since we're handling singleton creation in the system, we no longer bake any components.
    // Ensure that no GameObject in the scene has this component attached multiple times.
}
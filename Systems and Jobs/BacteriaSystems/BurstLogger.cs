using Unity.Burst;
using UnityEngine;

public static class BurstLogger
{
    [BurstDiscard]
    public static void Log(string message)
    {
        Debug.Log(message);
    }
}

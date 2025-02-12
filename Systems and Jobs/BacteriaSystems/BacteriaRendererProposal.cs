// using UnityEngine;
// using Unity.Burst;
// public static class BacteriaRenderer
// {
//     [BurstCompile]
//     public static void DrawBacteria(ref RenderMeshArray renderMeshArray, ref LocalTransform transform)
//     {
//         // Use instanced rendering where many quads or simple shapes representing bacteria 
//         // can be batched into fewer draw calls.
//         Graphics.DrawMeshInstanced(renderMeshArray.Get()[0], 0,
//                                    renderMeshArray.GetMaterial()[0],
//                                    new Matrix4x4[] { transform.localToWorldMatrix },
//                                    batchCount: 1, null,
//                                    castShadows: ShadowCastingMode.OFF,
//                                    dynamicBatching: true,
//                                    camera: Camera.main, lightProbeUsage: false);
//     }
// }
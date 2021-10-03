#region

using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.Core.Behaviours;
using Appalachia.Core.Helpers;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

#if UNITY_EDITOR

#endif

namespace Appalachia.Core.Wind
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MeshShadingComponent: InternalMonoBehaviour
    {
        private const string _PRF_PFX = nameof(MeshShadingComponent) + ".";
        private static readonly ProfilerMarker _PRF_Awake = new ProfilerMarker(_PRF_PFX + "Awake");
        private static readonly ProfilerMarker _PRF_Start = new ProfilerMarker(_PRF_PFX + "Start");
        private static readonly ProfilerMarker _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + "OnEnable");
        private static readonly ProfilerMarker _PRF_Update = new ProfilerMarker(_PRF_PFX + "Update");
        private static readonly ProfilerMarker _PRF_LateUpdate = new ProfilerMarker(_PRF_PFX + "LateUpdate");
        private static readonly ProfilerMarker _PRF_OnDisable = new ProfilerMarker(_PRF_PFX + "OnDisable");
        private static readonly ProfilerMarker _PRF_OnDestroy = new ProfilerMarker(_PRF_PFX + "OnDestroy");
        private static readonly ProfilerMarker _PRF_Reset = new ProfilerMarker(_PRF_PFX + "Reset");
        private static readonly ProfilerMarker _PRF_OnDrawGizmos = new ProfilerMarker(_PRF_PFX + "OnDrawGizmos");
        private static readonly ProfilerMarker _PRF_OnDrawGizmosSelected = new ProfilerMarker(_PRF_PFX + "OnDrawGizmosSelected");

        [HideLabel, InlineEditor(Expanded = true), HideReferenceObjectPicker]
        public MeshShadingComponentData componentData;

        private void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
#if UNITY_EDITOR
                try
                {
                    if (componentData == null)
                    {
                        if (PrefabUtility.IsPartOfNonAssetPrefabInstance(gameObject))
                        {
                            var prefabAssetPath = AssetDatabase.GetAssetPath(gameObject);

                            if (string.IsNullOrWhiteSpace(prefabAssetPath))
                            {
                                prefabAssetPath = AssetDatabase.GetAssetPath(gameObject);
                            }

                            if (string.IsNullOrWhiteSpace(prefabAssetPath))
                            {
                                prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
                            }

                            if (string.IsNullOrWhiteSpace(prefabAssetPath))
                            {
                                DebugHelper.LogError($"Could not find asset path for prefab {name}.");

                                return;
                            }

                            componentData = AssetDatabase.LoadAssetAtPath<MeshShadingComponentData>(prefabAssetPath);

                            if (componentData == null)
                            {
                                componentData = MeshShadingComponentData.CreateAndSaveInExisting<MeshShadingComponentData>(
                                    prefabAssetPath,
                                    "Mesh Shading Component Data"
                                );

                                PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.LogException($"Failed to assign mesh shading data to {name}:");

                    return;
                }
#endif
                AssignShadingMetadata();
            }
        }

        private void Start()
        {
            using (_PRF_Start.Auto())
            {
                AssignShadingMetadata();
            }
        }


        private static readonly ProfilerMarker _PRF_AssignShadingMetadata = new ProfilerMarker(_PRF_PFX + nameof(AssignShadingMetadata));
        
        [Button]
        public void AssignShadingMetadata()
        {
            using (_PRF_AssignShadingMetadata.Auto())
            {
                try
                {
                    if (componentData.updateChildMeshes)
                    {
                        var lod = GetComponent<LODGroup>();

                        if (lod != null)
                        {
                            var filters = lod.GetComponentsInChildren<MeshFilter>();

                            var biggestMeshSize = filters.Max(f => f.sharedMesh.vertexCount);
                            var biggestMesh = filters.First(f => f.sharedMesh.vertexCount == biggestMeshSize).sharedMesh;
                            biggestMesh.RecalculateBounds();

                            for (var index = 0; index < filters.Length; index++)
                            {
                                var filter = filters[index];
                                var r = filter.GetComponent<MeshRenderer>();

                                if ((r == null) || (filter == null))
                                {
                                    DebugHelper.LogError($"{gameObject.name}: Could not find mesh data to assign mesh UV shading data to.");
                                    return;
                                }

                                var sharedMesh = filter.sharedMesh;

                                sharedMesh.RecalculateBounds();

                                UpdateRenderer(r, biggestMesh, sharedMesh, componentData.metadata);
                            }

                            lod.RecalculateBounds();
                        }
                        else
                        {
                            var filters = GetComponentsInChildren<MeshFilter>();

                            foreach (var filter in filters)
                            {
                                var r = filter.GetComponent<MeshRenderer>();

                                if ((r == null) || (filter == null))
                                {
                                    DebugHelper.LogError($"{gameObject.name}: Could not find mesh data to assign mesh UV shading data to.");
                                    return;
                                }

                                var sharedMesh = filter.sharedMesh;

                                sharedMesh.RecalculateBounds();

                                UpdateRenderer(r, sharedMesh, sharedMesh, componentData.metadata);
                            }
                        }
                    }
                    else
                    {
                        var r = GetComponent<MeshRenderer>();
                        var filter = GetComponent<MeshFilter>();

                        if ((r == null) || (filter == null))
                        {
                            DebugHelper.LogError($"{gameObject.name}: Could not find mesh data to assign mesh UV shading data to.");
                            return;
                        }

                        var sharedMesh = filter.sharedMesh;
                        sharedMesh.RecalculateBounds();

                        UpdateRenderer(r, sharedMesh, sharedMesh, componentData.metadata);
                    }
                }
                catch (Exception ex)
                {
                    ex.LogException($"{gameObject.name}: Failed to assign mesh shading data to {name}");
                }
            }
        }

        [Button]
        public static void AssignAllShadingMetadata()
        {
            var m1 = FindObjectsOfType<MeshShadingComponent>();

            for (var index = 0; index < m1.Length; index++)
            {
                var m = m1[index];
                m.AssignShadingMetadata();
            }
        }

        private static readonly ProfilerMarker _PRF_UpdateRenderer = new ProfilerMarker(_PRF_PFX + nameof(UpdateRenderer));
        private void UpdateRenderer(Renderer r, Mesh sizingMesh, Mesh mesh, MeshShadingMetadata m)
        {
            using (_PRF_UpdateRenderer.Auto())
            {
                var minMaterialCount = System.Math.Min(r.sharedMaterials.Length, m.submeshMetadata.Count);
                var values = m.Calculate(sizingMesh, componentData.boundsCenterOffsetPercentage);

                for (var submeshIndex = 0; submeshIndex < minMaterialCount; submeshIndex++)
                {
                    var submeshMetadata = m.submeshMetadata[submeshIndex];
                    var submeshValues = values[submeshIndex];
                    var submeshIndices = mesh.GetIndices(submeshIndex).Distinct().ToArray();

                    for (var channelIndex = 0; channelIndex < submeshMetadata.channels.Count; channelIndex++)
                    {
                        var channelMetadata = submeshMetadata.channels[channelIndex];
                        var channel = (int) channelMetadata.channel;
                        var channelValues = submeshValues[channelIndex];

                        var uv = new List<Vector4>();
                        mesh.GetUVs(channel, uv);

                        if (uv.Count != mesh.vertexCount)
                        {
                            uv.Clear();

                            for (var i = 0; i < mesh.vertexCount; i++)
                            {
                                uv.Add(Vector4.zero);
                            }
                        }

                        for (var index = 0; index < submeshIndices.Length; index++)
                        {
                            var i = submeshIndices[index];
                            uv[i] = channelValues;
                        }

                        mesh.SetUVs(channel, uv);
                    }

                    r.sharedMaterials[submeshIndex] = submeshMetadata.material;
                }
            }
        }
    }
}

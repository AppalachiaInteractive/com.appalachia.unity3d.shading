#region

using System;
using System.Collections.Generic;
using Appalachia.Base.Scriptables;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Shading.Dynamic
{
    [CreateAssetMenu(menuName = "Internal/Metadata/Shading/Mesh Shading Metadata", order = 0)]
    public class MeshShadingMetadata : InternalScriptableObject<MeshShadingMetadata>
    {
        public enum MeshChannelValueType
        {
            None,
            MeshLocalCenter,
            MeshBounds,
            Explicit
        }

        public enum MeshDataChannel
        {
            UV = 0,
            UV2 = 1,
            UV3 = 2,
            UV4 = 3,
            UV5 = 4,
            UV6 = 5,
            UV7 = 6,
            UV8 = 7
        }

        public enum MeshFieldValueType
        {
            None,
            MaterialTextureArrayOverride,
            TextureArrayIndex,
            ShaderSet,
            Explicit
        }

        [ListDrawerSettings(IsReadOnly = false)]
        public List<SubmeshShadingMetadata> submeshMetadata = new();

        public Vector4[][] Calculate(Mesh mesh, Vector3 meshCenterOffset)
        {
            var results = new Vector4[submeshMetadata.Count][];

            for (var i = 0; i < submeshMetadata.Count; i++)
            {
                results[i] = submeshMetadata[i].Calculate(mesh, meshCenterOffset);
            }

            return results;
        }

        [Serializable]
        public class SubmeshShadingMetadata
        {
            public int submesh;
            public bool enabled = true;

            [EnableIf(nameof(enabled))]
            public Material material;

            [EnableIf(nameof(enabled))]
            public List<SubmeshShadingChannelMetadata> channels = new();

            public Vector4[] Calculate(Mesh mesh, Vector3 meshCenterOffset)
            {
                var results = new Vector4[channels.Count];

                for (var i = 0; i < results.Length; i++)
                {
                    results[i] = channels[i].Calculate(mesh, meshCenterOffset);
                }

                return results;
            }
        }

        [Serializable]
        public class SubmeshShadingChannelMetadata
        {
            public MeshDataChannel channel;
            public bool enabled = true;

            [EnableIf(nameof(enabled))]
            public MeshChannelValueType channelValueType = MeshChannelValueType.None;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(showExplicit))]
            public Vector4 explicitValue;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(showFields))]
            public SubmeshShadingFieldMetadata x;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(showFields))]
            public SubmeshShadingFieldMetadata y;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(showFields))]
            public SubmeshShadingFieldMetadata z;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(showFields))]
            public SubmeshShadingFieldMetadata w;

            private bool showExplicit => channelValueType == MeshChannelValueType.Explicit;
            private bool showFields => channelValueType == MeshChannelValueType.None;

            public Vector4 Calculate(Mesh mesh, Vector3 meshCenterOffset)
            {
                if (!enabled)
                {
                    return Vector4.zero;
                }

                switch (channelValueType)
                {
                    case MeshChannelValueType.None:

                        var result = Vector4.zero;

                        result.x = x.CalculateValue();
                        result.y = y.CalculateValue();
                        result.z = z.CalculateValue();
                        result.w = w.CalculateValue();

                        return result;

                    case MeshChannelValueType.MeshLocalCenter:

                        var size = mesh.bounds.size;
                        return new Vector3(
                            size.x * meshCenterOffset.x,
                            size.y * meshCenterOffset.y,
                            size.z * meshCenterOffset.z
                        );

                    case MeshChannelValueType.MeshBounds:
                        return mesh.bounds.size;

                    case MeshChannelValueType.Explicit:
                        return explicitValue;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [Serializable]
        public class SubmeshShadingFieldMetadata
        {
            public bool enabled = true;

            [EnableIf(nameof(enabled))]
            public MeshFieldValueType fieldValueType = MeshFieldValueType.None;

            [OnValueChanged(nameof(UpdateArrayConfig))]
            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(ShowConfigArray))]
            public InternalTextureArrayConfig arrayConfig;

            [ReadOnly]
            [ShowIf(nameof(ShowConfigArray))]
            public Texture2DArray textureArray;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(ShowTextureArrayIndex))]
            [PropertyRange(0, nameof(ArrayRangeMax))]
            public int textureArrayIndex;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(ShowShaderTextureSet))]
            public ShaderTextureSet shaderTextureSet;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(ShowShaderTextureSetId))]
            [PropertyRange(0, nameof(ShaderSetMax))]
            public int shaderTextureSetId;

            [EnableIf(nameof(enabled))]
            [ShowIf(nameof(ShowExplicit))]
            public float value;

            [ReadOnly]
            [ShowIf(nameof(ShowShaderTextureSet))]
            [ShowInInspector]
            private string ShaderTextureSetName =>
                shaderTextureSet == null
                    ? string.Empty
                    : shaderTextureSet.NameByIndex(shaderTextureSetId);

            private bool ShowConfigArray =>
                (fieldValueType == MeshFieldValueType.MaterialTextureArrayOverride) ||
                (fieldValueType == MeshFieldValueType.TextureArrayIndex) ||
                (fieldValueType == MeshFieldValueType.ShaderSet);

            private bool ShowTextureArrayIndex =>
                fieldValueType == MeshFieldValueType.TextureArrayIndex;

            private bool ShowShaderTextureSet => fieldValueType == MeshFieldValueType.ShaderSet;

            private bool ShowShaderTextureSetId => fieldValueType == MeshFieldValueType.ShaderSet;

            private bool ShowExplicit => fieldValueType == MeshFieldValueType.Explicit;

            private int ArrayRangeMax =>
                textureArray == null
                    ? 0
                    : arrayConfig == null
                        ? 0
                        : arrayConfig.diffuseArray == null
                            ? 0
                            : arrayConfig.diffuseArray.depth - 1;

            private int ShaderSetMax =>
                shaderTextureSet == null
                    ? 0
                    : shaderTextureSet.textureSets == null
                        ? 0
                        : shaderTextureSet.textureSets.Count == 0
                            ? 0
                            : shaderTextureSet.textureSets.Count - 1;

            private void UpdateArrayConfig()
            {
                textureArray = arrayConfig.diffuseArray;
            }

            public float CalculateValue()
            {
                switch (fieldValueType)
                {
                    case MeshFieldValueType.MaterialTextureArrayOverride:
                    case MeshFieldValueType.TextureArrayIndex:
                        return textureArrayIndex;

                    case MeshFieldValueType.ShaderSet:
                        return shaderTextureSetId;

                    case MeshFieldValueType.Explicit:
                        return value;

                    default:
                        return 0f;
                }
            }
        }
    }
}

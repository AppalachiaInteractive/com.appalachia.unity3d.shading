#region

using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.Base.Scriptables;
using Appalachia.Core.Extensions.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Shading.Dynamic
{
    [CreateAssetMenu(menuName = "Internal/Metadata/Shading/Shader Texture Set", order = 0)]
    public class ShaderTextureSet : InternalScriptableObject<ShaderTextureSet>
    {
/*#if UNITY_EDITOR
        [FoldoutGroup("Arrays", expanded: true)]
        [HorizontalGroup("Arrays/Division", 150), InlineProperty, SmartLabel]
        [VerticalGroup("Arrays/Division/Diffuse")]
        public DynamicTextureArrayDisplay diffuse = new DynamicTextureArrayDisplay();

        [VerticalGroup("Arrays/Division/Diffuse")]
        [OnValueChanged(nameof(OnValueChanged_LockIndices))]
        public bool lockIndices = true;
        
        [HorizontalGroup("Arrays/Division", 150), InlineProperty, SmartLabel]
        public DynamicTextureArrayDisplay normal = new DynamicTextureArrayDisplay();
        
        [HorizontalGroup("Arrays/Division", 150), InlineProperty, SmartLabel]
        public DynamicTextureArrayDisplay maohs = new DynamicTextureArrayDisplay();
        
        private void OnValueChanged_LockIndices()
        {
            if (lockIndices)
            {
                normal.SetTracking(diffuse);
                maohs.SetTracking(diffuse);
            }
            else
            {
                normal.SetTracking(null);
                maohs.SetTracking(null);
            }
        }
#endif*/

        [BoxGroup("Sets")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "subsetName")]
        public List<TextureSet> textureSets = new();

        public string globalFloatArray = "_GLOBAL_SHADER_SET_INDICES";

        [ListDrawerSettings(ShowIndexLabels = true)]
        public float[] globalTextureConfiguration = new float[1000];

        [Button]
        public void UpdateGlobalProperties()
        {
            try
            {
                var setCount = textureSets.Count;

                globalTextureConfiguration = new float[1000];
                globalTextureConfiguration[0] = setCount;

                var metadataOffset = 1 + (setCount * 2);
                var rollingLength = 0;

                for (var i = 0; i < setCount; i++)
                {
                    var startIndex = 1 + (i * 2);

                    var start = metadataOffset + rollingLength;
                    var length = textureSets[i].elements.Count * 2;

                    globalTextureConfiguration[startIndex] = start;
                    globalTextureConfiguration[startIndex + 1] = length;

                    rollingLength += length;
                }

                var index = metadataOffset;
                for (var index1 = 0; index1 < textureSets.Count; index1++)
                {
                    var set = textureSets[index1];
                    for (var i = 0; i < set.elements.Count; i++)
                    {
                        globalTextureConfiguration[index] = set.elements[i].colorArrayIndex;
                        globalTextureConfiguration[index + 1] = set.elements[i].normalArrayIndex;

                        index += 2;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException($"Error while updating image: {ex.Message}");
            }

            Shader.SetGlobalFloatArray(globalFloatArray, globalTextureConfiguration);
        }

        public string NameByIndex(int index)
        {
            if ((index >= textureSets.Count) || (index < 0))
            {
                return "INVALID";
            }

            return textureSets[index].subsetName;
        }

        [Serializable]
        public class TextureSet
        {
            public string subsetName;

            [FoldoutGroup("Elements")]
            [ListDrawerSettings(
                Expanded = true,
                NumberOfItemsPerPage = 5,
                AddCopiesLastElement = true,
                DraggableItems = false
            )]
            public List<ArrayElement> elements = new();

            [FoldoutGroup("Operations")]
            [PropertyOrder(100)]
            [Button]
            public void AddToAll(int i)
            {
                elements.Add(new ArrayElement(i, i));

                Cleanup();
            }

            [FoldoutGroup("Operations")]
            [PropertyOrder(100)]
            [Button]
            public void AddRangeToAll(int start, int end)
            {
                for (var i = start; i <= end; i++)
                {
                    elements.Add(new ArrayElement(i, i));
                }

                Cleanup();
            }

            [FoldoutGroup("Operations")]
            [PropertyOrder(100)]
            [Button]
            public void Cleanup()
            {
                elements = elements.Distinct().ToList();

                elements.Sort();
            }
        }

        [Serializable]
        public class ArrayElement : IComparable<ArrayElement>, IComparable, IEquatable<ArrayElement>
        {
            [HorizontalGroup("Indices")]
            public int colorArrayIndex;

            [HorizontalGroup("Indices")]
            public int normalArrayIndex;

            public ArrayElement(int colorArrayIndex, int normalArrayIndex)
            {
                this.colorArrayIndex = colorArrayIndex;
                this.normalArrayIndex = normalArrayIndex;
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 1;
                }

                if (ReferenceEquals(this, obj))
                {
                    return 0;
                }

                return obj is ArrayElement other
                    ? CompareTo(other)
                    : throw new ArgumentException($"Object must be of type {nameof(ArrayElement)}");
            }

            public int CompareTo(ArrayElement other)
            {
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                if (ReferenceEquals(null, other))
                {
                    return 1;
                }

                var colorArrayIndexComparison = colorArrayIndex.CompareTo(other.colorArrayIndex);
                if (colorArrayIndexComparison != 0)
                {
                    return colorArrayIndexComparison;
                }

                return normalArrayIndex.CompareTo(other.normalArrayIndex);
            }

            public bool Equals(ArrayElement other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return (colorArrayIndex == other.colorArrayIndex) &&
                       (normalArrayIndex == other.normalArrayIndex);
            }

            public static bool operator <(ArrayElement left, ArrayElement right)
            {
                return Comparer<ArrayElement>.Default.Compare(left, right) < 0;
            }

            public static bool operator >(ArrayElement left, ArrayElement right)
            {
                return Comparer<ArrayElement>.Default.Compare(left, right) > 0;
            }

            public static bool operator <=(ArrayElement left, ArrayElement right)
            {
                return Comparer<ArrayElement>.Default.Compare(left, right) <= 0;
            }

            public static bool operator >=(ArrayElement left, ArrayElement right)
            {
                return Comparer<ArrayElement>.Default.Compare(left, right) >= 0;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((ArrayElement) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (colorArrayIndex * 397) ^ normalArrayIndex;
                }
            }

            public static bool operator ==(ArrayElement left, ArrayElement right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ArrayElement left, ArrayElement right)
            {
                return !Equals(left, right);
            }
        }
    }
}

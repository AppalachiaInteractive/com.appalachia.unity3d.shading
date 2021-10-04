#region

using Appalachia.Base.Scriptables;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Shading.Dynamic
{
    public class MeshShadingComponentData : EmbeddedScriptableObject<MeshShadingComponentData>
    {
        [HideLabel]
        [InlineEditor(Expanded = true)]
        public MeshShadingMetadata metadata;

        public Vector3 boundsCenterOffsetPercentage;
        public bool updateChildMeshes;
    }
}

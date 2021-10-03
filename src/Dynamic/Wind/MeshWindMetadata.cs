#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Scriptables;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Core.Wind
{
    [CreateAssetMenu(menuName = "Internal/Metadata/Shading/Mesh Wind Metadata", order = 0)]
    public class MeshWindMetadata : InternalScriptableObject<MeshWindMetadata>
    {
        [Range(0f, 1f)] public float generalMotionXZInfluence = .35f;

        public float generalMotionNoiseScale = 3f;
        public float leafMotionNoiseScale = 3f;
        public float variationNoiseScale = 3f;

        [MinMaxSlider(0f, 1f)] public Vector2 generalMotionNoiseRange = new Vector2(0f, 1f);
        [MinMaxSlider(0f, 1f)] public Vector2 leafMotionNoiseRange = new Vector2(0f,    .2f);
        [MinMaxSlider(0f, 1f)] public Vector2 variationNoiseRange = new Vector2(0f,     1f);

        public List<MeshWindMaterialMatchGroup> materialMatches = new List<MeshWindMaterialMatchGroup>();

        public Color baseColor = new Color(.5f, 0, .5f, .5f);

        [MinMaxSlider(0f, 1f)] public Vector2 grassFadeR = new Vector2(0f, 1f);
        [MinMaxSlider(0f, 1f)] public Vector2 grassFadeB = new Vector2(0f, .2f);
        [MinMaxSlider(0f, 1f)] public Vector2 grassFadeA = new Vector2(0f, 1f);

        [Serializable]
        public class MeshWindMaterialMatchGroup
        {
            public string setName;
            public Color vertexColor = new Color(1f, 0f, .1f, 1f);
            public List<Material> materials = new List<Material>();
        }

        [Serializable]
        public class MeshWindModel
        {
            public Mesh mesh;
            public List<Material> materials = new List<Material>();
        }
    }
}

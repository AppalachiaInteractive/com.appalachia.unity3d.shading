#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Scriptables;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Appalachia.Core.Wind
{
    [CreateAssetMenu(menuName = "Internal/Metadata/Shading/Texture Array Config", order = 1)]
    public class InternalTextureArrayConfig : InternalScriptableObject<InternalTextureArrayConfig>
    {
        public enum AllTextureChannel
        {
            R = 0,
            G,
            B,
            A,
            Custom
        }

        public enum Compression
        {
            AutomaticCompressed,
            ForceDXT,
            ForcePVR,
            ForceETC2,
            ForceASTC,
            ForceCrunch,
            Uncompressed
        }

        public enum PackingMode
        {
            Fastest,
            Quality
        }

        public enum SourceTextureSize
        {
            Unchanged,
            k32 = 32,
            k256 = 256
        }

        public enum TextureChannel
        {
            R = 0,
            G,
            B,
            A
        }

        public enum TextureSize
        {
            k4096 = 4096,
            k2048 = 2048,
            k1024 = 1024,
            k512 = 512,
            k256 = 256,
            k128 = 128,
            k64 = 64,
            k32 = 32
        }

        private static List<InternalTextureArrayConfig> sAllConfigs = new List<InternalTextureArrayConfig>();
        [HideInInspector] public bool uiOpenTextures = true;
        [HideInInspector] public bool uiOpenOutput = true;
        [HideInInspector] public bool uiOpenImporter = true;

        [HideInInspector] public string extDiffuse = "_diff";
        [HideInInspector] public string extHeight = "_height";
        [HideInInspector] public string extNorm = "_norm";
        [HideInInspector] public string extSmoothness = "_smoothness";
        [HideInInspector] public string extAO = "_ao";

        public TextureSize diffuseTextureSize = TextureSize.k1024;
        public Compression diffuseCompression = Compression.AutomaticCompressed;
        public FilterMode diffuseFilterMode = FilterMode.Bilinear;
        public int diffuseAnisoLevel = 1;

        public TextureSize normalSAOTextureSize = TextureSize.k1024;
        public Compression normalCompression = Compression.AutomaticCompressed;
        public FilterMode normalFilterMode = FilterMode.Trilinear;
        public int normalAnisoLevel = 1;

        [HideInInspector] public int hash;

        [HideInInspector] public Texture2DArray diffuseArray;

        [HideInInspector] public Texture2DArray normalSAOArray;

        // default settings, and overrides
        public InternalTextureArraySettingsGroup defaultTextureSettings = new InternalTextureArraySettingsGroup();
        public List<PlatformTextureOverride> platformOverrides = new List<PlatformTextureOverride>();

        public SourceTextureSize sourceTextureSize = SourceTextureSize.Unchanged;

        [HideInInspector] public AllTextureChannel allTextureChannelHeight = AllTextureChannel.G;

        [HideInInspector] public AllTextureChannel allTextureChannelSmoothness = AllTextureChannel.G;

        [HideInInspector] public AllTextureChannel allTextureChannelAO = AllTextureChannel.G;

        [HideInInspector] public List<InternalTextureEntry> sourceTextures = new List<InternalTextureEntry>();

        private void Awake()
        {
            sAllConfigs.Add(this);
        }

        private void OnDestroy()
        {
            sAllConfigs.Remove(this);
        }

#if UNITY_EDITOR
        public static List<T> FindAssetsByType<T>()
            where T : Object
        {
            var assets = new List<T>();
            var guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).ToString().Replace("UnityEngine.", "")));
            for (var i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
#endif

        public static InternalTextureArrayConfig FindConfig(Texture2DArray diffuse)
        {
#if UNITY_EDITOR
            if (sAllConfigs.Count == 0)
            {
                sAllConfigs = FindAssetsByType<InternalTextureArrayConfig>();
            }
#endif

            for (var i = 0; i < sAllConfigs.Count; ++i)
            {
                if (sAllConfigs[i].diffuseArray == diffuse)
                {
                    return sAllConfigs[i];
                }
            }

            return null;
        }

        [Serializable]
        public class InternalTextureArraySettings
        {
            public TextureSize textureSize;
            public Compression compression;
            public FilterMode filterMode;

            [Range(0, 16)] public int Aniso;

            public InternalTextureArraySettings(TextureSize s, Compression c, FilterMode f, int a = 1)
            {
                textureSize = s;
                compression = c;
                filterMode = f;
                Aniso = a;
            }
        }

        [Serializable]
        public class InternalTextureArraySettingsGroup
        {
            public InternalTextureArraySettings diffuseSettings = new InternalTextureArraySettings(
                TextureSize.k1024,
                Compression.AutomaticCompressed,
                FilterMode.Bilinear
            );

            public InternalTextureArraySettings normalSettings = new InternalTextureArraySettings(
                TextureSize.k1024,
                Compression.AutomaticCompressed,
                FilterMode.Bilinear
            );
        }

        [Serializable]
        public class PlatformTextureOverride
        {
#if UNITY_EDITOR
            public BuildTarget platform = BuildTarget.StandaloneWindows;
#endif
            public InternalTextureArraySettingsGroup settings = new InternalTextureArraySettingsGroup();
        }

        [Serializable]
        public class InternalTextureEntry
        {
            public Texture2D diffuse;
            public Texture2D height;
            public TextureChannel heightChannel = TextureChannel.G;
            public Texture2D normal;
            public Texture2D smoothness;
            public TextureChannel smoothnessChannel = TextureChannel.G;
            public bool isRoughness;
            public Texture2D ao;
            public TextureChannel aoChannel = TextureChannel.G;

            public void Reset()
            {
                diffuse = null;
                height = null;
                normal = null;
                smoothness = null;
                ao = null;
                isRoughness = true;
                heightChannel = TextureChannel.G;
                smoothnessChannel = TextureChannel.G;
                aoChannel = TextureChannel.G;
            }

            public bool HasTextures()
            {
                return (diffuse != null) || (height != null) || (normal != null) || (smoothness != null) || (ao != null);
            }
        }
    }
}

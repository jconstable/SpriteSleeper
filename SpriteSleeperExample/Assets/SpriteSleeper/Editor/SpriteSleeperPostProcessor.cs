using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.U2D;
using SpriteSleeper;

// When a SpriteAtlas is imported, rebuild the config file
namespace SpriteSleeperEditor
{
    public class SpriteSleeperPostProcessor : AssetPostprocessor
    {
        private static string ResourcesIdentifier = "/Resources/";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<string> allAssets = new List<string>();
            allAssets.AddRange(importedAssets);
            allAssets.AddRange(deletedAssets);
            allAssets.AddRange(movedAssets);
            allAssets.AddRange(movedFromAssetPaths);

            foreach (var assetPath in allAssets)
            {
                if (assetPath.EndsWith(".spriteatlas"))
                {
                    RebuildAtlasCache();
                    return;
                }
            }
        }

        static SpriteAtlas[] FindAtlasesInResources()
        {
            List<SpriteAtlas> resourceAtlases = new List<SpriteAtlas>();

            SpriteAtlas[] atlases = Resources.FindObjectsOfTypeAll<SpriteAtlas>();

            foreach (var atlas in atlases)
            {
                var atlasPath = AssetDatabase.GetAssetPath(atlas);
                int resourcesIndex = atlasPath.IndexOf(ResourcesIdentifier);
                if (resourcesIndex > -1)
                {
                    resourceAtlases.Add(atlas);
                }
            }
            return resourceAtlases.ToArray();
        }

        static void RebuildAtlasCache()
        {
            SpriteAtlas[] atlases = FindAtlasesInResources();

            SpriteAtlasList list = new SpriteAtlasList();
            list.Atlases = new SpriteAtlasList.AtlasInfo[atlases.Length];

            int usedAtlasPairs = 0;
            foreach (var atlas in atlases)
            {
                var atlasPath = AssetDatabase.GetAssetPath(atlas);
                int resourcesIndex = atlasPath.IndexOf(ResourcesIdentifier);
                if (resourcesIndex > -1)
                {
                    SpriteAtlasList.AtlasInfo info = new SpriteAtlasList.AtlasInfo();
                    info.AtlasID = usedAtlasPairs;
                    info.AtlasTag = atlas.tag;
                    int startIndex = resourcesIndex + 1 + ResourcesIdentifier.Length;

                    string relativePath = atlasPath.Substring(startIndex - 1, atlasPath.Length - startIndex + 1);
                    info.ResourcesPath = relativePath.Substring(0, relativePath.IndexOf("."));

                    list.Atlases[usedAtlasPairs] = info;

                    usedAtlasPairs++;
                }
            }

            string filePath = SpriteSleeperManager.GetSpriteSleeperDataPath();
            if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            System.IO.File.WriteAllText(filePath, JsonUtility.ToJson(list));
            AssetDatabase.Refresh();
        }
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using SpriteSleeper;

namespace SpriteSleeperEditor
{
    [CustomEditor(typeof(SpriteSleeperManager))]
    public class SpriteSleeperManagerInspector : Editor
    {
        public SpriteSleeperManager Target
        {
            get
            {
                return (SpriteSleeperManager)target;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpriteSleeperManager myTarget = Target;

            var loadListeners = (myTarget.OnAtlasLoaded == null) ? 0 : myTarget.OnAtlasLoaded.GetInvocationList().Length;
            EditorGUILayout.LabelField("Load listeners: " + loadListeners.ToString());

            var unloadListeners = (myTarget.OnAtlasUnloaded == null) ? 0 : myTarget.OnAtlasUnloaded.GetInvocationList().Length;
            EditorGUILayout.LabelField("Unload listeners: " + unloadListeners.ToString());

            EditorGUILayout.LabelField("Managed canvases: " + myTarget.NumCanvasesPresent.ToString());

            var loadedInfos = myTarget.LoadedAtlases;
            EditorGUILayout.LabelField("Loaded atlases: " + loadedInfos.Count.ToString());
            foreach (var info in loadedInfos)
            {
                EditorGUILayout.LabelField(string.Format("   {0} : ({1} sprites), Ref: {2}", info.Tag, myTarget.NumSpritesByTag(info.Tag), info.RefCount.ToString()));
            }

            var infos = myTarget.AtlasInfo;
            EditorGUILayout.LabelField("Configured atlases: " + infos.Length.ToString());
            foreach ( var info in infos )
            {
                EditorGUILayout.LabelField(string.Format("   {0} : {1}", info.AtlasTag, info.ResourcesPath));
            }

            EditorUtility.SetDirty(myTarget);
        }

        void OnEnable()
        {
            Target.WantRepaint += this.Repaint;
        }

        // And then detach on disable.
        void OnDisable()
        {
            Target.WantRepaint -= this.Repaint;
        }
    }
}
#endif
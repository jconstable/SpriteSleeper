using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using System;

namespace SpriteSleeper
{
#if UNITY_EDITOR
    public
#endif
    class LoadedAtlasInfo
    {
        public SpriteAtlas Atlas = null;
        public int RefCount = 0;
        public string Tag = null;
        public string FirstSpriteName = null;
    }

    // Manager that controls all loaded and references SpriteAtlas objects
    // Multiple dictionaries allow for fast looking, based on contextual information
    // LoadedAtlasInfo tracks refCount for each atlas
    // Images in the hierarchy use their texture to find out what Atlas they are in
    public class SpriteSleeperManager : MonoBehaviour
    {
        // Path to config file
        public static string GetSpriteSleeperDataPath()
        {
            return Application.streamingAssetsPath + "/SpriteSleeperData.txt";
        }

        // Singleton setup
        private static SpriteSleeperManager _instance;
        private static bool _destroyed = false;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static SpriteSleeperManager Instance()
        {
            if (_instance == null && !_destroyed)
            {
                GameObject spriteSleeperGameObject = new GameObject("SpriteSleeperManager");
                GameObject.DontDestroyOnLoad(spriteSleeperGameObject);
                spriteSleeperGameObject.hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _instance = spriteSleeperGameObject.AddComponent<SpriteSleeperManager>();
            }

            return _instance;
        }

        // Delegate function for atlas load callbacks
        public delegate void OnAtlasTagDelegate(string tag);

        // Delegate to register for notification on atlases being unloaded
        public OnAtlasTagDelegate OnAtlasTagUnloaded;

        // Delegate to register for notification on atlases being loaded
        public OnAtlasTagDelegate OnAtlasTagLoaded;

        // Private variable for manager state
        private HashSet<SpriteSleeperCanvas> _canvases;
        private SpriteAtlasList.AtlasInfo[] _configInfo = null;
        private Dictionary<Texture2D, LoadedAtlasInfo> _textureToInfo;
        private Dictionary<string, LoadedAtlasInfo> _tagToInfo;
        private Dictionary<string, Dictionary<string, Sprite>> _tagToSprites;
        private bool _atlasLoadedThisFrame = false;

        // Constructor
        SpriteSleeperManager()
        {
            _canvases = new HashSet<SpriteSleeperCanvas>();
            _textureToInfo = new Dictionary<Texture2D, LoadedAtlasInfo>();
            _tagToInfo = new Dictionary<string, LoadedAtlasInfo>();
            _tagToSprites = new Dictionary<string, Dictionary<string, Sprite>>();
        }
        
        void Awake()
        {
            LoadAtlasData();

            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        private void LateUpdate()
        {
            // If we received atlas callbacks this frame, canvases need to know
            if( _atlasLoadedThisFrame )
            {
                _atlasLoadedThisFrame = false;
                OnAtlasLoaded();
            }

            // Manage the late update for each canvas
            foreach( var canvas in _canvases)
            {
                canvas.DoLateUpdate();
            }

#if UNITY_EDITOR
            // Redraw the custom inspector
            if(WantRepaint != null)
            {
                WantRepaint();
            }
#endif
        }

        // Called when a new canvas is created
        public void AddCanvas(SpriteSleeperCanvas canvas)
        {
            _canvases.Add(canvas);
        }

        // Called when a canvas is destroyed
        public void RemoveCanvas(SpriteSleeperCanvas canvas)
        {
            _canvases.Remove(canvas);
        }

        // Load config data
        void LoadAtlasData()
        {
            string dataPath = GetSpriteSleeperDataPath();
            if (!System.IO.File.Exists(dataPath))
            {
                throw new System.Exception("ERROR! No SpriteSleeper data exists! Please reimport your sprite atlases.");
            }
            string sleeperDataJson = System.IO.File.ReadAllText(dataPath);
            _configInfo = JsonUtility.FromJson<SpriteAtlasList>(sleeperDataJson).Atlases;
        }

        // Organize the information we need from a loaded atlas
        private Texture2D SetupAtlasContents(string tag, SpriteAtlas atlas, LoadedAtlasInfo info)
        {
            Texture2D texture = null;
            Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
            if (atlas.spriteCount > 0)
            {
                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);

                texture = sprites[0].texture;

                foreach (var sprite in sprites)
                {
                    string spriteName = sprite.name;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        info.FirstSpriteName = spriteName;
                        spriteDict[spriteName] = sprite;
                    }
                }
            }

            _tagToSprites[tag] = spriteDict;

            return texture;
        }

        // Attempt to load an atlas
        private LoadedAtlasInfo LoadAtlas(string tag)
        {
            // If the atlas is already loaded, return the info
            LoadedAtlasInfo info;
            if(_tagToInfo.TryGetValue(tag, out info))
            {
                return info;
            }

            // Find the requests atlas, by tag, in the config
            foreach (var atlasInfo in _configInfo)
            {
                if (atlasInfo.AtlasTag.Equals(tag))
                {
                    SpriteAtlas atlas = Resources.Load<SpriteAtlas>(atlasInfo.ResourcesPath);
                    if (atlas == null)
                    {
                        throw new Exception("Unable to load SpriteAtlas at path: " + atlasInfo.ResourcesPath);
                    }
                    
                    // Fill out a new info object
                    info = new LoadedAtlasInfo();
                    info.Atlas = atlas;
                    info.Tag = tag;
                    info.RefCount = 0;

                    Texture2D texture = SetupAtlasContents(tag, atlas, info);

                    // Two dictionaries to link to LoadedAtlasInfo
                    _textureToInfo[texture] = info;
                    _tagToInfo[tag] = info;

                    // Call delegate, if assigned
                    if (OnAtlasTagLoaded != null)
                    {
                        OnAtlasTagLoaded(tag);
                    }

                    return info;
                }
            }

            return null;
        }

        // Using a texture2D, find the atlas tag
        public string GetTag(Texture2D texture)
        {
            LoadedAtlasInfo info;
            if (!_textureToInfo.TryGetValue(texture, out info))
            {
                return null;
            }
            return info.Tag;
        }

        // Using the tag and the sprite name, find the sprite object
        public Sprite GetSprite(string tag, string spriteName)
        {
            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            Dictionary<string, Sprite> nameToSprite;
            if (!_tagToSprites.TryGetValue(tag, out nameToSprite))
            {
                return null;
            }

            Sprite sprite;
            if (nameToSprite.TryGetValue(spriteName, out sprite))
            {
                return sprite;
            }

            return null;
        }

        // Increase the ref counter for the given tag
        public void RefTag(string tag)
        {
            ChangeRef(tag, 1);
        }

        // Decrease the ref counter for the given tag
        public void UnrefTag(string tag)
        {
            ChangeRef(tag, -1);
        }

        // Modify the ref count. If it hits zero, unload the atlas.
        void ChangeRef(string tag, int value)
        {
            if (tag == null)
                return;

            LoadedAtlasInfo info;
            if (!_tagToInfo.TryGetValue(tag, out info))
            {
                info = LoadAtlas(tag);
            }

            // Images will not add refs when the awake, so floor at 0
            info.RefCount = Mathf.Max(info.RefCount + value, 0);

            // Atlas needs to be unloaded
            if (info.RefCount == 0)
            {
                UnloadAtlas(info);
            }
        }

        // Clean up
        private void OnDestroy()
        {
            _tagToInfo = null;
            _tagToSprites = null;
            _textureToInfo = null;
            _canvases = null;
            _destroyed = true;
        }

        // Unload the atlas, as well as our tracking of it
        void UnloadAtlas(LoadedAtlasInfo info)
        {
            _tagToInfo.Remove(info.Tag);

            Dictionary<string,Sprite> sprites;
            if(_tagToSprites.TryGetValue(info.Tag, out sprites))
            {
                if(!string.IsNullOrEmpty(info.FirstSpriteName))
                {
                    Sprite sprite;
                    if(sprites.TryGetValue(info.FirstSpriteName, out sprite))
                    {
                        _textureToInfo.Remove(sprite.texture);
                        Resources.UnloadAsset(sprite.texture);
                    }
                    sprites.Clear();
                }
            }
            _tagToSprites.Remove(info.Tag);

            Resources.UnloadAsset(info.Atlas);

            info.Atlas = null;
            if( OnAtlasTagUnloaded!= null )
            {
                OnAtlasTagUnloaded(tag);
            }
        }

        // Unity callback when a sprite requests a linked atlas
        void OnAtlasRequested(string tag, System.Action<SpriteAtlas> action)
        {
            Debug.Log("Atlas requested: " + tag);

            var atlasInfo = LoadAtlas(tag);

            if (atlasInfo != null)
            {
                _atlasLoadedThisFrame = true;
                action(atlasInfo.Atlas);
            }
        }

        // Interal callback when an atlas loads. Allow images to re-query for their atlas tag.
        void OnAtlasLoaded()
        {
            foreach( var canvas in _canvases)
            {
                canvas.FindTags();
            }
        }

#if UNITY_EDITOR
        // Accessors for the custom inspector
        public int NumCanvasesPresent { get { return _canvases.Count; } }

        public List<LoadedAtlasInfo> LoadedAtlases
        {
            get
            {
                List<LoadedAtlasInfo> infos = new List<LoadedAtlasInfo>();
                foreach (var pair in _tagToInfo)
                {
                    infos.Add(pair.Value);
                }
                return infos;
            }
        }

        public int NumSpritesByTag(string tag) { return _tagToSprites[tag].Count; }

        public SpriteAtlasList.AtlasInfo[] AtlasInfo { get { return _configInfo; } }

        public delegate void RepaintAction();

        public RepaintAction WantRepaint;
#endif
    }
}
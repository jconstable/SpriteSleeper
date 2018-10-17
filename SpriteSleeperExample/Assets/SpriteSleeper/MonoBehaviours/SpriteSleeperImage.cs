using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteSleeper
{
    // MonoBehaviour that should be attached to any Image that should be allowed to sleep
    public class SpriteSleeperImage : MonoBehaviour
    {
        // Public variables, possibly for serialization
        public string Tag = null;
        public string SpriteName = null;

        // Private variables
        private Image _image = null;
        private SpriteSleeperManager _manager = null;

        public void Start()
        {
            _image = GetComponent<Image>();
            _manager = SpriteSleeperManager.Instance();

            _manager.OnAtlasLoaded += FindTag;

            FindTag();

            Invoke("StopListeningForAtlases", 0.0001f);
        }

        private void OnDestroy()
        {
            StopListeningForAtlases();
        }

        private void StopListeningForAtlases()
        {
            // If we haven't found a tag by now, we probably won't
            if (string.IsNullOrEmpty(Tag))
            {
                _manager.OnAtlasLoaded -= FindTag;
            }
        }

        private void FindTag()
        {
            if (string.IsNullOrEmpty(Tag) && _image != null && _manager != null && !_manager.Equals(null))
            {
                Sprite sprite = _image.sprite;
                if (sprite != null)
                {
                    SpriteName = sprite.name + "(Clone)"; // This is dumb, but it's how the sprites are named when loaded from Resources

                    Texture2D texture = sprite.texture;
                    if (texture != null)
                    {
                        string tag = _manager.GetTag(texture);
                        if (tag != null)
                        {
                            StopListeningForAtlases();
                            Tag = tag;

                            _manager.RefTag(Tag);
                        }
                    }
                }
            }
        }

        public void Sleep()
        {
            if (!string.IsNullOrEmpty(Tag))
            {
                if (_manager != null && !_manager.Equals(null))
                {
                    _manager.UnrefTag(Tag);
                    _image.sprite = null;
                }
            }
        }

        public void Wake()
        {
            if (!string.IsNullOrEmpty(Tag))
            {
                if (_manager != null && !_manager.Equals(null))
                {
                    _manager.RefTag(Tag);
                    _image.sprite = _manager.GetSprite(Tag, SpriteName);
                }
            }
        }
    }
}
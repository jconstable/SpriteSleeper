using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteSleeper
{
    // MonoBehaviour that should be attached to any Image that should be allowed to sleep
    public class SpriteSleeperImage : MonoBehaviour
    {
        private static WaitForSeconds s_waitTime = new WaitForSeconds(5f);

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

            if (_image != null)
            {
                FindTag();
            }

            // Remove the callback from the manager in 5 sec if we still haven't found a tag
            if (string.IsNullOrEmpty(Tag))
            {
                StartCoroutine(GiveUpOnFindingTag());
            }
        }

        private void OnDestroy()
        {
            StopListeningForAtlases();
        }

        IEnumerator GiveUpOnFindingTag()
        {
            yield return s_waitTime;

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
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
        private TagStateValue _tagState = TagStateValue.NoTag;

        // The current state of the tag
        public TagStateValue TagState { get { return _tagState; } }

        public enum TagStateValue
        {
            HasTag,
            NoTag,
            NeverTag
        }

        public void Start()
        {
            _image = GetComponent<Image>();
            _manager = SpriteSleeperManager.Instance();
            if (_image != null)
            {
                FindTag();
            }
        }

        public void FindTag()
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
                        Tag = _manager.GetTag(texture);
                        if (Tag != null)
                        {
                            _tagState = TagStateValue.HasTag;
                        } else
                        {
                            _tagState = TagStateValue.NoTag;
                        }
                    }
                    else
                    {
                        _tagState = TagStateValue.NoTag;
                    }
                }
                else
                {
                    _tagState = TagStateValue.NeverTag;
                }
            }
        }

        public void Sleep()
        {
            if (_tagState == TagStateValue.HasTag)
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
            if (_tagState == TagStateValue.HasTag)
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
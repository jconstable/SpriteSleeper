using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteSleeper
{
    // MonoBehaviour that shouild be attached to Canvas GameObjects that allow for sleeping
    public class SpriteSleeperCanvas : MonoBehaviour
    {
        // Private variables
        private Canvas _canvas;
        private List<SpriteSleeperImage> _spriteSleepers;
        private SleepState _currentSleepState = SleepState.Sleeping;
        private SpriteSleeperManager _manager;

        // The current state of the canvas
        private enum SleepState
        {
            Sleeping,
            Awake
        }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();

            _manager = SpriteSleeperManager.Instance();

            if (_manager != null && !_manager.Equals(null))
            {
                _manager.AddCanvas(this);

                if (_canvas == null)
                {
                    Debug.LogError("SpriteSleeperCanvas is unable to find a Canvas component on the current object. Ensure that this component is added to the GameObject containing the Canvas.");
                    return;
                }

                _spriteSleepers = new List<SpriteSleeperImage>();
                RefreshImageList();
            }
        }

        private void OnDestroy()
        {
            if(_manager != null && !_manager.Equals(null))
                _manager.RemoveCanvas(this);
        }

        // Re-find all the images in the hierarchy under this GameObject
        void RefreshImageList()
        {
            _spriteSleepers.Clear();
            
            var images = gameObject.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                // Find or add the SpriteSleeperImage component
                var sleeperImage = image.GetComponent<SpriteSleeperImage>();
                if (sleeperImage == null)
                {
                    sleeperImage = image.gameObject.AddComponent<SpriteSleeperImage>();
                }

                _spriteSleepers.Add(sleeperImage);
            }
        }

        // Query the state of the actual Canvas component, and update as needed
        public void DoLateUpdate() {
            if (_canvas != null && !_canvas.Equals(null))
            {
                // If only there were callbacks for components being enabled or disabled
                SleepState state = (_canvas.isActiveAndEnabled) ? SleepState.Awake : SleepState.Sleeping;
                if (state != _currentSleepState)
                {
                    _currentSleepState = state;
                    if (state == SleepState.Awake)
                    {
                        Wake();
                    }
                    else
                    {
#if UNITY_EDITOR
                        if( !_canvas.gameObject.activeInHierarchy )
                        {
                            Debug.LogWarning("Did you know it's more efficient to disable the Canvas component than to deactivate the GameObject? Read more here https://unity3d.com/learn/tutorials/topics/best-practices/other-ui-optimization-techniques-and-tips");
                        }
#endif
                        Sleep();
                    }
                }
            }
        }

        void Sleep()
        {
            foreach (var sleeper in _spriteSleepers)
            {
                sleeper.Sleep();
            }
        }

        void Wake()
        {
            foreach (var sleeper in _spriteSleepers)
            {
                sleeper.Wake();
            }
        }
    }
}
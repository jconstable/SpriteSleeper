using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSleeper
{
    // Simple class for serializing the sleepable sprite atlases references
    [System.Serializable]
    public class SpriteAtlasList
    {

        [System.Serializable]
        public class AtlasInfo
        {
            public int AtlasID;
            public string AtlasTag;
            public string ResourcesPath;
            public string[] SpriteNames;
        }

        public AtlasInfo[] Atlases;
    }
}
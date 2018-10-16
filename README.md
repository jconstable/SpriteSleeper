# SpriteSleeper
For Unity applications that run on low-memory devices (mobile, mostly), this package allows for UGUI hierarchies to remain in memory, without paying the cost for their SpriteAtlases.

Loading UI prefabs at runtime can be expensive on low-end devices. UGUI performs a rebuild to create the Canvas geometry, which is costly and can cause hitching. Ideally, loaded UI screens should not be unloaded when dismissed, and should instead be hidden by disabling its Canvas component. However, disabled Canvases will keep their SpriteAtlases in memory, which could be expensive.

This package aims to allow the developer to choose which UI screens should keep their SpriteAtlases alive, and which can unload them when dismissed.

By default, each Canvas will maintain default functionality. To add this new functionality to a Canvas, add a SpriteSleeperCanvas component to the GameObject that contains this Canvases. Note: SpriteAtlas assets that can be managed by this library currently need to reside in a Resources folder.

We recommend splitting up your Sprite Atlases accordingly, if possible, so that multiple UI screens won't prevent Atlas unloading. Boilerplate UI Atlases are not a good candidate for this use case.

TODO:
- Add option on SpriteSleeperCanvas to strip Atlas references from scenes at build time
- Support Asset Bundles
- Support Addressables 
- Make call to Resources.UnloadUnusedAssets() optional
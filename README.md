# SpriteSleeper
For Unity applications that run on low-memory devices (mobile, mostly), this package allows for UGUI hierarchies to remain in memory, without paying the memory cost for their SpriteAtlases' textures.

Loading UI prefabs at runtime can be expensive on low-end devices. UGUI performs a rebuild to create the Canvas geometry, which is costly and can cause hitching. Ideally, loaded UI screens should not be unloaded when dismissed, and should instead be hidden by disabling their Canvas component. However, disabled Canvases will keep their SpriteAtlases in memory, which could be expensive.

This package aims to allow the developer to choose which UI screens should keep their SpriteAtlases alive, and which can unload them when dismissed.

By default, each Canvas will maintain default functionality. To add this new functionality to a Canvas, add a SpriteSleeperCanvas component to the Canvas GameObject. Note: SpriteAtlas assets that can be managed by this library currently need to reside in a Resources folder. Once the application is running in the Editor, verify that atlases are being tracked and loaded properly using the inspector on the SpriteSleeperManager object.

If possible, we recommend splitting up your Sprite Atlases such that Sprites aren't unnecessarily shared across multiple screens. Boilerplate UI Atlases are not good candidates for SpriteSleeper.

TODO:
- Add option on SpriteSleeperCanvas to strip Atlas references from scenes at build time
- Support Asset Bundles
- Support Addressables 
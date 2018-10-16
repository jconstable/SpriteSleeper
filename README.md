# SpriteSleeper
For Unity applications that run on low-memory devices (mobile, mostly), allow for UGUI hierarchies to remain in memory, without paying the cost for their SpriteAtlases.

To use, add a SpriteSleeperCanvas to the GameObject that contains one of your Canvases. Note: SpriteAtlas assets currently need to reside in a Resources folder in order for this to work.

TODO:
- Add option on SpriteSleeperCanvas to strip Atlas references from scenes at build time
- Support Asset Bundles
- Support Addressables 
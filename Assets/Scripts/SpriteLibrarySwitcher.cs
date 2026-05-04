using UnityEngine;
using UnityEngine.U2D.Animation;

public class SpriteLibrarySwitcher : MonoBehaviour
{
    [SerializeField]
    private SpriteLibrary spriteLibrary;

    [SerializeField]
    private SpriteLibraryAsset newLibraryAsset;

    private void Start()
    {
        if (newLibraryAsset != null)
        {
            ChangeSpriteLibrary(newLibraryAsset);
        }
    }

    public void ChangeSpriteLibrary(SpriteLibraryAsset newAsset)
    {
        if (spriteLibrary == null)
        {
            Debug.LogError("SpriteLibrary not assigned!");
            return;
        }

        if (newAsset == null)
        {
            Debug.LogError("New SpriteLibraryAsset is null!");
            return;
        }

        spriteLibrary.spriteLibraryAsset = newAsset;
        spriteLibrary.RefreshSpriteResolvers(); // Applique les changements
    }
}

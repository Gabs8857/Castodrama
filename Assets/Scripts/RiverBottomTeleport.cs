using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private GameObject fondRivièreObject;
    [SerializeField] private GameObject rivièreUpdateObject;
    [SerializeField] private GameObject tilemapGeneralObject;
    [SerializeField] private GameObject bouléauObject;
    [SerializeField] private GameObject peuplierObject;
    [SerializeField] private GameObject sauleObject;
    [SerializeField] private GameObject rivRivière1Object;
    private bool isInRiverBottomZone = false;
    private bool eKeyPressedLastFrame = false;

    private void Start()
    {
        // Auto-find if not assigned
        if (tilemapGeneralObject == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Tilemap") && obj.name.Contains("update"))
                {
                    tilemapGeneralObject = obj;
                    break;
                }
            }
        }

        // Auto-find trees if not assigned
        if (bouléauObject == null)
            bouléauObject = GameObject.Find("Bouleau");
        if (peuplierObject == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("peuptree"))
                {
                    peuplierObject = obj;
                    break;
                }
            }
        }
        if (sauleObject == null)
            sauleObject = GameObject.Find("Saule");

        // Auto-find surface river if not assigned
        if (rivRivière1Object == null)
        {
            rivRivière1Object = GameObject.Find("RivRivière update (1)");
            if (rivRivière1Object != null)
                Debug.Log("[RiverBottomTeleport] ✓ Found RivRivière update (1)");
            else
                Debug.Log("[RiverBottomTeleport] ⚠ Could not find RivRivière update (1)");
        }

        // Activer RivRivière update (1) au démarrage (surface)
        if (rivRivière1Object != null)
        {
            rivRivière1Object.SetActive(true);
            Debug.Log("[RiverBottomTeleport] ✓ Activated RivRivière update (1) at start");
        }
        else
            Debug.Log("[RiverBottomTeleport] ⚠ RivRivière update (1) is NULL at start");

        if (rivièreUpdateObject != null)
            rivièreUpdateObject.SetActive(true);
        
        if (fondRivièreObject != null)
            fondRivièreObject.SetActive(false);
        
        if (tilemapGeneralObject != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneralObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = true;
            Debug.Log("[RiverBottomTeleport] Entered zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = false;
            Debug.Log("[RiverBottomTeleport] Exited zone");
        }
    }

    private void Update()
    {
        if (isInRiverBottomZone && Keyboard.current != null)
        {
            bool eKeyPressed = Keyboard.current.eKey.isPressed;
            if (eKeyPressed && !eKeyPressedLastFrame)
            {
                Debug.Log("[RiverBottomTeleport] ✓ E pressed - rising to surface!");
                HandleWaterSceneTransition();
            }
            eKeyPressedLastFrame = eKeyPressed;
        }
        else
        {
            eKeyPressedLastFrame = false;
        }
    }

    public void HandleWaterSceneTransition()
    {
        Debug.Log("[RiverBottomTeleport] HandleWaterSceneTransition called - returning to surface");
        // Désactive le deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StopSwimmingDeep();
            Debug.Log("[RiverBottomTeleport] ✓ Stopped deep swimming animation");
        }

        if (fondRivièreObject != null)
        {
            fondRivièreObject.SetActive(false);
            Debug.Log("[RiverBottomTeleport] ✓ Deactivated fondRivièreObject");
        }

        if (rivièreUpdateObject != null)
        {
            rivièreUpdateObject.SetActive(true);
            Debug.Log("[RiverBottomTeleport] ✓ Activated rivièreUpdateObject");
        }

        // Réactiver RivRivière update (1) en remontant (surface réapparaît)
        if (rivRivière1Object != null)
        {
            rivRivière1Object.SetActive(true);
            Debug.Log("[RiverBottomTeleport] ✓ Activated RivRivière update (1)");
        }
        else
            Debug.Log("[RiverBottomTeleport] ⚠ Could not activate RivRivière update (1) - NULL");

        if (tilemapGeneralObject != null)
        {
            tilemapGeneralObject.SetActive(true);
            Debug.Log("[RiverBottomTeleport] ✓ Activated tilemapGeneralObject (Tilemap update)");
        }

        // Rendre visibles les arbres (activer les SpriteRenderers)
        if (bouléauObject != null)
        {
            foreach (SpriteRenderer renderer in bouléauObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
            Debug.Log("[RiverBottomTeleport] ✓ Enabled Bouleau sprites");
        }

        // Réactiver TOUS les peuptree
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("peuptree"))
            {
                foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>())
                    renderer.enabled = true;
                Debug.Log($"[RiverBottomTeleport] ✓ Enabled {obj.name} sprites");
            }
        }

        if (sauleObject != null)
        {
            foreach (SpriteRenderer renderer in sauleObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
            Debug.Log("[RiverBottomTeleport] ✓ Enabled Saule sprites");
        }
    }
}
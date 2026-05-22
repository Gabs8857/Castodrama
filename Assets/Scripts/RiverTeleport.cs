using UnityEngine;
using UnityEngine.InputSystem;

public class RiverTeleport : MonoBehaviour
{
    [SerializeField] private GameObject fondRivièreObject;
    [SerializeField] private GameObject rivièreUpdateObject;
    [SerializeField] private GameObject tilemapGeneralObject;
    [SerializeField] private GameObject bouléauObject;
    [SerializeField] private GameObject peuplierObject;
    [SerializeField] private GameObject sauleObject;
    [SerializeField] private GameObject rivRivière1Object;
    private bool isInRiverZone = false;
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
                Debug.Log("[RiverTeleport] ✓ Found RivRivière update (1)");
            else
                Debug.Log("[RiverTeleport] ⚠ Could not find RivRivière update (1)");
        }

        // Activer RivRivière update (1) au démarrage (surface)
        if (rivRivière1Object != null)
        {
            rivRivière1Object.SetActive(true);
            Debug.Log("[RiverTeleport] ✓ Activated RivRivière update (1) at start");
        }
        else
            Debug.Log("[RiverTeleport] ⚠ RivRivière update (1) is NULL at start");

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
            isInRiverZone = true;
            Debug.Log("[RiverTeleport] Entered zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = false;
            Debug.Log("[RiverTeleport] Exited zone");
        }
    }

    private void Update()
    {
        if (isInRiverZone && Keyboard.current != null)
        {
            bool eKeyPressed = Keyboard.current.eKey.isPressed;
            if (eKeyPressed && !eKeyPressedLastFrame)
            {
                Debug.Log("[RiverTeleport] ✓ E pressed - deep swim!");
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
        Debug.Log("[RiverTeleport] HandleWaterSceneTransition called - activating deep swim mode");

        // Activer le deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StartSwimmingDeep();
            Debug.Log("[RiverTeleport] ✓ Started deep swimming animation");
        }

        if (fondRivièreObject != null)
        {
            fondRivièreObject.SetActive(true);
            Debug.Log("[RiverTeleport] ✓ Activated fondRivièreObject");
        }

        if (rivièreUpdateObject != null)
        {
            rivièreUpdateObject.SetActive(false);
            Debug.Log("[RiverTeleport] ✓ Deactivated rivièreUpdateObject");
        }

        // Désactiver RivRivière update (1) en descendant (fin de la surface)
        if (rivRivière1Object != null)
        {
            rivRivière1Object.SetActive(false);
            Debug.Log("[RiverTeleport] ✓ Deactivated RivRivière update (1)");
        }
        else
            Debug.Log("[RiverTeleport] ⚠ Could not deactivate RivRivière update (1) - NULL");

        if (tilemapGeneralObject != null)
        {
            tilemapGeneralObject.SetActive(false);
            Debug.Log("[RiverTeleport] ✓ Deactivated tilemapGeneralObject (Tilemap update)");
        }

        // Rendre invisibles les arbres (désactiver les SpriteRenderers)
        if (bouléauObject != null)
        {
            foreach (SpriteRenderer renderer in bouléauObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
            Debug.Log("[RiverTeleport] ✓ Disabled Bouleau sprites");
        }

        // Désactiver TOUS les peuptree
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("peuptree"))
            {
                foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>())
                    renderer.enabled = false;
                Debug.Log($"[RiverTeleport] ✓ Disabled {obj.name} sprites");
            }
        }

        if (sauleObject != null)
        {
            foreach (SpriteRenderer renderer in sauleObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
            Debug.Log("[RiverTeleport] ✓ Disabled Saule sprites");
        }
    }
}
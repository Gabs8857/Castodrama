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
    private float teleportCooldownUntil = 0f;
    private const float TELEPORT_COOLDOWN = 0.5f;
    private float forceZoneEntryUntil = 0f;
    private const float FORCE_ZONE_ENTRY_DURATION = 1f;
    private bool shouldInitializeDeepState = true; // Flag pour éviter que Start() réactive les arbres

    private void Awake()
    {
        // Early initialization for tree objects to ensure they're found
        if (bouléauObject == null)
            bouléauObject = GameObject.Find("Bouleau");
        if (sauleObject == null)
            sauleObject = GameObject.Find("Saule");
        
        Debug.Log($"[RiverTeleport.Awake] Found Bouleau: {(bouléauObject != null ? "YES" : "NO")}, Saule: {(sauleObject != null ? "YES" : "NO")}");
    }

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
        // Ignore interact during teleport cooldown
        if (Time.time < teleportCooldownUntil)
        {
            eKeyPressedLastFrame = false;
            return;
        }

        if (isInRiverZone)
        {
            bool interactHeld = InputHelper.InteractHeld();
            if (interactHeld && !eKeyPressedLastFrame)
            {
                Debug.Log("[RiverTeleport] ✓ Interact held - deep swim!");
                HandleWaterSceneTransition();
                teleportCooldownUntil = Time.time + TELEPORT_COOLDOWN;
            }
            eKeyPressedLastFrame = interactHeld;
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
            
            // Force le RiverBottomTeleport à reconnaître que le joueur est dans la zone
            RiverBottomTeleport bottomTeleport = fondRivièreObject.GetComponent<RiverBottomTeleport>();
            if (bottomTeleport != null)
            {
                bottomTeleport.ForceZoneEntry(); // This sets shouldInitializeSurfaceState = false
                Debug.Log("[RiverTeleport] ✓ Forced RiverBottomTeleport zone entry");
            }
            else
            {
                Debug.LogWarning("[RiverTeleport] ⚠ RiverBottomTeleport component not found on fondRivièreObject!");
            }
        }
        else
        {
            Debug.LogError("[RiverTeleport] ✗ fondRivièreObject is NULL!");
        }

        if (rivièreUpdateObject != null)
        {
            rivièreUpdateObject.SetActive(false);
            Debug.Log("[RiverTeleport] ✓ Deactivated rivièreUpdateObject");
        }
        else
        {
            Debug.LogError("[RiverTeleport] ✗ rivièreUpdateObject is NULL!");
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
        else
        {
            Debug.LogError("[RiverTeleport] ✗ tilemapGeneralObject is NULL!");
        }

        // Rendre invisibles les arbres (désactiver les SpriteRenderers)
        int disabledCount = 0;
        
        if (bouléauObject != null)
        {
            foreach (SpriteRenderer renderer in bouléauObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = false;
                disabledCount++;
            }
            Debug.Log($"[RiverTeleport] ✓ Disabled {disabledCount} Bouleau sprites");
        }
        else
        {
            Debug.LogWarning("[RiverTeleport] ⚠ bouléauObject is NULL!");
        }

        // Désactiver TOUS les peuptree
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int peuptreeCount = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("peuptree"))
            {
                foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = false;
                    peuptreeCount++;
                }
                Debug.Log($"[RiverTeleport] ✓ Disabled {obj.name} sprites ({peuptreeCount} total)");
            }
        }
        if (peuptreeCount == 0)
            Debug.LogWarning("[RiverTeleport] ⚠ No peuptree objects found!");

        if (sauleObject != null)
        {
            int sauleCount = 0;
            foreach (SpriteRenderer renderer in sauleObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = false;
                sauleCount++;
            }
            Debug.Log($"[RiverTeleport] ✓ Disabled {sauleCount} Saule sprites");
        }
        else
        {
            Debug.LogWarning("[RiverTeleport] ⚠ sauleObject is NULL!");
        }
        
        Debug.Log("[RiverTeleport] ✓✓ Deep swim transition complete!");
    }
}
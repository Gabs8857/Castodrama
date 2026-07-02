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
    private float teleportCooldownUntil = 0f;
    private const float TELEPORT_COOLDOWN = 0.5f;
    private float forceZoneEntryUntil = 0f;
    private const float FORCE_ZONE_ENTRY_DURATION = 1f;
    private bool shouldInitializeSurfaceState = true; // Flag pour éviter que Start() réactive les arbres

    private void Awake()
    {
        // Early initialization for tree objects to ensure they're found
        if (bouléauObject == null)
            bouléauObject = GameObject.Find("Bouleau");
        if (sauleObject == null)
            sauleObject = GameObject.Find("Saule");
        
        Debug.Log($"[RiverBottomTeleport.Awake] Found Bouleau: {(bouléauObject != null ? "YES" : "NO")}, Saule: {(sauleObject != null ? "YES" : "NO")}");
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
                Debug.Log("[RiverBottomTeleport] ✓ Found RivRivière update (1)");
            else
                Debug.Log("[RiverBottomTeleport] ⚠ Could not find RivRivière update (1)");
        }

        // Only initialize surface state on first Start() call (actual initialization)
        if (!shouldInitializeSurfaceState)
        {
            Debug.Log("[RiverBottomTeleport] Skipping surface state initialization (already in deep swim mode)");
            return;
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
            Debug.Log("[RiverBottomTeleport] ✓✓ ENTERED zone - E-key should now work!");
        }
        else
        {
            Debug.Log($"[RiverBottomTeleport] OnTriggerEnter2D: {collision.name} (no TopDownPlayerController)");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Ignore exits pendant FORCE_ZONE_ENTRY_DURATION après un ForceZoneEntry
        if (Time.time < forceZoneEntryUntil)
        {
            Debug.Log($"[RiverBottomTeleport] Exit ignored (forceZoneEntry active for {forceZoneEntryUntil - Time.time:F2}s more)");
            return;
        }

        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = false;
            Debug.Log("[RiverBottomTeleport] ✓✓ EXITED zone");
        }
    }

    /// <summary>
    /// Force la reconnaissance que le joueur est dans la zone
    /// (utilisé quand on revient du terrier et OnTriggerEnter2D n'a pas été appelé)
    /// </summary>
    public void ForceZoneEntry()
    {
        isInRiverBottomZone = true;
        teleportCooldownUntil = 0f; // Réinitialiser le cooldown pour permettre E-key immédiatement
        forceZoneEntryUntil = Time.time + FORCE_ZONE_ENTRY_DURATION; // Ignorer les exits pendant 1s
        shouldInitializeSurfaceState = false; // Ne pas réactiver les arbres au Start()
        Debug.Log("[RiverBottomTeleport] ✓✓ Forced zone entry - ignoring exits for 1s, E-key should work now!");
    }

    private void Update()
    {
        // Forcer la zone entry pendant la durée spécifiée
        if (Time.time < forceZoneEntryUntil)
        {
            isInRiverBottomZone = true;
            Debug.Log($"[RiverBottomTeleport] Force zone active ({forceZoneEntryUntil - Time.time:F2}s remaining)");
        }

        // Cooldown debug
        if (Time.time < teleportCooldownUntil)
        {
            if (isInRiverBottomZone)
            {
                float remainingCooldown = teleportCooldownUntil - Time.time;
                Debug.Log($"[RiverBottomTeleport] Interact blocked by cooldown ({remainingCooldown:F2}s remaining)");
            }
            eKeyPressedLastFrame = false;
            return;
        }

        if (!isInRiverBottomZone)
        {
            // Silently skip if not in zone
            eKeyPressedLastFrame = false;
            return;
        }

        Debug.Log("[RiverBottomTeleport] ✓ In zone, checking Interact...");
        
        bool interactHeld = InputHelper.InteractHeld();
        Debug.Log($"[RiverBottomTeleport] Interact state: held={interactHeld}, lastFrame={eKeyPressedLastFrame}");
        
        if (interactHeld && !eKeyPressedLastFrame)
        {
            Debug.Log("[RiverBottomTeleport] ✓✓ Interact held - rising to surface!");
            HandleWaterSceneTransition();
            teleportCooldownUntil = Time.time + TELEPORT_COOLDOWN;
        }
        eKeyPressedLastFrame = interactHeld;
    }

    public void HandleWaterSceneTransition()
    {
        Debug.Log("[RiverBottomTeleport] HandleWaterSceneTransition called - returning to surface");

        WaterSceneTransition sceneTransition = FindObjectOfType<WaterSceneTransition>();
        if (sceneTransition != null)
        {
            sceneTransition.OnExitWater();
            Debug.Log("[RiverBottomTeleport] ✓ Synced WaterSceneTransition exit state");
        }

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
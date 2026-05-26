using UnityEngine;

public class WaterSceneTransition : MonoBehaviour
{
    [SerializeField] private GameObject fondRivière;
    [SerializeField] private GameObject rivièreUpdate;
    [SerializeField] private GameObject tilemapGeneral;
    
    private Collider2D fondRivièreCollider;
    private Collider2D rivièreUpdateCollider;
    private float lastHandleTime = -1f;
    private const float HANDLE_COOLDOWN = 1f;

    private void Start()
    {
        // Auto-find objects if not assigned
        if (fondRivière == null)
        {
            fondRivière = GameObject.Find("FondRivière");
            if (fondRivière != null)
                Debug.Log("[WaterSceneTransition] ✓ Found FondRivière");
        }
        
        if (rivièreUpdate == null)
        {
            rivièreUpdate = GameObject.Find("Rivière update");
            if (rivièreUpdate != null)
                Debug.Log("[WaterSceneTransition] ✓ Found Rivière update");
        }
        
        if (tilemapGeneral == null)
        {
            // Try to find any Tilemap with "update" in name
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Tilemap") && obj.name.Contains("update"))
                {
                    tilemapGeneral = obj;
                    Debug.Log("[WaterSceneTransition] ✓ Found Tilemap: " + obj.name);
                    break;
                }
            }
        }
        
        if (fondRivière != null)
        {
            fondRivièreCollider = fondRivière.GetComponent<Collider2D>();
        }
        
        if (rivièreUpdate != null)
        {
            rivièreUpdateCollider = rivièreUpdate.GetComponent<Collider2D>();
        }
    }

    /// <summary>
    /// Appelé quand le joueur rentre dans l'eau (TP vers la rivière)
    /// </summary>
    public void OnEnterWater()
    {
        Debug.Log("[WaterSceneTransition] OnEnterWater called");
        
        // FondRivière devient ACTIF (affiche la rivière + trigger actif)
        if (fondRivière != null)
        {
            fondRivière.SetActive(true);
            if (fondRivièreCollider != null)
            {
                fondRivièreCollider.enabled = true;
            }
            Debug.Log("[WaterSceneTransition] ✓ Activated fondRivière");
        }
        else
            Debug.Log("[WaterSceneTransition] ⚠ fondRivière is NULL in OnEnterWater");
        
        // Rivière update devient INACTIF (désactive son trigger pour éviter double-swim)
        if (rivièreUpdate != null)
        {
            rivièreUpdate.SetActive(false);
            if (rivièreUpdateCollider != null)
            {
                rivièreUpdateCollider.enabled = false;
            }
            Debug.Log("[WaterSceneTransition] ✓ Deactivated rivièreUpdate");
        }
        else
            Debug.Log("[WaterSceneTransition] ⚠ rivièreUpdate is NULL in OnEnterWater");
        
        // La tilemap générale devient INACTIVE (invisibilité sous l'eau)
        if (tilemapGeneral != null)
        {
            tilemapGeneral.SetActive(false);
            Debug.Log("[WaterSceneTransition] ✓ Deactivated tilemapGeneral");
        }
    }

    /// <summary>
    /// Appelé quand le joueur sort de l'eau (TP vers l'extérieur)
    /// </summary>
    public void OnExitWater()
    {
        Debug.Log("[WaterSceneTransition] OnExitWater called");
        
        // Réinitialiser l'état du deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StopSwimmingDeep();
            Debug.Log("[WaterSceneTransition] ✓ Stopped deep swimming on exit water");
        }

        // FondRivière devient INACTIF
        if (fondRivière != null)
        {
            fondRivière.SetActive(false);
            if (fondRivièreCollider != null)
            {
                fondRivièreCollider.enabled = false;
            }
            Debug.Log("[WaterSceneTransition] ✓ Deactivated fondRivière");
        }
        
        // Rivière update redevient ACTIF (réactive son trigger)
        if (rivièreUpdate != null)
        {
            rivièreUpdate.SetActive(true);
            if (rivièreUpdateCollider != null)
            {
                rivièreUpdateCollider.enabled = true;
            }
            
            // Force les SpriteRenderers à être visibles
            foreach (SpriteRenderer renderer in rivièreUpdate.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = true;
            }
            Debug.Log("[WaterSceneTransition] ✓ Activated rivièreUpdate and forced sprite visibility");
        }
        else
            Debug.Log("[WaterSceneTransition] ⚠ rivièreUpdate is NULL in OnExitWater");
        
        // La tilemap générale redevient ACTIVE
        if (tilemapGeneral != null)
        {
            tilemapGeneral.SetActive(true);
            Debug.Log("[WaterSceneTransition] ✓ Activated tilemapGeneral");
        }
    }

    /// <summary>
    /// Appelé lors d'un TP pour identifier la destination
    /// </summary>
    public void HandleTeleportToDestination(Transform destination)
    {
        // Cooldown pour éviter plusieurs appels rapidement
        if (Time.time < lastHandleTime + HANDLE_COOLDOWN)
        {
            Debug.Log("[WaterSceneTransition] HandleTeleportToDestination ignored (cooldown active)");
            return;
        }
        lastHandleTime = Time.time;

        if (destination == null)
            return;

        string destName = destination.name.ToLower();
        Debug.Log("[WaterSceneTransition] HandleTeleportToDestination: " + destination.name);
        
        // Vérifier si c'est un retour du terrier (TP_EnHutte)
        if (destName.Contains("enhutte"))
        {
            Debug.Log("[WaterSceneTransition] ✓ Returning from Terrier - forcing deep swim mode");
            ForceDeepSwimMode();
            return;
        }
        
        // Vérifier si le joueur revient dans la zone rivière (par son collider)
        TopDownPlayerController player = FindObjectOfType<TopDownPlayerController>();
        bool isInRiverZone = false;
        
        if (player != null && rivièreUpdate != null)
        {
            // Vérifier si le joueur overlape avec la zone rivière
            Collider2D[] overlappingColliders = Physics2D.OverlapPointAll(player.transform.position);
            foreach (Collider2D col in overlappingColliders)
            {
                if (col.gameObject == rivièreUpdate)
                {
                    isInRiverZone = true;
                    Debug.Log("[WaterSceneTransition] ✓ Player detected in river surface zone after teleport at " + player.transform.position);
                    break;
                }
            }
        }

        // Si la destination contient "eau" ou "rivière", le joueur entre dans l'eau
        if (destName.Contains("eau") || destName.Contains("rivière") || destName.Contains("water"))
        {
            OnEnterWater();
        }
        // Si on détecte qu'on est dans la zone rivière surface (ex: revenant du terrier)
        else if (isInRiverZone)
        {
            ForceReturnToSurfaceMode();
            Debug.Log("[WaterSceneTransition] ✓ Forced return to surface mode (detected in river zone)");
        }
        // Sinon le joueur sort complètement de l'eau
        else
        {
            OnExitWater();
        }
    }

    /// <summary>
    /// Force le mode deep swim complet (utilisé au retour du terrier)
    /// </summary>
    private void ForceDeepSwimMode()
    {
        Debug.Log("[WaterSceneTransition] ForceDeepSwimMode called");
        
        // Activer le deep swim animation
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StartSwimmingDeep();
            Debug.Log("[WaterSceneTransition] ✓ Started deep swimming animation");
        }

        // Activer FondRivière (deep)
        if (fondRivière != null)
        {
            fondRivière.SetActive(true);
            if (fondRivièreCollider != null)
                fondRivièreCollider.enabled = true;
            Debug.Log("[WaterSceneTransition] ✓ Activated fondRivière");
        }

        // Activer rivière update (surface) - visible comme calque
        if (rivièreUpdate != null)
        {
            rivièreUpdate.SetActive(true);
            if (rivièreUpdateCollider != null)
                rivièreUpdateCollider.enabled = true;
            Debug.Log("[WaterSceneTransition] ✓ Activated rivièreUpdate");
        }

        // Désactiver la tilemap (on est sous l'eau)
        if (tilemapGeneral != null)
        {
            tilemapGeneral.SetActive(false);
            Debug.Log("[WaterSceneTransition] ✓ Deactivated tilemapGeneral");
        }

        // Force RiverBottomTeleport à reconnaître qu'on est dans la zone
        RiverBottomTeleport riverBottom = FindObjectOfType<RiverBottomTeleport>();
        if (riverBottom != null)
        {
            riverBottom.ForceZoneEntry();
            Debug.Log("[WaterSceneTransition] ✓ Forced RiverBottomTeleport zone entry");
        }
    }

    /// <summary>
    /// Force le retour au mode surface avec toutes les vérifications
    /// (utilisé quand on revient du terrier dans une zone rivière)
    /// </summary>
    private void ForceReturnToSurfaceMode()
    {
        Debug.Log("[WaterSceneTransition] ForceReturnToSurfaceMode called");
        
        // Vérifier si le joueur est actuellement en deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        bool isInDeepSwimMode = animator != null && animator.IsSwimmingDeep;
        
        if (isInDeepSwimMode)
        {
            // Rester en deep swim - juste réactiver les rivières
            Debug.Log("[WaterSceneTransition] ✓ Keeping deep swim mode - reactivating both river layers");
            
            // Activer FondRivière (deep)
            if (fondRivière != null)
            {
                fondRivière.SetActive(true);
                if (fondRivièreCollider != null)
                    fondRivièreCollider.enabled = true;
                Debug.Log("[WaterSceneTransition] ✓ Activated fondRivière");
            }
            
            // Activer rivière update (surface)
            if (rivièreUpdate != null)
            {
                rivièreUpdate.SetActive(true);
                if (rivièreUpdateCollider != null)
                    rivièreUpdateCollider.enabled = true;
                Debug.Log("[WaterSceneTransition] ✓ Activated rivièreUpdate");
            }
            
            // Désactiver la tilemap (on est sous l'eau)
            if (tilemapGeneral != null)
            {
                tilemapGeneral.SetActive(false);
                Debug.Log("[WaterSceneTransition] ✓ Deactivated tilemapGeneral");
            }
        }
        else
        {
            // Revenir au mode surface normal
            Debug.Log("[WaterSceneTransition] ✓ Returning to surface mode");
            
            // Appeler directement RiverBottomTeleport.HandleWaterSceneTransition() pour reproduire
            // exactement ce qui se passe quand on remonte avec E
            RiverBottomTeleport riverBottom = FindObjectOfType<RiverBottomTeleport>();
            if (riverBottom != null)
            {
                riverBottom.HandleWaterSceneTransition();
                Debug.Log("[WaterSceneTransition] ✓ Called RiverBottomTeleport.HandleWaterSceneTransition()");
            }
            else
            {
                Debug.Log("[WaterSceneTransition] ⚠ RiverBottomTeleport not found, doing manual surface restoration");
                
                // Arrêter le deep swim
                if (animator != null)
                {
                    animator.StopSwimmingDeep();
                    Debug.Log("[WaterSceneTransition] ✓ Stopped deep swimming");
                }
                
                // Désactiver fond rivière
                if (fondRivière != null)
                {
                    fondRivière.SetActive(false);
                    if (fondRivièreCollider != null)
                        fondRivièreCollider.enabled = false;
                    Debug.Log("[WaterSceneTransition] ✓ Deactivated fondRivière");
                }

                // Activer rivière update
                if (rivièreUpdate != null)
                {
                    rivièreUpdate.SetActive(true);
                    if (rivièreUpdateCollider != null)
                        rivièreUpdateCollider.enabled = true;
                    
                    // Force la visibilité de TOUS les SpriteRenderers
                    foreach (SpriteRenderer renderer in rivièreUpdate.GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.enabled = true;
                    }
                    Debug.Log("[WaterSceneTransition] ✓ Activated rivièreUpdate with visible sprites");
                }

                // Activer la tilemap
                if (tilemapGeneral != null)
                {
                    tilemapGeneral.SetActive(true);
                    Debug.Log("[WaterSceneTransition] ✓ Activated tilemapGeneral");
                }
            }
        }
    }
}

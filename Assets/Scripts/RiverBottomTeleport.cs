using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverBottom; // La Tilemap/GameObject du fond de rivière
    [SerializeField] private Transform riverSurface; // La Tilemap/GameObject de la rivière surface
    private GameObject fondRivière;
    private GameObject rivièreUpdate;
    private GameObject tilemapGeneral;
    private bool isInRiverBottomZone = false;

    private void Start()
    {
        // Récupère automatiquement les références à partir des Transforms assignés
        if (riverBottom != null)
            fondRivière = riverBottom.gameObject;
        
        if (riverSurface != null)
            rivièreUpdate = riverSurface.gameObject;
        
        // Cherche la tilemap générale (arbres, herbe) dans la scène
        if (tilemapGeneral == null)
        {
            // Cherche "Tilemap update" dans la scène
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Tilemap") && obj.name.Contains("update"))
                {
                    tilemapGeneral = obj;
                    break;
                }
            }
        }

        // État initial: rivièreUpdate visible, fondRivière invisible
        if (rivièreUpdate != null)
        {
            foreach (SpriteRenderer renderer in rivièreUpdate.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
            foreach (Collider2D collider in rivièreUpdate.GetComponentsInChildren<Collider2D>())
                if (!collider.isTrigger) collider.enabled = true;
        }
        
        if (fondRivière != null)
        {
            foreach (SpriteRenderer renderer in fondRivière.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
            foreach (Collider2D collider in fondRivière.GetComponentsInChildren<Collider2D>())
                if (!collider.isTrigger) collider.enabled = false;
        }
        
        if (tilemapGeneral != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneral.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[RiverBottomTeleport] OnTriggerEnter2D détecté avec: " + collision.gameObject.name);
        
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = true;
            Debug.Log("[RiverBottomTeleport] ✓ JOUEUR AU FOND! Appuie sur E pour remonter!");
            
            // Active l'animation de nage profonde
            CharacterAnimator animator = controller.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                animator.StartSwimmingDeep();
                Debug.Log("[RiverBottomTeleport] ✓ Animation nage profonde activée");
            }
        }
        else
        {
            Debug.Log("[RiverBottomTeleport] ✗ Pas le joueur: " + collision.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = false;
            Debug.Log("[RiverBottomTeleport] Sortie du fond");
            
            // Désactive l'animation de nage profonde
            CharacterAnimator animator = controller.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                animator.StopSwimmingDeep();
                Debug.Log("[RiverBottomTeleport] ✓ Animation nage profonde désactivée");
            }
        }
    }

    private void Update()
    {
        if (isInRiverBottomZone)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("[RiverBottomTeleport] E pressé! Téléportation...");
                TeleportToRiverSurface();
            }
        }
    }

    private void TeleportToRiverSurface()
    {
        // Cherche le joueur via TopDownPlayerController au lieu du tag
        TopDownPlayerController playerController = FindObjectOfType<TopDownPlayerController>();
        Transform player = playerController != null ? playerController.transform : null;
        
        if (player == null)
        {
            Debug.LogError("[RiverBottomTeleport] ✗ JOUEUR NON TROUVÉ!");
            return;
        }
        
        Debug.Log("[RiverBottomTeleport] ✓ Joueur trouvé à: " + player.position);

        if (riverSurface == null)
        {
            Debug.LogError("[RiverBottomTeleport] ✗ River Surface NON ASSIGNÉE!");
            return;
        }
        
        if (riverBottom == null)
        {
            Debug.LogError("[RiverBottomTeleport] ✗ River Bottom NON ASSIGNÉE!");
            return;
        }

        // Calculer la position relative du joueur par rapport au fond
        Vector3 relativePosition = player.position - riverBottom.position;
        Debug.Log("[RiverBottomTeleport] Position relative: " + relativePosition);
        
        // Appliquer cette position relative à la surface de la rivière
        Vector3 newPosition = riverSurface.position + relativePosition;
        Debug.Log("[RiverBottomTeleport] Nouvelle position: " + newPosition);
        
        player.position = newPosition;
        Debug.Log("[RiverBottomTeleport] ✓ TÉLÉPORTÉ À LA SURFACE!");

        // Gère les transitions de visibilité
        HandleWaterSceneTransition();
    }

    private void HandleWaterSceneTransition()
    {
        // FondRivière: désactive juste le rendu et les colliders non-trigger
        if (fondRivière != null)
        {
            // Désactive tous les SpriteRenderers
            foreach (SpriteRenderer renderer in fondRivière.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = false;
            }
            
            // Désactive les colliders qui ne sont pas des triggers
            foreach (Collider2D collider in fondRivière.GetComponentsInChildren<Collider2D>())
            {
                if (!collider.isTrigger)
                {
                    collider.enabled = false;
                }
            }
        }

        // Rivière update: réactive le rendu et les colliders
        if (rivièreUpdate != null)
        {
            // Réactive tous les SpriteRenderers
            foreach (SpriteRenderer renderer in rivièreUpdate.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = true;
            }
            
            // Réactive les colliders qui ne sont pas des triggers
            foreach (Collider2D collider in rivièreUpdate.GetComponentsInChildren<Collider2D>())
            {
                if (!collider.isTrigger)
                {
                    collider.enabled = true;
                }
            }
        }

        // Tilemap: réactive le rendu
        if (tilemapGeneral != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneral.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = true;
            }
        }

        Debug.Log("[RiverBottomTeleport] ✓ Transitions de visibilité appliquées");
    }
}

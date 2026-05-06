using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverBottom; // La Tilemap/GameObject du fond de rivière
    [SerializeField] private Transform riverSurface; // La Tilemap/GameObject de la rivière surface
    private bool isInRiverBottomZone = false;

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
    }
}

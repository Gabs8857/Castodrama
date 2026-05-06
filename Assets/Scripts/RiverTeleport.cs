using UnityEngine;
using UnityEngine.InputSystem;

public class RiverTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverSurface; // La Tilemap/GameObject de la rivière surface
    [SerializeField] private Transform riverBottom; // La Tilemap/GameObject du fond de rivière
    private bool isInRiverZone = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[RiverTeleport] OnTriggerEnter2D détecté avec: " + collision.gameObject.name);
        
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = true;
            Debug.Log("[RiverTeleport] ✓ JOUEUR EN RIVIÈRE! Appuie sur E pour aller au fond!");
        }
        else
        {
            Debug.Log("[RiverTeleport] ✗ Pas le joueur: " + collision.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = false;
            Debug.Log("[RiverTeleport] Sortie de la rivière");
        }
    }

    private void Update()
    {
        if (isInRiverZone)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("[RiverTeleport] E pressé! Téléportation...");
                TeleportToRiverBottom();
            }
        }
    }

    private void TeleportToRiverBottom()
    {
        // Cherche le joueur via TopDownPlayerController au lieu du tag
        TopDownPlayerController playerController = FindObjectOfType<TopDownPlayerController>();
        Transform player = playerController != null ? playerController.transform : null;
        
        if (player == null)
        {
            Debug.LogError("[RiverTeleport] ✗ JOUEUR NON TROUVÉ!");
            return;
        }
        
        Debug.Log("[RiverTeleport] ✓ Joueur trouvé à: " + player.position);

        if (riverSurface == null)
        {
            Debug.LogError("[RiverTeleport] ✗ River Surface NON ASSIGNÉE!");
            return;
        }
        
        if (riverBottom == null)
        {
            Debug.LogError("[RiverTeleport] ✗ River Bottom NON ASSIGNÉE!");
            return;
        }

        // Calculer la position relative du joueur par rapport à la rivière surface
        Vector3 relativePosition = player.position - riverSurface.position;
        Debug.Log("[RiverTeleport] Position relative: " + relativePosition);
        
        // Appliquer cette position relative au fond de la rivière
        Vector3 newPosition = riverBottom.position + relativePosition;
        Debug.Log("[RiverTeleport] Nouvelle position: " + newPosition);
        
        player.position = newPosition;
        Debug.Log("[RiverTeleport] ✓ TÉLÉPORTÉ AU FOND!");
        
        // Change l'animation en nage profonde
        CharacterAnimator animator = playerController.GetComponent<CharacterAnimator>();
        if (animator != null)
        {
            animator.StartSwimmingDeep();
            Debug.Log("[RiverTeleport] ✓ Animation nage profonde activée");
        }
        else
        {
            Debug.LogError("[RiverTeleport] ✗ CharacterAnimator NON TROUVÉ!");
        }
    }
}

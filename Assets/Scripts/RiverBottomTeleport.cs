using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverBottom; // La Tilemap/GameObject du fond de rivière
    [SerializeField] private Transform riverSurface; // La Tilemap/GameObject de la rivière surface
    private bool isInRiverBottomZone = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRiverBottomZone = true;
            Debug.Log("Entrée dans le fond - Appuie sur E pour remonter!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRiverBottomZone = false;
            Debug.Log("Sortie du fond");
        }
    }

    private void Update()
    {
        if (isInRiverBottomZone && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TeleportToRiverSurface();
        }
    }

    private void TeleportToRiverSurface()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogWarning("Joueur non trouvé!");
            return;
        }

        if (riverSurface == null || riverBottom == null)
        {
            Debug.LogWarning("Rivière surface ou fond non assignée!");
            return;
        }

        // Calculer la position relative du joueur par rapport au fond
        Vector3 relativePosition = player.position - riverBottom.position;
        
        // Appliquer cette position relative à la surface de la rivière
        Vector3 newPosition = riverSurface.position + relativePosition;
        
        player.position = newPosition;
        Debug.Log("Téléporté à la surface à position: " + newPosition + " | Décalage relatif: " + relativePosition);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class RiverTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverSurface; // La Tilemap/GameObject de la rivière surface
    [SerializeField] private Transform riverBottom; // La Tilemap/GameObject du fond de rivière
    private bool isInRiverZone = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRiverZone = true;
            Debug.Log("Entrée dans la rivière - Appuie sur E pour aller au fond!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRiverZone = false;
            Debug.Log("Sortie de la rivière");
        }
    }

    private void Update()
    {
        if (isInRiverZone && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TeleportToRiverBottom();
        }
    }

    private void TeleportToRiverBottom()
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

        // Calculer la position relative du joueur par rapport à la rivière surface
        Vector3 relativePosition = player.position - riverSurface.position;
        
        // Appliquer cette position relative au fond de la rivière
        Vector3 newPosition = riverBottom.position + relativePosition;
        
        player.position = newPosition;
        Debug.Log("Téléporté au fond à position: " + newPosition + " | Décalage relatif: " + relativePosition);
    }
}

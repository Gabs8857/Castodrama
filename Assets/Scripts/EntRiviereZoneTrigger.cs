using UnityEngine;

/// <summary>
/// Détecteur de zone "Ent-rivière" (entrée de rivière) - Notifie le TopDownPlayerController
/// Cette zone déclenche les animations de transition Dive et DiveExit
/// </summary>
public class EntRiviereZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[EntRiviereZoneTrigger] {collision.gameObject.name} est entré dans Ent-rivière");
        
        TopDownPlayerController playerController = collision.GetComponent<TopDownPlayerController>();
        
        if (playerController != null)
        {
            playerController.OnEnterZone(ZoneType.EntRiviere);
            Debug.Log($"[EntRiviereZoneTrigger] ✓ Transition zone activée pour {collision.gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"[EntRiviereZoneTrigger] {collision.gameObject.name} a quitté Ent-rivière");
        
        TopDownPlayerController playerController = collision.GetComponent<TopDownPlayerController>();
        
        if (playerController != null)
        {
            playerController.OnExitZone(ZoneType.EntRiviere);
            Debug.Log($"[EntRiviereZoneTrigger] ✓ Transition zone désactivée pour {collision.gameObject.name}");
        }
    }
}

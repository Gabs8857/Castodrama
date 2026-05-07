using UnityEngine;

/// <summary>
/// Détecteur de zone d'eau - Notifie le TopDownPlayerController
/// Le joueur se charge de notifier les autres composants (CharacterAnimator, EquippableItem, etc.)
/// </summary>
public class WaterZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[WaterZoneTrigger] {collision.gameObject.name} est entré dans l'eau");
        
        // Cherche le TopDownPlayerController
        TopDownPlayerController playerController = collision.GetComponent<TopDownPlayerController>();
        
        if (playerController != null)
        {
            playerController.OnEnterZone(ZoneType.Water);
            Debug.Log($"[WaterZoneTrigger] ✓ Notifié {collision.gameObject.name} de l'entrée en eau");
        }
        else
        {
            Debug.Log($"[WaterZoneTrigger] ⚠ Pas de TopDownPlayerController sur {collision.gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"[WaterZoneTrigger] {collision.gameObject.name} a quitté l'eau");
        
        // Cherche le TopDownPlayerController
        TopDownPlayerController playerController = collision.GetComponent<TopDownPlayerController>();
        
        if (playerController != null)
        {
            playerController.OnExitZone(ZoneType.Water);
            Debug.Log($"[WaterZoneTrigger] ✓ Notifié {collision.gameObject.name} de la sortie d'eau");
        }
    }
}

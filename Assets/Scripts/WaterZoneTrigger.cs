using UnityEngine;

/// <summary>
/// Script à attacher sur la zone d'eau (rivière)
/// Détecte quand un objet avec CharacterAnimator rentre/sort de l'eau
/// Et active/désactive l'animation de nage correspondante
/// </summary>
public class WaterZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterAnimator animator = collision.GetComponent<CharacterAnimator>();
        
        if (animator != null)
        {
            Debug.Log($"[WaterZoneTrigger] {collision.gameObject.name} est entré dans l'eau - Mode NAGE ACTIVÉ");
            animator.StartSwimming();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CharacterAnimator animator = collision.GetComponent<CharacterAnimator>();
        
        if (animator != null)
        {
            Debug.Log($"[WaterZoneTrigger] {collision.gameObject.name} a quitté l'eau - Mode MARCHE RÉACTIVÉ");
            animator.StopSwimming();
        }
    }
}

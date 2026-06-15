using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DangerZoneTrigger : MonoBehaviour
{
    [Tooltip("Vitesse d'augmentation du danger par seconde dans cette zone (plus on s'éloigne, plus c'est élevé)")]
    [SerializeField] private float dangerIncreaseRate = 14f;

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TopDownDanger danger = collision.GetComponentInParent<TopDownDanger>();
        if (danger != null)
        {
            danger.EnterDangerZone(dangerIncreaseRate);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownDanger danger = collision.GetComponentInParent<TopDownDanger>();
        if (danger != null)
        {
            danger.ExitDangerZone(dangerIncreaseRate);
        }
    }
}

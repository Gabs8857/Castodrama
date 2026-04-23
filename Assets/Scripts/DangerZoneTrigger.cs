using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DangerZoneTrigger : MonoBehaviour
{
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
            danger.EnterDangerZone();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownDanger danger = collision.GetComponentInParent<TopDownDanger>();
        if (danger != null)
        {
            danger.ExitDangerZone();
        }
    }
}

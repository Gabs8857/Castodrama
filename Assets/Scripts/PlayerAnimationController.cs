using UnityEngine;

/// <summary>
/// Gère les animations via un Animator Controller.
/// Nécessite un Animator avec :
/// - Un paramètre "IsMoving" (bool)
/// - Un paramètre "IsHittingBarrage" (bool)
/// - 2 états : "Move" (avec 2 sprites) et "Hit" (avec 2 sprites).
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("🎮 Références")]
    [SerializeField] private Animator animator;
    [SerializeField] private TopDownPlayerController playerController;
    [SerializeField] private DamManager damManager;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<TopDownPlayerController>();

        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();
    }

    private void Update()
    {
        // Met à jour l'animation de déplacement
        if (animator != null && playerController != null)
        {
            animator.SetBool("IsMoving", playerController.IsMoving);
        }
    }

    // Appelé quand le joueur touche le barrage
    public void OnHitBarrage()
    {
        if (animator != null)
        {
            animator.SetBool("IsHittingBarrage", true);
            // Réinitialise après l'animation (via un événement dans Animator)
        }
    }

    // À appeler depuis un événement dans l'Animator (à la fin de l'animation "Hit")
    public void ResetHitBarrage()
    {
        if (animator != null)
            animator.SetBool("IsHittingBarrage", false);
    }
}
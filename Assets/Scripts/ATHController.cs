using UnityEngine;

public class ATHController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isAnimationPlaying = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Désactiver l'animator pour éviter les relances d'animation automatiques
        if (animator != null)
        {
            animator.enabled = false;
        }

        // S'assurer que l'ATH est visible par défaut
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Joue l'animation de l'ATH une seule fois sans cligotement
    /// </summary>
    public void PlayAnimation()
    {
        if (isAnimationPlaying || animator == null)
            return;

        isAnimationPlaying = true;
        animator.enabled = true;
        animator.SetTrigger("Play");
    }

    /// <summary>
    /// Arrête l'animation et revient à l'état visible stable
    /// </summary>
    public void StopAnimation()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }

        isAnimationPlaying = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Garde l'ATH visible en permanence
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }
}

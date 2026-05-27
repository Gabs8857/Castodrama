using UnityEngine;
using System.Collections;

public class CarAnimator : MonoBehaviour
{
    [SerializeField] private Animator carAnimator;
    [SerializeField] private float frameDelay = 0.1f;
    [SerializeField] private int frameCount = 4;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float ReverseMoveSpeed = 5f;

    private bool isClone = false;
    private bool movingRight = true; // Détermine la direction du mouvement

    private void Start()
    {
        // Vérifier si c'est un clone
        isClone = gameObject.name.Contains("(Clone)");

        // Gérer la visibilité
        if (!isClone)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }

        // Déterminer la direction selon la position de spawn
        if (transform.position.x < -50f) // Spawn de gauche → bouge vers la droite
        {
            movingRight = true;
        }
        else // Spawn de droite → bouge vers la gauche
        {
            movingRight = false;
            FlipSprite();
        }

        if (carAnimator == null)
            carAnimator = GetComponent<Animator>();
        
        StartCoroutine(PlayCarAnimation());
    }

    private void Update()
    {
        // Mouvement seulement pour les clones
        if (isClone)
        {
            if (movingRight)
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.left * ReverseMoveSpeed * Time.deltaTime);
            }

            // Détruire le clone après qu'il ait voyagé une certaine distance
            if (transform.position.x >= 20f || transform.position.x <= -120f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Retourne le sprite horizontalement pour changer la direction
    /// </summary>
    private void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private IEnumerator PlayCarAnimation()
    {
        // Vérifier que l'Animator existe
        if (carAnimator == null)
            yield break;

        int currentFrame = 0;
        
        while (true)
        {
            // Osciller entre les frames (aller puis revenir)
            carAnimator.SetInteger("FrameIndex", currentFrame);
            yield return new WaitForSeconds(frameDelay);
            
            currentFrame++;
            if (currentFrame >= frameCount)
            {
                currentFrame = frameCount - 2;
            }
            if (currentFrame < 0)
            {
                currentFrame = 1;
            }
        }
    }
}
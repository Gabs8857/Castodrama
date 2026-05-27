using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    public GameObject interactPromptUI;

    private bool playerInRange;

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void Update()
    {
        if (interactPromptUI == null)
            return;

        // afficher seulement si joueur proche
        interactPromptUI.SetActive(playerInRange);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
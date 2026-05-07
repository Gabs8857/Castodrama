using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;

    private bool playerNearby = false;

    void Update()
    {
        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E appuyé");

            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
            }
            else
            {
                Debug.LogError("DialogueManager NON assigné !");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entrée trigger : " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Le joueur est proche");

            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Sortie trigger : " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Le joueur est parti");

            playerNearby = false;
        }
    }
}
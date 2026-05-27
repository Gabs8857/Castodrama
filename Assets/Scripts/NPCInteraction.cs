using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public TextAsset[] npcDialogues;

    private bool playerNearby = false;

    private int dialogueIndex = 0;

    private bool isTalking = false;

    void Update()
    {
        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isTalking)
        {
            if (dialogueIndex < npcDialogues.Length)
            {
                Debug.Log("Dialogue index = " + dialogueIndex);

                isTalking = true;

                dialogueManager.StartDialogue(npcDialogues[dialogueIndex]);

                // Si le dialogue était bloqué (quiz en cours), on annule isTalking
                // pour que le joueur puisse réessayer après le quiz
                if (dialogueManager.dialogueBlocked)
                {
                    isTalking = false;
                }
            }
        }

        // Détecte fin dialogue proprement
        if (isTalking && dialogueManager.dialogueFinished)
        {
            dialogueIndex++;
            isTalking = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}
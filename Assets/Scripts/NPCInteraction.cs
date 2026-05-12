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
        // 🔥 interaction
        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isTalking)
        {
            if (dialogueIndex < npcDialogues.Length)
            {
                Debug.Log("Dialogue index = " + dialogueIndex);

                isTalking = true;

                dialogueManager.StartDialogue(npcDialogues[dialogueIndex]);
            }
        }

        // 🔥 détecte fin dialogue proprement
        if (isTalking && dialogueManager.dialogueFinished)
        {
            dialogueIndex++;        // passe au suivant
            isTalking = false;      // reset état
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
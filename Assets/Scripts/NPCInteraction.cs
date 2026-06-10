using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public TextAsset[] npcDialogues;

    [Header("Crédits")]
    public string creditsSceneName = "Credits"; // nom exact de ta scène crédits

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

                if (dialogueManager.dialogueBlocked)
                    isTalking = false;
            }
        }

        // Détecte fin dialogue
        if (isTalking && dialogueManager.dialogueFinished)
        {
            dialogueIndex++;
            isTalking = false;

            // Si tous les dialogues sont terminés → scène crédits
            if (dialogueIndex >= npcDialogues.Length)
            {
                Debug.Log("[NPCInteraction] Fin du dernier dialogue → Crédits");
                SceneManager.LoadScene(creditsSceneName);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }
}
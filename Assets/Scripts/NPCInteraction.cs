using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Crédits")]
    public string creditsSceneName = "Credits";

    private bool playerNearby = false;
    private bool isTalking = false;

    // SetDayDialogue conservé pour compatibilité avec DayManager,
    // mais rien à faire ici : c'est DialogueManager + current_day qui gèrent le bon ink.
    public void SetDayDialogue(int day)
    {
        Debug.Log("[NPC] Dialogue prêt pour le jour " + day);
    }

    void Update()
    {
        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isTalking)
        {
            isTalking = true;
            dialogueManager.StartDialogue();

            // Hook tuto
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnPlayerInteracted();

            if (dialogueManager.dialogueBlocked)
                isTalking = false;
        }

        if (isTalking && dialogueManager.dialogueFinished)
        {
            isTalking = false;

            DayManager dm = FindObjectOfType<DayManager>();
            if (dm != null) dm.OnBilanDone();
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
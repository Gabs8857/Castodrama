using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Dialogues par jour")]
    public TextAsset dialogueJour1;
    public TextAsset dialogueJour2;
    public TextAsset dialogueJour3;

    [Header("Crédits")]
    public string creditsSceneName = "Credits";

    private bool playerNearby = false;
    private bool isTalking = false;
    private TextAsset currentDialogue;

    void Start()
    {
        // Charger le bon dialogue selon le jour actuel au démarrage
        SetDayDialogue(GameState.currentDay);
    }

    public void SetDayDialogue(int day)
    {
        switch (day)
        {
            case 1: currentDialogue = dialogueJour1; break;
            case 2: currentDialogue = dialogueJour2; break;
            case 3: currentDialogue = dialogueJour3; break;
            default: currentDialogue = dialogueJour1; break;
        }
        Debug.Log("[NPC] Dialogue chargé pour le jour " + day);
    }

    void Update()
    {
        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isTalking &&
            currentDialogue != null)
        {
            isTalking = true;
            dialogueManager.StartDialogue(currentDialogue);

            if (dialogueManager.dialogueBlocked)
                isTalking = false;
        }

        if (isTalking && dialogueManager.dialogueFinished)
        {
            isTalking = false;

            // Notifier le DayManager que le bilan est fait
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
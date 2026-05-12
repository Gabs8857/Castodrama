using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private Story story;

    public bool dialogueFinished = false;

    //  référence au script de mouvement du joueur
    public MonoBehaviour playerMovement;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        Debug.Log("Dialogue démarré");

        dialogueFinished = false;

        if (inkJSON == null)
        {
            Debug.LogError("Ink file NULL !");
            return;
        }

        story = new Story(inkJSON.text);

        story.ChoosePathString("start");

        dialoguePanel.SetActive(true);

        // 🔥 BLOQUE LE JOUEUR
        if (playerMovement != null)
            playerMovement.enabled = false;

        ContinueStory();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ContinueStory();
        }
    }

    void ContinueStory()
    {
        if (story == null)
        {
            Debug.LogError("Story NULL");
            return;
        }

        if (story.canContinue)
        {
            string line = story.Continue();

            Debug.Log("LINE = [" + line + "]");

            dialogueText.text = line.Trim();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        dialogueFinished = true;

        //  RÉACTIVE LE JOUEUR
        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public TextAsset inkJSON;

    private Story story;

    void Start()
    {
        Debug.Log("DialogueManager lancé");

        dialoguePanel.SetActive(false);

        story = new Story(inkJSON.text);

        story.ChoosePathString("start");

        Debug.Log("Story créée");
    }

    public void StartDialogue()
    {
        Debug.Log("Dialogue démarré");

        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    void ContinueStory()
    {
        Debug.Log("ContinueStory appelé");

        if (story.canContinue)
        {
            string line = story.Continue().Trim();

            Debug.Log("LINE = " + line);

            dialogueText.text = line;
        }
        else
        {
            Debug.Log("Fin du dialogue");

            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // 🔥 Efface le texte
        dialogueText.text = "";

        story.ResetState();
        story.ChoosePathString("start");
    }

    void Update()
    {
        // Dialogue suivant
        if (dialoguePanel.activeSelf &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE appuyé");

            ContinueStory();
        }

        // 🔥 Annuler dialogue si le joueur bouge
        if (dialoguePanel.activeSelf && IsMoving())
        {
            Debug.Log("Dialogue annulé car mouvement");

            EndDialogue();
        }
    }

    bool IsMoving()
    {
        return
            Keyboard.current.zKey.isPressed ||
            Keyboard.current.qKey.isPressed ||
            Keyboard.current.sKey.isPressed ||
            Keyboard.current.dKey.isPressed ||

            Keyboard.current.wKey.isPressed ||
            Keyboard.current.aKey.isPressed ||

            Keyboard.current.upArrowKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;
    }
}
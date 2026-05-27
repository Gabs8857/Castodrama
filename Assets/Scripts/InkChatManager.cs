using UnityEngine;
using Ink.Runtime;
using System.Collections;

public class InkChatManager : MonoBehaviour
{
    public TextAsset inkJSON;

    private Story story;

    public float delay = 2f;

    public ChatManager chatManager;

    // -------------------------------------------------
    void Start()
    {
        if (inkJSON == null)
        {
            Debug.LogError("❌ Ink JSON NULL");
            return;
        }

        if (chatManager == null)
            chatManager = GameState.chatManager;

        if (chatManager == null)
        {
            Debug.LogError("❌ ChatManager introuvable");
            return;
        }

        story = new Story(inkJSON.text);

        story.ChoosePathString("start");

        StartCoroutine(PlayChat());
    }

    // -------------------------------------------------
    IEnumerator PlayChat()
    {
        while (story.canContinue)
        {
            string text = story.Continue().Trim();

            bool isChat = false;

            foreach (string tag in story.currentTags)
            {
                if (tag == "CHAT")
                {
                    isChat = true;
                    break;
                }
            }

            // 🔥 ajout message
            if (isChat)
            {
                chatManager.AddMessage(text);

                Debug.Log("CHAT : " + text);
            }

            yield return new WaitForSeconds(delay);
        }

        Debug.Log("✅ Fin Ink Chat");
    }
}
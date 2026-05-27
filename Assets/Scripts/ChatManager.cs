using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    public Transform chatContent;
    public GameObject messagePrefab;
    public GameObject chatRoot;

    // 🔥 Référence du ScrollRect
    public ScrollRect scrollRect;

    void Awake()
    {
        GameState.chatManager = this;
    }

    void Start()
    {
        UpdateVisibility();
    }

    public void UpdateVisibility()
    {
        if (chatRoot == null)
            return;

        chatRoot.SetActive(true);
    }

    public void ForceUpdate()
    {
        UpdateVisibility();
    }

    public void AddMessage(string msg)
    {
        if (chatContent == null || messagePrefab == null)
            return;

        GameObject obj = Instantiate(messagePrefab, chatContent);

        TMP_Text txt = obj.GetComponent<TMP_Text>();

        if (txt != null)
            txt.text = msg;

        // 🔥 Force le layout à se rebuild
        Canvas.ForceUpdateCanvases();

        // 🔥 Scroll en bas
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearChat()
    {
        foreach (Transform child in chatContent)
            Destroy(child.gameObject);
    }
}
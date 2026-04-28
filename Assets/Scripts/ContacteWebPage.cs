using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ContacteWebPage : MonoBehaviour
{
    private const string DefaultEndpoint = "https://gabrielmuller.rf.gd";

    [SerializeField]
    private string formEndpoint = DefaultEndpoint;
    [SerializeField]
    private string queryParameterName = "message";
    [SerializeField]
    private float slideDuration = 0.45f;

    private RectTransform panelRect;
    private InputField messageInput;
    private Text statusText;
    private bool isSending;
    private Canvas runtimeCanvas;

    void Start()
    {
        NormalizeEndpoint();
        Debug.Log("ContacteWebPage: initialisation UI...");
        Debug.Log("ContacteWebPage: endpoint actif -> " + formEndpoint);
        BuildUI();
        StartCoroutine(SlideInFromBottom());
    }

    private void NormalizeEndpoint()
    {
        if (string.IsNullOrWhiteSpace(formEndpoint))
        {
            formEndpoint = DefaultEndpoint;
            return;
        }

        formEndpoint = formEndpoint.Trim();

        // Inspector values are serialized and can keep stale domains from previous versions.
        if (formEndpoint.Contains("gabrielmuller.dev"))
        {
            formEndpoint = formEndpoint.Replace("gabrielmuller.dev", "gabrielmuller.rf.gd");
        }
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("ContactCanvasRuntime", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        runtimeCanvas = canvasGO.GetComponent<Canvas>();
        runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimeCanvas.sortingOrder = 500;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = LoadBuiltinFont();

        GameObject panelGO = new GameObject("ContactPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(runtimeCanvas.transform, false);
        panelRect = panelGO.GetComponent<RectTransform>();
        Image panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.78f);

        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(0f, 220f);
        panelRect.anchoredPosition = new Vector2(0f, -240f);

        CreateLabel(panelGO.transform, "Titre", "Contacte-nous", font, 28, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(600f, 36f), TextAnchor.MiddleCenter, Color.white);

        messageInput = CreateInputField(panelGO.transform, font);
        statusText = CreateLabel(panelGO.transform, "Status", "", font, 18, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 14f), new Vector2(-40f, 26f), TextAnchor.MiddleLeft, Color.white);

        Button submitButton = CreateButton(panelGO.transform, font);
        submitButton.onClick.AddListener(OnSubmitClicked);

        Debug.Log("ContacteWebPage: UI creee (Canvas Overlay + panel bas).");
    }

    private static Font LoadBuiltinFont()
    {
        // Unity 6 deprecated Arial.ttf as built-in runtime font.
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            return font;
        }

        Debug.LogError("ContacteWebPage: impossible de charger une police built-in (LegacyRuntime.ttf / Arial.ttf).");
        return null;
    }

    private IEnumerator SlideInFromBottom()
    {
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 endPos = new Vector2(0f, 0f);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        panelRect.anchoredPosition = endPos;
    }

    private void OnSubmitClicked()
    {
        if (isSending)
        {
            return;
        }

        if (messageInput == null)
        {
            return;
        }

        string message = messageInput.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            SetStatus("Tape un message avant d'envoyer.", Color.yellow);
            return;
        }

        StartCoroutine(SendMessage(message));
    }

    private IEnumerator SendMessage(string message)
    {
        isSending = true;
        SetStatus("Envoi en cours...", Color.white);

        string requestUrl = BuildRequestUrl(message);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                SetStatus("Message envoye.", Color.green);
                Debug.Log("Requete envoyee vers: " + requestUrl);
            }
            else
            {
                SetStatus("Echec de l'envoi: " + webRequest.error, Color.red);
                Debug.LogError("Erreur requete: " + requestUrl + " -> " + webRequest.error);
            }
        }

        isSending = false;
    }

    private string BuildRequestUrl(string message)
    {
        string separator = formEndpoint.Contains("?") ? "&" : "?";
        return formEndpoint + separator + queryParameterName + "=" + UnityWebRequest.EscapeURL(message);
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = color;
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    private Text CreateLabel(
        Transform parent,
        string objectName,
        string content,
        Font font,
        int fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAnchor alignment,
        Color color)
    {
        GameObject textGO = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(parent, false);

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Text text = textGO.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.text = content;

        return text;
    }

    private InputField CreateInputField(Transform parent, Font font)
    {
        GameObject inputGO = new GameObject("MessageInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGO.transform.SetParent(parent, false);

        RectTransform rect = inputGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 58f);
        rect.sizeDelta = new Vector2(-220f, 56f);

        Image image = inputGO.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);

        InputField input = inputGO.GetComponent<InputField>();
        input.targetGraphic = image;

        Text inputText = CreateLabel(inputGO.transform, "Text", "", font, 20, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-20f, -10f), TextAnchor.MiddleLeft, Color.black);
        Text placeholder = CreateLabel(inputGO.transform, "Placeholder", "Ton message...", font, 20, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-20f, -10f), TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.45f, 1f));
        placeholder.fontStyle = FontStyle.Italic;

        input.textComponent = inputText;
        input.placeholder = placeholder;
        input.lineType = InputField.LineType.SingleLine;

        return input;
    }

    private Button CreateButton(Transform parent, Font font)
    {
        GameObject buttonGO = new GameObject("SubmitButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-20f, 58f);
        rect.sizeDelta = new Vector2(180f, 56f);

        Image image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.12f, 0.57f, 0.88f, 1f);

        Button button = buttonGO.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.18f, 0.63f, 0.94f, 1f);
        colors.pressedColor = new Color(0.08f, 0.46f, 0.76f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        CreateLabel(buttonGO.transform, "Text", "Envoyer", font, 22, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);

        return button;
    }
}

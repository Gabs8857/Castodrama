using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'écran de fin de partie (ex: "J'ai été trop aventureux...").
/// Crée un panneau plein écran à la volée, affiche un titre et un sous-titre,
/// puis met le jeu en pause.
/// </summary>
public static class EndingManager
{
    private static GameObject endingPanel;

    /// <summary>
    /// Affiche l'écran de fin avec un titre et un sous-titre, puis met le jeu en pause.
    /// </summary>
    public static void TriggerEnding(string title, string subtitle)
    {
        if (endingPanel != null)
        {
            // Déjà affiché, ne fait rien
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("EndingCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Le panel doit être au-dessus de tout : on le met sur un Canvas séparé avec une sortOrder élevée
        GameObject endingCanvasObject = new GameObject("EndingCanvas");
        Canvas endingCanvas = endingCanvasObject.AddComponent<Canvas>();
        endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        endingCanvas.overrideSorting = true;
        endingCanvas.sortingOrder = 1000;
        endingCanvasObject.AddComponent<CanvasScaler>();
        endingCanvasObject.AddComponent<GraphicRaycaster>();

        // --- Fond noir plein écran ---
        endingPanel = new GameObject("EndingPanel");
        endingPanel.transform.SetParent(endingCanvasObject.transform, false);

        RectTransform panelRect = endingPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = endingPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.92f);

        // --- Titre ---
        GameObject titleObject = new GameObject("EndingTitle");
        titleObject.transform.SetParent(endingPanel.transform, false);

        RectTransform titleRect = titleObject.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.55f);
        titleRect.anchorMax = new Vector2(0.5f, 0.55f);
        titleRect.sizeDelta = new Vector2(900f, 100f);
        titleRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 48f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // --- Sous-titre ---
        GameObject subtitleObject = new GameObject("EndingSubtitle");
        subtitleObject.transform.SetParent(endingPanel.transform, false);

        RectTransform subtitleRect = subtitleObject.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.45f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.45f);
        subtitleRect.sizeDelta = new Vector2(900f, 80f);
        subtitleRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI subtitleText = subtitleObject.AddComponent<TextMeshProUGUI>();
        subtitleText.text = subtitle;
        subtitleText.fontSize = 28f;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        Debug.Log($"[EndingManager] Fin de partie déclenchée : {title} / {subtitle}");

        // Met le jeu en pause
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Ferme l'écran de fin et reprend le jeu (utile pour un bouton "Réessayer").
    /// </summary>
    public static void CloseEnding()
    {
        if (endingPanel != null)
        {
            Object.Destroy(endingPanel.transform.parent.gameObject);
            endingPanel = null;
        }
        Time.timeScale = 1f;
    }
}

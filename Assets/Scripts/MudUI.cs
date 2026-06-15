using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Affiche l'icône de boue dans l'UI quand le joueur en possède une.
/// </summary>
public class MudUI : MonoBehaviour
{
    private const string MudSpritePath = "Assets/ATH/mud.png";

    [SerializeField] private Image mudIcon;
    [SerializeField] private Sprite mudSprite;

    private void Awake()
    {
        EnsureIconExists();
        UpdateDisplay(false);
        Debug.Log("[MudUI] ✓ Interface de boue prête.");
    }

    private void EnsureIconExists()
    {
        if (mudIcon != null) return;

        // Cherche un enfant existant
        Transform existing = transform.Find("MudIcon");
        if (existing != null)
        {
            mudIcon = existing.GetComponent<Image>();
            return;
        }

        // Crée l'icône
        GameObject iconGO = new GameObject("MudIcon");
        iconGO.transform.SetParent(transform, false);

        RectTransform rect = iconGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64f, 64f);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(80f, 80f); // Position dans l'Inspector

        mudIcon = iconGO.AddComponent<Image>();
        mudIcon.raycastTarget = false;

        if (mudSprite == null) mudSprite = TryLoadMudSprite();
        if (mudSprite != null) mudIcon.sprite = mudSprite;
    }

    public void UpdateDisplay(bool hasMud)
    {
        if (mudIcon != null)
        {
            mudIcon.enabled = hasMud;
            Debug.Log($"[MudUI] Mise à jour de l'icône - Visible: {hasMud}");
        }
    }

    private static Sprite TryLoadMudSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(MudSpritePath);
#else
        return null;
#endif
    }
}

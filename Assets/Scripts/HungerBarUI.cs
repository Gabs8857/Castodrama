using UnityEngine;
using UnityEngine.UI;
// using ink.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Gestionnaire de la barre de faim UI circulaire.
/// </summary>
public class HungerBarUI : MonoBehaviour
{
    private const string FoodCircleAssetPath = "Assets/ATH/Foodcircle.png";

    [SerializeField] private TopDownHunger hungerSystem;
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Image hungerBarBackground;
    [SerializeField] private Sprite defaultBackgroundSprite;

    // [Header("CONFIGURATION DU DIALOGUE")]
    // [SerializeField] private TextAsset inkJSON;

    private static Sprite runtimeRingSprite;
    private bool hungerUpdateLogged = false;

    private void Awake()
    {
        if (hungerBarFill == null)
            hungerBarFill = GetComponentInChildren<Image>();

        EnsureBarIsRenderable();
        EnsureBackgroundImageExists();
        EnsureBackgroundRenderable();
        // Le positionnement est géré directement dans l'Inspector.
    }

    private void Start()
    {
        if (hungerSystem == null)
        {
            GameObject player = GameObject.Find("Castor");
            if (player != null) hungerSystem = player.GetComponent<TopDownHunger>();

            if (hungerSystem != null) Debug.Log("[HungerBarUI] ✓ Système TopDownHunger lié avec succès.");
            else Debug.LogError("[HungerBarUI] ✗ TopDownHunger introuvable sur 'Castor' !");
        }
    }

    private void Update()
    {
        bool shouldBeVisible = true;

        if (hungerBarFill != null && hungerBarFill.enabled != shouldBeVisible)
            hungerBarFill.enabled = shouldBeVisible;

        if (hungerBarBackground != null && hungerBarBackground.enabled != shouldBeVisible)
            hungerBarBackground.enabled = shouldBeVisible;

        if (!shouldBeVisible) return;

        if (hungerSystem != null && hungerBarFill != null)
        {
            float normalized = hungerSystem.NormalizedHunger;
            hungerBarFill.fillAmount = normalized;
            hungerBarFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f, 1f), new Color(0.32f, 0.85f, 0.35f, 1f), normalized);

            if (!hungerUpdateLogged)
            {
                Debug.Log($"[HungerBarUI] Hunger bar working! normalized={normalized:F2}");
                hungerUpdateLogged = true;
            }
        }
        else if (Time.frameCount % 200 == 0)
        {
            if (hungerSystem == null) Debug.LogError("[HungerBarUI] hungerSystem (TopDownHunger) est introuvable !");
            if (hungerBarFill == null) Debug.LogError("[HungerBarUI] hungerBarFill (Image) est introuvable !");
        }
    }

    private void EnsureBarIsRenderable()
    {
        if (hungerBarFill == null) return;
        if (hungerBarFill.sprite == null)
        {
            if (runtimeRingSprite == null) runtimeRingSprite = CreateRingSprite();
            hungerBarFill.sprite = runtimeRingSprite;
        }
        hungerBarFill.type = Image.Type.Filled;
        hungerBarFill.fillMethod = Image.FillMethod.Radial360;
        hungerBarFill.fillOrigin = (int)Image.Origin360.Top;
        hungerBarFill.color = new Color(0.32f, 0.85f, 0.35f, 1f);
    }

    private void EnsureBackgroundImageExists()
    {
        if (hungerBarBackground != null) return;
        Transform existing = transform.Find("HungerBarBackground");
        if (existing != null)
        {
            hungerBarBackground = existing.GetComponent<Image>();
        }
        else
        {
            GameObject bg = new GameObject("HungerBarBackground");
            bg.transform.SetParent(transform, false);
            bg.transform.SetAsFirstSibling();
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            hungerBarBackground = bg.AddComponent<Image>();
            hungerBarBackground.raycastTarget = false;
        }
    }

    private void EnsureBackgroundRenderable()
    {
        if (hungerBarBackground == null) return;
        if (hungerBarBackground.sprite == null)
        {
            if (defaultBackgroundSprite == null) defaultBackgroundSprite = TryLoadFoodCircleSprite();
            if (defaultBackgroundSprite != null) hungerBarBackground.sprite = defaultBackgroundSprite;
        }
        hungerBarBackground.preserveAspect = true;
    }

    private static Sprite TryLoadFoodCircleSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(FoodCircleAssetPath);
#else
        return null;
#endif
    }

    private Sprite CreateRingSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float outer = size / 2f - 2f;
        float inner = outer * 0.65f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = (dist >= inner && dist <= outer) ? Color.white : Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}

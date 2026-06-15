using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Barre de fissures UI circulaire — calquée sur HungerBarUI.
/// Se remplit en bleu quand les fissures augmentent (vide = barrage intact, plein = barrage critique).
/// 
/// Setup : identique à HungerBarUI, glisse une Image enfant et assigne-la à crackBarFill.
/// </summary>
public class CrackBarUI : MonoBehaviour
{
    private const string CrackCircleAssetPath = "Assets/ATH/Foodcircle.png"; // réutilise le même sprite ring

    [SerializeField] private DamManager damManager;
    [SerializeField] private Image crackBarFill;
    [SerializeField] private Image crackBarBackground;
    [SerializeField] private Sprite defaultBackgroundSprite;

    private static Sprite runtimeRingSprite;
    private bool crackUpdateLogged = false;

    private int maxCracks = 4;
    private int currentCracks = 0;

    private void Awake()
    {
        if (crackBarFill == null)
            crackBarFill = GetComponentInChildren<Image>();

        EnsureBarIsRenderable();
        EnsureBackgroundImageExists();
        EnsureBackgroundRenderable();
    }

    private void Start()
    {
        if (damManager == null)
        {
            damManager = FindObjectOfType<DamManager>();
            if (damManager != null) Debug.Log("[CrackBarUI] ✓ DamManager lié avec succès.");
            else Debug.LogError("[CrackBarUI] ✗ DamManager introuvable dans la scène !");
        }

        // Init à 0 fissure
        UpdateBar(0, 4);
    }

    private void Update()
    {
        if (crackBarFill == null) return;

        if (damManager != null)
        {
            int cracks = damManager.GetCurrentCrackCount();
            float normalized = maxCracks > 0 ? (float)cracks / maxCracks : 0f;
            crackBarFill.fillAmount = normalized;
            crackBarFill.color = GetCrackColor(normalized);

            if (!crackUpdateLogged)
            {
                Debug.Log($"[CrackBarUI] Crack bar working! cracks={cracks}/{maxCracks}");
                crackUpdateLogged = true;
            }
        }
        else if (Time.frameCount % 200 == 0)
        {
            Debug.LogError("[CrackBarUI] damManager est introuvable !");
        }
    }

    /// <summary>
    /// Appelé par DamManager à chaque création/réparation de fissure.
    /// </summary>
    public void UpdateBar(int current, int total)
    {
        currentCracks = current;
        maxCracks = total;

        if (crackBarFill == null) return;

        float normalized = total > 0 ? (float)current / total : 0f;
        crackBarFill.fillAmount = normalized;
        crackBarFill.color = GetCrackColor(normalized);
    }

    /// <summary>
    /// Bleu calme → bleu intense → rouge critique selon le remplissage
    /// </summary>
    private Color GetCrackColor(float normalized)
    {
        if (normalized <= 0f)
            return new Color(0.2f, 0.6f, 1f, 1f);   // Bleu clair — aucune fissure
        if (normalized < 0.5f)
            return new Color(0.1f, 0.4f, 0.9f, 1f);  // Bleu moyen — quelques fissures
        if (normalized < 1f)
            return new Color(0.05f, 0.2f, 0.8f, 1f); // Bleu foncé — beaucoup de fissures
        return new Color(0.8f, 0.1f, 0.1f, 1f);      // Rouge — barrage critique (4/4)
    }

    private void EnsureBarIsRenderable()
    {
        if (crackBarFill == null) return;
        if (crackBarFill.sprite == null)
        {
            if (runtimeRingSprite == null) runtimeRingSprite = CreateRingSprite();
            crackBarFill.sprite = runtimeRingSprite;
        }
        crackBarFill.type = Image.Type.Filled;
        crackBarFill.fillMethod = Image.FillMethod.Radial360;
        crackBarFill.fillOrigin = (int)Image.Origin360.Top;
        crackBarFill.fillAmount = 0f;
        crackBarFill.color = new Color(0.2f, 0.6f, 1f, 1f);
    }

    private void EnsureBackgroundImageExists()
    {
        if (crackBarBackground != null) return;
        Transform existing = transform.Find("CrackBarBackground");
        if (existing != null)
        {
            crackBarBackground = existing.GetComponent<Image>();
        }
        else
        {
            GameObject bg = new GameObject("CrackBarBackground");
            bg.transform.SetParent(transform, false);
            bg.transform.SetAsFirstSibling();
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            crackBarBackground = bg.AddComponent<Image>();
            crackBarBackground.raycastTarget = false;
        }
    }

    private void EnsureBackgroundRenderable()
    {
        if (crackBarBackground == null) return;
        if (crackBarBackground.sprite == null)
        {
            if (defaultBackgroundSprite == null) defaultBackgroundSprite = TryLoadCrackCircleSprite();
            if (defaultBackgroundSprite != null) crackBarBackground.sprite = defaultBackgroundSprite;
        }
        crackBarBackground.preserveAspect = true;
    }

    private static Sprite TryLoadCrackCircleSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(CrackCircleAssetPath);
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

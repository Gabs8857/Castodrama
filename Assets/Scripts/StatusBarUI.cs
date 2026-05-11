using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Gestionnaire unifié des barres d'état UI (Faim + Danger)
/// Combine la logique de HungerBar.cs, TopDownHungerBarUI.cs, et TopDownDangerBarUI.cs
/// 
/// Gère:
/// - Barre de faim circulaire avec positionnement orbital adaptatif
/// - Barre de danger circulaire
/// - Mise à jour en temps réel selon TopDownHunger et TopDownDanger
/// </summary>
public class StatusBarUI : MonoBehaviour
{
    private const string PrimaryTargetName = "ATH_SANSFOND_SANSREPONDRE_0";
    private const string FallbackTargetName = "Map_V2_0";
    private const string FoodCircleAssetPath = "Assets/ATH/Foodcircle.png";

    // ==================== FAIM ====================
    [SerializeField]
    private TopDownHunger hungerSystem;

    [SerializeField]
    private Image hungerBarFill;

    [SerializeField]
    private Image hungerBarBackground;

    [SerializeField]
    private Sprite defaultBackgroundSprite;

    [SerializeField]
    private bool hungerFollowFixedCanvasPoint = true;

    [SerializeField]
    private RectTransform hungerFixedCanvasPoint;

    [SerializeField]
    private bool hungerUseCameraViewportPointWhenFixed = true;

    [SerializeField]
    private Vector2 hungerFixedViewportPoint = new Vector2(0.5f, 0.12f);

    [SerializeField]
    private Vector2 hungerFixedPointOffset = Vector2.zero;

    [SerializeField]
    private Transform hungerFollowTarget;

    [SerializeField]
    private Vector2 hungerFollowOffset = Vector2.zero;

    [SerializeField]
    private bool hungerAutoPlaceAroundImage = true;

    [SerializeField]
    private bool hungerWrapAroundImage = true;

    [SerializeField]
    private bool hungerMatchTargetImageSize = true;

    [SerializeField]
    private bool hungerUsePointOnTargetSprite = true;

    [SerializeField]
    private Vector2 hungerTargetPointNormalized = new Vector2(0.5f, 0.5f);

    [SerializeField]
    private float hungerOrbitAngleDegrees = 130f;

    [SerializeField]
    private float hungerImagePaddingPixels = 14f;

    [SerializeField]
    private float hungerRingScaleMultiplier = 1.1f;

    [SerializeField]
    private float hungerMinRingDiameterPixels = 200f;

    [SerializeField]
    private float hungerMaxRingDiameterPixels = 400f;

    // ==================== DANGER ====================
    [SerializeField]
    private TopDownDanger dangerSystem;

    [SerializeField]
    private Image dangerBarFill;

    [SerializeField]
    private Image dangerBarBackground;

    [SerializeField]
    private bool dangerFixedOnScreen = true;

    [SerializeField]
    private Vector2 dangerAnchor = new Vector2(0f, 0f);

    [SerializeField]
    private Vector2 dangerAnchoredPosition = new Vector2(132f, 124f);

    [SerializeField]
    private Vector2 dangerBarSize = new Vector2(96f, 96f);

    [SerializeField]
    private float dangerRingThicknessFactor = 0.34f;

    // ==================== PRIVATE ====================
    private static Sprite runtimeWhiteSprite;
    private static Sprite runtimeRingSprite;

    private RectTransform hungerRectTransform;
    private RectTransform dangerRectTransform;
    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;
    private SpriteRenderer hungerFollowSpriteRenderer;
    private AdaptiveHUDWidth hungerAdaptiveHudWidth;
    private bool debugLogged = false;
    private bool hungerUpdateLogged = false;
    private RectTransform hungerParentRectTransform;

    private void Awake()
    {
        Debug.Log("[StatusBarUI.Awake] Starting Awake");
        
        if (hungerBarFill == null)
        {
            // First try to get Image on this GameObject
            hungerBarFill = GetComponent<Image>();
            Debug.Log($"[StatusBarUI.Awake] GetComponent<Image>: {(hungerBarFill != null ? "FOUND" : "NOT FOUND")}");
            
            // If not found, search in children (the actual Image is on a child)
            if (hungerBarFill == null)
            {
                hungerBarFill = GetComponentInChildren<Image>();
                Debug.Log($"[StatusBarUI.Awake] GetComponentInChildren<Image>: {(hungerBarFill != null ? "FOUND" : "NOT FOUND")}");
                
                if (hungerBarFill != null)
                {
                    Debug.Log($"[StatusBarUI.Awake] Found Image on child: {hungerBarFill.gameObject.name}");
                }
            }
            
            // If still not found, create one
            if (hungerBarFill == null)
            {
                Debug.Log("[StatusBarUI.Awake] Image not found, creating one");
                hungerBarFill = gameObject.AddComponent<Image>();
            }
        }

        if (hungerBarFill != null)
        {
            Debug.Log($"[StatusBarUI.Awake] hungerBarFill sprite: {hungerBarFill.sprite}, color: {hungerBarFill.color}");
        }
        else
        {
            Debug.LogError("[StatusBarUI.Awake] Could not find Image component!");
        }

        hungerRectTransform = hungerBarFill != null ? hungerBarFill.rectTransform : GetComponent<RectTransform>();
        hungerParentRectTransform = GetComponent<RectTransform>(); // Parent that has StatusBarUI
        dangerRectTransform = dangerBarFill != null ? dangerBarFill.GetComponent<RectTransform>() : null;

        EnsureCanvasReferences();
        EnsureHungerBarIsRenderable();
        EnsureHungerBackgroundImageExists();
        EnsureHungerBackgroundRenderable();
        
        Debug.Log("[StatusBarUI.Awake] Awake complete");
    }

    private void Start()
    {
        Debug.Log("[StatusBarUI.Start] Starting Start");
        EnsureCanvasReferences();

        // Trouve les systèmes
        if (hungerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Castor");
            Debug.Log($"[StatusBarUI.Start] Looking for Castor: {(playerObject != null ? "FOUND" : "NOT FOUND")}");
            
            if (playerObject != null)
            {
                hungerSystem = playerObject.GetComponent<TopDownHunger>();
                Debug.Log($"[StatusBarUI.Start] GetComponent<TopDownHunger>: {(hungerSystem != null ? "FOUND" : "NOT FOUND")}");
                
                if (hungerSystem == null)
                {
                    Debug.LogWarning($"[StatusBarUI.Start] Castor found but TopDownHunger component not found. Adding it now.");
                    hungerSystem = playerObject.AddComponent<TopDownHunger>();
                    Debug.Log("[StatusBarUI.Start] Added TopDownHunger component");
                }
            }
            else
            {
                Debug.LogError("[StatusBarUI.Start] Player object 'Castor' not found!");
            }
        }

        if (dangerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Castor");
            if (playerObject == null)
            {
                playerObject = GameObject.Find("Player");
            }

            if (playerObject != null)
            {
                dangerSystem = playerObject.GetComponent<TopDownDanger>();
            }
            else
            {
                Debug.LogError("[StatusBarUI.Start] Player object not found for danger system!");
            }
        }

        // Résout la cible de hunger
        Transform resolvedTarget = FindTargetByName(PrimaryTargetName);
        if (resolvedTarget == null)
        {
            resolvedTarget = FindTargetByName(FallbackTargetName);
        }

        if (resolvedTarget != null)
        {
            hungerFollowTarget = resolvedTarget;
        }

        if (hungerFollowTarget != null)
        {
            hungerFollowSpriteRenderer = hungerFollowTarget.GetComponent<SpriteRenderer>();
            hungerAdaptiveHudWidth = hungerFollowTarget.GetComponent<AdaptiveHUDWidth>();
        }

        // Configure danger bar
        if (dangerRectTransform != null)
        {
            dangerRectTransform.anchoredPosition = dangerAnchoredPosition;
            dangerRectTransform.sizeDelta = dangerBarSize;
        }
        
        Debug.Log($"[StatusBarUI.Start] Complete - hungerSystem: {(hungerSystem != null ? "OK" : "NULL")}, hungerBarFill: {(hungerBarFill != null ? "OK" : "NULL")}, hungerFollowTarget: {(hungerFollowTarget != null ? "OK" : "NULL")}");
    }

    private void Update()
    {
        // Try to find hunger system if not already found (handles timing issues)
        if (hungerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Castor");
            if (playerObject != null)
            {
                hungerSystem = playerObject.GetComponent<TopDownHunger>();
                if (hungerSystem == null && !debugLogged)
                {
                    Debug.LogWarning($"[StatusBarUI] TopDownHunger not found on Castor, will try to add it");
                    hungerSystem = playerObject.AddComponent<TopDownHunger>();
                }
            }
        }

        // Mets à jour la barre de faim
        if (hungerSystem != null && hungerBarFill != null)
        {
            UpdateHungerBarVisuals();
        }
        else if (!debugLogged && (hungerSystem == null || hungerBarFill == null))
        {
            if (hungerSystem == null)
                Debug.LogWarning("[StatusBarUI] hungerSystem is null - hunger bar cannot update");
            if (hungerBarFill == null)
                Debug.LogWarning("[StatusBarUI] hungerBarFill is null - hunger bar cannot update");
            debugLogged = true;
        }

        // Mets à jour la barre de danger
        if (dangerSystem != null && dangerBarFill != null)
        {
            UpdateDangerBarVisuals();
        }
    }

    private void LateUpdate()
    {
        if (parentCanvas == null || canvasRectTransform == null)
        {
            EnsureCanvasReferences();
        }

        if (TryFollowFixedCanvasPoint())
        {
            return;
        }

        if (hungerFollowTarget == null || hungerRectTransform == null || canvasRectTransform == null)
        {
            if (!debugLogged && hungerFollowTarget == null)
            {
                Debug.LogWarning("[StatusBarUI] hungerFollowTarget is null - bar won't follow player");
            }
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 screenPoint;
        if (hungerAdaptiveHudWidth != null && hungerAdaptiveHudWidth.TryGetAnchorWorldPosition(out Vector3 adaptiveWorldPosition))
        {
            screenPoint = mainCamera.WorldToScreenPoint(adaptiveWorldPosition);
        }
        else
        {
            screenPoint = mainCamera.WorldToScreenPoint(hungerFollowTarget.position);
        }

        Vector2 screenOffset = hungerFollowOffset;
        float canvasScale = parentCanvas != null ? Mathf.Max(0.0001f, parentCanvas.scaleFactor) : 1f;

        if (hungerAutoPlaceAroundImage)
        {
            if (hungerFollowSpriteRenderer == null)
            {
                hungerFollowSpriteRenderer = hungerFollowTarget.GetComponent<SpriteRenderer>();
            }

            if (hungerFollowSpriteRenderer != null)
            {
                Bounds bounds = hungerFollowSpriteRenderer.bounds;
                Vector3 worldPoint = bounds.center;
                if (hungerUsePointOnTargetSprite)
                {
                    float px = Mathf.Clamp01(hungerTargetPointNormalized.x);
                    float py = Mathf.Clamp01(hungerTargetPointNormalized.y);
                    worldPoint = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, px),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, py),
                        bounds.center.z);
                }

                Vector3 center = mainCamera.WorldToScreenPoint(worldPoint);
                Vector3 right = mainCamera.WorldToScreenPoint(bounds.center + Vector3.right * bounds.extents.x);
                Vector3 up = mainCamera.WorldToScreenPoint(bounds.center + Vector3.up * bounds.extents.y);

                float spriteRadiusPixels = Mathf.Max(Mathf.Abs(right.x - center.x), Mathf.Abs(up.y - center.y));
                float spriteDiameterPixels = spriteRadiusPixels * 2f;
                float targetRingDiameter = hungerMatchTargetImageSize
                    ? spriteDiameterPixels * Mathf.Max(0.25f, hungerRingScaleMultiplier)
                    : Mathf.Max(hungerMinRingDiameterPixels, (spriteRadiusPixels + hungerImagePaddingPixels) * Mathf.Max(0.25f, hungerRingScaleMultiplier) * 2f);

                targetRingDiameter = Mathf.Clamp(targetRingDiameter, hungerMinRingDiameterPixels, Mathf.Max(hungerMinRingDiameterPixels, hungerMaxRingDiameterPixels));

                float uiDiameter = targetRingDiameter / canvasScale;
                hungerRectTransform.sizeDelta = new Vector2(uiDiameter, uiDiameter);

                if (hungerWrapAroundImage)
                {
                    screenPoint = center;
                }
                else
                {
                    Vector2 dir = new Vector2(Mathf.Cos(hungerOrbitAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(hungerOrbitAngleDegrees * Mathf.Deg2Rad));
                    float ringRadiusPixels = targetRingDiameter * 0.5f;
                    float totalRadius = spriteRadiusPixels + ringRadiusPixels + hungerImagePaddingPixels;
                    screenPoint = center + (Vector3)(dir * totalRadius);
                }

                screenOffset = hungerFollowOffset;
            }
        }

        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            hungerRectTransform.position = screenPoint + (Vector3)screenOffset;
            return;
        }

        Camera uiCamera = null;
        if (parentCanvas != null)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            hungerRectTransform.anchoredPosition = localPoint + screenOffset;
        }
    }

    void EnsureCanvasReferences()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas != null)
            {
                transform.SetParent(parentCanvas.transform, false);
            }
        }

        canvasRectTransform = parentCanvas != null ? parentCanvas.transform as RectTransform : null;
    }

    void EnsureHungerBarIsRenderable()
    {
        if (hungerBarFill == null)
        {
            Debug.LogError("[StatusBarUI.EnsureHungerBarIsRenderable] hungerBarFill is NULL!");
            return;
        }

        Debug.Log($"[StatusBarUI.EnsureHungerBarIsRenderable] hungerBarFill sprite before: {hungerBarFill.sprite}");

        if (hungerBarFill.sprite == null)
        {
            if (runtimeWhiteSprite == null)
            {
                runtimeWhiteSprite = CreateRingSprite();
                Debug.Log($"[StatusBarUI.EnsureHungerBarIsRenderable] Created ring sprite: {runtimeWhiteSprite}");
            }
            hungerBarFill.sprite = runtimeWhiteSprite;
            Debug.Log($"[StatusBarUI.EnsureHungerBarIsRenderable] Assigned ring sprite");
        }

        hungerBarFill.type = Image.Type.Filled;
        hungerBarFill.fillMethod = Image.FillMethod.Radial360;
        hungerBarFill.fillOrigin = (int)Image.Origin360.Top;
        hungerBarFill.fillClockwise = true;
        hungerBarFill.preserveAspect = true;
        hungerBarFill.color = new Color(0.32f, 0.85f, 0.35f, 1f);
        
        Debug.Log($"[StatusBarUI.EnsureHungerBarIsRenderable] Complete - sprite: {hungerBarFill.sprite}, color: {hungerBarFill.color}");
    }

    void EnsureHungerBackgroundRenderable()
    {
        if (hungerBarBackground == null)
        {
            return;
        }

        if (hungerBarBackground.transform.parent != transform)
        {
            hungerBarBackground.transform.SetParent(transform, false);
        }

        hungerBarBackground.transform.SetAsFirstSibling();

        if (hungerBarBackground.sprite == null)
        {
            if (defaultBackgroundSprite == null)
            {
                defaultBackgroundSprite = TryLoadFoodCircleSprite();
            }

            if (defaultBackgroundSprite != null)
            {
                hungerBarBackground.sprite = defaultBackgroundSprite;
            }
            else
            {
                // If we can't load the sprite, disable the background
                hungerBarBackground.enabled = false;
                Debug.LogWarning("[StatusBarUI] Could not load FoodCircle sprite, disabling background");
                return;
            }
        }

        hungerBarBackground.enabled = true;
        Color bgColor = hungerBarBackground.color;
        if (bgColor.a < 0.95f)
        {
            bgColor.a = 1f;
            hungerBarBackground.color = bgColor;
        }

        hungerBarBackground.preserveAspect = true;
    }

    void EnsureHungerBackgroundImageExists()
    {
        if (hungerBarBackground != null)
        {
            if (hungerBarBackground.transform.parent != transform)
            {
                hungerBarBackground.transform.SetParent(transform, false);
            }

            hungerBarBackground.transform.SetAsFirstSibling();
            return;
        }

        Transform existingBg = transform.Find("HungerBarBackground");
        if (existingBg != null)
        {
            hungerBarBackground = existingBg.GetComponent<Image>();
            if (hungerBarBackground != null)
            {
                return;
            }
        }

        GameObject bgObject = new GameObject("HungerBarBackground");
        bgObject.transform.SetParent(transform, false);
        bgObject.transform.SetAsFirstSibling();

        RectTransform bgRect = bgObject.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        hungerBarBackground = bgObject.AddComponent<Image>();
        hungerBarBackground.raycastTarget = false;
    }

    Transform FindTargetByName(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        return GameObject.Find(targetName)?.transform;
    }

    bool TryFollowFixedCanvasPoint()
    {
        if (!hungerFollowFixedCanvasPoint || hungerRectTransform == null || canvasRectTransform == null)
        {
            return false;
        }

        if (hungerFixedCanvasPoint == null)
        {
            return TryFollowViewportPoint();
        }

        Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, hungerFixedCanvasPoint.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        hungerRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        hungerRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        hungerRectTransform.anchoredPosition = localPoint + hungerFixedPointOffset;

        if (hungerBarBackground != null && hungerBarBackground.rectTransform != null)
        {
            hungerBarBackground.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            hungerBarBackground.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            hungerBarBackground.rectTransform.anchoredPosition = localPoint + hungerFixedPointOffset;
        }

        return true;
    }

    bool TryFollowViewportPoint()
    {
        if (!hungerUseCameraViewportPointWhenFixed || hungerRectTransform == null || canvasRectTransform == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        Vector3 viewport = new Vector3(
            Mathf.Clamp01(hungerFixedViewportPoint.x),
            Mathf.Clamp01(hungerFixedViewportPoint.y),
            Mathf.Max(0f, mainCamera.nearClipPlane + 0.01f));
        Vector2 screenPoint = mainCamera.ViewportToScreenPoint(viewport);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        hungerRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        hungerRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        hungerRectTransform.anchoredPosition = localPoint + hungerFixedPointOffset;

        if (hungerBarBackground != null && hungerBarBackground.rectTransform != null)
        {
            hungerBarBackground.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            hungerBarBackground.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            hungerBarBackground.rectTransform.anchoredPosition = localPoint + hungerFixedPointOffset;
        }

        return true;
    }

    static Sprite TryLoadFoodCircleSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(FoodCircleAssetPath);
#else
        return null;
#endif
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (defaultBackgroundSprite == null)
        {
            defaultBackgroundSprite = TryLoadFoodCircleSprite();
        }

        if (hungerBarBackground != null && hungerBarBackground.sprite == null && defaultBackgroundSprite != null)
        {
            hungerBarBackground.sprite = defaultBackgroundSprite;
            EditorUtility.SetDirty(hungerBarBackground);
        }
    }
#endif

    Sprite CreateRingSprite()
    {
        int width = 256;
        int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        float centerX = width / 2f;
        float centerY = height / 2f;
        float outerRadius = width / 2f - 2f;
        float innerRadius = outerRadius * 0.65f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist >= innerRadius && dist <= outerRadius)
                {
                    pixels[y * width + x] = Color.white;
                }
                else
                {
                    pixels[y * width + x] = new Color(1f, 1f, 1f, 0f);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    void UpdateHungerBarVisuals()
    {
        if (hungerBarFill == null || hungerSystem == null)
        {
            return;
        }

        float normalized = hungerSystem.NormalizedHunger;
        hungerBarFill.fillAmount = normalized;
        hungerBarFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f, 1f), new Color(0.32f, 0.85f, 0.35f, 1f), normalized);
        
        if (!hungerUpdateLogged)
        {
            Debug.Log($"[StatusBarUI] Hunger bar working! normalized={normalized:F2}, currentHunger={hungerSystem.CurrentHunger:F2}");
            hungerUpdateLogged = true;
        }
    }

    void UpdateDangerBarVisuals()
    {
        if (dangerBarFill == null || dangerSystem == null)
        {
            return;
        }

        float normalized = dangerSystem.NormalizedDanger;
        dangerBarFill.fillAmount = normalized;

        Color lowColor = new Color(0.95f, 0.9f, 0.2f, 1f);
        Color highColor = new Color(0.95f, 0.2f, 0.2f, 1f);
        dangerBarFill.color = Color.Lerp(lowColor, highColor, normalized);
    }

    /// <summary>
    /// Retourne la valeur de faim actuelle
    /// </summary>
    public float GetHungerValue()
    {
        if (hungerSystem != null)
        {
            return hungerSystem.NormalizedHunger;
        }
        return 0f;
    }

    /// <summary>
    /// Retourne la valeur de danger actuelle
    /// </summary>
    public float GetDangerValue()
    {
        if (dangerSystem != null)
        {
            return dangerSystem.NormalizedDanger;
        }
        return 0f;
    }
}

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Renders a circular hunger ring around a HUD target using TopDownHunger.
/// </summary>
public class TopDownHungerBarUI : MonoBehaviour
{
    private const string PrimaryTargetName = "ATH_SANSFOND_SANSREPONDRE_0";
    private const string FallbackTargetName = "Map_V2_0";
    private const string FoodCircleAssetPath = "Assets/ATH/Foodcircle.png";

    [SerializeField]
    private TopDownHunger hungerSystem;

    [SerializeField]
    private Image hungerBarFill;

    [SerializeField]
    private Image hungerBarBackground;

    [SerializeField]
    private Sprite defaultBackgroundSprite;

    [SerializeField]
    private Image centerForkIcon;

    [SerializeField]
    private bool fixedOnScreen = true;

    [SerializeField]
    private RectTransform fixedCanvasPoint;

    [SerializeField]
    private bool useCameraViewportPointWhenFixed = true;

    [SerializeField]
    private Vector2 fixedViewportPoint = new Vector2(0.5f, 0.12f);

    [SerializeField]
    private Vector2 fixedPointOffset = Vector2.zero;

    [SerializeField]
    private Vector2 fixedAnchor = new Vector2(0.5f, 0f);

    [SerializeField]
    private Vector2 fixedAnchoredPosition = new Vector2(0f, 84f);

    [SerializeField]
    private bool alignExtraElementsWithBar = true;

    [SerializeField]
    private Transform followTarget;

    [SerializeField]
    private Vector2 followOffset = Vector2.zero;

    [SerializeField]
    private bool autoPlaceAroundImage = true;

    [SerializeField]
    private bool wrapAroundImage = true;

    [SerializeField]
    private bool matchTargetImageSize = true;

    [SerializeField]
    private bool usePointOnTargetSprite = true;

    [SerializeField]
    private Vector2 targetPointNormalized = new Vector2(0.5f, 0.5f);

    [SerializeField]
    private float orbitAngleDegrees = 130f;

    [SerializeField]
    private float imagePaddingPixels = 14f;

    [SerializeField]
    private float ringScaleMultiplier = 1.1f;

    [SerializeField]
    private float minRingDiameterPixels = 200f;

    [SerializeField]
    private float maxRingDiameterPixels = 400f;

    [SerializeField]
    private float backgroundDiameterMultiplier = 0.5f;

    [SerializeField]
    private string autoTargetName = PrimaryTargetName;

    private static Sprite runtimeWhiteRingSprite;
    private RectTransform barRectTransform;
    private RectTransform backgroundRectTransform;
    private RectTransform forkRectTransform;
    private RectTransform canvasRectTransform;
    private Canvas parentCanvas;
    private SpriteRenderer followSpriteRenderer;
    private AdaptiveHUDWidth adaptiveHudWidth;

    public TopDownHunger HungerSystem
    {
        get => hungerSystem;
        set => hungerSystem = value;
    }

    public Image HungerBarFill
    {
        get => hungerBarFill;
        set => hungerBarFill = value;
    }

    public Transform FollowTarget
    {
        get => followTarget;
        set => followTarget = value;
    }

    private void Awake()
    {
        if (hungerBarFill == null)
        {
            hungerBarFill = GetComponent<Image>();
        }

        barRectTransform = hungerBarFill != null ? hungerBarFill.rectTransform : GetComponent<RectTransform>();
        backgroundRectTransform = hungerBarBackground != null ? hungerBarBackground.rectTransform : null;
        forkRectTransform = centerForkIcon != null ? centerForkIcon.rectTransform : null;
        EnsureCanvasReferences();
        EnsureRingRenderable();
        EnsureBackgroundImageExists();
        EnsureBackgroundRenderable();
        EnsureAnchoredToScreen();
    }

    private void Start()
    {
        EnsureCanvasReferences();
        AutoAssignReferences();
        EnsureAnchoredToScreen();
    }

    private void AutoAssignReferences()
    {
        // Find player's hunger system if not assigned
        if (hungerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Castor");
            if (playerObject != null)
            {
                hungerSystem = playerObject.GetComponent<TopDownHunger>();
            }
        }

        // Find fill image if not assigned
        if (hungerBarFill == null)
        {
            hungerBarFill = GetComponent<Image>();
        }

        if (followTarget == null)
        {
            Transform resolvedTarget = FindTargetByName(PrimaryTargetName);
            if (resolvedTarget == null && !string.IsNullOrWhiteSpace(autoTargetName) && autoTargetName != PrimaryTargetName)
            {
                resolvedTarget = FindTargetByName(autoTargetName);
            }
            if (resolvedTarget == null)
            {
                resolvedTarget = FindTargetByName(FallbackTargetName);
            }

            if (resolvedTarget != null)
            {
                followTarget = resolvedTarget;
                autoTargetName = resolvedTarget.name;
            }
        }

        if (followTarget != null)
        {
            followSpriteRenderer = followTarget.GetComponent<SpriteRenderer>();
            adaptiveHudWidth = followTarget.GetComponent<AdaptiveHUDWidth>();
        }
    }

    private void LateUpdate()
    {
        if (hungerSystem == null || hungerBarFill == null)
        {
            return;
        }

        UpdateHungerBar();

        if (parentCanvas == null || canvasRectTransform == null)
        {
            EnsureCanvasReferences();
        }

        if (fixedOnScreen)
        {
            UpdateFixedHudLayout();
            return;
        }

        UpdateHudPosition();
    }

    private void UpdateHungerBar()
    {
        float normalizedHunger = hungerSystem.NormalizedHunger;
        
        // Update fill amount
        hungerBarFill.fillAmount = normalizedHunger;
        
        // Update color: red (hungry) -> green (full)
        Color hungryColor = new Color(0.9f, 0.2f, 0.15f, 1f);  // Red
        Color fullColor = new Color(0.35f, 0.8f, 0.25f, 1f);    // Green
        hungerBarFill.color = Color.Lerp(hungryColor, fullColor, normalizedHunger);
    }

    private Transform FindTargetByName(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        GameObject found = GameObject.Find(targetName);
        return found != null ? found.transform : null;
    }

    private void UpdateHudPosition()
    {
        if (followTarget == null || barRectTransform == null || canvasRectTransform == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 screenPoint;
        if (adaptiveHudWidth != null && adaptiveHudWidth.TryGetAnchorWorldPosition(out Vector3 adaptiveWorldPosition))
        {
            screenPoint = mainCamera.WorldToScreenPoint(adaptiveWorldPosition);
        }
        else
        {
            screenPoint = mainCamera.WorldToScreenPoint(followTarget.position);
        }

        Vector2 screenOffset = followOffset;
        float canvasScale = parentCanvas != null ? Mathf.Max(0.0001f, parentCanvas.scaleFactor) : 1f;

        if (autoPlaceAroundImage)
        {
            if (followSpriteRenderer == null)
            {
                followSpriteRenderer = followTarget.GetComponent<SpriteRenderer>();
            }

            if (followSpriteRenderer != null)
            {
                Bounds bounds = followSpriteRenderer.bounds;
                Vector3 worldPoint = bounds.center;
                if (usePointOnTargetSprite)
                {
                    float px = Mathf.Clamp01(targetPointNormalized.x);
                    float py = Mathf.Clamp01(targetPointNormalized.y);
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
                float targetRingDiameter = matchTargetImageSize
                    ? spriteDiameterPixels * Mathf.Max(0.25f, ringScaleMultiplier)
                    : Mathf.Max(minRingDiameterPixels, (spriteRadiusPixels + imagePaddingPixels) * Mathf.Max(0.25f, ringScaleMultiplier) * 2f);

                targetRingDiameter = Mathf.Clamp(targetRingDiameter, minRingDiameterPixels, Mathf.Max(minRingDiameterPixels, maxRingDiameterPixels));

                float uiDiameter = targetRingDiameter / canvasScale;
                barRectTransform.sizeDelta = new Vector2(uiDiameter, uiDiameter);

                if (wrapAroundImage)
                {
                    screenPoint = center;
                }
                else
                {
                    Vector2 dir = new Vector2(Mathf.Cos(orbitAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(orbitAngleDegrees * Mathf.Deg2Rad));
                    float ringRadiusPixels = targetRingDiameter * 0.5f;
                    float totalRadius = spriteRadiusPixels + ringRadiusPixels + imagePaddingPixels;
                    screenPoint = center + (Vector3)(dir * totalRadius);
                }

                screenOffset = followOffset;
            }
        }

        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            barRectTransform.position = screenPoint + (Vector3)screenOffset;
            return;
        }

        Camera uiCamera = null;
        if (parentCanvas != null)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            barRectTransform.anchoredPosition = localPoint + screenOffset;
        }
    }

    private void EnsureRingRenderable()
    {
        if (hungerBarFill == null)
        {
            return;
        }

        if (hungerBarFill.sprite == null)
        {
            if (runtimeWhiteRingSprite == null)
            {
                runtimeWhiteRingSprite = CreateRingSprite();
            }
            hungerBarFill.sprite = runtimeWhiteRingSprite;
        }

        hungerBarFill.type = Image.Type.Filled;
        hungerBarFill.fillMethod = Image.FillMethod.Radial360;
        hungerBarFill.fillOrigin = (int)Image.Origin360.Top;
        hungerBarFill.fillClockwise = true;
        hungerBarFill.preserveAspect = true;
    }

    private void EnsureBackgroundRenderable()
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
    private void EnsureBackgroundImageExists()
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
                backgroundRectTransform = hungerBarBackground.rectTransform;
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
        backgroundRectTransform = bgRect;
    }
    private static Sprite TryLoadFoodCircleSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(FoodCircleAssetPath);
#else
        return null;
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
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

    private void EnsureAnchoredToScreen()
    {
        if (!fixedOnScreen || barRectTransform == null)
        {
            return;
        }

        SetAnchoring(barRectTransform, fixedAnchor);
    }

    private void UpdateFixedHudLayout()
    {
        if (barRectTransform == null)
        {
            return;
        }

        if (TryApplyFixedCanvasPoint())
        {
            return;
        }

        if (TryApplyCameraViewportPoint())
        {
            return;
        }

        SetAnchoring(barRectTransform, fixedAnchor);
        barRectTransform.anchoredPosition = fixedAnchoredPosition;

        if (!alignExtraElementsWithBar)
        {
            return;
        }

        if (hungerBarBackground != null)
        {
            if (backgroundRectTransform == null)
            {
                backgroundRectTransform = hungerBarBackground.rectTransform;
            }

            SetAnchoring(backgroundRectTransform, fixedAnchor);
            backgroundRectTransform.anchoredPosition = fixedAnchoredPosition;
        }

        if (centerForkIcon != null)
        {
            if (forkRectTransform == null)
            {
                forkRectTransform = centerForkIcon.rectTransform;
            }

            SetAnchoring(forkRectTransform, fixedAnchor);
            forkRectTransform.anchoredPosition = fixedAnchoredPosition;
        }
    }

    private static void SetAnchoring(RectTransform rectTransform, Vector2 anchor)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
    }

    private bool TryApplyFixedCanvasPoint()
    {
        if (fixedCanvasPoint == null || barRectTransform == null || canvasRectTransform == null)
        {
            return false;
        }

        Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, fixedCanvasPoint.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        barRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        barRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        Vector2 anchoredPosition = localPoint + fixedPointOffset;
        barRectTransform.anchoredPosition = anchoredPosition;

        if (alignExtraElementsWithBar)
        {
            if (hungerBarBackground != null)
            {
                if (backgroundRectTransform == null)
                {
                    backgroundRectTransform = hungerBarBackground.rectTransform;
                }

                backgroundRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                backgroundRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                backgroundRectTransform.anchoredPosition = anchoredPosition;
            }

            if (centerForkIcon != null)
            {
                if (forkRectTransform == null)
                {
                    forkRectTransform = centerForkIcon.rectTransform;
                }

                forkRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                forkRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                forkRectTransform.anchoredPosition = anchoredPosition;
            }
        }

        return true;
    }

    private bool TryApplyCameraViewportPoint()
    {
        if (!useCameraViewportPointWhenFixed || barRectTransform == null || canvasRectTransform == null)
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
            Mathf.Clamp01(fixedViewportPoint.x),
            Mathf.Clamp01(fixedViewportPoint.y),
            Mathf.Max(0f, mainCamera.nearClipPlane + 0.01f));
        Vector2 screenPoint = mainCamera.ViewportToScreenPoint(viewport);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        Vector2 anchoredPosition = localPoint + fixedPointOffset;
        barRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        barRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        barRectTransform.anchoredPosition = anchoredPosition;

        if (alignExtraElementsWithBar)
        {
            if (hungerBarBackground != null)
            {
                if (backgroundRectTransform == null)
                {
                    backgroundRectTransform = hungerBarBackground.rectTransform;
                }

                backgroundRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                backgroundRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                backgroundRectTransform.anchoredPosition = anchoredPosition;
            }

            if (centerForkIcon != null)
            {
                if (forkRectTransform == null)
                {
                    forkRectTransform = centerForkIcon.rectTransform;
                }

                forkRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                forkRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                forkRectTransform.anchoredPosition = anchoredPosition;
            }
        }

        return true;
    }

    private void EnsureCanvasReferences()
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

    private static Sprite CreateRingSprite()
    {
        const int width = 256;
        const int height = 256;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        float centerX = width * 0.5f;
        float centerY = height * 0.5f;
        float outerRadius = centerX - 2f;
        float innerRadius = outerRadius * 0.65f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                pixels[y * width + x] = (dist >= innerRadius && dist <= outerRadius)
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}

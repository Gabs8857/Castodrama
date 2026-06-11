using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Gestionnaire de la barre de faim UI circulaire avec positionnement orbital adaptatif.
/// </summary>
public class HungerBarUI : MonoBehaviour
{
    private const string PrimaryTargetName = "ATH_SANSFOND_SANSREPONDRE_0";
    private const string FallbackTargetName = "Map_V2_0";
    private const string FoodCircleAssetPath = "Assets/ATH/Foodcircle.png";

    [SerializeField] private TopDownHunger hungerSystem;
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Image hungerBarBackground;
    [SerializeField] private Sprite defaultBackgroundSprite;

    [Header("Positioning")]
    [SerializeField] private bool followFixedCanvasPoint = true;
    [SerializeField] private RectTransform fixedCanvasPoint;
    [SerializeField] private bool useCameraViewportPointWhenFixed = true;
    [SerializeField] private Vector2 fixedViewportPoint = new Vector2(0.5f, 0.12f);
    [SerializeField] private Vector2 fixedPointOffset = Vector2.zero;

    [Header("Orbital Follow")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector2 followOffset = Vector2.zero;
    [SerializeField] private bool autoPlaceAroundImage = true;
    [SerializeField] private bool wrapAroundImage = true;
    [SerializeField] private bool matchTargetImageSize = true;
    [SerializeField] private bool usePointOnTargetSprite = true;
    [SerializeField] private Vector2 targetPointNormalized = new Vector2(0.5f, 0.5f);
    [SerializeField] private float orbitAngleDegrees = 130f;
    [SerializeField] private float imagePaddingPixels = 14f;
    [SerializeField] private float ringScaleMultiplier = 1.1f;
    [SerializeField] private float minRingDiameterPixels = 200f;
    [SerializeField] private float maxRingDiameterPixels = 400f;

    private static Sprite runtimeRingSprite;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;
    private SpriteRenderer followSpriteRenderer;
    private AdaptiveHUDWidth adaptiveHudWidth;
    private bool hungerUpdateLogged = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (hungerBarFill == null) hungerBarFill = GetComponentInChildren<Image>();
        
        EnsureCanvasReferences();
        EnsureBarIsRenderable();
        EnsureBackgroundImageExists();
        EnsureBackgroundRenderable();
    }

    private void Start()
    {
        if (hungerSystem == null)
        {
            GameObject player = GameObject.Find("Castor");
            if (player != null) hungerSystem = player.GetComponent<TopDownHunger>();
        }

        if (followTarget == null)
        {
            Transform target = GameObject.Find(PrimaryTargetName)?.transform ?? GameObject.Find(FallbackTargetName)?.transform;
            if (target != null) followTarget = target;
        }

        if (followTarget != null)
        {
            followSpriteRenderer = followTarget.GetComponent<SpriteRenderer>();
            adaptiveHudWidth = followTarget.GetComponent<AdaptiveHUDWidth>();
        }
    }

    private void Update()
    {
        bool shouldBeVisible = !GameState.IsBlockingUI();
        if (hungerBarFill != null) hungerBarFill.enabled = shouldBeVisible;
        if (hungerBarBackground != null) hungerBarBackground.enabled = shouldBeVisible;

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
    }

    private void LateUpdate()
    {
        if (GameState.IsBlockingUI() || parentCanvas == null) return;

        if (TryFollowFixedCanvasPoint()) return;

        if (followTarget == null || rectTransform == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 screenPoint;
        if (adaptiveHudWidth != null && adaptiveHudWidth.TryGetAnchorWorldPosition(out Vector3 adaptiveWorldPosition))
            screenPoint = mainCamera.WorldToScreenPoint(adaptiveWorldPosition);
        else
            screenPoint = mainCamera.WorldToScreenPoint(followTarget.position);

        if (autoPlaceAroundImage)
        {
            if (followSpriteRenderer == null) followSpriteRenderer = followTarget.GetComponent<SpriteRenderer>();
            if (followSpriteRenderer != null)
            {
                Bounds bounds = followSpriteRenderer.bounds;
                Vector3 worldPoint = bounds.center;
                if (usePointOnTargetSprite)
                {
                    worldPoint = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, targetPointNormalized.x),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, targetPointNormalized.y),
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

                targetRingDiameter = Mathf.Clamp(targetRingDiameter, minRingDiameterPixels, maxRingDiameterPixels);
                float uiDiameter = targetRingDiameter / parentCanvas.scaleFactor;
                rectTransform.sizeDelta = new Vector2(uiDiameter, uiDiameter);

                if (wrapAroundImage) screenPoint = center;
                else
                {
                    Vector2 dir = new Vector2(Mathf.Cos(orbitAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(orbitAngleDegrees * Mathf.Deg2Rad));
                    screenPoint = center + (Vector3)(dir * (spriteRadiusPixels + (targetRingDiameter * 0.5f) + imagePaddingPixels));
                }
            }
        }

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rectTransform.position = screenPoint + (Vector3)followOffset;
        }
        else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, parentCanvas.worldCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + followOffset;
        }
    }

    private bool TryFollowFixedCanvasPoint()
    {
        if (!followFixedCanvasPoint || rectTransform == null) return false;
        if (fixedCanvasPoint == null) return TryFollowViewportPoint();

        Camera uiCamera = parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, fixedCanvasPoint.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + fixedPointOffset;
            return true;
        }
        return false;
    }

    private bool TryFollowViewportPoint()
    {
        if (!useCameraViewportPointWhenFixed || rectTransform == null || Camera.main == null) return false;

        Camera uiCamera = parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;
        Vector2 screenPoint = Camera.main.ViewportToScreenPoint(fixedViewportPoint);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + fixedPointOffset;
            return true;
        }
        return false;
    }

    private void EnsureCanvasReferences()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) parentCanvas = FindObjectOfType<Canvas>();
        if (parentCanvas != null) canvasRectTransform = parentCanvas.transform as RectTransform;
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
        if (existing != null) hungerBarBackground = existing.GetComponent<Image>();
        else
        {
            GameObject bg = new GameObject("HungerBarBackground");
            bg.transform.SetParent(transform, false);
            bg.transform.SetAsFirstSibling();
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
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
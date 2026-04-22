using UnityEngine;
using UnityEngine.UI;

public class Hunger : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;

    public Image hungerBar;
    public Transform followTarget;
    public Vector2 followOffset = Vector2.zero;
    public bool autoPlaceAroundImage = true;
    public bool wrapAroundImage = true;
    public bool matchTargetImageSize = true;
    public bool usePointOnTargetSprite = true;
    public Vector2 targetPointNormalized = new Vector2(0.62f, 0.59f);
    public float orbitAngleDegrees = 130f;
    public float imagePaddingPixels = 14f;
    public float ringScaleMultiplier = 1.1f;
    public float minRingDiameterPixels = 56f;
    public string autoTargetName = "Map_V2_0";

    private static Sprite runtimeWhiteSprite;
    private RectTransform barRectTransform;
    private RectTransform canvasRectTransform;
    private Canvas parentCanvas;
    private SpriteRenderer followSpriteRenderer;

    void Awake()
    {
        if (hungerBar == null)
        {
            hungerBar = GetComponent<Image>();
        }

        barRectTransform = hungerBar != null ? hungerBar.rectTransform : GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas != null ? parentCanvas.transform as RectTransform : null;
        EnsureBarIsRenderable();
    }

    void Start()
    {
        if (maxHunger <= 0f)
        {
            maxHunger = 100f;
        }

        if (!string.IsNullOrWhiteSpace(autoTargetName))
        {
            GameObject candidate = GameObject.Find(autoTargetName);
            if (candidate != null)
            {
                // Always prioritize the named target so serialized scene refs cannot drift across machines.
                followTarget = candidate.transform;
            }
        }

        if (followTarget != null)
        {
            followSpriteRenderer = followTarget.GetComponent<SpriteRenderer>();
        }

        currentHunger = maxHunger;
        UpdateBarVisuals();
    }

    void Update()
    {
        currentHunger -= Time.deltaTime * 2f;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        UpdateBarVisuals();
    }

    void LateUpdate()
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

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(followTarget.position);
        Vector2 screenOffset = followOffset;

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

                float canvasScale = parentCanvas != null ? Mathf.Max(0.0001f, parentCanvas.scaleFactor) : 1f;
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

        Camera uiCamera = parentCanvas != null ? parentCanvas.worldCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            barRectTransform.anchoredPosition = localPoint + screenOffset;
        }
    }

    void EnsureBarIsRenderable()
    {
        if (hungerBar == null)
        {
            return;
        }

        if (hungerBar.sprite == null)
        {
            if (runtimeWhiteSprite == null)
            {
                runtimeWhiteSprite = CreateRingSprite();
            }
            hungerBar.sprite = runtimeWhiteSprite;
        }

        hungerBar.type = Image.Type.Filled;
        hungerBar.fillMethod = Image.FillMethod.Radial360;
        hungerBar.fillOrigin = (int)Image.Origin360.Top;
        hungerBar.fillClockwise = true;
        hungerBar.preserveAspect = true;
        hungerBar.color = new Color(0.32f, 0.85f, 0.35f, 1f);
    }

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
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                pixels[y * width + x] = distance <= outerRadius && distance >= innerRadius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    void UpdateBarVisuals()
    {
        if (hungerBar == null)
        {
            return;
        }

        float normalized = maxHunger > 0f ? currentHunger / maxHunger : 0f;
        hungerBar.fillAmount = normalized;
        hungerBar.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f, 1f), new Color(0.32f, 0.85f, 0.35f, 1f), normalized);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class TopDownDangerBarUI : MonoBehaviour
{
    [SerializeField]
    private TopDownDanger dangerSystem;

    [SerializeField]
    private Image dangerFill;

    [SerializeField]
    private Image dangerBackground;

    [SerializeField]
    private bool fixedOnScreen = true;

    [SerializeField]
    private Vector2 anchor = new Vector2(0f, 0f);

    [SerializeField]
    private Vector2 anchoredPosition = new Vector2(132f, 124f);

    [SerializeField]
    private Vector2 barSize = new Vector2(96f, 96f);

    [SerializeField]
    private float ringThicknessFactor = 0.34f;

    private static Sprite runtimeWhiteSprite;
    private static Sprite runtimeRingSprite;
    private RectTransform rootRect;

    private void Awake()
    {
        rootRect = GetComponent<RectTransform>();
        EnsureVisuals();
    }

    private void Start()
    {
        if (dangerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                dangerSystem = playerObject.GetComponent<TopDownDanger>();
            }
        }

        ApplyLayout();
    }

    private void LateUpdate()
    {
        if (dangerSystem == null || dangerFill == null)
        {
            return;
        }

        if (fixedOnScreen)
        {
            ApplyLayout();
        }

        float normalized = dangerSystem.NormalizedDanger;
        dangerFill.fillAmount = normalized;

        Color lowColor = new Color(0.95f, 0.9f, 0.2f, 1f);
        Color highColor = new Color(0.95f, 0.2f, 0.2f, 1f);
        dangerFill.color = Color.Lerp(lowColor, highColor, normalized);
    }

    private void EnsureVisuals()
    {
        if (runtimeWhiteSprite == null)
        {
            runtimeWhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        if (runtimeRingSprite == null)
        {
            runtimeRingSprite = CreateRingSprite(256, Mathf.Clamp(ringThicknessFactor, 0.15f, 0.8f));
        }

        if (dangerBackground == null)
        {
            dangerBackground = GetComponent<Image>();
            if (dangerBackground == null)
            {
                dangerBackground = gameObject.AddComponent<Image>();
            }
        }

        dangerBackground.sprite = runtimeRingSprite;
        dangerBackground.type = Image.Type.Simple;
        dangerBackground.color = new Color(0f, 0f, 0f, 0.55f);
        dangerBackground.preserveAspect = true;
        dangerBackground.raycastTarget = false;

        if (dangerFill == null)
        {
            Transform existingFill = transform.Find("DangerFill");
            if (existingFill != null)
            {
                dangerFill = existingFill.GetComponent<Image>();
            }
        }

        if (dangerFill == null)
        {
            GameObject fillObject = new GameObject("DangerFill");
            fillObject.transform.SetParent(transform, false);
            dangerFill = fillObject.AddComponent<Image>();
        }

        RectTransform fillRect = dangerFill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        dangerFill.sprite = runtimeRingSprite;
        dangerFill.type = Image.Type.Filled;
        dangerFill.fillMethod = Image.FillMethod.Radial360;
        dangerFill.fillOrigin = (int)Image.Origin360.Top;
        dangerFill.fillClockwise = true;
        dangerFill.preserveAspect = true;
        dangerFill.fillAmount = 0f;
        dangerFill.raycastTarget = false;
    }

    private void ApplyLayout()
    {
        if (rootRect == null)
        {
            return;
        }

        rootRect.anchorMin = anchor;
        rootRect.anchorMax = anchor;
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = barSize;
    }

    private static Sprite CreateRingSprite(int size, float thicknessFactor)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float center = size * 0.5f;
        float outerRadius = center - 2f;
        float innerRadius = outerRadius * (1f - thicknessFactor);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                pixels[y * size + x] = (dist >= innerRadius && dist <= outerRadius)
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}

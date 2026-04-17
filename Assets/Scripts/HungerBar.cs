using UnityEngine;
using UnityEngine.UI;

public class Hunger : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;

    public Image hungerBar;

    private RectTransform barRect;
    private static Sprite runtimeWhiteSprite;

    void Awake()
    {
        if (hungerBar == null)
        {
            hungerBar = GetComponent<Image>();
        }

        barRect = hungerBar != null ? hungerBar.rectTransform : GetComponent<RectTransform>();
        EnsureBarIsRenderable();
        EnsureBarIsOnScreen();
    }

    void Start()
    {
        if (maxHunger <= 0f)
        {
            maxHunger = 100f;
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
        hungerBar.color = new Color(0.32f, 0.85f, 0.35f, 1f);
    }

    Sprite CreateRingSprite()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float centerX = size / 2f;
        float centerY = size / 2f;
        float outerRadius = size / 2f - 2f;
        float innerRadius = outerRadius * 0.6f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= outerRadius && distance >= innerRadius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void EnsureBarIsOnScreen()
    {
        if (barRect == null)
        {
            return;
        }

        if (barRect.sizeDelta.x < 100f || barRect.sizeDelta.y < 100f)
        {
            barRect.sizeDelta = new Vector2(100f, 100f);
        }

        if (barRect.anchoredPosition.x < 0f || barRect.anchoredPosition.y < 0f)
        {
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 0f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.anchoredPosition = new Vector2(253f, 218f);
        }
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
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
                runtimeWhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            }
            hungerBar.sprite = runtimeWhiteSprite;
        }

        hungerBar.type = Image.Type.Filled;
        hungerBar.fillMethod = Image.FillMethod.Horizontal;
        hungerBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        hungerBar.color = new Color(0.32f, 0.85f, 0.35f, 1f);
    }

    void EnsureBarIsOnScreen()
    {
        if (barRect == null)
        {
            return;
        }

        if (barRect.sizeDelta.x < 100f || barRect.sizeDelta.y < 8f)
        {
            barRect.sizeDelta = new Vector2(220f, 18f);
        }

        if (barRect.anchoredPosition.x < 0f || barRect.anchoredPosition.y < 0f)
        {
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 0f);
            barRect.pivot = new Vector2(0f, 0f);
            barRect.anchoredPosition = new Vector2(24f, 24f);
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
using UnityEngine;
using UnityEngine.UI;

public class TopDownHungerBarUI : MonoBehaviour
{
    [SerializeField]
    private TopDownHunger hunger;

    [SerializeField]
    private Image fillImage;

    public TopDownHunger Hunger
    {
        get => hunger;
        set => hunger = value;
    }

    public Image FillImage
    {
        get => fillImage;
        set => fillImage = value;
    }

    private void Start()
    {
        // Auto-assign the TopDownHunger from the player
        if (hunger == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                hunger = playerObject.GetComponent<TopDownHunger>();
            }
        }

        // Auto-assign the Image component if not assigned
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }
    }

    private void LateUpdate()
    {
        if (hunger == null || fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = hunger.NormalizedHunger;
        fillImage.color = Color.Lerp(new Color(0.9f, 0.2f, 0.15f, 1f), new Color(0.35f, 0.8f, 0.25f, 1f), hunger.NormalizedHunger);
    }
}

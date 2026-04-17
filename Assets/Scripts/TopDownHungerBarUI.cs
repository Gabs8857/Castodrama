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

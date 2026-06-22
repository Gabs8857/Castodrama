using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barre de progression pour le claim des buttes à Castoreum (Radial 360).
/// </summary>
public class CastoreumProgressUI : MonoBehaviour
{
    [Header("📊 Références")]
    [SerializeField] private CastoreumManager castoreumManager;
    [SerializeField] private Image progressFill;
    [SerializeField] private Text progressText;

    [Header("🎨 Style")]
    [SerializeField] private Color barColor = new Color(0.8f, 0.4f, 0.8f);

    private void Start()
    {
        if (castoreumManager == null)
            castoreumManager = FindObjectOfType<CastoreumManager>();

        if (progressFill == null)
            progressFill = GetComponentInChildren<Image>();

        if (progressText == null)
            progressText = GetComponentInChildren<Text>();

        if (progressFill != null)
        {
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Radial360;
            progressFill.fillOrigin = (int)Image.Origin360.Top;  
            progressFill.color = barColor;
        }
    }

    private void Update()
    {
        if (castoreumManager == null || progressFill == null) return;

        float totalProgress = castoreumManager.GetTotalClaimProgress();
        progressFill.fillAmount = totalProgress;

        if (progressText != null)
        {
            int claimedCount = castoreumManager.GetClaimedMoundsCount();
            progressText.text = $"{totalProgress:P0} ({claimedCount}/5)";
        }
    }
}
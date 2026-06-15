using UnityEngine;
using UnityEngine.UI;
using TMPro;

// À attacher sur le GameObject du panel stats (au-dessus du chat)
public class StatsUIManager : MonoBehaviour
{
    [Header("Viewers")]
    public TMP_Text viewersText;

    [Header("Signatures")]
    public TMP_Text signaturesText;
    public Slider signaturesProgressBar; // barre de progression vers 1000

    void Awake()
    {
        GameState.statsUIManager = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (viewersText != null)
            viewersText.text = GameState.viewers.ToString();

        if (signaturesText != null)
            signaturesText.text = GameState.signatures + " / " + GameState.signaturesGoal;

        if (signaturesProgressBar != null)
        {
            signaturesProgressBar.minValue = 0;
            signaturesProgressBar.maxValue = GameState.signaturesGoal;
            signaturesProgressBar.value = GameState.signatures;
        }

        Debug.Log($"[StatsUIManager] Mise à jour UI : Viewers={GameState.viewers}, Signatures={GameState.signatures}/{GameState.signaturesGoal}");
    }
}
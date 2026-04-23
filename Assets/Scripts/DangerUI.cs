using UnityEngine;
using UnityEngine.UI;

public class DangerUI : MonoBehaviour
{
    public Danger playerDanger;
    public Image bar;

    void Update()
    {
        bar.fillAmount = playerDanger.currentDanger / playerDanger.maxDanger;
    }
}
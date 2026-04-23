using UnityEngine;

public class TopDownDanger : MonoBehaviour
{
    [SerializeField]
    private float maxDanger = 100f;

    [SerializeField]
    private float currentDanger = 0f;

    [SerializeField]
    private float increasePerSecond = 20f;

    [SerializeField]
    private float decreasePerSecond = 12f;

    private int activeDangerZones;

    public float MaxDanger => maxDanger;

    public float CurrentDanger => currentDanger;

    public float NormalizedDanger => maxDanger <= 0f ? 0f : Mathf.Clamp01(currentDanger / maxDanger);

    public bool IsInDangerZone => activeDangerZones > 0;

    private void Update()
    {
        if (maxDanger <= 0f)
        {
            currentDanger = 0f;
            return;
        }

        float delta = IsInDangerZone ? increasePerSecond : -decreasePerSecond;
        currentDanger = Mathf.Clamp(currentDanger + delta * Time.deltaTime, 0f, maxDanger);
    }

    public void EnterDangerZone()
    {
        activeDangerZones++;
    }

    public void ExitDangerZone()
    {
        activeDangerZones = Mathf.Max(0, activeDangerZones - 1);
    }

    public void SetDanger(float value)
    {
        currentDanger = Mathf.Clamp(value, 0f, maxDanger);
    }
}

using UnityEngine;

public class TopDownHunger : MonoBehaviour
{
    [SerializeField]
    private float maxHunger = 100f;

    [SerializeField]
    private float currentHunger = 100f;

    [SerializeField]
    private float drainPerSecond = 0.5f;

    public float MaxHunger => maxHunger;

    public float CurrentHunger => currentHunger;

    public float NormalizedHunger => maxHunger <= 0f ? 0f : Mathf.Clamp01(currentHunger / maxHunger);

    private void Update()
    {
        if (drainPerSecond <= 0f)
        {
            return;
        }

        currentHunger = Mathf.Max(0f, currentHunger - drainPerSecond * Time.deltaTime);
    }

    public void SetHunger(float value)
    {
        currentHunger = Mathf.Clamp(value, 0f, maxHunger);
    }

    public void AddHunger(float amount)
    {
        SetHunger(currentHunger + amount);
    }
}

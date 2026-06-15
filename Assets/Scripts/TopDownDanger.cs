using System.Collections.Generic;
using UnityEngine;

public class TopDownDanger : MonoBehaviour
{
    [SerializeField] private float maxDanger = 100f;
    [SerializeField] private float currentDanger = 0f;
    [SerializeField] private float decreasePerSecond = 12f;

    [Header("Téléportation")]
    [SerializeField] private string spawnPointName = "TP_EnSortie";

    [Header("Fin de partie")]
    [Tooltip("Nombre de fois où le danger peut atteindre son maximum avant la fin de partie")]
    [SerializeField] private int maxDangerTriggersBeforeEnding = 3;

    // Liste des vitesses d'augmentation actives (une par zone de danger dans laquelle on se trouve)
    private readonly List<float> activeIncreaseRates = new List<float>();
    private bool hasTeleported = false;
    private int dangerMaxedCount = 0;

    public float MaxDanger => maxDanger;
    public float CurrentDanger => currentDanger;
    public float NormalizedDanger => maxDanger <= 0f ? 0f : Mathf.Clamp01(currentDanger / maxDanger);
    public bool IsInDangerZone => activeIncreaseRates.Count > 0;
    public int DangerMaxedCount => dangerMaxedCount;

    private void Update()
    {
        if (maxDanger <= 0f)
        {
            currentDanger = 0f;
            return;
        }

        float delta;
        if (IsInDangerZone)
        {
            // Utilise la vitesse la plus élevée parmi les zones actives
            float currentRate = activeIncreaseRates[0];
            for (int i = 1; i < activeIncreaseRates.Count; i++)
            {
                if (activeIncreaseRates[i] > currentRate)
                    currentRate = activeIncreaseRates[i];
            }
            delta = currentRate;
        }
        else
        {
            delta = -decreasePerSecond;
        }

        currentDanger = Mathf.Clamp(currentDanger + delta * Time.deltaTime, 0f, maxDanger);

        if (currentDanger >= maxDanger && !hasTeleported)
        {
            OnDangerMaxed();
        }

        // Réarme le téléport une fois que le danger a redescendu
        if (currentDanger < maxDanger * 0.9f)
            hasTeleported = false;
    }

    private void OnDangerMaxed()
    {
        dangerMaxedCount++;
        hasTeleported = true;

        if (dangerMaxedCount >= maxDangerTriggersBeforeEnding)
        {
            EndingManager.TriggerEnding(
                "J'ai été trop aventureux...",
                "Je recommencerai une autre fois."
            );
            return;
        }

        TeleportToSpawn();
        currentDanger = 0f;
    }

    private void TeleportToSpawn()
    {
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[TopDownDanger] Point de spawn '{spawnPointName}' introuvable dans la scène !");
            return;
        }

        transform.position = spawnPoint.transform.position;
        Debug.Log($"[TopDownDanger] Joueur téléporté à '{spawnPointName}'. ({dangerMaxedCount}/{maxDangerTriggersBeforeEnding})");
    }

    public void EnterDangerZone(float increaseRate)
    {
        activeIncreaseRates.Add(increaseRate);
    }

    public void ExitDangerZone(float increaseRate)
    {
        activeIncreaseRates.Remove(increaseRate);
    }

    public void SetDanger(float value)
    {
        currentDanger = Mathf.Clamp(value, 0f, maxDanger);
    }

    /// <summary>
    /// Réinitialise le compteur de "danger max" (utile pour un nouveau run après l'écran de fin).
    /// </summary>
    public void ResetDangerMaxedCount()
    {
        dangerMaxedCount = 0;
        hasTeleported = false;
        currentDanger = 0f;
    }
}

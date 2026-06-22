using UnityEngine;

/// <summary>
/// Gère les 5 buttes à Castoreum et calcule le taux de claim global.
/// </summary>
public class CastoreumManager : MonoBehaviour
{
    [Header("Buttes à Castoreum")]
    [SerializeField] private CastoreumMound[] mounds = new CastoreumMound[5]; // Assigne tes 5 buttes ici

    public float GetTotalClaimProgress()
    {
        if (mounds == null || mounds.Length == 0) return 0f;

        float total = 0f;
        foreach (var mound in mounds)
        {
            if (mound != null)
                total += mound.GetClaimProgress();
        }
        return total / mounds.Length; // Moyenne (0-1)
    }

    public int GetClaimedMoundsCount()
    {
        int count = 0;
        foreach (var mound in mounds)
        {
            if (mound != null && mound.IsFullyClaimed())
                count++;
        }
        return count;
    }
}
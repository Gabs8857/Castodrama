using UnityEngine;
using System.Collections.Generic;

public class ZoneRoute : MonoBehaviour
{
[SerializeField] private List<ZoneType> routeZones = new List<ZoneType>();
    /// <summary>
    /// Vérifie si la route est complète (toutes les zones ont été traversées)
    /// </summary>
    public bool IsRouteComplete(ZoneDetectionManager zoneDetectionManager)
    {
        foreach (var zone in routeZones)
        {
            if (!zoneDetectionManager.IsInZone(zone))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Affiche les zones de la route dans la console
    /// </summary>
    public void LogRoute()
    {
        Debug.Log($"[ZoneRoute] Route zones: {string.Join(", ", routeZones)}", this);
    }
}
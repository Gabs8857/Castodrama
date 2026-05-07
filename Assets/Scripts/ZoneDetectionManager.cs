using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire centralisé de détection de zones
/// Suivi des zones actuellement occupées par un objet (joueur, items, etc.)
/// Permet aux scripts de vérifier si un objet est dans une zone spécifique
/// </summary>
public class ZoneDetectionManager
{
    private HashSet<ZoneType> currentZones = new HashSet<ZoneType>();
    private IZoneDetectable owner;

    public ZoneDetectionManager(IZoneDetectable owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Ajoute une zone et notifie le propriétaire
    /// </summary>
    /// <param name="zoneType">Type de zone dans laquelle on entre</param>
    public void EnterZone(ZoneType zoneType)
    {
        if (!currentZones.Contains(zoneType))
        {
            currentZones.Add(zoneType);
            owner?.OnEnterZone(zoneType);
            Debug.Log($"[ZoneDetectionManager] Entrée zone: {zoneType}. Zones actuelles: {string.Join(", ", currentZones)}");
        }
    }

    /// <summary>
    /// Retire une zone et notifie le propriétaire
    /// </summary>
    /// <param name="zoneType">Type de zone qu'on quitte</param>
    public void ExitZone(ZoneType zoneType)
    {
        if (currentZones.Contains(zoneType))
        {
            currentZones.Remove(zoneType);
            owner?.OnExitZone(zoneType);
            Debug.Log($"[ZoneDetectionManager] Sortie zone: {zoneType}. Zones restantes: {string.Join(", ", currentZones)}");
        }
    }

    /// <summary>
    /// Vérifie si l'objet est dans une zone spécifique
    /// </summary>
    public bool IsInZone(ZoneType zoneType)
    {
        return currentZones.Contains(zoneType);
    }

    /// <summary>
    /// Retourne les zones actuelles
    /// </summary>
    public IReadOnlyCollection<ZoneType> GetCurrentZones()
    {
        return currentZones;
    }

    /// <summary>
    /// Vérifie s'il y a au moins une zone active
    /// </summary>
    public bool IsInAnyZone()
    {
        return currentZones.Count > 0;
    }

    /// <summary>
    /// Réinitialise toutes les zones
    /// </summary>
    public void ClearZones()
    {
        currentZones.Clear();
    }
}

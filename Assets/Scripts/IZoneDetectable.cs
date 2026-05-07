using UnityEngine;

/// <summary>
/// Interface pour les objets qui réagissent aux changements de zones
/// Implémentée par TopDownPlayerController, CharacterAnimator, EquippableItem, etc.
/// Permet une détection centralisée des zones (eau, lave, feu, etc.)
/// </summary>
public interface IZoneDetectable
{
    /// <summary>
    /// Appelé quand l'objet entre dans une zone
    /// </summary>
    /// <param name="zoneType">Type de zone (Water, Lava, Fire, Ice)</param>
    void OnEnterZone(ZoneType zoneType);

    /// <summary>
    /// Appelé quand l'objet sort d'une zone
    /// </summary>
    /// <param name="zoneType">Type de zone (Water, Lava, Fire, Ice)</param>
    void OnExitZone(ZoneType zoneType);
}

/// <summary>
/// Types de zones possibles dans le jeu
/// Extensible pour ajouter de nouvelles zones (lave, feu, glace, etc.)
/// </summary>
public enum ZoneType
{
    Water,    // Zone d'eau - Active la nage
    Lava,     // Zone de lave - Dégâts/Destruction (futur)
    Fire,     // Zone de feu - Dégâts (futur)
    Ice,      // Zone de glace - Ralentissement (futur)
}

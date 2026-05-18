using UnityEngine;

/// <summary>
/// Bouleau (Birch) - Comestible food item with progression system.
/// First eat: frame 2 -> Second eat: frame 3 -> Third eat: disappears
/// </summary>
public class BirchFoodItem : FoodItem
{
    protected override void Awake()
    {
        // Set Birch-specific defaults
        hungerRestoreAmount = 15f;
        visibleTint = new Color(0.8f, 0.7f, 0.5f, 1f); // Birch light color
        glowIntensity = 0.7f;
        base.Awake();
    }
}

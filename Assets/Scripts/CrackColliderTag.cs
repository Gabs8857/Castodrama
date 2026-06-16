using UnityEngine;

/// <summary>
/// Tag à mettre sur le BoxCollider2D de chaque fissure du barrage.
/// Permet au MudCloud d'identifier quelle fissure il touche.
/// 
/// Setup : ajoute ce script sur chaque GameObject de fissure,
/// et assigne l'index correspondant (0, 1, 2, 3).
/// </summary>
public class CrackColliderTag : MonoBehaviour
{
    [Tooltip("Index de la fissure (doit correspondre à l'ordre dans DamManager.crackPositions)")]
    [SerializeField] private int crackIndex = 0;

    public int CrackIndex => crackIndex;
}

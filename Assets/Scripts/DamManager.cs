using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère le système du barrage avec ses fissures progressives
/// - 4 emplacements de fissures potentielles
/// - Les fissures apparaissent progressivement avec le temps
/// - Peut être réparé en apportant une branche à l'emplacement de réparation
/// </summary>
public class DamManager : MonoBehaviour
{
    [Header("Dam Crack Positions")]
    [SerializeField] private Transform[] crackPositions = new Transform[4];
    
    [Header("Crack Settings")]
    [SerializeField] private float[] crackAppearanceTime = { 10f, 15f, 20f, 25f }; // Temps d'apparition pour chaque fissure (en secondes)
    [SerializeField] private float repairCooldown = 2f; // Temps avant de pouvoir réparer à nouveau
    
    [Header("Visual")]
    [SerializeField] private Sprite crackSprite;
    [SerializeField] private SpriteRenderer[] crackVisuals = new SpriteRenderer[4];
    [SerializeField] private LeakAnimator[] leakAnimators = new LeakAnimator[4];
    
    [Header("Time Reference")]
    [SerializeField] private DayAndNightCycle dayNightCycle;
    
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private List<DamCrack> cracks = new List<DamCrack>();
    private float elapsedTime = 0f;
    private int cracksCreated = 0;
    private float lastRepairTime = -10f;
    private const int MAX_CRACKS = 4;
    private Collider2D damCollider;

    private void Start()
    {
    Debug.Log($"[DamManager] Starting initialization... Barrage");
        // Récupérer le collider du barrage lui-même
        damCollider = GetComponent<Collider2D>();
        if (damCollider == null)
        {
            if (debugLogs)
                Debug.LogWarning("[DamManager] No Collider2D found on Barrage! Add a BoxCollider2D as trigger for repair zone.");
        }

        // Initialiser les fissures (même sans collider)
        for (int i = 0; i < MAX_CRACKS; i++)
        {
            var crack = new DamCrack(i, crackPositions[i], crackVisuals[i], leakAnimators[i]);
            cracks.Add(crack);
            if (debugLogs)
                Debug.Log($"[DamManager] Crack {i + 1} initialized");
        }

        if (debugLogs)
            Debug.Log("[DamManager] Initialized with 4 empty crack positions");
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (debugLogs && Mathf.FloorToInt(elapsedTime) > Mathf.FloorToInt(elapsedTime - Time.deltaTime))  // Log chaque seconde
            Debug.Log($"[DamManager] Elapsed time: {elapsedTime:F1}s - Cracks: {cracksCreated}/{MAX_CRACKS}");

        // Ajouter des fissures progressivement
        UpdateCrackProgression();
        
        // Vérifier la réparation si une branche est dans la zone
        CheckRepairZone();
    }

    /// <summary>
    /// Gère la progression des fissures basée sur le temps
    /// </summary>
    private void UpdateCrackProgression()
    {
        // Vérifier si on doit ajouter une nouvelle fissure
        if (cracksCreated < MAX_CRACKS)
        {
            float timeUntilNextCrack = crackAppearanceTime[cracksCreated];

            if (elapsedTime >= timeUntilNextCrack)
            {
                CreateNewCrack();
            }
        }
    }

    /// <summary>
    /// Crée une nouvelle fissure et l'affiche
    /// </summary>
    private void CreateNewCrack()
    {
        if (cracksCreated >= MAX_CRACKS) return;

        if (cracksCreated >= cracks.Count)
        {
            if (debugLogs)
                Debug.LogError($"[DamManager] Crack index {cracksCreated} out of range! Cracks list count: {cracks.Count}");
            return;
        }

        DamCrack newCrack = cracks[cracksCreated];
        newCrack.Appear();

        if (debugLogs)
            Debug.Log($"[DamManager] Crack #{cracksCreated + 1} appeared at position {cracksCreated}");

        cracksCreated++;
    }

    /// <summary>
    /// Vérifie si une branche se trouve dans la zone de réparation (le Barrage lui-même)
    /// </summary>
    private void CheckRepairZone()
    {
        if (damCollider == null || cracksCreated == 0)
        {
            if (debugLogs && damCollider == null && cracksCreated > 0)
                Debug.LogWarning("[DamManager] No collider for repair detection!");
            return;
        }

        // Cooldown de réparation
        if (Time.time - lastRepairTime < repairCooldown) return;

        // Chercher les items dans le collider du Barrage
        Collider2D[] colliders = Physics2D.OverlapBoxAll(damCollider.bounds.center, damCollider.bounds.size, 0f);

        if (debugLogs && colliders.Length > 0)
            Debug.Log($"[DamManager] Found {colliders.Length} items in repair zone");

        foreach (Collider2D collider in colliders)
        {
            EquippableItem item = collider.GetComponent<EquippableItem>();
            if (item != null && IsBranch(item))
            {
                if (debugLogs)
                    Debug.Log($"[DamManager] Branch detected! Repairing...");
                RepairDam(item);
                return;
            }
        }
    }

    /// <summary>
    /// Vérifie si un item est une branche
    /// </summary>
    private bool IsBranch(EquippableItem item)
    {
        // Vérifier par type ou par nom
        string itemName = item.gameObject.name.ToLower();
        return itemName.Contains("branch") || 
               itemName.Contains("baton") ||
               item.GetComponent<FoodItem>() != null && item.GetComponent<FoodItem>().GetType().Name.Contains("Birch");
    }

    /// <summary>
    /// Répare le barrage en réduisant le nombre de fissures
    /// </summary>
    private void RepairDam(EquippableItem branchItem)
    {
        if (cracksCreated == 0) return;

        // Enlever la dernière fissure (la plus récente)
        DamCrack lastCrack = cracks[cracksCreated - 1];
        lastCrack.Disappear();
        cracksCreated--;

        // Détruire ou déposer la branche
        Destroy(branchItem.gameObject);

        lastRepairTime = Time.time;

        if (debugLogs)
            Debug.Log($"[DamManager] Dam repaired! Cracks remaining: {cracksCreated}/{MAX_CRACKS}");
    }

    /// <summary>
    /// Répare le barrage avec une branche (version publique pour BranchRepairItem)
    /// </summary>
    public void RepairDamWithBranch(BranchRepairItem branchItem)
    {
        if (cracksCreated == 0) return;

        // Enlever la dernière fissure (la plus récente)
        DamCrack lastCrack = cracks[cracksCreated - 1];
        lastCrack.Disappear();
        cracksCreated--;

        // Détruire la branche
        Destroy(branchItem.gameObject);

        lastRepairTime = Time.time;

        if (debugLogs)
            Debug.Log($"[DamManager] Dam repaired! Cracks remaining: {cracksCreated}/{MAX_CRACKS}");
    }

    /// <summary>
    /// Obtient le nombre de fissures actuelles
    /// </summary>
    public int GetCurrentCrackCount()
    {
        return cracksCreated;
    }

    /// <summary>
    /// Obtient le temps total écoulé depuis le début
    /// </summary>
    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    /// <summary>
    /// Classe interne pour gérer une fissure
    /// </summary>
    private class DamCrack
    {
        private int index;
        private Transform position;
        private SpriteRenderer visual;
        private LeakAnimator leakAnimator;
        private bool isActive = false;
        private bool debugLogs = false;

        public DamCrack(int index, Transform position, SpriteRenderer visual, LeakAnimator animator)
        {
            this.index = index;
            this.position = position;
            this.visual = visual;
            this.leakAnimator = animator;
            debugLogs = true;
        }

        public void Appear()
        {
            if (visual != null)
            {
                visual.enabled = true;
                if (debugLogs)
                    Debug.Log($"[DamCrack {index}] Visual enabled");
            }
            else if (debugLogs)
                Debug.LogWarning($"[DamCrack {index}] No SpriteRenderer!");

            if (leakAnimator != null)
            {
                leakAnimator.StartLeaking();
                if (debugLogs)
                    Debug.Log($"[DamCrack {index}] LeakAnimator started");
            }
            else if (debugLogs)
                Debug.LogWarning($"[DamCrack {index}] No LeakAnimator!");

            isActive = true;
        }

        public void Disappear()
        {
            if (visual != null)
            {
                visual.enabled = false;
                if (debugLogs)
                    Debug.Log($"[DamCrack {index}] Visual disabled");
            }

            if (leakAnimator != null)
            {
                leakAnimator.StopLeaking();
                if (debugLogs)
                    Debug.Log($"[DamCrack {index}] LeakAnimator stopped");
            }

            isActive = false;
        }

        public bool IsActive => isActive;
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère le système du barrage avec ses fissures progressives.
/// Réparable avec des branches (BranchRepairItem) OU des nuages de boue (MudCloud).
/// 
/// Chaque fissure a HITS_TO_REPAIR charges : un nuage de boue retire 1 charge,
/// une branche répare instantanément.
/// 
/// nextCrackIndex : avance séquentiellement 0→4, jamais décrémenté
/// activeCrackCount : nombre de fissures visibles en ce moment
/// </summary>
public class DamManager : MonoBehaviour
{
    [Header("Dam Crack Positions")]
    [SerializeField] private Transform[] crackPositions = new Transform[4];

    [Header("Branch Detection")]
    [SerializeField] private string branchNameFilter = "branch";

    [Header("Crack Settings")]
    [SerializeField] private float[] crackAppearanceTime = { 10f, 15f, 20f, 25f };
    [SerializeField] private float repairCooldown = 0.5f;

    [Header("Mud Charges")]
    [Tooltip("Nombre de nuages de boue nécessaires pour réparer une fissure")]
    [SerializeField] private int hitsToRepair = 3;

    [Header("Visual")]
    [SerializeField] private Sprite noneLeakSprite;
    [SerializeField] private SpriteRenderer[] crackVisuals = new SpriteRenderer[4];
    [SerializeField] private LeakAnimator[] leakAnimators = new LeakAnimator[4];

    [Header("UI")]
    [SerializeField] private CrackBarUI crackBarUI;

    [Header("Time Reference")]
    [SerializeField] private DayAndNightCycle dayNightCycle;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private List<DamCrack> cracks = new List<DamCrack>();
    private float elapsedTime = 0f;
    private float timerToNextCrack = 0f;
    private int activeCrackCount = 0;
    private int nextCrackIndex = 0;
    private float lastRepairTime = -10f;
    public const int MAX_CRACKS = 4;
    private Collider2D damCollider;

    private void Start()
    {
        Debug.Log("[DamManager] Starting initialization... Barrage");

        damCollider = GetComponent<Collider2D>();
        if (damCollider == null && debugLogs)
            Debug.LogWarning("[DamManager] No Collider2D found on Barrage!");

        for (int i = 0; i < MAX_CRACKS; i++)
        {
            if (crackVisuals[i] != null && noneLeakSprite != null)
            {
                crackVisuals[i].sprite = noneLeakSprite;
                crackVisuals[i].enabled = false;
            }

            cracks.Add(new DamCrack(i, crackPositions[i], crackVisuals[i], leakAnimators[i], noneLeakSprite, hitsToRepair));
            if (debugLogs)
                Debug.Log($"[DamManager] Crack {i + 1} initialized ({hitsToRepair} charges requises)");
        }

        if (crackAppearanceTime.Length > 0)
            timerToNextCrack = crackAppearanceTime[0];

        if (crackBarUI != null)
            crackBarUI.UpdateBar(0, MAX_CRACKS);

        if (debugLogs)
            Debug.Log("[DamManager] Initialized with 4 empty crack positions");
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (debugLogs && Mathf.FloorToInt(elapsedTime) > Mathf.FloorToInt(elapsedTime - Time.deltaTime))
            Debug.Log($"[DamManager] Prochaine fissure dans: {timerToNextCrack:F1}s - Actives: {activeCrackCount}/{MAX_CRACKS}");

        UpdateCrackProgression();
        CheckRepairZone();
    }

    private void UpdateCrackProgression()
    {
        if (nextCrackIndex >= MAX_CRACKS) return;
        if (activeCrackCount >= MAX_CRACKS) return;

        timerToNextCrack -= Time.deltaTime;
        if (timerToNextCrack <= 0f)
        {
            CreateNextCrack();
            if (nextCrackIndex < MAX_CRACKS)
                timerToNextCrack = crackAppearanceTime[Mathf.Min(nextCrackIndex, crackAppearanceTime.Length - 1)];
        }
    }

    private void CreateNextCrack()
    {
        int slotToActivate = -1;
        for (int i = nextCrackIndex; i < MAX_CRACKS; i++)
        {
            if (!cracks[i].IsActive)
            {
                slotToActivate = i;
                break;
            }
        }

        if (slotToActivate < 0) return;

        cracks[slotToActivate].Appear();
        activeCrackCount++;
        nextCrackIndex = slotToActivate + 1;

        UpdateCrackUI();

        if (debugLogs)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"[DamManager] ✓✓✓ CRACK #{slotToActivate + 1} APPEARED ✓✓✓");
            Debug.Log($"[DamManager] Actives: {activeCrackCount}/{MAX_CRACKS}");
            Debug.Log("═══════════════════════════════════════");
        }
    }

    /// <summary>
    /// Appelé par MudCloud quand il touche une fissure.
    /// Retire 1 charge. Retourne true si la fissure est complètement réparée.
    /// </summary>
    public bool ApplyMudCharge(int crackIndex)
    {
        if (crackIndex < 0 || crackIndex >= MAX_CRACKS) return false;
        if (!cracks[crackIndex].IsActive)
        {
            if (debugLogs)
                Debug.Log($"[DamManager] Fissure {crackIndex} déjà inactive, nuage ignoré.");
            return false;
        }

        bool fullyRepaired = cracks[crackIndex].ApplyCharge();

        if (debugLogs)
            Debug.Log($"[DamManager] 🟤 Charge appliquée sur fissure {crackIndex} — charges restantes : {cracks[crackIndex].RemainingCharges}/{hitsToRepair}");

        if (fullyRepaired)
        {
            ApplyRepair(crackIndex);
            if (debugLogs)
                Debug.Log($"[DamManager] ✓ Fissure {crackIndex} réparée par boue ! Actives : {activeCrackCount}/{MAX_CRACKS}");
        }

        return fullyRepaired;
    }

    /// <summary>
    /// Réparation par branche (détection via OverlapBox sur le collider du barrage)
    /// </summary>
    private void CheckRepairZone()
    {
        if (damCollider == null || activeCrackCount == 0) return;
        if (Time.time - lastRepairTime < repairCooldown) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(damCollider.bounds.center, damCollider.bounds.size, 0f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            EquippableItem item = collider.GetComponent<EquippableItem>();
            if (item != null && IsBranch(item))
            {
                BranchRepairItem branchRepairItem = collider.GetComponent<BranchRepairItem>();
                if (branchRepairItem != null && branchRepairItem.isActiveAndEnabled)
                    continue;

                if (debugLogs)
                    Debug.Log("[DamManager] Branch detected! Repairing...");
                RepairDam(item);
                return;
            }
        }
    }

    private void ApplyRepair(int crackIndex)
    {
        cracks[crackIndex].Disappear();
        activeCrackCount--;

        if (crackIndex < nextCrackIndex)
            nextCrackIndex = crackIndex;

        timerToNextCrack = crackAppearanceTime[Mathf.Min(activeCrackCount, crackAppearanceTime.Length - 1)];
        lastRepairTime = Time.time;
        UpdateCrackUI();
    }

    private bool IsBranch(EquippableItem item)
    {
        string nameLower = item.gameObject.name.ToLower();
        return nameLower.Contains(branchNameFilter.ToLower()) || nameLower.Contains("baton");
    }

    private void RepairDam(EquippableItem branchItem)
    {
        if (activeCrackCount == 0) return;

        int idx = GetNearestActiveCrackIndex(branchItem.transform.position);
        if (idx < 0) return;

        ApplyRepair(idx);

        branchItem.Drop();
        branchItem.gameObject.SetActive(false);

        if (debugLogs)
            Debug.Log($"[DamManager] Branch used! Actives: {activeCrackCount}/{MAX_CRACKS}");
    }

    public void RepairDamWithBranch(BranchRepairItem branchItem)
    {
        if (activeCrackCount == 0) return;

        int idx = GetNearestActiveCrackIndex(branchItem.transform.position);
        if (idx < 0) return;

        ApplyRepair(idx);

        EquippableItem equippable = branchItem.GetComponent<EquippableItem>();
        if (equippable != null) equippable.Drop();
        branchItem.gameObject.SetActive(false);

        if (debugLogs)
            Debug.Log($"[DamManager] Dam repaired with branch! Actives: {activeCrackCount}/{MAX_CRACKS}");
    }

    public void RepairCrackAtIndex(int index, BranchRepairItem branchItem)
    {
        if (index < 0 || index >= MAX_CRACKS || !cracks[index].IsActive) return;

        ApplyRepair(index);

        EquippableItem equippable = branchItem.GetComponent<EquippableItem>();
        if (equippable != null) equippable.Drop();
        branchItem.gameObject.SetActive(false);

        if (debugLogs)
            Debug.Log($"[DamManager] ✓ Fissure {index} réparée avec branche ! Actives: {activeCrackCount}/{MAX_CRACKS}");
    }

    private void UpdateCrackUI()
    {
        if (crackBarUI != null)
            crackBarUI.UpdateBar(activeCrackCount, MAX_CRACKS);
    }

    public int GetCurrentCrackCount() => activeCrackCount;
    public float GetElapsedTime() => elapsedTime;

    /// <summary>
    /// Retourne la position monde d'une fissure par son index.
    /// Utilisé par MudCloud pour calculer la distance jusqu'à la fissure ciblée.
    /// </summary>
    public Vector2 GetCrackPosition(int index)
    {
        if (index < 0 || index >= MAX_CRACKS || crackPositions[index] == null)
            return transform.position; // fallback : position du barrage lui-même

        return crackPositions[index].position;
    }

    public int GetNearestActiveCrackIndex(Vector2 position)
    {
        int nearest = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < MAX_CRACKS; i++)
        {
            if (!cracks[i].IsActive) continue;
            if (crackPositions[i] == null) continue;

            float dist = Vector2.Distance(position, crackPositions[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }

        return nearest;
    }

    // ── Classe interne ───────────────────────────────────────────────────
    private class DamCrack
    {
        private int index;
        private Transform position;
        private SpriteRenderer visual;
        private LeakAnimator leakAnimator;
        private Sprite noneSprite;
        private bool isActive = false;
        private int remainingCharges;
        private int maxCharges;

        public bool IsActive => isActive;
        public int RemainingCharges => remainingCharges;

        public DamCrack(int index, Transform position, SpriteRenderer visual, LeakAnimator animator, Sprite noneSprite, int maxCharges)
        {
            this.index      = index;
            this.position   = position;
            this.visual     = visual;
            this.leakAnimator = animator;
            this.noneSprite = noneSprite;
            this.maxCharges = maxCharges;
            this.remainingCharges = maxCharges;
        }

        /// <summary>
        /// Applique une charge de boue. Retourne true si la fissure est réparée.
        /// </summary>
        public bool ApplyCharge()
        {
            remainingCharges--;
            return remainingCharges <= 0;
        }

        public void Appear()
        {
            remainingCharges = maxCharges; // Reset les charges à chaque apparition

            if (visual != null)
            {
                Vector3 pos = visual.transform.position;
                pos.x = -16.5f;
                visual.transform.position = pos;
                if (noneSprite != null) visual.sprite = noneSprite;
                visual.enabled = true;
                Debug.Log($"[DamCrack {index}] Visual enabled ({maxCharges} charges)");
            }
            else Debug.LogWarning($"[DamCrack {index}] No SpriteRenderer!");

            if (leakAnimator != null)
            {
                leakAnimator.StartLeaking();
                Debug.Log($"[DamCrack {index}] LeakAnimator started");
            }
            else Debug.LogWarning($"[DamCrack {index}] No LeakAnimator!");

            isActive = true;
        }

        public void Disappear()
        {
            if (visual != null)
            {
                Vector3 pos = visual.transform.position;
                pos.x = -17f;
                visual.transform.position = pos;
                if (noneSprite != null) visual.sprite = noneSprite;
                visual.enabled = false;
                Debug.Log($"[DamCrack {index}] Visual disabled");
            }

            if (leakAnimator != null)
            {
                leakAnimator.StopLeaking();
                Debug.Log($"[DamCrack {index}] LeakAnimator stopped");
            }

            isActive = false;
            remainingCharges = maxCharges;
        }
    }
}

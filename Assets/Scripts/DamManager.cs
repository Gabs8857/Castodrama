using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère le système du barrage avec ses fissures progressives.
/// Réparable avec des branches (via BranchRepairItem) OU avec de la boue (MudSystem).
/// La boue répare au contact du BoxCollider2D de la fissure.
/// </summary>
public class DamManager : MonoBehaviour
{
    [Header("Dam Crack Positions")]
    [SerializeField] private Transform[] crackPositions = new Transform[4];

    [Header("Branch Detection")]
    [Tooltip("Le nom (ou partie du nom) que l'objet doit avoir pour être accepté")]
    [SerializeField] private string branchNameFilter = "branch";

    [Header("Crack Settings")]
    [SerializeField] private float[] crackAppearanceTime = { 10f, 15f, 20f, 25f };
    [SerializeField] private float repairCooldown = 2f;

    [Header("Mud Repair")]
    [Tooltip("BoxCollider2D de chaque fissure (dans l'ordre des crackPositions)")]
    [SerializeField] private Collider2D[] crackColliders = new Collider2D[4];

    [Header("Visual")]
    [SerializeField] private Sprite crackSprite;
    [SerializeField] private Sprite noneLeakSprite;
    [SerializeField] private SpriteRenderer[] crackVisuals = new SpriteRenderer[4];
    [SerializeField] private LeakAnimator[] leakAnimators = new LeakAnimator[4];

    [Header("UI")]
    [Tooltip("Référence au CrackBarUI pour mettre à jour la barre de fissures")]
    [SerializeField] private CrackBarUI crackBarUI;

    [Header("Time Reference")]
    [SerializeField] private DayAndNightCycle dayNightCycle;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private List<DamCrack> cracks = new List<DamCrack>();
    private float elapsedTime = 0f;
    private float timerToNextCrack = 0f;
    private int cracksCreated = 0;
    private float lastRepairTime = -10f;
    private const int MAX_CRACKS = 4;
    private Collider2D damCollider;
    private MudSystem mudSystem;

    private void Start()
    {
        Debug.Log("[DamManager] Starting initialization... Barrage");

        damCollider = GetComponent<Collider2D>();
        if (damCollider == null && debugLogs)
            Debug.LogWarning("[DamManager] No Collider2D found on Barrage!");

        // Trouve le MudSystem sur le joueur
        GameObject player = GameObject.Find("Castor") ?? GameObject.Find("Player");
        if (player != null)
            mudSystem = player.GetComponent<MudSystem>();

        if (mudSystem == null)
            Debug.LogWarning("[DamManager] ✗ MudSystem introuvable sur le joueur !");
        else
            Debug.Log("[DamManager] ✓ MudSystem lié avec succès.");

        // Initialiser les fissures
        for (int i = 0; i < MAX_CRACKS; i++)
        {
            if (crackVisuals[i] != null && noneLeakSprite != null)
            {
                crackVisuals[i].sprite = noneLeakSprite;
                crackVisuals[i].enabled = false;
            }

            var crack = new DamCrack(i, crackPositions[i], crackVisuals[i], leakAnimators[i], noneLeakSprite);
            cracks.Add(crack);
            if (debugLogs)
                Debug.Log($"[DamManager] Crack {i + 1} initialized");
        }

        if (crackAppearanceTime.Length > 0)
            timerToNextCrack = crackAppearanceTime[0];

        // Init UI
        if (crackBarUI != null)
            crackBarUI.UpdateBar(0, MAX_CRACKS);

        if (debugLogs)
            Debug.Log("[DamManager] Initialized with 4 empty crack positions");
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (debugLogs && Mathf.FloorToInt(elapsedTime) > Mathf.FloorToInt(elapsedTime - Time.deltaTime))
            Debug.Log($"[DamManager] Prochaine fissure dans: {timerToNextCrack:F1}s - État: {cracksCreated}/{MAX_CRACKS}");

        UpdateCrackProgression();
        CheckRepairZone();
        CheckMudRepair();
    }

    private void UpdateCrackProgression()
    {
        if (cracksCreated < MAX_CRACKS)
        {
            timerToNextCrack -= Time.deltaTime;
            if (timerToNextCrack <= 0)
            {
                CreateNewCrack();
                if (cracksCreated < MAX_CRACKS)
                    timerToNextCrack = crackAppearanceTime[cracksCreated];
            }
        }
    }

    private void CreateNewCrack()
    {
        if (cracksCreated >= MAX_CRACKS || cracksCreated >= cracks.Count) return;

        cracks[cracksCreated].Appear();
        cracksCreated++;

        UpdateCrackUI();

        if (debugLogs)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"[DamManager] ✓✓✓ CRACK #{cracksCreated} APPEARED ✓✓✓");
            Debug.Log($"[DamManager] Total cracks now: {cracksCreated}/{MAX_CRACKS}");
            Debug.Log("═══════════════════════════════════════");
        }
    }

    /// <summary>
    /// Réparation par branche (détection via OverlapBox sur le collider du barrage)
    /// </summary>
    private void CheckRepairZone()
    {
        if (damCollider == null || cracksCreated == 0) return;
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

    /// <summary>
    /// Réparation par boue : détecte si le joueur (avec mud) touche le collider d'une fissure active
    /// </summary>
    private void CheckMudRepair()
    {
        if (mudSystem == null || !mudSystem.HasMud) return;
        if (cracksCreated == 0) return;
        if (Time.time - lastRepairTime < repairCooldown) return;

        Vector2 playerPos = mudSystem.transform.position;

        // Vérifie chaque fissure active (dans l'ordre d'apparition, de la plus récente)
        for (int i = cracksCreated - 1; i >= 0; i--)
        {
            if (!cracks[i].IsActive) continue;

            // Utilise le crackCollider assigné si disponible
            if (crackColliders != null && i < crackColliders.Length && crackColliders[i] != null)
            {
                if (crackColliders[i].OverlapPoint(playerPos))
                {
                    if (debugLogs)
                        Debug.Log($"[DamManager] 🟤 Joueur avec boue dans la zone fissure {i} → réparation !");
                    RepairWithMud(i);
                    return;
                }
            }
            else
            {
                // Fallback : distance à la crackPosition
                if (crackPositions[i] != null)
                {
                    float dist = Vector2.Distance(playerPos, crackPositions[i].position);
                    if (dist <= 1.5f)
                    {
                        if (debugLogs)
                            Debug.Log($"[DamManager] 🟤 Joueur avec boue proche fissure {i} (dist={dist:F2}) → réparation !");
                        RepairWithMud(i);
                        return;
                    }
                }
            }
        }
    }

    private void RepairWithMud(int crackIndex)
    {
        if (!mudSystem.UseMud()) return;

        cracks[crackIndex].Disappear();

        // Réorganise : décale les fissures actives pour combler le trou
        // (on retire la fissure réparée et on réajuste cracksCreated)
        cracksCreated--;

        // Réinitialise le timer
        if (cracksCreated < MAX_CRACKS)
            timerToNextCrack = crackAppearanceTime[Mathf.Min(cracksCreated, crackAppearanceTime.Length - 1)];

        lastRepairTime = Time.time;
        UpdateCrackUI();

        if (debugLogs)
            Debug.Log($"[DamManager] ✓ Fissure {crackIndex} réparée avec boue ! Restantes : {cracksCreated}/{MAX_CRACKS}");
    }

    private bool IsBranch(EquippableItem item)
    {
        string nameLower = item.gameObject.name.ToLower();
        string filterLower = branchNameFilter.ToLower();
        return nameLower.Contains(filterLower) || nameLower.Contains("baton");
    }

    private void RepairDam(EquippableItem branchItem)
    {
        if (cracksCreated == 0) return;
        ApplyRepairLogic();

        if (branchItem != null)
        {
            branchItem.Drop();
            branchItem.gameObject.SetActive(false);
            if (debugLogs)
                Debug.Log("[DamManager] Branch used and destroyed");
        }
    }

    public void RepairDamWithBranch(BranchRepairItem branchItem)
    {
        if (cracksCreated == 0) return;
        ApplyRepairLogic();

        EquippableItem equippableItem = branchItem.GetComponent<EquippableItem>();
        if (equippableItem != null)
        {
            equippableItem.Drop();
            if (debugLogs)
                Debug.Log("[DamManager] Branch dropped from player");
        }

        branchItem.gameObject.SetActive(false);
    }

    private void ApplyRepairLogic()
    {
        DamCrack lastCrack = cracks[cracksCreated - 1];
        lastCrack.Disappear();
        cracksCreated--;

        if (cracksCreated < MAX_CRACKS)
            timerToNextCrack = crackAppearanceTime[cracksCreated];

        lastRepairTime = Time.time;
        UpdateCrackUI();

        if (debugLogs)
            Debug.Log($"[DamManager] Dam repaired! Cracks remaining: {cracksCreated}/{MAX_CRACKS}");
    }

    private void UpdateCrackUI()
    {
        if (crackBarUI != null)
            crackBarUI.UpdateBar(cracksCreated, MAX_CRACKS);
    }

    public int GetCurrentCrackCount() => cracksCreated;
    public float GetElapsedTime() => elapsedTime;

    /// <summary>
    /// Retourne l'index de la fissure active la plus proche d'une position donnée.
    /// Retourne -1 si aucune fissure active.
    /// </summary>
    public int GetNearestActiveCrackIndex(Vector2 position)
    {
        int nearest = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < cracksCreated; i++)
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

    /// <summary>
    /// Répare une fissure spécifique par son index (utilisé par BranchRepairItem).
    /// </summary>
    public void RepairCrackAtIndex(int index, BranchRepairItem branchItem)
    {
        if (index < 0 || index >= cracksCreated) return;

        cracks[index].Disappear();
        cracksCreated--;

        if (cracksCreated < MAX_CRACKS)
            timerToNextCrack = crackAppearanceTime[Mathf.Min(cracksCreated, crackAppearanceTime.Length - 1)];

        lastRepairTime = Time.time;
        UpdateCrackUI();

        if (debugLogs)
            Debug.Log($"[DamManager] ✓ Fissure {index} réparée avec branche ! Restantes : {cracksCreated}/{MAX_CRACKS}");

        // Détruit la branche
        EquippableItem equippable = branchItem.GetComponent<EquippableItem>();
        if (equippable != null)
        {
            equippable.Drop();
            if (debugLogs) Debug.Log("[DamManager] Branche déposée du joueur");
        }
        branchItem.gameObject.SetActive(false);
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

        public bool IsActive => isActive;

        public DamCrack(int index, Transform position, SpriteRenderer visual, LeakAnimator animator, Sprite noneSprite)
        {
            this.index        = index;
            this.position     = position;
            this.visual       = visual;
            this.leakAnimator = animator;
            this.noneSprite   = noneSprite;
        }

        public void Appear()
        {
            if (visual != null)
            {
                Vector3 pos = visual.transform.position;
                pos.x = -16.5f;
                visual.transform.position = pos;
                if (noneSprite != null) visual.sprite = noneSprite;
                visual.enabled = true;
                Debug.Log($"[DamCrack {index}] Visual enabled");
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
        }
    }
}

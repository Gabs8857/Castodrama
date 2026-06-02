/** GUIDE DE SETUP - SYSTÈME DE BRANCHES */

=== STRUCTURE IMPORTANTE ===

Les branches pour réparer le barrage NE sont PAS des FoodItem!
- FoodTree (Poplar/Birch) → FoodItem (tu les manges)
- Branch (Poplar/Birch) → BranchItem + EquippableItem + BranchRepairItem (réparation)


=== CRÉER LES PREFABS DE BRANCHES ===

1. POPLAR BRANCH:
   - Créer GameObject vide "PoplarBranchPrefab"
   - Ajouter SpriteRenderer avec sprite branche
   - Ajouter CircleCollider2D (isTrigger: TRUE)
   - Ajouter Rigidbody2D (Body Type: Kinematic, Gravity: 0)
   - Scripts à ajouter (DANS CET ORDRE):
     ✓ EquippableItem
     ✓ BranchRepairItem
     ✓ PoplarBranch
   - Sauvegarder en prefab

2. BIRCH BRANCH:
   - Même procédure que Poplar
   - Remplacer le dernier script par BirchBranch au lieu de PoplarBranch
   - Sauvegarder en prefab


=== CONFIGURER TREEFALLMANAGER ===

1. Créer GameObject "TreeFallManager" dans la scène
2. Ajouter script TreeFallManager
3. Dans l'inspecteur:
   - Poplar Branch Prefab: draguer le prefab PoplarBranch
   - Birch Branch Prefab: draguer le prefab BirchBranch
   - Poplar Branch Count: 2 (ou ce que tu veux)
   - Birch Branch Count: 1 (ou ce que tu veux)
   - Dam Manager: draguer le DamManager de la scène
   - Debug Logs: TRUE (pour tester)


=== VÉRIFICATION ===

Lance le jeu et attends que 2 cassures apparaissent.
Console devrait afficher:
  ✓ TreeFallManager - 2ème cassure détectée
  ✓ Branches générées (2 Poplar + 1 Birch)
  ✓ PoplarBranch #1 spawned
  ✓ PoplarBranch #2 spawned
  ✓ BirchBranch #1 spawned

Si tu vois des ✗ pour BranchItem/EquippableItem/BranchRepairItem,
c'est que les prefabs ne sont pas correctement configurés!


=== CONTRÔLE DU SPAWN ===

Si rien ne spawn:
1. Vérifier que Debug Logs = TRUE dans TreeFallManager
2. Checker console pour erreurs
3. S'assurer que poplarBranchPrefab et birchBranchPrefab ne sont pas NULL
4. Vérifier que DamManager se trouve bien dans la scène

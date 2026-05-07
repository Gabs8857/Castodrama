# Notes de Nettoyage et Refactorisation

## Architecture Refactorisée

### 1. Héritage PlayerController

**Nouveau flux:**
```
PlayerController (classe de base avec mouvement)
    |
    +-- TopDownPlayerController (classe spécialisée pour le joueur)
```

**Avantages:**
- Code réutilisable
- Pas de duplication
- PlayerController peut servir pour NPJ, ennemis, etc.

---

### 2. Barres d'État Unifiées

**Ancien système:**
- TopDownHungerBarUI.cs (barre faim)
- TopDownDangerBarUI.cs (barre danger)
- HungerBar.cs (ancien doublon)

**Nouveau système:**
- StatusBarUI.cs (gère les 2 barres)

**Avantages:**
- 1 seul fichier au lieu de 3
- Plus facile à maintenir
- Cohérent et organisé

---

## Scripts À Conserver

### Systèmes de Joueur
- `PlayerController.cs` - Contrôle simple (base réutilisable)
- `TopDownPlayerController.cs` - Contrôle du joueur principal
- `CharacterAnimator.cs` - Animations du personnage
- `EquippableItem.cs` - Items ramassables
- `StatusBarUI.cs` - Barres d'état (Faim + Danger)

### Système de Zones
- `WaterZoneTrigger.cs` - Détecteur d'eau
- `IZoneDetectable.cs` - Interface de zones
- `ZoneDetectionManager.cs` - Gestionnaire de zones

### Autres Systèmes
- `DialogueManager.cs` - Système de dialogue
- `NPCInteraction.cs` - Interactions PNJ
- `TopDownHunger.cs` - Logique de faim
- `TopDownDanger.cs` - Logique de danger
- `DayAndNightCycle.cs` - Cycle jour/nuit

---

## Scripts À Supprimer

| Script | Raison | Remplacé par |
|--------|--------|--------------|
| **SwimmingAnimator.cs** | Doublon | CharacterAnimator.cs |
| **TopDownHungerBarUI.cs** | Remplacé | StatusBarUI.cs |
| **TopDownDangerBarUI.cs** | Remplacé | StatusBarUI.cs |
| **HungerBar.cs** | Ancien doublon | StatusBarUI.cs |

---

## Refactorisations Effectuées

### 1. Héritage PlayerController ✅
- TopDownPlayerController hérite maintenant de PlayerController
- Suppression de la duplication de ReadMoveInput()
- Appels à base.Awake() et base.Update()

### 2. StatusBarUI ✅
- Nouveau fichier unifié pour les barres
- Gère faim et danger
- Plus facile à configurer dans l'éditeur Unity

### 3. Animation d'Items ✅
- PickUpItem() ne force plus la nage
- DropItem() ne force plus la marche
- L'animation reste déterminée uniquement par la zone

---

## Migration

Pour migrer les scènes:
1. Remplacer TopDownHungerBarUI par StatusBarUI sur le Canvas
2. Remplacer TopDownDangerBarUI par StatusBarUI sur le Canvas
3. Supprimer les anciens scripts du projet

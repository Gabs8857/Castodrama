## Scripts Obsolètes et À Nettoyer

### 🗑️ Scripts À Supprimer

#### **SwimmingAnimator.cs** ❌ DOUBLON
- **Raison** : Remplacé par `CharacterAnimator.cs`
- **Fonction** : Gère uniquement la nage avec 2 frames
- **Alternative** : `CharacterAnimator.cs` gère marche + nage + nage profonde
- **Statut** : Plus utilisé dans le projet
- **Action** : Supprimer ce fichier

### 📋 Scripts À Évaluer

#### **FoodItem.cs** ⚠️ À Évaluer
- **Raison** : Alternative ancienne à `EquippableItem.cs`
- **Fonction** : Items de nourriture qui s'auto-détruisent et restaurent la faim
- **Alternative** : `EquippableItem.cs` permet ramassage/dépôt
- **Différence** : FoodItem = destructible automatiquement vs EquippableItem = réutilisable
- **Recommandation** : 
  - Si tu veux des items destructibles (nourriture, bonus) → Garder FoodItem
  - Si tu veux seulement des items réutilisables (branche) → Supprimer FoodItem
  - **Suggestion** : Garder FoodItem pour la variété, mais l'améliorer pour utiliser IZoneDetectable

#### **ATHController.cs** ⚠️ À Évaluer
- **Statut** : Script pour gérer l'ATH (animation/visibility)
- **Utilisation** : À vérifier dans les scènes

### ✅ Scripts Essentiels (À Conserver)

- `TopDownPlayerController.cs` - Centre névralgique
- `CharacterAnimator.cs` - Gestion des animations
- `EquippableItem.cs` - Items ramassables
- `WaterZoneTrigger.cs` - Détecteur d'eau
- `IZoneDetectable.cs` - Interface de zones
- `ZoneDetectionManager.cs` - Gestionnaire de zones
- `DialogueManager.cs` - Système de dialogue
- `NPCInteraction.cs` - Interactions PNJ
- `TopDownHunger.cs` - Système de faim
- `TopDownDanger.cs` - Système de danger
- `DayAndNightCycle.cs` - Cycle jour/nuit

---

## ✅ Corrections Effectuées

### TopDownPlayerController.PickUpItem() et DropItem()

**Problème** : L'animation était forcée lors du ramassage/dépôt
- PickUpItem() → Forçait `StartSwimming()`
- DropItem() → Forçait `StopSwimming()`

**Solution** : Conserver l'état actuel du personnage
- PickUpItem() → Log l'état actuel, ne change pas l'animation
- DropItem() → Log l'état actuel, ne change pas l'animation
- La nage/marche reste déterminée par la zone (eau/terre) uniquement

**Résultat** :
- ✅ Tu peux nager tout en tenant une branche
- ✅ Tu peux marcher tout en tenant une branche
- ✅ La branche n'interfère plus avec l'animation

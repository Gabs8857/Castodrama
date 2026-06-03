# Castodrama

Jeu 2D coopératif en Unity 6 LTS.

## Démarrage rapide

1. Ouvre le projet avec **Unity 6 LTS** (voir [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt))
2. Laisse Unity régénérer les fichiers nécessaires
3. Travaille sur une branche dédiée avant de faire une PR

## ⌨️ Contrôles

- **C** : **Casser** ou manger les arbres (Peuplier, Bouleau, Saule).
- **F** : Manger les items de nourriture (**Food**) classiques.
- **G** : Ramasser ou déposer les branches (**Grab**).
- **E** : Interaction contextuelle (Parler aux PNJs, plonger en Nage Profonde).
- **WASD / ZQSD** : Se déplacer.
- **D-Pad (Manette)** : Zoom caméra.

## Architecture générale

```
HUB CENTRAL: TopDownPlayerController
├─ Mouvement (hérité de PlayerController)
├─ Inventaire & ramassage d'items
├─ Animations (CharacterAnimator)
├─ Détection de zones (eau, lave, feu, glace)
├─ Gestion de la faim
└─ Suivi de la caméra (TopDownCameraFollow)
```

## Systèmes clés

### 🎮 Mouvement & Contrôle
- **PlayerController** : Base réutilisable (clavier + gamepad)
- **TopDownPlayerController** : Centre du jeu, gère tout ce qui touche au joueur
- **TopDownCameraFollow** : Suit le joueur avec zoom gamepad

### 🎨 Animations
- **CharacterAnimator** : Gère les sprites (marche, nage, nage profonde) avec direction dynamique
- **SpriteLibrarySwitcher** : Change l'apparence du personnage au runtime

### 📊 État du joueur
- **TopDownHunger** : Système de faim
- **TopDownDanger** : Suivi du danger environnemental (UI actuellement désactivée)
- **DayAndNightCycle** : Cycle jour/nuit avec effets visuels

### 🎒 Inventaire
- **EquippableItem** : Items interactifs (ramassage/dépôt)
- **FoodItem** : Items alimentaires qui restaurent la faim
- **BranchRepairItem** : Branches spécifiques (Peuplier/Bouleau) pour la réparation

### 🏗️ Réparation du Barrage
- **DamManager** : Gère l'intégrité du barrage et les points de rupture
- **TreeFallManager** : Gère le spawn automatique des branches de réparation lors des dégâts

### 🗺️ Zones & Environnement
- **ZoneDetectionManager** : Suivi centralisé des zones
- **WaterZoneTrigger / DangerZoneTrigger** : Détection des zones spéciales
- **RiverTeleport / RiverBottomTeleport** : Système de nage profonde à la rivière
  - Gère la transition visuelle entre le monde supérieur et le fond
  - Activation/désactivation de FondRivière, Rivière et Tilemap au passage sous l'eau
  - Utilise E pour entrer/sortir de la nage profonde

### 💬 Dialogue & NPCs
- **DialogueManager** : Gestion narrative avec Ink
- **NPCInteraction** : Détection et dialogue des PNJs

### 🌐 UI & Autres
- **StatusBarUI** : Gestionnaire unifié (Faim circulaire avec positionnement orbital, Danger désactivé)
- **AdaptiveHUDWidth** : Adaptation HUD au ratio d'écran
- **ATHController** : Animations du décor
- **ContacteWebPage** : Système de contact web (voir section "Système de Contact Web")
- **TopDownBootstrap** : Initialisation de scène

## 🎬 Système d'Animation (CharacterAnimator)

L'animation change en temps réel selon la **direction du mouvement** et l'état du joueur.

### Catégories disponibles

**Marche** (3 frames): `Walk`, `Walk_Up`, `Walk_Down`
**Nage** (2 frames): `Swim`, `Swim_Up`, `Swim_Down`
**Nage Profonde** (2 frames): `deep_swim`, `deep_swim_Up`, `deep_swim_Down`

### Logique de sélection

```
Si isSwimmingDeep → utilise catégories deep_swim
Sinon si isSwimming → utilise catégories Swim
Sinon → utilise catégories Walk

Puis selon la direction:
Si mouvement vertical → ajoute _Up ou _Down
Sinon → version standard
```

## 📁 Structure du projet

```
Assets/
├─ Scripts/           (31 scripts, structure plate)
├─ Scenes/
├─ Resources/
├─ Ink/              (Fichiers Ink pour dialogues)
└─ Settings/

ProjectSettings/     (Config Unity)
Packages/           (Dépendances)
```

## ⚙️ Configuration

- **Input System** : New InputSystem activé
- **Render Pipeline** : Universal Render Pipeline
- **Version Unity** : 6 LTS

## 🎯 River Deep Swim System

Le système de nage profonde permet au joueur de :
1. Presser **E** à la surface du fleuve pour descendre en nage profonde
2. Voir la forêt, la rive et les arbres disparaître
3. Voir la nage profonde s'activer automatiquement
4. Presser **E** en profondeur pour remonter à la surface

**Éléments contrôlés:**
- FondRivière (apparaît en profondeur)
- Tilemap update (forêt, disparaît)
- Bouleau, Peuplier, Saule (arbres, disparaissent)
- Animation deep swim (activée automatiquement)

## 🪵 Système de Réparation du Barrage

Le maintien du barrage est vital et utilise une logique de ressources distincte :
1. **Différenciation** : Les branches de réparation ne sont PAS de la nourriture (`FoodItem`).
2. **Types de bois** : Supporte le Peuplier (*Poplar*) et le Bouleau (*Birch*).
3. **Composants requis** : Une branche de réparation doit posséder les scripts `EquippableItem`, `BranchRepairItem` et son script de type spécifique.
4. **Flux** : `DamManager` détecte une cassure -> `TreeFallManager` génère les branches nécessaires dans la scène.

## Notes de développement

- Système de zones extensible avec interfaces `IZoneDetectable`
- Bootstrap crée automatiquement joueur et UI au runtime
- Système de transition d'eau (mai 2026) gère l'activation/désactivation des éléments visuels
# Castodrama

Jeu 2D coopératif en Unity 6 LTS.

## Démarrage rapide

1. Ouvre le projet avec **Unity 6 LTS** (voir [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt))
2. Laisse Unity régénérer les fichiers nécessaires
3. Travaille sur une branche dédiée avant de faire une PR

## Collaboration

- ✅ Crée une issue avant de commencer une fonctionnalité importante
- ✅ Garde les commits petits et descriptifs
- ✅ Ouvre une PR pour toute modification partagée


## Architecture générale

```
HUB CENTRAL: TopDownPlayerController
├─ Mouvement (hérité de PlayerController)
├─ Inventaire & ramassage d'items
├─ Animations (CharacterAnimator)
├─ Détection de zones (eau, lave, feu, glace)
├─ Gestion de la faim
├─ Gestion du danger
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
- **TopDownDanger** : Suivi du danger environnemental
- **DayAndNightCycle** : Cycle jour/nuit avec effets visuels

### 🎒 Inventaire
- **EquippableItem** : Items interactifs (ramassage/dépôt)
- **FoodItem** : Items alimentaires qui restaurent la faim

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
- **StatusBarUI** : Barres de faim et danger
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

## 🌐 Système de Contact Web

**ContacteWebPage** : Système de contact intégré au jeu
- Crée un panneau UI bas-glissant au runtime avec InputField + Button
- Envoie les messages via GET requête avec le paramètre `message`
- Utilise `UnityWebRequest.result` pour les vérifications d'erreur (pas `isNetworkError`)
- Attention : Les URLs doivent inclure le schéma complet (ex: `https://domain/path`)

## Notes de développement

- Système de zones extensible avec interfaces `IZoneDetectable`
- Dialogues Ink compilés en JSON
- Animations gérées par Sprite Resolver
- Bootstrap crée automatiquement joueur et UI au runtime
- Système de transition d'eau (mai 2026) gère l'activation/désactivation des éléments visuels
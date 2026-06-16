# Castodrama

Jeu 2D coopératif en Unity 6 LTS où le joueur incarne un castor streameur qui doit maintenir son barrage tout en sensibilisant sa communauté à l'environnement.

## Démarrage rapide

1. Ouvre le projet avec **Unity 6 LTS** (voir [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt))
2. Laisse Unity régénérer les fichiers nécessaires
3. Travaille sur une branche dédiée avant de faire une PR
4. **F9** en jeu ou dans le MainMenu pour activer le mode debug

## ⌨️ Contrôles

- **WASD / ZQSD** : Se déplacer
- **C** : Couper les arbres (Peuplier, Bouleau, Saule)
- **F** : Manger les items de nourriture (herbes)
- **G** : Ramasser / déposer une branche — ramasse aussi de la **boue** en nage profonde
- **E** : Interaction contextuelle (Parler aux PNJs, plonger / remonter en nage profonde)
- **Echap** : Ouvrir/Fermer le menu pause
- **D-Pad (Manette)** : Zoom caméra

## Architecture générale

```
HUB CENTRAL: TopDownPlayerController
├─ Mouvement (hérité de PlayerController)
├─ Inventaire & ramassage d'items
├─ Animations (CharacterAnimator)
├─ Détection de zones (ZoneDetectionManager)
├─ Gestion de la faim (TopDownHunger)
├─ Gestion du danger (TopDownDanger)
└─ Suivi de la caméra (TopDownCameraFollow)
```

## Systèmes clés

### 🎮 Mouvement & Contrôle
- **PlayerController** : Base réutilisable (clavier + gamepad)
- **TopDownPlayerController** : Centre du jeu, gère tout ce qui touche au joueur
- **TopDownCameraFollow** : Suit le joueur avec zoom gamepad
- **CarAnimator / CarCollisionHandler** : Voitures animées qui téléportent le joueur au spawn au contact
- **TeleportTrigger** : Téléportation générique entre zones avec cooldown

### 🎨 Animations
- **CharacterAnimator** : Gère les sprites (marche, nage, nage profonde) avec direction dynamique
- **SpriteLibrarySwitcher** : Change l'apparence du personnage au runtime
- **LeakAnimator** : Anime les fissures du barrage (cycle de frames via SpriteResolver)

### 📊 État du joueur
- **TopDownHunger** : Système de faim — se vide progressivement, téléporte le joueur au spawn et mange des herbes proches si elle atteint 0. Fin de partie après 3 famines.
- **TopDownDanger** : Suivi du niveau de danger environnemental
- **DayAndNightCycle** : Gère la progression du temps par jour
  - Réduit le champ de vision (Light2D) au fil du temps
  - Déclenche un flash et un message de fin de nuit
  - Démarre sur appel de `ResumeTimer()` par `DayManager`

### 📅 Progression des jours
- **DayManager** : Orchestre la progression des jours (3 jours max)
  - Téléporte le joueur à la hutte en fin de journée
  - Avance au jour suivant après le bilan NPC
  - Déclenche la scène Crédits au jour 3
- **GameState** : État global statique partagé par tous les scripts
  - Mode de jeu (Free / Dialogue / Question / Result)
  - Données quiz par jour, viewers, signatures
  - Références globales (ChatManager, StatsUIManager, GrassSpawner, DayManager)
- **GrassSpawner** : Réinitialise toutes les herbes (FoodItem) au début de chaque nouveau jour

### 🎒 Inventaire & Items
- **EquippableItem** : Items interactifs (ramassage avec G, dépôt, réactions aux zones)
- **FoodItem** : Herbes/nourriture qui restaurent la faim au contact
- **FoodTreePrefab** : Setup automatique des arbres nourriture (collider, rigidbody, FoodItem)
- **BranchItem** : Classe de base des branches (type Poplar ou Birch)
- **BirchBranch / PoplarBranch** : Types de branches spécifiques
- **BranchRepairItem** : Répare la fissure du barrage **la plus proche** de l'endroit du dépôt
- **IBranchSpawner** : Interface pour les arbres qui spawnent des branches

### 🏗️ Réparation du Barrage
- **DamManager** : Gère les fissures progressives du barrage
  - 4 fissures max, apparaissent selon des timers configurables
  - Réparable par **branche** (dépôt sur la fissure) ou par **boue** (contact avec le collider de fissure)
  - Expose `GetNearestActiveCrackIndex()` et `RepairCrackAtIndex()` pour BranchRepairItem
- **TreeFallManager** : Spawne automatiquement des branches (Poplar + Birch) quand la 2e fissure apparaît
- **CrackBarUI** : Barre circulaire bleue affichant le nombre de fissures actives (0 = vide, 4/4 = rouge critique)

### 🟤 Système de Boue
- **MudSystem** : Ramassage de boue en nage profonde (G). Max 1 boue à la fois.
- **MudUI** : Affiche l'icône de boue dans le HUD

### 🌊 Nage Profonde & Transitions
- **RiverTeleport** : Détecte le joueur dans la rivière, déclenche la nage profonde au E
- **RiverBottomTeleport** : Détecte le joueur au fond, remonte à la surface au E
- **WaterSceneTransition** : Gère les transitions visuelles lors des téléportations (TP_EnHutte, zones eau)
- **InvisibleWallsManager** : Toggle les murs invisibles du fond de rivière selon l'état de plongée
- **WaterZoneTrigger** : Notifie le joueur qu'il entre/sort de l'eau (active la nage)
- **EntRiviereZoneTrigger** : Zone de transition rivière (animations Dive/DiveExit)

**Éléments contrôlés lors de la plongée :**
- FondRivière (apparaît en profondeur)
- Tilemap update + arbres Bouleau, Peuplier, Saule (disparaissent)
- Animation deep_swim (activée automatiquement)

### 💬 Dialogue, Quiz & Stream
- **NPCInteraction** : Dialogue avec les PNJs via Ink (E pour interagir, dialogue différent par jour)
- **InkChatManager** : Lit un fichier Ink et envoie les messages tagués `CHAT` dans le chat stream
- **ChatManager** : Affiche les messages du chat stream (scroll automatique)
- **StatsUIManager** : Affiche viewers et signatures en temps réel depuis GameState
- **QuizDataSender** : Envoie les résultats du quiz à l'API distante (`beaverse.alwaysdata.net`)
- **ContacteWebPage** : Formulaire de contact glissant depuis le bas (envoi GET vers l'endpoint configuré)

### 🌐 UI
- **HungerBarUI** : Barre circulaire verte→rouge selon la faim (Radial360, sprite ring généré en code)
- **DangerBarUI** : Barre circulaire jaune→rouge selon le danger (toujours pleine, couleur variable)
- **CrackBarUI** : Barre circulaire bleue pour les fissures du barrage
- **MudUI** : Icône de boue (visible / cachée)
- **AdaptiveHUDWidth** : Positionne les éléments HUD selon le ratio d'écran (ancre gauche/droite)
- **InteractPromptUI** : Affiche un prompt quand le joueur est proche d'un interactable
- **TopDownBootstrap** : Crée automatiquement le joueur, la caméra, le sol et les barres UI au runtime

### 🗺️ Zones & Environnement
- **ZoneDetectionManager** : Suivi centralisé des zones actives (Water, EntRiviere, Lava, Fire, Ice)
- **IZoneDetectable** : Interface implémentée par TopDownPlayerController, CharacterAnimator, EquippableItem
- **DangerZoneTrigger** : Zone de danger environnemental
- **GrassSpawner** : Gère la repousse des herbes entre les jours

## 🎬 Système d'Animation (CharacterAnimator)

```
Si isSwimmingDeep → deep_swim / deep_swim_Up / deep_swim_Down
Sinon si isSwimming → Swim / Swim_Up / Swim_Down
Sinon → Walk / Walk_Up / Walk_Down
```

## 📁 Structure du projet

```
Assets/
├─ Scripts/           (structure plate, ~35 scripts)
├─ Scenes/
│   ├─ Rivière        (scène principale)
│   └─ Credits
├─ ATH/              (sprites UI : Foodcircle.png, mud.png...)
├─ Ink/              (fichiers Ink pour dialogues et chat)
└─ Settings/

ProjectSettings/
Packages/
```

## ⚙️ Configuration

- **Input System** : New Input System
- **Render Pipeline** : Universal Render Pipeline (URP)
- **Version Unity** : 6 LTS

## Notes de développement

- `TopDownBootstrap` s'exécute via `[RuntimeInitializeOnLoadMethod]` — pas besoin de GameObject dédié
- Le tag `"Player"` doit être assigné au Castor pour que `NPCInteraction` et `InteractPromptUI` fonctionnent
- `InvisibleWallsManager.OnEnterRiverZone()` / `OnExitRiverZone()` doivent être appelés manuellement depuis `RiverTeleport` / `RiverBottomTeleport` (non encore connectés)
- `UnderwaterCrackManager` supprimé (juin 2026) — toute la logique de réparation est centralisée dans `DamManager`
- `Danger.cs` et `DangerUI.cs` supprimés (juin 2026) — remplacés par `TopDownDanger` + `DangerBarUI`
- `StatusBarUI.cs` supprimé (juin 2026) — remplacé par `HungerBarUI` et `DangerBarUI` séparés

# Castodrama

Base Unity du projet collaboratif Castodrama.

## Démarrage

1. Ouvre le projet avec la version de Unity indiquée dans [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
2. Laisse Unity régénérer les fichiers nécessaires si besoin.
3. Travaille sur une branche dédiée avant d'ouvrir une pull request.

## Collaboration

- Crée une issue avant de commencer une fonctionnalité importante.
- Garde les commits petits et descriptifs.
- Ouvre une pull request pour toute modification partagée.

## Structure

- [Assets/](Assets/)
- [Packages/](Packages/)
- [ProjectSettings/](ProjectSettings/)

## Architecture des Scripts

### �️ Architecture Générale

```
┌─────────────────────────────────────────────────────────┐
│                      JOUEUR                              │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  PlayerController (base)                                │
│    • Mouvement simple (clavier + gamepad)               │
│    • Configuration Rigidbody2D                          │
│    • À la base de tout                                  │
│                   ▲                                      │
│                   │ hérité par                           │
│                   │                                      │
│  TopDownPlayerController (spécialisé)                   │
│    • Hérite du mouvement de PlayerController            │
│    • Inventaire (ramassage/dépôt d'items)              │
│    • Détection centralisée de zones (eau, etc.)        │
│    • Gestion des animations selon la direction         │
│    • Centre névralgique du jeu                         │
│                   ▼                                      │
│    ┌─────────────┬──────────────┬─────────────┐         │
│    │             │              │             │         │
│    ▼             ▼              ▼             ▼         │
│ Zones      Animations       Inventaire       ...        │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

### 🎮 Système de Détection de Zones

L'architecture utilise un système d'interfaces pour gérer la détection de zones (eau, lave, feu, glace, etc.). Cela permet une gestion centralisée et extensible des interactions environnementales.

#### 📊 Diagramme Détaillé des Zones

```
WaterZoneTrigger (détecte collision)
        │
        │ collision détectée
        ▼
TopDownPlayerController.OnEnterZone()
        │
        ├─ Enregistre dans ZoneDetectionManager
        │
        └─ Notifie les composants:
           ├─ CharacterAnimator (nage/marche)
           ├─ EquippableItem (réagit aux zones)
           └─ Autres composants IZoneDetectable
```

#### 🔄 Flux de Détection

1. **WaterZoneTrigger** détecte une collision (`OnTriggerEnter2D`)
2. Il appelle `TopDownPlayerController.OnEnterZone(ZoneType.Water)`
3. Le PlayerController enregistre la zone dans son **ZoneDetectionManager**
4. Le PlayerController notifie tous ses autres composants (`CharacterAnimator`, `EquippableItem`)
5. Chaque composant réagit selon ses besoins

---

### 📊 Systèmes d'Interface Utilisateur

#### Barres d'État Unifiées (StatusBarUI)

```
StatusBarUI (unique script pour 2 barres)
    │
    ├─ Connexion: TopDownHunger
    │   └─ Affiche barre de faim circulaire (haut centre)
    │
    └─ Connexion: TopDownDanger
        └─ Affiche barre de danger circulaire (bas gauche)
```

**Avantages:**
- ✅ Un seul script au lieu de 3
- ✅ Gestion unifiée des 2 barres
- ✅ Plus facile à maintenir
- ✅ Configuration cohérente

---

---

## 📚 Inventaire Complet des Scripts (31 fichiers)

Tous les scripts sont localisés dans le dossier `Assets/Scripts/` (structure plate, pas de sous-dossiers).

### 🎮 Système de Joueur & Mouvement (3 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `PlayerController.cs` | `PlayerController` | Base réutilisable avec mouvement simple (clavier + gamepad), configuration Rigidbody2D - À la base de tout |
| `TopDownPlayerController.cs` | `TopDownPlayerController` | Contrôle du joueur principal (hérite de PlayerController). Gère l'inventaire, détection de zones, animations selon la direction. **Hub central du jeu** |
| `TopDownCameraFollow.cs` | `TopDownCameraFollow` | Système de caméra qui suit le joueur en douceur avec contrôle du zoom au gamepad et modes d'affichage multiples |

### 🎨 Animations & Visuels (4 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `CharacterAnimator.cs` | `CharacterAnimator` | Gère les animations du personnage (marche 3 frames, nage 2 frames, nage profonde) avec commutation dynamique |
| `SpriteLibrarySwitcher.cs` | `SpriteLibrarySwitcher` | Bascule les assets de la sprite library au runtime pour changer l'apparence du personnage |
| `ATHController.cs` | `ATHController` | Contrôle les animations du décor ATH avec lecture unique sans scintillement |
| `AdaptiveHUDWidth.cs` | `AdaptiveHUDWidth` | Ajuste dynamiquement les éléments HUD selon le ratio d'aspect de l'écran tout en conservant les proportions |

### 📊 État du Jeu & Mécaniques (3 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `TopDownHunger.cs` | `TopDownHunger` | Système de faim du joueur avec taux de vidage et mécaniques de restauration |
| `TopDownDanger.cs` | `TopDownDanger` | Suivi du niveau de danger qui augmente dans les zones dangereuses et diminue ailleurs |
| `DayAndNightCycle.cs` | `DayAndNightCycle` | Implémente le cycle jour/nuit avec rayon de vision ajustable et effets de flash UI |

### 🎒 Inventaire & Items (2 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `EquippableItem.cs` | `EquippableItem` | Items interactifs que le joueur peut ramasser/déposer avec détection de proximité et réactions aux zones |
| `FoodItem.cs` | `FoodItem` | Items alimentaires qui restaurent la faim au ramassage avec teinte visuelle et montant customizable |

### 🗺️ Systèmes de Zones & Triggers (4 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `IZoneDetectable.cs` | `IZoneDetectable` / `ZoneType` | Interface et enum pour le système de détection de zones. Permet aux objets de réagir aux changements de zones (Eau, Lave, Feu, Glace) |
| `ZoneDetectionManager.cs` | `ZoneDetectionManager` | Système centralisé de suivi des zones qui notifie les objets zone-aware lors de l'entrée/sortie |
| `DangerZoneTrigger.cs` | `DangerZoneTrigger` | Trigger qui augmente le niveau de danger à l'entrée, le diminue à la sortie |
| `WaterZoneTrigger.cs` | `WaterZoneTrigger` | Trigger qui détecte quand le joueur entre/sort des zones d'eau |

### 🌊 Téléportation & Navigation (3 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `TeleportTrigger.cs` | `TeleportTrigger` | Système de téléportation générique avec cooldown, rotation de destination et logging de débogage |
| `RiverTeleport.cs` | `RiverTeleport` | Permet au joueur de se téléporter de la surface à la rivière profonde en pressant 'E' |
| `RiverBottomTeleport.cs` | `RiverBottomTeleport` | Gère la téléportation de la rivière profonde vers la surface et active l'animation de nage profonde |

### 🖼️ Interface Utilisateur (3 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `StatusBarUI.cs` | `StatusBarUI` | Gestionnaire unifié pour les 2 barres circulaires (faim + danger) avec positionnement adaptatif |
| `DangerUI.cs` | `DangerUI` | Met à jour le remplissage de la barre de danger selon le niveau de danger du joueur |
| `Danger.cs` | `Danger` | Système de danger hérité (probablement remplacé par TopDownDanger) avec détection de zones forestières |

### 💬 Dialogue & Interaction (2 scripts)

| Script | Classe | Utilité |
|--------|--------|---------|
| `DialogueManager.cs` | `DialogueManager` | Gère le système narratif Ink avec progression de l'histoire et interface de dialogue UI |
| `NPCInteraction.cs` | `NPCInteraction` | Détecte la proximité du joueur aux PNJs et déclenche des dialogues multi-parties avec touche 'E' |

### 🌐 Systèmes Externes (1 script)

| Script | Classe | Utilité |
|--------|--------|---------|
| `ContacteWebPage.cs` | `ContacteWebPage` | Envoie les messages de formulaire à un endpoint web avec animation UI glissante |

### 🚀 Initialisation & Bootstrap (1 script)

| Script | Classe | Utilité |
|--------|--------|---------|
| `TopDownBootstrap.cs` | `TopDownBootstrap` | Système d'initialisation de scène qui crée/configure le joueur et les éléments UI au runtime |

---

## 🏗️ Architecture et Hiérarchie

### Hiérarchie d'Héritage

```
MonoBehaviour
├─ PlayerController (base réutilisable)
│  └─ TopDownPlayerController (centre névralgique) ← HUB CENTRAL
│     ├─ CharacterAnimator
│     ├─ TopDownHunger
│     ├─ TopDownDanger
│     ├─ ZoneDetectionManager
│     └─ ...autres composants

├─ TopDownCameraFollow
├─ DayAndNightCycle
├─ DialogueManager
├─ TopDownBootstrap
└─ ...autres systèmes autonomes
```

### Pattern de Communication

```
Triggers (eau, danger, etc.)
         │
         │ détectent collisions
         ▼
TopDownPlayerController.OnEnterZone()
         │
         ├─ Enregistre dans ZoneDetectionManager
         │
         └─ Notifie les composants:
            ├─ CharacterAnimator (change animations)
            ├─ EquippableItem (réactions aux zones)
            ├─ TopDownDanger (ajuste danger)
            └─ Autres composants IZoneDetectable
```

### Patterns de Conception Utilisés

- ✅ **Héritage** : PlayerController → TopDownPlayerController (DRY)
- ✅ **Interface Pattern** : `IZoneDetectable` pour extensibilité
- ✅ **Manager Pattern** : `ZoneDetectionManager` centralisé
- ✅ **Bootstrap Pattern** : `TopDownBootstrap` pour initialisation
- ✅ **Component Pattern** : Architecture basée sur les composants Unity
- ✅ **Observer Pattern** : Systèmes notifient les observateurs des changements

---

### 📝 Exemple d'Utilisation

```csharp
// Accéder au gestionnaire de zones
TopDownPlayerController playerController = GetComponent<TopDownPlayerController>();

// Vérifier si le joueur est dans l'eau
if (playerController.ZoneManager.IsInZone(ZoneType.Water))
{
    // Faire quelque chose dans l'eau
}

// Récupérer toutes les zones actuelles
var currentZones = playerController.ZoneManager.GetCurrentZones();
foreach (var zone in currentZones)
{
    Debug.Log($"Le joueur est dans la zone: {zone}");
}

// Accéder aux systèmes connexes
var hunger = playerController.GetComponent<TopDownHunger>();
var danger = playerController.GetComponent<TopDownDanger>();
```

---

### ✨ Principes d'Architecture

- **Centralisation** : `TopDownPlayerController` est le hub central
- **Interfaces** : `IZoneDetectable` permet une extensibilité facile
- **Découplage** : Les scripts ne dépendent pas directement les uns des autres
- **Réutilisabilité** : `PlayerController` peut servir de base pour NPJ, ennemis, etc.
- **Maintenabilité** : Code organisé, bien documenté et facile à modifier
- **Extensibilité** : Nouveaux systèmes peuvent être ajoutés sans modification des existants

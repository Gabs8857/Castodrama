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

### 📝 Scripts Principaux

#### Contrôleurs de Joueur

| Script | Rôle | Hérite de |
|--------|------|-----------|
| `PlayerController.cs` | Base réutilisable avec mouvement simple | MonoBehaviour |
| `TopDownPlayerController.cs` | Contrôle du joueur principal | PlayerController |

#### Systèmes Essentiels

| Script | Fonction |
|--------|----------|
| `CharacterAnimator.cs` | Gestion animations (marche/nage/nage profonde) |
| `EquippableItem.cs` | Items à ramasser/déposer |
| `WaterZoneTrigger.cs` | Détecteur de zone d'eau |
| `StatusBarUI.cs` | Barres d'état unifiées (faim + danger) |

#### Systèmes de Zones

| Script | Rôle |
|--------|------|
| `IZoneDetectable.cs` | Interface commune pour les objets réagissant aux zones |
| `ZoneDetectionManager.cs` | Gestionnaire utilitaire de suivi des zones |

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
```

---

### ✨ Principes d'Architecture

- **Héritage** : PlayerController → TopDownPlayerController (DRY - Don't Repeat Yourself)
- **Centralisation** : TopDownPlayerController est le hub central
- **Interfaces** : IZoneDetectable permet une extensibilité facile
- **Découplage** : Les scripts ne dépendent pas directement les uns des autres
- **Réutilisabilité** : PlayerController peut servir de base pour NPJ, ennemis, etc.
- **Maintenabilité** : Code organisé et bien documenté

#### 🔧 Interfaces et Classes Clés

- **IZoneDetectable** : Interface commune pour les objets qui réagissent aux zones
- **ZoneType** : Énumération des types de zones possibles
- **ZoneDetectionManager** : Gestionnaire utilitaire de suivi des zones
- **TopDownPlayerController** : Centre névralgique de gestion

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

### 🎮 Système de Détection de Zones

L'architecture utilise un système d'interfaces pour gérer la détection de zones (eau, lave, feu, glace, etc.). Cela permet une gestion centralisée et extensible des interactions environnementales.

#### 📊 Diagramme d'Imbrication

```
┌──────────────────────────────────────────────────────┐
│  TopDownPlayerController                             │
│  • Gère le mouvement et l'inventaire                 │
│  • Centralise la détection de zones                  │
│  • Contient: ZoneDetectionManager                    │
│  • Implémente: IZoneDetectable                       │
└──────────────────────────────────────────────────────┘
              │
    ┌─────────┼─────────┐
    │         │         │
    ▼         ▼         ▼
┌─────────────────────────────────────────────────────┐
│        CharacterAnimator                             │
│  • Gère les animations (marche/nage/nage profonde)  │
│  • Implémente: IZoneDetectable                      │
│  • Réagit aux changements de zones du joueur        │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│        EquippableItem                                │
│  • Gère les items à ramasser                        │
│  • Implémente: IZoneDetectable                      │
│  • Peut réagir aux zones (eau, lave, etc.)          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│        WaterZoneTrigger                              │
│  • Détecteur de zone d'eau                          │
│  • Notifie TopDownPlayerController                  │
│  • Modèle pour d'autres détecteurs de zones         │
└─────────────────────────────────────────────────────┘
```

#### 🔄 Flux de Détection

1. **WaterZoneTrigger** détecte une collision (`OnTriggerEnter2D`)
2. Il appelle `TopDownPlayerController.OnEnterZone(ZoneType.Water)`
3. Le PlayerController enregistre la zone dans son **ZoneDetectionManager**
4. Le PlayerController notifie tous ses autres composants (`CharacterAnimator`, `EquippableItem`)
5. Chaque composant réagit selon ses besoins

#### 📝 Exemple d'Utilisation

```csharp
// Depuis n'importe quel script
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

#### ✨ Avantages de cette Architecture

- **Centralisation** : Toute la logique de zones est gérée au même endroit
- **Découplage** : Les scripts ne dépendent pas directement les uns des autres
- **Extensibilité** : Facile d'ajouter de nouvelles zones ou comportements
- **Réutilisabilité** : N'importe quel objet peut implémenter `IZoneDetectable`
- **Maintenabilité** : Code organisé et facile à comprendre
- **Performance** : Les zones sont trackées une seule fois au niveau du joueur

#### 🔧 Interfaces et Classes Clés

- **IZoneDetectable** : Interface commune pour les objets qui réagissent aux zones
- **ZoneType** : Énumération des types de zones possibles
- **ZoneDetectionManager** : Gestionnaire utilitaire de suivi des zones
- **TopDownPlayerController** : Centre névralgique de gestion

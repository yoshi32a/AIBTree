# RPG Example Package

This package contains a complete RPG game AI implementation using the ArcBT framework.

## 📦 Contents

### Components (BehaviourTree.Samples.RPG.Components)
- **Health**: Health point management with damage/healing
- **Mana**: Mana point management with regeneration
- **Inventory**: Item storage and management system
- **InventoryItem**: Item definition and properties

### Actions (BehaviourTree.Samples.RPG.Actions)
- **AttackAction**: Generic physical attack with damage
- **AttackEnemyAction**: Targeted enemy attack
- **AttackTargetAction**: BlackBoard-based target attack
- **CastSpellAction**: Magic spell casting with mana cost
- **FleeToSafetyAction**: Escape to safe zones
- **UseItemAction**: Consume inventory items
- **RestoreSmallManaAction**: Quick mana restoration
- **InteractAction**: Object interaction
- **InitializeResourcesAction**: Setup initial resources
- **SearchForEnemyAction**: Active enemy detection
- **MoveToEnemyAction**: Movement toward enemies
- **MoveToTargetAction**: Generic target movement
- **NormalAttackAction**: Standard attack implementation

### Conditions (BehaviourTree.Samples.RPG.Conditions)
- **HealthCheckCondition**: Health threshold monitoring
- **HasManaCondition**: Mana availability check
- **HasItemCondition**: Inventory item verification
- **EnemyCheckCondition**: Enemy presence detection
- **EnemyHealthCheckCondition**: Enemy health monitoring
- **EnemyInRangeCondition**: Distance-based enemy detection
- **HasTargetCondition**: Target availability check
- **IsInitializedCondition**: Initialization state check
- **ScanForInterestCondition**: Environmental interest detection
- **CheckManaResourceCondition**: Detailed mana resource check

## 🚀 Quick Setup

1. Import this sample package via Package Manager
2. Add components to your GameObjects:
   ```csharp
   gameObject.AddComponent<Health>();
   gameObject.AddComponent<Mana>();
   gameObject.AddComponent<Inventory>();
   ```
3. Create .bt files using the provided Actions/Conditions
4. Assign BehaviourTreeRunner with your .bt file

## 🎯 Example Usage

### Basic Combat Setup
```csharp
// GameObject setup
var health = gameObject.AddComponent<Health>();
health.maxHealth = 100;

var runner = gameObject.AddComponent<BehaviourTreeRunner>();
runner.behaviourTreeFilePath = "combat_example.bt";
```

### .bt File Example
```
tree RPGCombat {
    Selector root {
        Sequence emergency_heal {
            Condition HealthCheck { min_health: 25 }
            Condition HasItem { item_type: "healing_potion", min_quantity: 1 }
            Action UseItem { item_type: "healing_potion" }
        }
        Sequence magic_attack {
            Condition HasMana { required_mana: 30 }
            Condition EnemyInRange { max_distance: 8.0 }
            Action CastSpell { spell_name: "fireball", mana_cost: 30 }
        }
        Sequence physical_attack {
            Condition EnemyInRange { max_distance: 2.0 }
            Action AttackEnemy { damage: 25 }
        }
        Action SearchForEnemy { search_radius: 10.0 }
    }
}
```

## 🔧 Customization

### Adding Custom Spells
Modify `CastSpellAction.cs`:
```csharp
switch (spellName)
{
    case "fireball":
        damage = 50; manaCost = 30; break;
    case "heal":
        // Implement healing logic
        break;
    case "lightning":
        damage = 75; manaCost = 45; break;
}
```

### Custom Item Types
Extend `UseItemAction.cs`:
```csharp
switch (itemType)
{
    case "healing_potion":
        health.Heal(50); break;
    case "mana_potion":
        mana.RestoreMana(30); break;
    case "strength_boost":
        // Implement temporary strength buff
        break;
}
```

## 📚 Integration Guide

See `Documentation/RPG_IMPLEMENTATION_GUIDE.md` for detailed implementation patterns and best practices.

## ⚠️ Dependencies

- ArcBT Core framework
- Unity Input System (for MovementController)
- Standard Unity components (Transform, Collider)

## 🎮 Test Scenes

Access test environments via:
`ArcBT → Test Environment Setup → RPG Example`

## 📄 Namespace

All classes use `ArcBT.Samples.RPG.*` namespace to avoid conflicts with your main project.
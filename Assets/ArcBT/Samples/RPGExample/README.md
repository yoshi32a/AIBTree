# RPG Example Package

This package contains a complete RPG game AI implementation using the ArcBT v1.0.0 framework with GameplayTagSystem integration and Decorator node support.

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
```bt
tree RPGCombat {
    Selector root {
        # Emergency healing with timeout
        Timeout emergency_timeout {
            time: 5.0
            Sequence emergency_heal {
                Condition HealthCheck { min_health: 25 }
                Condition HasItem { item_type: "healing_potion", min_quantity: 1 }
                Action UseItem { item_type: "healing_potion" }
            }
        }
        
        # Magic attack with retry on failure
        Retry magic_retry {
            max_retries: 2
            Sequence magic_attack {
                Condition HasMana { required_mana: 30 }
                Condition EnemyInRange { 
                    target_tag: "Character.Enemy"  # GameplayTag integration
                    max_distance: 8.0 
                }
                Action CastSpell { spell_name: "fireball", mana_cost: 30 }
            }
        }
        
        # Physical attack with result inversion for retreat logic
        Sequence physical_attack {
            Condition EnemyInRange { 
                target_tag: "Character.Enemy"
                max_distance: 2.0 
            }
            Action AttackEnemy { damage: 25 }
        }
        
        Action SearchForEnemy { 
            search_tag: "Character.Enemy"
            search_radius: 10.0 
        }
    }
}
```

## 🔧 Customization

### Adding Custom Spells
Modify `CastSpellAction.cs` with GameplayTagSystem integration:
```csharp
[BTNode("CastSpell")] // Simplified attribute (auto-detects as Action)
public class CastSpellAction : BTActionNode
{
    protected override BTNodeResult ExecuteAction()
    {
        // Use GameplayTagSystem for high-speed target search
        using var enemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");
        
        switch (spellName)
        {
            case "fireball":
                damage = 50; manaCost = 30; 
                // Apply to all enemies in range with hierarchical tag matching
                break;
            case "heal":
                // Heal allies with "Character.Player" tag
                break;
            case "lightning":
                damage = 75; manaCost = 45; break;
        }
    }
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

## 🏷️ GameplayTagSystem Integration

### Hierarchical Enemy Classification
```csharp
// Setup enemy GameObjects with hierarchical tags
gameObject.AddComponent<GameplayTagComponent>();
tagComponent.AddTag("Character.Enemy.Boss");    // Boss enemies
tagComponent.AddTag("Character.Enemy.Minion");  // Regular enemies
```

### High-Performance Enemy Detection
```csharp
// 10-100x faster than GameObject.FindGameObjectsWithTag
using var allEnemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");
using var bossEnemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy.Boss");
```

## 🎭 Advanced Decorator Patterns

### Combat Loop with Timeout and Retry
```bt
tree AdvancedCombat {
    Repeat combat_loop {
        count: -1  # Infinite loop
        stop_on_failure: false
        
        Selector combat_actions {
            # Try magic attack up to 3 times within 10 seconds
            Timeout magic_window {
                time: 10.0
                Retry magic_attempts {
                    max_retries: 3
                    retry_delay: 1.0
                    Action CastSpell { spell_name: "fireball" }
                }
            }
            
            # Physical attack as fallback
            Action AttackEnemy { damage: 25 }
        }
    }
}
```

### Adaptive Behavior with Inverter
```bt
tree AdaptiveBehavior {
    Selector strategy {
        # Only attack if NOT outnumbered
        Sequence safe_attack {
            Inverter outnumbered_check {
                Condition CompareBlackBoard {
                    condition: "enemy_count > ally_count"
                }
            }
            Action AttackEnemy { damage: 30 }
        }
        
        # Retreat if outnumbered
        Action FleeToSafety { speed_multiplier: 1.5 }
    }
}
```

## ⚠️ Dependencies

- ArcBT Core framework
- Unity Input System (for MovementController)
- Standard Unity components (Transform, Collider)

## 🎮 Test Scenes

Access test environments via:
`ArcBT → Test Environment Setup → RPG Example`

## 🧪 Testing Framework

### Comprehensive Test Coverage
- **12 RPG-specific tests** in separated Samples test assembly
- **Integration with 314 total tests** (Runtime: 302 + Samples: 12)
- **GameplayTagSystem performance tests** for RPG scenarios
- **Decorator combination tests** for complex behavior validation

### Running RPG Tests
```
Window → General → Test Runner → Samples Tests
```

### Performance Benchmarks
- **Enemy detection**: 10-100x faster with GameplayTagSystem
- **Memory efficiency**: 0-allocation search with pooled arrays
- **Combat scenarios**: Tested with 50+ simultaneous AI characters

## 📄 Namespace

All classes use `ArcBT.Samples.RPG.*` namespace to avoid conflicts with your main project.

## 🔍 Node Registration

RPG nodes are automatically registered via source generator:
```csharp
// All RPG nodes use simplified BTNode attribute
[BTNode("AttackEnemy")] // Auto-detects as Action
[BTNode("HealthCheck")] // Auto-detects as Condition

// Source generator creates RPGExample.NodeRegistration.g.cs automatically
// No manual registration required!
```
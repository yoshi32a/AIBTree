# RPG Implementation Guide

This guide explains how to use the ArcBT framework to create RPG-style AI systems.

## 🎯 Overview

The RPG example demonstrates a complete game AI implementation with:
- Health/Mana resource management
- Combat system with different attack types
- Magic system with spell casting
- Item usage and inventory management
- Enemy detection and team coordination

## 📁 Structure

```
Samples~/RPGExample/
├── Components/
│   ├── Health.cs           # Health management
│   ├── Mana.cs             # Mana/MP management
│   ├── Inventory.cs        # Item container
│   └── InventoryItem.cs    # Item definition
├── Actions/
│   ├── AttackAction.cs     # Basic physical attack
│   ├── CastSpellAction.cs  # Magic spell casting
│   ├── UseItemAction.cs    # Consume items
│   └── FleeToSafetyAction.cs # Escape behavior
└── Conditions/
    ├── HealthCheckCondition.cs    # HP threshold check
    ├── HasManaCondition.cs        # MP availability
    └── HasItemCondition.cs        # Item possession check
```

## 🎮 Component Usage

### Health Component
```csharp
var health = gameObject.AddComponent<Health>();
health.maxHealth = 100;
health.currentHealth = 75;

// In .bt file:
Condition HealthCheck { min_health: 30 }
```

### Mana Component  
```csharp
var mana = gameObject.AddComponent<Mana>();
mana.maxMana = 50;
mana.manaRegenRate = 5; // per second

// In .bt file:
Condition HasMana { required_mana: 20 }
Action CastSpell { spell_name: "fireball", mana_cost: 25 }
```

### Inventory Component
```csharp
var inventory = gameObject.AddComponent<Inventory>();
inventory.AddItem("healing_potion", 3);

// In .bt file:
Condition HasItem { item_type: "healing_potion", min_quantity: 1 }
Action UseItem { item_type: "healing_potion" }
```

## 🤖 AI Behavior Patterns

### Basic Combat AI
```
tree CombatAI {
    Selector combat_root {
        Sequence low_health_sequence {
            Condition HealthCheck { min_health: 30 }
            Condition HasItem { item_type: "healing_potion", min_quantity: 1 }
            Action UseItem { item_type: "healing_potion" }
        }
        Sequence attack_sequence {
            Condition EnemyInRange { max_distance: 3.0 }
            Action AttackEnemy { damage: 25, attack_range: 2.0 }
        }
        Action MoveToEnemy { move_speed: 5.0 }
    }
}
```

### Magic User AI
```
tree MageAI {
    Selector mage_root {
        Sequence emergency_sequence {
            Condition HealthCheck { min_health: 20 }
            Action FleeToSafety { safe_distance: 10.0 }
        }
        Sequence spell_sequence {
            Condition HasMana { required_mana: 30 }
            Condition EnemyInRange { max_distance: 8.0 }
            Action CastSpell { spell_name: "fireball", mana_cost: 30, damage: 50 }
        }
        Sequence restore_mana_sequence {
            Condition HasMana { required_mana: 10, invert: true }
            Condition HasItem { item_type: "mana_potion", min_quantity: 1 }
            Action UseItem { item_type: "mana_potion" }
        }
        Action MoveToEnemy { move_speed: 3.0 }
    }
}
```

### Team Coordination
```
tree TeamCoordinator {
    Sequence team_root {
        Action ScanEnvironment { scan_radius: 15.0 }
        Selector coordination {
            Sequence shared_target {
                Condition HasSharedEnemyInfo {}
                Action MoveToEnemy { use_shared_info: true }
                Action AttackTarget { use_shared_info: true }
            }
            Action RandomWander { wander_radius: 10.0 }
        }
    }
}
```

## 🔧 Custom Implementation

### Creating Custom Components

```csharp
using UnityEngine;

namespace ArcBT.Samples.RPG.Components
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] int maxStamina = 100;
        [SerializeField] int currentStamina = 100;
        [SerializeField] float staminaRegenRate = 10f;
        
        public int MaxStamina => maxStamina;
        public int CurrentStamina => currentStamina;
        
        public bool ConsumeStamina(int amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                return true;
            }
            return false;
        }
        
        void Update()
        {
            if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, 
                    currentStamina + Mathf.RoundToInt(staminaRegenRate * Time.deltaTime));
            }
        }
    }
}
```

### Creating Custom Actions

```csharp
using UnityEngine;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Samples.RPG.Actions
{
    public class PowerAttackAction : BTActionNode
    {
        int damage = 50;
        int staminaCost = 25;
        float attackRange = 2.5f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    damage = System.Convert.ToInt32(value);
                    break;
                case "stamina_cost":
                    staminaCost = System.Convert.ToInt32(value);
                    break;
                case "attack_range":
                    attackRange = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            var stamina = ownerComponent.GetComponent<Stamina>();
            if (stamina == null || !stamina.ConsumeStamina(staminaCost))
            {
                return BTNodeResult.Failure;
            }
            
            // Find and attack enemy
            GameObject target = blackBoard.GetValue<GameObject>("current_target");
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= attackRange)
                {
                    var targetHealth = target.GetComponent<Health>();
                    if (targetHealth != null)
                    {
                        targetHealth.TakeDamage(damage);
                        return BTNodeResult.Success;
                    }
                }
            }
            
            return BTNodeResult.Failure;
        }
    }
}
```

### Creating Custom Conditions

```csharp
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Samples.RPG.Conditions
{
    public class StaminaCheckCondition : BTConditionNode
    {
        int minStamina = 20;
        
        public override void SetProperty(string key, string value)
        {
            if (key == "min_stamina")
            {
                minStamina = System.Convert.ToInt32(value);
            }
        }
        
        protected override BTNodeResult CheckCondition()
        {
            var stamina = ownerComponent.GetComponent<Stamina>();
            if (stamina == null)
            {
                return BTNodeResult.Failure;
            }
            
            return stamina.CurrentStamina >= minStamina ? 
                BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}
```

## 🎯 Best Practices

### 1. Resource Management
- Always check resource availability before consumption
- Implement fallback behaviors for resource-depleted states
- Use BlackBoard to share resource information between AI agents

### 2. Combat Balance
- Define clear priority systems (health > mana > stamina)
- Implement escape mechanisms for low-health situations
- Use range-based attack selection

### 3. Team Coordination
- Share enemy information via BlackBoard
- Implement role-based behaviors (tank, healer, DPS)
- Use environmental scanning for situational awareness

### 4. Performance Optimization
- Cache component references in Initialize()
- Use BTLogger for debugging without performance impact
- Implement smart condition checking to avoid unnecessary computations

## 📊 Debugging and Testing

### Visual Debugging with Enhanced Logging
```csharp
// Enable detailed logging with categories
BTLogger.SetLogLevel(LogLevel.Debug);
BTLogger.SetCategoryFilter(LogCategory.Combat, true);
BTLogger.SetCategoryFilter(LogCategory.Movement, true);

// Monitor BlackBoard contents
Debug.Log(blackBoard.GetValueAsString("current_target"));

// GameplayTagSystem debugging
BTLogger.LogSystem($"Found {enemies.Count} enemies with tag Character.Enemy");
BTLogger.LogCombat($"Attacking {target.name} with {damage} damage", "PowerAttack", this);
```

### Performance Profiling
```csharp
// Profile GameplayTagSystem performance
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
using var enemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");
stopwatch.Stop();
BTLogger.LogSystem($"Tag search took {stopwatch.ElapsedTicks} ticks for {enemies.Count} objects");
```

### Test Scenarios
1. **Resource Depletion**: Test behavior when health/mana/stamina is low
2. **Combat Situations**: Verify attack sequences, combos, and damage calculation
3. **Item Usage**: Ensure proper inventory management and item effects
4. **Team Coordination**: Test information sharing between multiple AI agents
5. **GameplayTag Performance**: Benchmark search speed vs traditional methods
6. **Decorator Combinations**: Verify complex Timeout+Retry+Repeat patterns
7. **Hierarchical Tag Matching**: Test parent/child tag relationships
8. **Memory Allocation**: Ensure 0-allocation with pooled arrays

## 🔗 Integration with Main Project

To integrate RPG components into your main project:

1. Copy desired components from `Samples~/RPGExample/`
2. Update namespaces to your project structure
3. Add custom nodes to BTParser's CreateNodeFromScript method
4. Create .bt files using the RPG action/condition names

## 🎮 Example Scenes

The RPG example includes test scenes demonstrating:
- Basic combat AI vs player
- Multi-enemy scenarios with team coordination
- Resource management under pressure
- Complex spell-casting behaviors

Access via: `BehaviourTree → Test Environment Setup → RPG Example`
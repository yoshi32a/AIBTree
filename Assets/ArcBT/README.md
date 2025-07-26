# ArcBT AI System

ArcBT is an elegant, extensible ArcBT framework for Unity with BlackBoard support, .bt file format, and comprehensive node system.

## 🚀 Features

### Core Framework
- **Generic ArcBT Engine**: Hierarchical node structure with Sequence, Selector, and Parallel composites
- **BlackBoard System**: Shared data storage for inter-node communication
- **Dynamic Condition Checking**: Real-time condition monitoring during action execution
- **.bt File Format**: Human-readable hierarchical AI definition format
- **BTLogger System**: High-performance conditional logging with categories and levels
- **Extensible Architecture**: Easy to create custom Actions and Conditions

### Included Components
- **Core Nodes**: Sequence, Selector, Parallel composition nodes
- **Generic Actions**: MoveToPosition, Wait, RandomWander, ScanEnvironment
- **Generic Conditions**: HasSharedEnemyInfo (BlackBoard-based)
- **Parser System**: Dynamic .bt file loading and node instantiation
- **Testing Framework**: Comprehensive test suite with 152+ test cases

## 📦 Installation

### Via Package Manager
1. Open Unity Package Manager
2. Add package from git URL: `https://github.com/yoshi32a/AIBTree.git?path=Assets/ArcBT`

### Manual Installation
1. Clone or download this repository
2. Copy `Assets/ArcBT/` folder to your project's `Assets/` directory

## 🎮 Quick Start

### 1. Basic ArcBT
```csharp
// Create a simple ArcBT component
var runner = gameObject.AddComponent<BehaviourTreeRunner>();
runner.behaviourTreeFilePath = "my_tree.bt";
```

### 2. Create .bt File
```
tree MyAI {
    Sequence root {
        Condition HasSharedEnemyInfo {}
        Action MoveToPosition { target_x: 5, target_z: 3 }
        Action Wait { duration: 2.0 }
    }
}
```

### 3. Custom Action Node
```csharp
using ArcBT.Core;

public class MyCustomAction : BTActionNode
{
    string message = "Hello";
    
    public override void SetProperty(string key, string value)
    {
        if (key == "message") message = value;
    }
    
    protected override BTNodeResult ExecuteAction()
    {
        Debug.Log(message);
        return BTNodeResult.Success;
    }
}
```

## 📚 Documentation

### Core Classes
- **BTNode**: Base class for all nodes
- **BTActionNode**: Base for action implementations
- **BTConditionNode**: Base for condition checks
- **BehaviourTreeRunner**: Main execution engine
- **BlackBoard**: Shared data storage
- **BTParser**: .bt file parser

### BlackBoard Usage
```csharp
// Store data
blackBoard.SetValue("enemy_position", enemyTransform.position);
blackBoard.SetValue("health", 75);

// Retrieve data
Vector3 pos = blackBoard.GetValue<Vector3>("enemy_position");
int hp = blackBoard.GetValue<int>("health", 100); // with default
```

### Logging System
```csharp
// Category-based logging
BTLogger.LogMovement("Moving to target position");
BTLogger.LogCombat("Attack completed");
BTLogger.LogSystem("ArcBT initialized");

// Conditional compilation - only logs in development builds
BTLogger.SetLogLevel(LogLevel.Warning); // Filter log levels
BTLogger.SetCategoryFilter(LogCategory.Movement, false); // Disable category
```

## 🎯 Examples

The package includes complete RPG game examples in `Samples~/RPGExample/`:

### RPG Components
- **Health/Mana Systems**: Complete resource management
- **Combat Actions**: Attack, CastSpell, FleeToSafety
- **Item System**: UseItem, RestoreSmallMana, HasItem conditions
- **AI Behaviors**: Enemy detection, team coordination, resource management

### Example Usage
```csharp
// Import samples via Package Manager
// Use AIBTreeTestEnvironmentSetup for quick setup
// Menu: ArcBT → Test Environment Setup
```

## 🧪 Testing

### Running Tests
```
Window → General → Test Runner
```

### Test Coverage
- **152 Test Cases**: Parser, BlackBoard, Logger, File validation
- **Performance Tests**: Concurrent access, memory usage
- **Integration Tests**: Complete .bt file parsing and execution

## 🔧 Requirements

- **Unity**: 6000.1.10f1 or later
- **Input System Package**: 1.14.0 (for MovementController)
- **C# .NET Standard 2.1**

## 📋 Architecture

```
Runtime/
├── Core/           # Framework essentials
│   ├── BTNode.cs           # Base node class
│   ├── BTActionNode.cs     # Action base class
│   ├── BTConditionNode.cs  # Condition base class
│   ├── BehaviourTreeRunner.cs  # Main engine
│   ├── BlackBoard.cs       # Data sharing
│   └── BTLogger.cs         # Logging system
├── Parser/         # .bt file processing
│   └── BTParser.cs         # File parser
├── Actions/        # Generic actions
│   ├── MoveToPositionAction.cs
│   ├── WaitAction.cs
│   └── ...
└── Conditions/     # Generic conditions
    └── HasSharedEnemyInfoCondition.cs

Samples~/RPGExample/
├── Components/     # Game-specific components
├── Actions/        # RPG-specific actions
└── Conditions/     # RPG-specific conditions
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

MIT License - see LICENSE.md for details

## 🙏 Acknowledgments

- Unity Technologies for the amazing game engine
- Community contributors and testers
- Claude Code for AI-assisted development
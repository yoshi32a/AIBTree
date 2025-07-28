# Behaviour Tree Language Support

VSCode extension for `.bt` (Behaviour Tree) files with syntax highlighting, auto-completion, and error checking.

## Features

- **Syntax Highlighting**: Beautiful color coding for keywords, strings, numbers, and comments
- **Auto Completion**: Intelligent suggestions for node types, properties, and script names
- **Error Detection**: Real-time syntax checking with helpful error messages
- **Code Snippets**: Pre-built templates for common behaviour tree patterns
- **Hover Information**: Contextual help when hovering over keywords

## Supported Syntax

### Node Types
- `tree` - Root tree definition
- `sequence` - Sequential execution node
- `selector` - Selection node (fallback)
- `action` - Action execution node
- `condition` - Condition checking node

### Common Properties
- `script` - Script name to execute
- `target` - Target position or object
- `speed` - Movement speed
- `damage` - Damage amount
- `duration` - Duration in seconds
- And many more...

## Installation

### From VSIX Package
1. Download the `.vsix` file
2. Open VSCode
3. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
4. Type "Extensions: Install from VSIX"
5. Select the downloaded `.vsix` file

### Development Mode

#### Setup
```bash
cd vscode-bt-extension
npm install
```

#### Running in Development
1. Open the `vscode-bt-extension` folder in VSCode
2. Press `F5` to launch Extension Development Host
3. A new VSCode window opens with the extension loaded
4. Create or open a `.bt` file in the new window to test
5. Make changes to the extension code and press `Ctrl+R` in the Extension Development Host to reload

#### Building VSIX Package
```bash
npm install -g vsce
vsce package
```
This creates a `.vsix` file that can be installed in VSCode.

#### Debugging
- Use `console.log()` in `src/extension.js` and check the Output panel → "Behaviour Tree Language Support"
- Use VSCode's built-in debugger by setting breakpoints in the extension code

## Usage

1. Create a new file with `.bt` extension
2. Start typing behavior tree code
3. Use `Ctrl+Space` for auto-completion
4. Enjoy syntax highlighting and error checking

## Example

```bt
tree ExampleAI {
    sequence root {
        condition health_check {
            script: "HealthCheck"
            min_health: 50
        }
        
        selector main_behavior {
            sequence combat {
                condition enemy_detected {
                    script: "EnemyCheck"
                    detection_range: 10.0
                }
                action attack {
                    script: "Attack"
                    damage: 25
                }
            }
            
            action patrol {
                script: "Patrol"
                speed: 3.5
            }
        }
    }
}
```

## Code Snippets

Type these prefixes and press Tab:

- `tree` - Basic tree structure
- `sequence` - Sequence node
- `selector` - Selector node
- `action` - Action node
- `condition` - Condition node
- `combat` - Complete combat sequence
- `patrol` - Complete patrol sequence
- `move` - Move action
- `wait` - Wait action
- `attack` - Attack action
- `health` - Health check condition
- `enemy` - Enemy detection condition

## Contributing

1. Fork the repository
2. Create your feature branch
3. Make your changes
4. Test with F5 in VSCode
5. Submit a pull request

## License

MIT License
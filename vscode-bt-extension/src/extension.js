const vscode = require('vscode');

function activate(context) {
    console.log('Behaviour Tree Language extension is now active');

    // 入力補完プロバイダーを登録
    const completionProvider = vscode.languages.registerCompletionItemProvider(
        'bt',
        {
            provideCompletionItems(document, position, token, context) {
                const completions = [];
                const lineText = document.lineAt(position).text;
                const textBeforeCursor = lineText.substring(0, position.character);
                
                // コンテキストを分析
                const contextInfo = analyzeContext(document, position);
                
                // コンテキストに応じて適切な補完候補を提供
                if (contextInfo.expectingNodeType) {
                    // ノードタイプの補完
                    addNodeTypeCompletions(completions);
                } else if (contextInfo.expectingScriptName) {
                    // Action/Conditionのスクリプト名補完
                    addScriptNameCompletions(completions, contextInfo.textBeforeCursor);
                } else if (contextInfo.expectingPropertyName) {
                    // プロパティ名の補完（ノードタイプに応じて）
                    addPropertyCompletions(completions, contextInfo.currentNodeType);
                } else if (contextInfo.expectingPropertyValue) {
                    // プロパティ値の補完
                    addPropertyValueCompletions(completions, contextInfo.currentProperty, contextInfo.currentNodeType);
                } else if (contextInfo.expectingTreeName) {
                    // tree名の補完
                    addTreeNameCompletion(completions);
                } else {
                    // 一般的な補完（キーワードなど）
                    addGeneralCompletions(completions, contextInfo);
                }

                return completions;
            }
        },
        ' ', ':', '{', '\n' // 複数のトリガー文字
    );

    function analyzeContext(document, position) {
        const line = document.lineAt(position).text;
        const textBeforeCursor = line.substring(0, position.character);
        const trimmedBefore = textBeforeCursor.trim();
        
        // 現在のノードタイプを検出
        let currentNodeType = null;
        let currentProperty = null;
        
        // 上の行からコンテキストを探す
        for (let i = position.line; i >= 0; i--) {
            const checkLine = document.lineAt(i).text.trim();
            if (checkLine.startsWith('Action ')) {
                currentNodeType = 'Action';
                break;
            } else if (checkLine.startsWith('Condition ')) {
                currentNodeType = 'Condition';
                break;
            } else if (checkLine.startsWith('Sequence ') || checkLine.startsWith('Selector ')) {
                currentNodeType = 'composite';
                break;
            }
        }
        
        // プロパティ名を検出
        const propertyMatch = trimmedBefore.match(/(\w+):\s*$/);
        if (propertyMatch) {
            currentProperty = propertyMatch[1];
        }
        
        return {
            expectingNodeType: /^\s*$/.test(trimmedBefore) || /^\s*(Sequence|Selector|Action|Condition)\s*$/.test(trimmedBefore),
            expectingScriptName: /^\s*(Action|Condition)\s*$/.test(trimmedBefore),
            expectingPropertyName: /^\s*$/.test(trimmedBefore) && currentNodeType && currentNodeType !== 'composite',
            expectingPropertyValue: /:\s*$/.test(trimmedBefore),
            expectingTreeName: /tree\s*$/.test(trimmedBefore),
            currentNodeType: currentNodeType,
            currentProperty: currentProperty,
            lineText: line,
            textBeforeCursor: textBeforeCursor
        };
    }

    function addNodeTypeCompletions(completions) {
        const nodeTypes = [
            { name: 'tree', desc: 'Root tree definition', template: 'tree ${1:name} {\n\t$0\n}' },
            { name: 'Sequence', desc: 'Execute children in order (all must succeed)', template: 'Sequence ${1:name} {\n\t$0\n}' },
            { name: 'Selector', desc: 'Try children until one succeeds', template: 'Selector ${1:name} {\n\t$0\n}' },
            { name: 'Action', desc: 'Perform an action', template: 'Action ${1:ScriptName} {\n\t$0\n}' },
            { name: 'Condition', desc: 'Check a condition', template: 'Condition ${1:ScriptName} {\n\t$0\n}' }
        ];
        
        nodeTypes.forEach(nodeType => {
            const completion = new vscode.CompletionItem(nodeType.name, vscode.CompletionItemKind.Keyword);
            completion.documentation = new vscode.MarkdownString(nodeType.desc);
            completion.insertText = nodeType.template;
            completion.insertSnippet = true;
            completions.push(completion);
        });
    }

    function addScriptNameCompletions(completions, textBeforeCursor) {
        let scriptNames = [];
        
        if (textBeforeCursor.includes('Action')) {
            scriptNames = [
                { name: 'MoveToPosition', desc: 'Move to a target position' },
                { name: 'Wait', desc: 'Wait for a specified duration' },
                { name: 'AttackEnemy', desc: 'Attack nearby enemies' },
                { name: 'Attack', desc: 'Generic attack action' },
                { name: 'UseItem', desc: 'Use an item from inventory' },
                { name: 'FleeToSafety', desc: 'Flee to a safe location' },
                { name: 'Interact', desc: 'Interact with an object' },
                { name: 'RandomWander', desc: 'Move randomly within an area' }
            ];
        } else if (textBeforeCursor.includes('Condition')) {
            scriptNames = [
                { name: 'HealthCheck', desc: 'Check current health level' },
                { name: 'EnemyCheck', desc: 'Check for nearby enemies' },
                { name: 'HasItem', desc: 'Check if item is in inventory' },
                { name: 'HasTarget', desc: 'Check if target exists' },
                { name: 'HasMana', desc: 'Check current mana level' },
                { name: 'IsInitialized', desc: 'Check if system is initialized' },
                { name: 'EnemyHealthCheck', desc: 'Check enemy health status' }
            ];
        }
        
        scriptNames.forEach(script => {
            const completion = new vscode.CompletionItem(script.name, vscode.CompletionItemKind.Class);
            completion.documentation = new vscode.MarkdownString(script.desc);
            completion.insertText = `${script.name} {\n\t$0\n}`;
            completion.insertSnippet = true;
            completions.push(completion);
        });
    }

    function addPropertyCompletions(completions, nodeType) {
        let properties = [];
        
        if (nodeType === 'action') {
            properties = [
                { name: 'script', desc: 'Script name to execute', value: '"${1:ScriptName}"' },
                { name: 'target', desc: 'Target position or object', value: '"${1:target_name}"' },
                { name: 'speed', desc: 'Movement speed', value: '${1:3.5}' },
                { name: 'damage', desc: 'Damage amount', value: '${1:25}' },
                { name: 'duration', desc: 'Duration in seconds', value: '${1:2.0}' },
                { name: 'tolerance', desc: 'Position tolerance', value: '${1:0.5}' },
                { name: 'cooldown', desc: 'Cooldown time', value: '${1:1.0}' }
            ];
        } else if (nodeType === 'condition') {
            properties = [
                { name: 'script', desc: 'Script name to execute', value: '"${1:ScriptName}"' },
                { name: 'min_health', desc: 'Minimum health threshold', value: '${1:50}' },
                { name: 'detection_range', desc: 'Detection range', value: '${1:10.0}' },
                { name: 'min_mana', desc: 'Minimum mana required', value: '${1:30}' }
            ];
        } else {
            // 汎用プロパティ
            properties = [
                { name: 'script', desc: 'Script name to execute', value: '"${1:ScriptName}"' }
            ];
        }
        
        properties.forEach(prop => {
            const completion = new vscode.CompletionItem(prop.name, vscode.CompletionItemKind.Property);
            completion.insertText = new vscode.SnippetString(`${prop.name}: ${prop.value}`);
            completion.documentation = new vscode.MarkdownString(prop.desc);
            completions.push(completion);
        });
    }

    function addPropertyValueCompletions(completions, propertyName, nodeType) {
        if (propertyName === 'script') {
            let scripts = [];
            
            if (nodeType === 'action') {
                scripts = [
                    'MoveToPosition', 'Wait', 'Attack', 'AttackEnemy', 'UseItem',
                    'FleeToSafety', 'Interact', 'RandomWander'
                ];
            } else if (nodeType === 'condition') {
                scripts = [
                    'HealthCheck', 'EnemyCheck', 'HasTarget', 'HasItem', 'HasMana',
                    'IsInitialized', 'EnemyHealthCheck'
                ];
            }
            
            scripts.forEach(script => {
                const completion = new vscode.CompletionItem(`"${script}"`, vscode.CompletionItemKind.Value);
                completion.documentation = new vscode.MarkdownString(getScriptDescription(script));
                completions.push(completion);
            });
        } else if (['speed', 'damage', 'duration', 'min_health', 'detection_range'].includes(propertyName)) {
            // 数値の候補
            const numericSuggestions = getNumericSuggestions(propertyName);
            numericSuggestions.forEach(num => {
                const completion = new vscode.CompletionItem(num.toString(), vscode.CompletionItemKind.Value);
                completions.push(completion);
            });
        }
    }

    function addTreeNameCompletion(completions) {
        const completion = new vscode.CompletionItem('TreeName', vscode.CompletionItemKind.Class);
        completion.insertText = new vscode.SnippetString('${1:MyAI} {\n\t$0\n}');
        completion.documentation = new vscode.MarkdownString('Define a new behavior tree');
        completions.push(completion);
    }

    function addGeneralCompletions(completions, contextInfo) {
        // 一般的なキーワードやスニペット
        if (!contextInfo.currentNodeType) {
            addNodeTypeCompletions(completions);
        }
    }

    function getNumericSuggestions(propertyName) {
        const suggestions = {
            'speed': [1.0, 2.0, 3.5, 5.0],
            'damage': [10, 25, 50, 100],
            'duration': [1.0, 2.0, 3.0, 5.0],
            'min_health': [20, 50, 75, 100],
            'detection_range': [5.0, 10.0, 15.0, 20.0]
        };
        return suggestions[propertyName] || [1, 2, 5, 10];
    }

    // ホバープロバイダーを登録
    const hoverProvider = vscode.languages.registerHoverProvider('bt', {
        provideHover(document, position, token) {
            const range = document.getWordRangeAtPosition(position);
            if (!range) return;

            const word = document.getText(range);
            const hoverText = getHoverText(word);
            
            if (hoverText) {
                return new vscode.Hover(new vscode.MarkdownString(hoverText));
            }
        }
    });

    // 診断プロバイダー（構文チェック）
    const diagnosticCollection = vscode.languages.createDiagnosticCollection('bt');
    
    function updateDiagnostics(document) {
        if (document.languageId !== 'bt') return;

        // 設定を確認して診断が無効になっている場合はスキップ
        const config = vscode.workspace.getConfiguration('bt');
        const diagnosticsEnabled = config.get('diagnostics.enabled', true);
        
        if (!diagnosticsEnabled) {
            diagnosticCollection.set(document.uri, []);
            return;
        }

        const diagnostics = [];
        const text = document.getText();
        const lines = text.split('\n');

        // より正確な構文チェック
        let braceCount = 0;
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            const trimmedLine = line.trim();
            
            // 波括弧のバランスチェック
            for (const char of line) {
                if (char === '{') braceCount++;
                if (char === '}') braceCount--;
            }

            // 空行やコメント行をスキップ
            if (!trimmedLine || trimmedLine.startsWith('#')) {
                continue;
            }

            // ノードタイプの後に名前がない
            if (/^(Sequence|Selector|Action|Condition|tree)\s*\{/.test(trimmedLine)) {
                const diagnostic = new vscode.Diagnostic(
                    new vscode.Range(i, 0, i, line.length),
                    'Node type requires a name',
                    vscode.DiagnosticSeverity.Error
                );
                diagnostics.push(diagnostic);
                continue;
            }

            // デバッグ用ログ（必要時のみ有効化）
            // console.log(`Line ${i}: "${trimmedLine}"`);

            // 正しいプロパティ形式をより厳密にチェック
            const isNodeDeclaration = /^(sequence|selector|action|condition|tree)\s+\w+\s*\{/.test(trimmedLine);
            const isClosingBrace = trimmedLine === '}';
            const isValidProperty = /^\s*\w+\s*:\s*.+/.test(trimmedLine);
            
            // より保守的なプロパティチェック - 明らかに間違っているもののみ警告
            // コメント行を除外
            const isCommentLine = trimmedLine.startsWith('#');
            
            // 明らかに単語だけでコロンがない行のみチェック（ただし、コメント行は除外）
            if (!isNodeDeclaration && !isClosingBrace && !isCommentLine && trimmedLine.length > 0) {
                // 単語だけで終わっている行で、明らかにプロパティっぽい場合のみ警告
                const isSingleWordLine = /^\s*[a-zA-Z_]\w*\s*$/.test(trimmedLine);
                if (isSingleWordLine) {
                    const diagnostic = new vscode.Diagnostic(
                        new vscode.Range(i, 0, i, line.length),
                        'Property requires a colon (:) followed by a value',
                        vscode.DiagnosticSeverity.Warning
                    );
                    diagnostics.push(diagnostic);
                }
            }
        }

        // 波括弧の不一致
        if (braceCount !== 0) {
            const diagnostic = new vscode.Diagnostic(
                new vscode.Range(0, 0, lines.length - 1, lines[lines.length - 1].length),
                'Mismatched braces',
                vscode.DiagnosticSeverity.Error
            );
            diagnostics.push(diagnostic);
        }

        diagnosticCollection.set(document.uri, diagnostics);
    }

    // ドキュメント変更時の診断更新
    const activeEditor = vscode.window.activeTextEditor;
    if (activeEditor) {
        updateDiagnostics(activeEditor.document);
    }

    context.subscriptions.push(
        completionProvider,
        hoverProvider,
        vscode.window.onDidChangeActiveTextEditor(editor => {
            if (editor) {
                updateDiagnostics(editor.document);
            }
        }),
        vscode.workspace.onDidChangeTextDocument(e => updateDiagnostics(e.document)),
        diagnosticCollection
    );
}

function getNodeTypeDescription(nodeType) {
    const descriptions = {
        'sequence': 'Executes child nodes in order. Succeeds if all children succeed.',
        'selector': 'Executes child nodes until one succeeds. Succeeds if any child succeeds.',
        'action': 'Performs an action. Success depends on the action implementation.',
        'condition': 'Checks a condition. Returns success or failure immediately.'
    };
    return descriptions[nodeType] || '';
}

function getPropertyDescription(property) {
    const descriptions = {
        'script': 'The script name to execute for this node',
        'target': 'Target position or object name',
        'speed': 'Movement speed (float)',
        'damage': 'Damage amount (integer)',
        'duration': 'Duration in seconds (float)',
        'min_health': 'Minimum health threshold (integer)',
        'detection_range': 'Detection range in units (float)',
        'attack_range': 'Attack range in units (float)',
        'tolerance': 'Position tolerance for movement (float)',
        'cooldown': 'Cooldown time in seconds (float)'
    };
    return descriptions[property] || '';
}

function getScriptDescription(script) {
    const descriptions = {
        'HealthCheck': 'Checks if health is above minimum threshold',
        'EnemyCheck': 'Detects enemies within specified range',
        'MoveToPosition': 'Moves to a target position',
        'Wait': 'Waits for specified duration',
        'Attack': 'Performs basic attack',
        'AttackEnemy': 'Attacks detected enemy'
    };
    return descriptions[script] || '';
}

function getHoverText(word) {
    const nodeTypes = {
        'sequence': 'Composite node that executes children sequentially',
        'selector': 'Composite node that tries children until one succeeds',
        'action': 'Leaf node that performs an action',
        'condition': 'Leaf node that checks a condition',
        'tree': 'Root definition of a behavior tree'
    };

    return nodeTypes[word];
}

function deactivate() {}

module.exports = {
    activate,
    deactivate
};
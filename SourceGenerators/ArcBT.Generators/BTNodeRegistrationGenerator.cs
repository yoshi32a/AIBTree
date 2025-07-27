using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ArcBT.Generators
{
    [Generator]
    public class BTNodeRegistrationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // デバッグ用
            // System.Diagnostics.Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                // 診断情報: Source Generator動作開始（デバッグ時のみ）
                #if DEBUG
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("ARCBT001", "Source Generator Start", "ArcBT Source Generator started executing", "ArcBT", DiagnosticSeverity.Info, true),
                    Location.None));
                #endif

                if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("ARCBT002", "No Syntax Receiver", "No syntax receiver found", "ArcBT", DiagnosticSeverity.Error, true),
                        Location.None));
                    return;
                }

                ExecuteCore(context, receiver);
            }
            catch (System.Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("ARCBT099", "Generator Exception", $"Source Generator exception: {ex.Message}", "ArcBT", DiagnosticSeverity.Error, true),
                    Location.None));
                return;
            }
        }

        private void ExecuteCore(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            var compilation = context.Compilation;
            
            // 現在のアセンブリ名を取得
            var currentAssemblyName = compilation.AssemblyName;
            
            // より柔軟なシンボル検索を試行
            var btNodeAttributeSymbol = compilation.GetTypeByMetadataName("ArcBT.Core.BTNodeAttribute") 
                ?? GetTypeFromAllAssemblies(compilation, "BTNodeAttribute");
            var btActionNodeSymbol = compilation.GetTypeByMetadataName("ArcBT.Core.BTActionNode")
                ?? GetTypeFromAllAssemblies(compilation, "BTActionNode");
            var btConditionNodeSymbol = compilation.GetTypeByMetadataName("ArcBT.Core.BTConditionNode")
                ?? GetTypeFromAllAssemblies(compilation, "BTConditionNode");

            #if DEBUG
            // 診断情報: 現在のアセンブリ名を表示
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("ARCBT002", "Current Assembly", 
                    $"Processing assembly: {currentAssemblyName}", 
                    "ArcBT", DiagnosticSeverity.Info, true),
                Location.None));

            // 診断情報: 利用可能なアセンブリとシンボル情報（デバッグ時のみ）
            var assemblyNames = compilation.SourceModule.ReferencedAssemblySymbols
                .Select(a => a.Name)
                .Where(name => name.Contains("ArcBT") || name.Contains("Unity") || name.Contains("System"))
                .Take(10)
                .ToArray();
            
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("ARCBT003", "Available Assemblies", 
                    $"Found assemblies: {string.Join(", ", assemblyNames)}", 
                    "ArcBT", DiagnosticSeverity.Info, true),
                Location.None));

            // 診断情報: シンボル検索結果（デバッグ時のみ）
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("ARCBT005", "Symbol Search", 
                    $"BTNodeAttribute: {(btNodeAttributeSymbol != null ? "Found" : "Not Found")}, " +
                    $"BTActionNode: {(btActionNodeSymbol != null ? "Found" : "Not Found")}, " +
                    $"BTConditionNode: {(btConditionNodeSymbol != null ? "Found" : "Not Found")}", 
                    "ArcBT", DiagnosticSeverity.Info, true),
                Location.None));
            #endif

            if (btNodeAttributeSymbol == null || btActionNodeSymbol == null || btConditionNodeSymbol == null)
            {
                // シンボルが見つからない場合は静かに終了
                return;
            }

            // アセンブリごとにノードをグループ化
            var nodesByAssembly = new Dictionary<string, List<NodeInfo>>();

            // 一時的な診断情報: 候補クラス数を表示
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("ARCBT006", "Candidate Classes", 
                    $"Assembly '{currentAssemblyName}': Found {receiver.CandidateClasses.Count} candidate classes", 
                    "ArcBT", DiagnosticSeverity.Warning, true),
                Location.None));

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration);

                if (classSymbol == null)
                    continue;

                // BTNodeAttribute を持つクラスを探す
                foreach (var attribute in classSymbol.GetAttributes())
                {
                    if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, btNodeAttributeSymbol))
                        continue;

                    string scriptName = null;
                    string nodeType = null;

                    // 基底クラスからNodeTypeを自動判定
                    if (IsInheritedFrom(classSymbol, btActionNodeSymbol))
                    {
                        nodeType = "Action";
                    }
                    else if (IsInheritedFrom(classSymbol, btConditionNodeSymbol))
                    {
                        nodeType = "Condition";
                    }

                    // 属性の第1引数：スクリプト名を取得
                    if (attribute.ConstructorArguments.Length >= 1)
                    {
                        scriptName = attribute.ConstructorArguments[0].Value?.ToString();
                    }
                    else
                    {
                        // スクリプト名が指定されていない場合、クラス名を使用
                        scriptName = classSymbol.Name;
                    }

                    if (scriptName == null || nodeType == null)
                        continue;

                    // AssemblyName プロパティを取得（オプション）
                    // 現在のアセンブリ名をデフォルトとして使用
                    var assemblyName = currentAssemblyName;
                    
                    foreach (var namedArg in attribute.NamedArguments)
                    {
                        if (namedArg.Key == "AssemblyName" && namedArg.Value.Value is string asmName)
                        {
                            assemblyName = asmName;
                        }
                    }

                    var nodeInfo = new NodeInfo
                    {
                        ClassName = classSymbol.Name,
                        FullTypeName = classSymbol.ToDisplayString(),
                        ScriptName = scriptName,
                        NodeType = nodeType,
                        Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "global"
                    };

                    if (!nodesByAssembly.ContainsKey(assemblyName))
                        nodesByAssembly[assemblyName] = new List<NodeInfo>();

                    nodesByAssembly[assemblyName].Add(nodeInfo);
                }
            }

            // アセンブリごとに登録コードを生成
            foreach (var kvp in nodesByAssembly)
            {
                var assemblyName = kvp.Key;
                var nodes = kvp.Value;
                
                if (nodes.Count == 0)
                    continue;

                var source = GenerateRegistrationCode(currentAssemblyName, assemblyName, nodes);
                // ファイル名には実際のアセンブリ名を使用
                var fileName = $"{SanitizeAssemblyName(currentAssemblyName)}.NodeRegistration.g.cs";
                context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
            }
        }

        private string GenerateRegistrationCode(string currentAssemblyName, string logicalAssemblyName, List<NodeInfo> nodes)
        {
            var sb = new StringBuilder();
            
            // ヘッダー
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using System;");
            sb.AppendLine("using ArcBT.Core;");
            sb.AppendLine("using ArcBT.Logger;");
            sb.AppendLine("using UnityEngine;");
            
            // 各ノードの名前空間を追加
            var namespaces = nodes.Select(n => n.Namespace)
                .Distinct()
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != "global" && IsValidNamespace(ns));
            foreach (var ns in namespaces)
            {
                sb.AppendLine($"using {ns};");
            }
            
            sb.AppendLine();
            var safeAssemblyName = SanitizeAssemblyName(currentAssemblyName);
            sb.AppendLine($"namespace {safeAssemblyName}.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {currentAssemblyName} の BTNode 自動登録クラス");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [DefaultExecutionOrder(-1000)]");
            sb.AppendLine($"    public static class {safeAssemblyName.Replace(".", "")}NodeRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
            sb.AppendLine("        static void RegisterNodes()");
            sb.AppendLine("        {");
            
            // Action ノードの登録
            var actionNodes = nodes.Where(n => n.NodeType == "Action").OrderBy(n => n.ScriptName);
            if (actionNodes.Any())
            {
                sb.AppendLine("            // Action ノードの登録");
                foreach (var node in actionNodes)
                {
                    sb.AppendLine($"            BTStaticNodeRegistry.RegisterAction(\"{node.ScriptName}\", () => new {node.ClassName}());");
                }
                sb.AppendLine();
            }
            
            // Condition ノードの登録
            var conditionNodes = nodes.Where(n => n.NodeType == "Condition").OrderBy(n => n.ScriptName);
            if (conditionNodes.Any())
            {
                sb.AppendLine("            // Condition ノードの登録");
                foreach (var node in conditionNodes)
                {
                    sb.AppendLine($"            BTStaticNodeRegistry.RegisterCondition(\"{node.ScriptName}\", () => new {node.ClassName}());");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine($"            BTLogger.LogSystem($\"{currentAssemblyName} ノードを自動登録しました (Actions: {actionNodes.Count()}, Conditions: {conditionNodes.Count()})\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }


        /// <summary>
        /// 指定されたクラスが基底クラスを継承しているかチェック
        /// </summary>
        private static bool IsInheritedFrom(INamedTypeSymbol classSymbol, INamedTypeSymbol baseClassSymbol)
        {
            var current = classSymbol.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, baseClassSymbol))
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private class NodeInfo
        {
            public string ClassName { get; set; } = "";
            public string FullTypeName { get; set; } = "";
            public string ScriptName { get; set; } = "";
            public string NodeType { get; set; } = "";
            public string Namespace { get; set; } = "";
        }

        private INamedTypeSymbol GetTypeFromAllAssemblies(Compilation compilation, string typeName)
        {
            // 全てのアセンブリから指定された型名を検索
            foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Concat(new[] { compilation.Assembly }))
            {
                foreach (var namespaceName in new[] { "ArcBT.Core", "ArcBT", "" })
                {
                    var fullName = string.IsNullOrEmpty(namespaceName) ? typeName : $"{namespaceName}.{typeName}";
                    var symbol = assembly.GetTypeByMetadataName(fullName);
                    if (symbol != null)
                        return symbol;
                }
            }
            return null;
        }

        /// <summary>
        /// 名前空間が有効かチェック
        /// </summary>
        private static bool IsValidNamespace(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                return false;

            // C#の有効な識別子文字をチェック
            return namespaceName.Split('.')
                .All(part => !string.IsNullOrEmpty(part) && 
                            char.IsLetter(part[0]) && 
                            part.All(c => char.IsLetterOrDigit(c) || c == '_'));
        }

        /// <summary>
        /// アセンブリ名をサニタイズ
        /// </summary>
        private static string SanitizeAssemblyName(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return "ArcBT";

            // C#の有効な識別子に変換
            var sanitized = new StringBuilder();
            foreach (char c in assemblyName)
            {
                if (char.IsLetterOrDigit(c) || c == '.')
                {
                    sanitized.Append(c);
                }
                else if (c == '-' || c == '_')
                {
                    sanitized.Append('_');
                }
            }

            var result = sanitized.ToString();
            return string.IsNullOrEmpty(result) ? "ArcBT" : result;
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // すべてのクラスを候補として収集（属性の有無に関係なく）
                if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}

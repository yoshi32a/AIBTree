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

                    string? scriptName = null;

                    // BTNode継承チェック（NodeType判定不要）
                    if (!InheritsFromBTNode(classSymbol))
                        continue;

                    // 属性の第1引数：スクリプト名を取得
                    if (attribute.ConstructorArguments.Length >= 1)
                    {
                        scriptName = attribute.ConstructorArguments[0].Value?.ToString();
                    }
                    
                    // スクリプト名が指定されていない場合、クラス名を使用
                    if (string.IsNullOrEmpty(scriptName))
                    {
                        scriptName = classSymbol.Name;
                    }

                    if (string.IsNullOrEmpty(scriptName))
                        continue;

                    var nodeInfo = new NodeInfo
                    {
                        ClassName = classSymbol.Name,
                        FullTypeName = classSymbol.ToDisplayString(),
                        ScriptName = scriptName,
                        Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "global"
                    };

                    var safeAssemblyName = currentAssemblyName ?? "Unknown";
                    if (!nodesByAssembly.ContainsKey(safeAssemblyName))
                        nodesByAssembly[safeAssemblyName] = new List<NodeInfo>();

                    nodesByAssembly[safeAssemblyName].Add(nodeInfo);
                }
            }

            // アセンブリごとに登録コードを生成
            foreach (var kvp in nodesByAssembly)
            {
                var assemblyName = kvp.Key;
                var nodes = kvp.Value;
                
                if (nodes.Count == 0)
                    continue;

                var source = GenerateRegistrationCode(currentAssemblyName ?? "Unknown", assemblyName, nodes);
                // ファイル名には実際のアセンブリ名を使用
                var fileName = $"{SanitizeAssemblyName(currentAssemblyName ?? "Unknown")}.NodeRegistration.g.cs";
                context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
            }
        }

        private string GenerateRegistrationCode(string currentAssemblyName, string logicalAssemblyName, List<NodeInfo> nodes)
        {
            var sb = new StringBuilder();
            
            // ヘッダー
            sb.AppendLine("// <auto-generated/>");
            
            // 基本的なusing文
            var defaultUsings = new HashSet<string>
            {
                "System",
                "ArcBT.Core", 
                "ArcBT.Logger",
                "UnityEngine"
            };
            
            // 各ノードの名前空間を取得（重複排除）
            var nodeNamespaces = new HashSet<string>(
                nodes.Select(n => n.Namespace)
                    .Distinct()
                    .Where(ns => !string.IsNullOrEmpty(ns) && ns != "global" && IsValidNamespace(ns))
            );
            
            // 基本using文と重複しないものだけを追加
            var allUsings = defaultUsings.Union(nodeNamespaces).OrderBy(x => x);
            
            foreach (var usingNamespace in allUsings)
            {
                sb.AppendLine($"using {usingNamespace};");
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
            
            // 統一ノード登録（全ノードタイプ対応）
            var allNodes = nodes.OrderBy(n => n.ScriptName);
            if (allNodes.Any())
            {
                sb.AppendLine("            // 統一ノード登録（全ノードタイプ対応）");
                foreach (var node in allNodes)
                {
                    sb.AppendLine($"            BTStaticNodeRegistry.RegisterNode(\"{node.ScriptName}\", () => new {node.ClassName}());");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine($"            BTLogger.LogSystem($\"{currentAssemblyName} ノードを自動登録しました ({allNodes.Count()} nodes)\");");
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

        private bool InheritsFromBTNode(INamedTypeSymbol classSymbol)
        {
            var currentType = classSymbol.BaseType;
            while (currentType != null)
            {
                // BTNodeまたはその派生クラスを継承していればOK
                switch (currentType.Name)
                {
                    case "BTNode":
                    case "BTActionNode":
                    case "BTConditionNode":
                    case "BTCompositeNode":
                    case "BTDecoratorNode":
                    case "BTServiceNode":
                        return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }

        private class NodeInfo
        {
            public string ClassName { get; set; } = "";
            public string FullTypeName { get; set; } = "";
            public string ScriptName { get; set; } = "";
            public string Namespace { get; set; } = "";
        }

        private INamedTypeSymbol? GetTypeFromAllAssemblies(Compilation compilation, string typeName)
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

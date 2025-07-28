using UnityEngine;
using UnityEditor;
using ArcBT.Core;
using System;
using ArcBT.Logger;
using Microsoft.Extensions.Logging;

[CustomEditor(typeof(BehaviourTreeRunner))]
public class BTLoggerEditor : Editor
{
    static bool showLogSettings = false;
    
    public override void OnInspectorGUI()
    {
        // 既存のインスペクター表示
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("BT Logger Controls (ZLogger統合版)", EditorStyles.boldLabel);
        
        // ZLogger統合情報
        EditorGUILayout.HelpBox(
            "ZLogger統合による高性能ログシステム (Phase 6.3)\n" +
            "• ゼロアロケーション最適化完了\n" +
            "• Microsoft.Extensions.Logging完全統合\n" +
            "• ユーザーLoggerFactoryによるフィルタリング制御\n" +
            "• 履歴管理はZLoggerプロバイダーに委譲", 
            MessageType.Info);
        
        // ログ設定セクション
        showLogSettings = EditorGUILayout.Foldout(showLogSettings, "ログ設定", true);
        if (showLogSettings)
        {
            DrawLogSettings();
        }
        
        EditorGUILayout.Space(5);
        
        // ログ制御ボタン
        DrawLogControlButtons();
    }
    
    void DrawLogSettings()
    {
        EditorGUI.indentLevel++;
        
        // 設定状況表示
        EditorGUILayout.LabelField("BTLogger設定状況", EditorStyles.miniBoldLabel);
        
        var isConfigured = BTLogger.IsConfigured;
        var statusColor = isConfigured ? Color.green : Color.yellow;
        var statusMessage = isConfigured ? "✅ LoggerFactory設定済み" : "⚠️ LoggerFactory未設定（NullLogger使用中）";
        
        var originalColor = GUI.color;
        GUI.color = statusColor;
        EditorGUILayout.LabelField(statusMessage);
        GUI.color = originalColor;
        
        EditorGUILayout.Space(3);
        
        // ユーザー向け設定ガイド
        EditorGUILayout.LabelField("フィルタリング制御", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox(
            "Phase 6.3: ログフィルタリングはユーザーのLoggerFactory設定で制御します\n\n" +
            "設定例:\n" +
            "builder.AddFilter(\"ArcBT\", LogLevel.Information);", 
            MessageType.Info);
        
        EditorGUI.indentLevel--;
    }
    
    
    void DrawLogControlButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("設定ガイド表示"))
        {
            EditorGUILayout.HelpBox(
                "BTLogger設定方法:\n\n" +
                "1. アプリケーション初期化時にLoggerFactoryを作成\n" +
                "2. BTLogger.Configure(loggerFactory)を呼び出し\n" +
                "3. AddFilter()でArcBTログレベルを制御\n\n" +
                "詳細はCLAUDE.mdを参照してください。", 
                MessageType.Info);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // ZLoggerプロバイダー情報
        EditorGUILayout.HelpBox(
            "Phase 6.3完了: 履歴管理はZLoggerプロバイダーに完全委譲\n" +
            "Consoleウィンドウまたは設定したログ出力先でログを確認してください。", 
            MessageType.Info);
    }
}

using UnityEngine;
using UnityEditor;
using BehaviourTree.Core;
using System;
using System.Linq;

[CustomEditor(typeof(BehaviourTreeRunner))]
public class BTLoggerEditor : Editor
{
    static bool showLogSettings = false;
    static bool showRecentLogs = false;
    static LogCategory selectedLogCategory = LogCategory.System;
    static int logDisplayCount = 10;
    
    Vector2 logScrollPosition;
    
    public override void OnInspectorGUI()
    {
        // 既存のインスペクター表示
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("BT Logger Controls", EditorStyles.boldLabel);
        
        // ログ設定セクション
        showLogSettings = EditorGUILayout.Foldout(showLogSettings, "ログ設定", true);
        if (showLogSettings)
        {
            DrawLogSettings();
        }
        
        EditorGUILayout.Space(5);
        
        // 最近のログ表示セクション
        showRecentLogs = EditorGUILayout.Foldout(showRecentLogs, "最近のログ", true);
        if (showRecentLogs)
        {
            DrawRecentLogs();
        }
        
        EditorGUILayout.Space(5);
        
        // ログ制御ボタン
        DrawLogControlButtons();
    }
    
    void DrawLogSettings()
    {
        EditorGUI.indentLevel++;
        
        // ログレベル設定
        EditorGUILayout.LabelField("ログレベル", EditorStyles.miniBoldLabel);
        var currentLevel = BTLogger.GetCurrentLogLevel();
        var newLevel = (LogLevel)EditorGUILayout.EnumPopup("表示レベル", currentLevel);
        if (newLevel != currentLevel)
        {
            BTLogger.SetLogLevel(newLevel);
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.Space(3);
        
        // カテゴリフィルター設定
        EditorGUILayout.LabelField("カテゴリフィルター", EditorStyles.miniBoldLabel);
        
        foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
        {
            var currentEnabled = BTLogger.IsCategoryEnabled(category);
            var tag = GetCategoryTag(category);
            var newEnabled = EditorGUILayout.Toggle($"{tag} {category}", currentEnabled);
            
            if (newEnabled != currentEnabled)
            {
                BTLogger.SetCategoryFilter(category, newEnabled);
                EditorUtility.SetDirty(target);
            }
        }
        
        EditorGUI.indentLevel--;
    }
    
    void DrawRecentLogs()
    {
        EditorGUI.indentLevel++;
        
        // ログ表示設定
        EditorGUILayout.BeginHorizontal();
        selectedLogCategory = (LogCategory)EditorGUILayout.EnumPopup("カテゴリ", selectedLogCategory);
        logDisplayCount = EditorGUILayout.IntSlider("表示件数", logDisplayCount, 5, 50);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("ログを更新"))
        {
            Repaint();
        }
        
        EditorGUILayout.Space(3);
        
        // ログ表示エリア
        var logs = selectedLogCategory == LogCategory.System 
            ? BTLogger.GetRecentLogs(logDisplayCount)
            : BTLogger.GetLogsByCategory(selectedLogCategory, logDisplayCount);
            
        if (logs.Length > 0)
        {
            EditorGUILayout.LabelField($"最新 {logs.Length} 件のログ:", EditorStyles.miniBoldLabel);
            
            logScrollPosition = EditorGUILayout.BeginScrollView(logScrollPosition, 
                GUILayout.Height(200), GUILayout.ExpandWidth(true));
            
            foreach (var log in logs.Reverse())
            {
                DrawLogEntry(log);
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("表示するログがありません", MessageType.Info);
        }
        
        EditorGUI.indentLevel--;
    }
    
    void DrawLogEntry(LogEntry log)
    {
        var style = GetLogStyle(log.level);
        var categoryTag = GetCategoryTag(log.category);
        var levelTag = GetLevelTag(log.level);
        var timeStr = log.timestamp.ToString("HH:mm:ss.fff");
        var nodeInfo = !string.IsNullOrEmpty(log.nodeName) ? $"[{log.nodeName}]" : "";
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // ログヘッダー
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{timeStr}", GUILayout.Width(80));
        EditorGUILayout.LabelField($"{levelTag}", GUILayout.Width(70));
        EditorGUILayout.LabelField($"{categoryTag}", GUILayout.Width(80));
        if (!string.IsNullOrEmpty(nodeInfo))
        {
            EditorGUILayout.LabelField(nodeInfo, GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();
        
        // ログメッセージ
        EditorGUILayout.LabelField(log.message, style);
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawLogControlButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("すべて有効"))
        {
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                BTLogger.SetCategoryFilter(category, true);
            }
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("すべて無効"))
        {
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                BTLogger.SetCategoryFilter(category, false);
            }
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("デフォルト設定"))
        {
            BTLogger.ResetToDefaults();
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("ログ履歴をクリア"))
        {
            BTLogger.ClearHistory();
            Repaint();
        }
    }
    
    GUIStyle GetLogStyle(LogLevel level)
    {
        var style = new GUIStyle(EditorStyles.label);
        style.wordWrap = true;
        
        switch (level)
        {
            case LogLevel.Error:
                style.normal.textColor = Color.red;
                break;
            case LogLevel.Warning:
                style.normal.textColor = new Color(1f, 0.6f, 0f); // オレンジ
                break;
            case LogLevel.Info:
                style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                break;
            case LogLevel.Debug:
                style.normal.textColor = Color.cyan;
                break;
            case LogLevel.Trace:
                style.normal.textColor = Color.gray;
                break;
        }
        
        return style;
    }
    
    string GetCategoryTag(LogCategory category)
    {
        switch (category)
        {
            case LogCategory.Combat: return "[ATK]";     // Attack
            case LogCategory.Movement: return "[MOV]";   // Move
            case LogCategory.Condition: return "[CHK]";  // Check
            case LogCategory.BlackBoard: return "[BBD]"; // BlackBoard
            case LogCategory.Parser: return "[PRS]";     // Parse
            case LogCategory.System: return "[SYS]";     // System
            case LogCategory.Debug: return "[DBG]";      // Debug
            default: return "[UNK]";                     // Unknown
        }
    }
    
    string GetLevelTag(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Error: return "[ERR]";   // Error
            case LogLevel.Warning: return "[WRN]"; // Warning
            case LogLevel.Info: return "[INF]";    // Info
            case LogLevel.Debug: return "[DBG]";   // Debug
            case LogLevel.Trace: return "[TRC]";   // Trace
            default: return "[UNK]";               // Unknown
        }
    }
}
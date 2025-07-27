using UnityEngine;
using UnityEditor;
using ArcBT.TagSystem;

namespace AIBTree.Editor
{
    [CustomEditor(typeof(GameplayTagAsset))]
    public class GameplayTagAssetEditor : UnityEditor.Editor
    {
        SerializedProperty categoriesProperty;

        void OnEnable()
        {
            categoriesProperty = serializedObject.FindProperty("categories");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Gameplay Tag Definitions", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // カテゴリリスト
            EditorGUILayout.PropertyField(categoriesProperty, new GUIContent("Tag Categories"), true);

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Sample Tags"))
            {
                var asset = (GameplayTagAsset)target;
                TagMigrationHelper.AddSampleTagsToAsset(asset);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

    // カスタムプロパティドロワー
    [CustomPropertyDrawer(typeof(GameplayTag))]
    public class GameplayTagPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tagNameProp = property.FindPropertyRelative("tagName");
            
            // ドロップダウンで選択できるようにする
            if (GUI.Button(position, new GUIContent(string.IsNullOrEmpty(tagNameProp.stringValue) ? "None" : tagNameProp.stringValue)))
            {
                ShowTagSelectionMenu(tagNameProp);
            }

            EditorGUI.EndProperty();
        }

        void ShowTagSelectionMenu(SerializedProperty tagNameProp)
        {
            var menu = new GenericMenu();
            
            // Noneオプション
            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(tagNameProp.stringValue), () =>
            {
                tagNameProp.stringValue = "";
                tagNameProp.serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            // タグアセットから定義済みタグを取得
            var tagAssets = AssetDatabase.FindAssets("t:GameplayTagAsset");
            foreach (var guid in tagAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameplayTagAsset>(path);
                if (asset != null)
                {
                    foreach (var tag in asset.GetAllTags())
                    {
                        var tagName = tag.TagName;
                        menu.AddItem(new GUIContent(tagName), tagNameProp.stringValue == tagName, () =>
                        {
                            tagNameProp.stringValue = tagName;
                            tagNameProp.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }
            }

            menu.ShowAsContext();
        }
    }
}

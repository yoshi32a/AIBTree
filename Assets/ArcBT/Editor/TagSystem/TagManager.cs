using UnityEditor;
using ArcBT.Logger;

namespace ArcBT.Editor
{
    public static class TagManager
    {
        [MenuItem("BehaviourTree/Setup Required Tags")]
        public static void CreateRequiredTags()
        {
            CreateTag("SafeZone");
            CreateTag("Interactable");
            CreateTag("Item");
            CreateTag("Treasure");
            
            BTLogger.Info("Required tags created successfully: SafeZone, Interactable, Item, Treasure");
        }
        
        static void CreateTag(string tagName)
        {
            // TagManagerアセットを取得
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            
            // タグが既に存在するかチェック
            var found = false;
            for (var i = 0; i < tagsProp.arraySize; i++)
            {
                var t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName))
                {
                    found = true;
                    break;
                }
            }
            
            // タグが存在しない場合は追加
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                var newTagProp = tagsProp.GetArrayElementAtIndex(0);
                newTagProp.stringValue = tagName;
                tagManager.ApplyModifiedProperties();
                BTLogger.Info($"Tag '{tagName}' created successfully");
            }
            else
            {
                BTLogger.Info($"Tag '{tagName}' already exists");
            }
        }
    }
}

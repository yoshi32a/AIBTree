using System.Collections.Generic;
using UnityEngine;
using ArcBT.Core;
using ArcBT.Logger;

namespace ArcBT.Samples.RPG.Components
{
    /// <summary>インベントリシステムを管理するコンポーネント</summary>
    [System.Serializable]
    public class Inventory : MonoBehaviour
{
    [SerializeField] List<InventoryItem> items = new List<InventoryItem>();
    
    void Start()
    {
        // デフォルトアイテムを追加（テスト用）
        if (items.Count == 0)
        {
            items.Add(new InventoryItem("health_potion", 2));
            items.Add(new InventoryItem("bow", 1));
            BTLogger.LogSystem($"Inventory: Added default items to {gameObject.name}");
        }
    }
    
    public bool HasItem(string itemType)
    {
        foreach (var item in items)
        {
            if (item.itemType == itemType && item.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool UseItem(string itemType, int amount = 1)
    {
        foreach (var item in items)
        {
            if (item.itemType == itemType && item.quantity >= amount)
            {
                item.quantity -= amount;
                BTLogger.LogSystem($"Inventory: Used {amount} {itemType}. Remaining: {item.quantity}");
                return true;
            }
        }
        BTLogger.LogSystem($"Inventory: Cannot use {itemType} - insufficient quantity");
        return false;
    }
    
    public void AddItem(string itemType, int amount = 1)
    {
        foreach (var item in items)
        {
            if (item.itemType == itemType)
            {
                item.quantity += amount;
                BTLogger.LogSystem($"Inventory: Added {amount} {itemType}. Total: {item.quantity}");
                return;
            }
        }
        
        // 新しいアイテムタイプを追加
        items.Add(new InventoryItem(itemType, amount));
        BTLogger.LogSystem($"Inventory: Added new item type {itemType} x{amount}");
    }
    
    public int GetItemCount(string itemType)
    {
        foreach (var item in items)
        {
            if (item.itemType == itemType)
            {
                return item.quantity;
            }
        }
        return 0;
    }
    
    // インスペクターでアイテムを追加するためのメソッド
    [ContextMenu("Add Health Potion")]
    void AddHealthPotion()
    {
        AddItem("health_potion", 1);
    }
    
    [ContextMenu("Add Bow")]
    void AddBow()
    {
        AddItem("bow", 1);
    }
    
    [ContextMenu("Use Health Potion")]
    void UseHealthPotion()
    {
        UseItem("health_potion", 1);
    }
    
    // UIシステム用のアイテム一覧取得メソッド
    public System.Collections.Generic.Dictionary<string, int> GetAllItems()
    {
        var itemDict = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var item in items)
        {
            if (item.quantity > 0)
            {
                itemDict[item.itemType] = item.quantity;
            }
        }
        return itemDict;
    }
}
}

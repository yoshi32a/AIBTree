namespace BehaviourTree.Components
{
    /// <summary>インベントリアイテムを表すクラス</summary>
    [System.Serializable]
    public class InventoryItem
    {
        public string itemType;
        public int quantity;
        
        public InventoryItem(string type, int qty)
        {
            itemType = type;
            quantity = qty;
        }
    }
}
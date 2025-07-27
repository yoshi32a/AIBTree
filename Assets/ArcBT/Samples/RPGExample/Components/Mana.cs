using UnityEngine;
using ArcBT.Logger;

namespace ArcBT.Samples.RPG.Components
{
    /// <summary>マナシステムを管理するコンポーネント</summary>
    [System.Serializable]
    public class Mana : MonoBehaviour
    {
        [SerializeField] int maxMana = 100;
        [SerializeField] int currentMana;
        [SerializeField] float manaRegenRate = 5f; // 毎秒のマナ回復量
        
        public int MaxMana 
        { 
            get => maxMana; 
            set => maxMana = value; 
        }
        
        public int CurrentMana 
        { 
            get => currentMana; 
            set => currentMana = Mathf.Clamp(value, 0, maxMana); 
        }
        
        void Start()
        {
            currentMana = maxMana;
        }
        
        void Update()
        {
            // マナの自動回復
            if (currentMana < maxMana)
            {
                currentMana = Mathf.Min(maxMana, currentMana + Mathf.RoundToInt(manaRegenRate * Time.deltaTime));
            }
        }
        
        public bool ConsumeMana(int amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                BTLogger.LogSystem($"🔵 マナ消費: -{amount} (残り: {currentMana}/{maxMana})");
                return true;
            }
            return false;
        }
        
        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            BTLogger.LogSystem($"💙 マナ回復: +{amount} (現在: {currentMana}/{maxMana})");
        }
        
        public bool HasEnoughMana(int required)
        {
            return currentMana >= required;
        }
        
        // インスペクターでマナを調整できるようにする
        [ContextMenu("Use 25 Mana")]
        void TestManaUse()
        {
            ConsumeMana(25);
        }
        
        [ContextMenu("Restore 25 Mana")]
        void TestManaRestore()
        {
            RestoreMana(25);
        }
    }
}
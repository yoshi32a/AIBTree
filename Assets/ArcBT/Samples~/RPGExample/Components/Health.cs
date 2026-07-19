using UnityEngine;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Interfaces;

namespace ArcBT.Samples.RPG.Components
{
    /// <summary>体力システムを管理するコンポーネント</summary>
    [System.Serializable]
    public class Health : MonoBehaviour, IHealth
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] int currentHealth;

        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }

        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        // IHealth インターフェースの実装
        float IHealth.CurrentHealth => currentHealth;
        float IHealth.MaxHealth => maxHealth;
        bool IHealth.IsDead => currentHealth <= 0;

        void Start()
        {
            currentHealth = maxHealth;
        }

        // IHealth インターフェースの実装
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - (int)damage);
            BTLogger.LogSystem($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            BTLogger.LogSystem($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
        }

        public bool IsAlive => currentHealth > 0;

        // インスペクターで体力を調整できるようにする
        [ContextMenu("Take 25 Damage")]
        void TestDamage()
        {
            TakeDamage(25);
        }

        [ContextMenu("Heal 25")]
        void TestHeal()
        {
            Heal(25);
        }
    }
}

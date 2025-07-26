using UnityEngine;

namespace ArcBT.Samples.RPG.Components
{
    /// <summary>体力システムを管理するコンポーネント</summary>
    [System.Serializable]
    public class Health : MonoBehaviour
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
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
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

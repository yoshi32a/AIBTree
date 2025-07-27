namespace ArcBT.Samples.RPG.Interfaces
{
    /// <summary>
    /// Healthコンポーネントのインターフェース
    /// リフレクションを避けるために使用
    /// </summary>
    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        void TakeDamage(float damage);
        bool IsDead { get; }
    }
}
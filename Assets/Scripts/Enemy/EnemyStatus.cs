
using UnityEngine;

[System.Serializable]
public class EnemyStatus
{
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    public int Attack { get; private set; }
    public int Defense { get; private set; }

    public EnemyStatus(EnemyData data)
    {
        Initialize(data);
    }

    public void Initialize(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("EnemyData‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB");
            return;
        }

        MaxHP = Mathf.Max(1, data.maxHP);
        CurrentHP = MaxHP;

        Attack = Mathf.Max(0, data.attack);
        Defense = Mathf.Max(0, data.defense);
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    public void ChangeAttack(int amount)
    {
        Attack = Mathf.Max(0, Attack + amount);
    }

    public void ChangeDefense(int amount)
    {
        Defense = Mathf.Max(0, Defense + amount);
    }

    public bool IsDead()
    {
        return CurrentHP <= 0;
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }

    public int CalculateDamage(int playerDefense)
    {
        playerDefense = Mathf.Max(0, playerDefense);

        float damage = Attack * 100f / (100f + 2f * playerDefense);

        return Mathf.FloorToInt(damage);
    }
}

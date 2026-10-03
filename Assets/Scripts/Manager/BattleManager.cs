using UnityEditor;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private BattleEnemySpawner spawner;

    private bool isBattle = false;

    public bool IsBattle()
    {
        return isBattle;
    }

    public void StartBattle()
    {
        isBattle = true;
    }

    public void EndBattle()
    {
        isBattle = false;
    }

    public void CompleteCommand()
    {
        Enemy enemy = spawner.GetBattleEnemy();

        enemy.Damage(1);
    }
}

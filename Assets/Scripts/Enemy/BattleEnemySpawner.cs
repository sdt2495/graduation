using System;
using UnityEngine;

public class BattleEnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Transform battlePoint;
    [SerializeField] private Player player;

    [SerializeField] private HPManager hpManager;

    [SerializeField] private Canvas canvas;

    private Enemy battleEnemy;

    [Header("Debug")]
    [SerializeField] private bool spawnOnStart = false;

    //private void Start()
    //{
    //    if(spawnOnStart)
    //    {
    //        SpawnEnemy();
    //    }
    //}

    public void SpawnEnemy(EnemyData data)
    {
        if(data == null) return;

        battleEnemy = Instantiate(enemyPrefab, battlePoint.position, Quaternion.identity);

        battleEnemy.Initialize(data);

        // ダメージポップアップの表示先を設定
        battleEnemy.SetDamageCanvas(canvas);

        hpManager.SetEnemy(battleEnemy);

        battleEnemy.SetRandomCommands();

        player.SetEnemy(battleEnemy);
    }

    public Enemy GetBattleEnemy()
    {
        return battleEnemy;
    }
}

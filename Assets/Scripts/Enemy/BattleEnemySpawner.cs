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

    [Header("”wŒi")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

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

        // ƒ_ƒ[ƒWƒ|ƒbƒvƒAƒbƒv‚Ì•\¦æ‚ğİ’è
        battleEnemy.SetDamageCanvas(canvas);

        hpManager.SetEnemy(battleEnemy);

        battleEnemy.SetRandomCommands();

        player.SetEnemy(battleEnemy);
    }

    public void SetBackground(EnemyData data)
    {
        if (data == null || backgroundRenderer == null)
        {
            Debug.LogWarning("”wŒi‚Ìİ’è‚É¸”s‚µ‚Ü‚µ‚½B");
            return;
        }

        if (data.backgroundSprite == null)
        {
            Debug.LogWarning("EnemyData‚É”wŒi‰æ‘œ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB");
            return;
        }

        backgroundRenderer.sprite = data.backgroundSprite;
    }

    public Enemy GetBattleEnemy()
    {
        return battleEnemy;
    }
}

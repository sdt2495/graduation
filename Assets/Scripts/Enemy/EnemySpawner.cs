using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Player player;

    [Header("EnemySpawnPoint")]
    [SerializeField] private Transform battlePoint;
    [SerializeField] private Transform nextPoint1;
    [SerializeField] private Transform nextPoint2;

    private Enemy battleEnemy;
    private Enemy nextEnemy1;
    private Enemy nextEnemy2;

    private void Start()
    {
        // Enemyを生成
        battleEnemy = SpawnEnemy(battlePoint);
        nextEnemy1 = SpawnEnemy(nextPoint1);
        nextEnemy2 = SpawnEnemy(nextPoint2);

        UpdateEnemyView();

        // Playerにバトル中のEnemyを渡す
        player.SetEnemy(battleEnemy);
    }

    /// <summary>
    /// 敵のスポーン
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private Enemy SpawnEnemy(Transform point)
    {
        Enemy enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);

        enemy.SetRandomCommands();
        enemy.UpdateCommanedText();

        return enemy;
    }

    private void ShiftEnemies()
    {
        Destroy(battleEnemy.gameObject);

        // Enemyを次のEnemyへ変更
        battleEnemy = nextEnemy1;
        nextEnemy1 = nextEnemy2;
        nextEnemy2 = SpawnEnemy(nextPoint2);

        UpdateEnemyView();

        player.SetEnemy(battleEnemy);
    }

    public void StartSpawn()
    {
        ShiftEnemies();
    }

    /// <summary>
    /// EnemyのPosition　Scale変更
    /// </summary>
    private void UpdateEnemyView()
    {
        // Positionを変更
        battleEnemy.transform.position = battlePoint.position;
        nextEnemy1.transform.position = nextPoint1.position;

        // Scaleを変更
        battleEnemy.transform.localScale = Vector3.one;
        nextEnemy1.transform.localScale = Vector3.one * 0.8f;
        nextEnemy2.transform.localScale = Vector3.one * 0.6f;
    }

    /// <summary>
    /// 演出入れるなら使う
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnAfterDelay()
    {
       yield return new WaitForSeconds(2);
        ShiftEnemies();
    }
}

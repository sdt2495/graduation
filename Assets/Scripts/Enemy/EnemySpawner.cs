using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Player player;

    [Header("EnemySpawnPoint")]
    [SerializeField] private Transform battlePoint;
    [SerializeField] private Transform nextPoint;

    [Header("コマンドUI")]
    [SerializeField] private CommandUI commandUI;

    [Header("Wave")]
    [SerializeField] private WaveUI waveUI;
    [SerializeField] private int maxWave = 10;
    private int currrentWave = 1;

    private Enemy battleEnemy;
    private Enemy nextEnemy;

    private void Start()
    {
        // Enemyを生成
        battleEnemy = SpawnEnemy(battlePoint);
        nextEnemy = SpawnEnemy(nextPoint);

        UpdateEnemyView();

        // Playerにバトル中のEnemyを渡す
        player.SetEnemy(battleEnemy);

        commandUI.UpdateCommanedText(battleEnemy, nextEnemy);

        battleEnemy.GetCurrentIndex();
        waveUI.UpdateWave(currrentWave, maxWave);
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

        return enemy;
    }

    private void ShiftEnemies()
    {
        Destroy(battleEnemy.gameObject);

        // Enemyを次のEnemyへ変更
        battleEnemy = nextEnemy;
        nextEnemy = SpawnEnemy(nextPoint);

        UpdateEnemyView();

        player.SetEnemy(battleEnemy);
        commandUI.UpdateCommanedText(battleEnemy, nextEnemy);

        currrentWave++;
        waveUI.UpdateWave(currrentWave, maxWave);
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
        nextEnemy.transform.position = nextPoint.position;

        // Scaleを変更
        battleEnemy.transform.localScale = Vector3.one;
        nextEnemy.transform.localScale = Vector3.one * 0.8f;
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

using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Player player;

    private void Start()
    {
        StartSpawn();
    }

    public void Spawn()
    {
        Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemyPrefab.correctCommand = (CommandType)Random.Range(0, 4);
        player.SetEnemy(enemy);
    }

    public void StartSpawn()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
       yield return new WaitForSeconds(2);
        Spawn();
    }
}

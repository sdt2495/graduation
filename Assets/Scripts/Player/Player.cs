using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemySpawner spawner;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
           if( enemy.Check(CommandType.Left))
            {
                spawner.StartSpawn();
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
           if( enemy.Check(CommandType.Right))
            {
                spawner.StartSpawn();
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(enemy.Check(CommandType.Up))
            {
                spawner.StartSpawn();
            }   
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
           if (enemy.Check(CommandType.Down))
            {
                spawner.StartSpawn();
            }
        }
    }

    public void SetEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;
    }
}

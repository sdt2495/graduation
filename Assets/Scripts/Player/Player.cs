using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private SpriteRenderer renderer;

    private void Update()
    {
        if (enemy == null)
        {
            return;
        }
        
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckCommaned(CommandType.Left);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckCommaned(CommandType.Right);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            CheckCommaned(CommandType.Up);
        }
        else if( Input.GetKeyDown(KeyCode.DownArrow))
        {
            CheckCommaned(CommandType.Down);
        }
    }

    public void SetEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;
    }

    private void CheckCommaned(CommandType command)
    {
        CheckResult result = enemy.Check(command);

        switch (result)
        {
            case CheckResult.Success:
                // è¨Ç≥Ç¢âÊñ óhÇÍ
                CameraShake.instance.Shake(0.08f, 0.05f);
                break;

            case CheckResult.Complete:
                // ëÂÇ´Ç¢âÊñ óhÇÍ
                CameraShake.instance.Shake(0.18f, 0.2f);
                spawner.StartSpawn();
                break;

            case CheckResult.Miss:
                // ê‘ì_ñ≈
                StartCoroutine(FlashRed());
                break;
        }
    }

    private IEnumerator FlashRed()
    {
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.white;
    }
}

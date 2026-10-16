using UnityEngine;

public class Furemi : MonoBehaviour
{
    public GameObject damageTextPrefab;

    // ダメージ表示位置を指定するオブジェクト
    public Transform damageSpawnPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowDamage(100);
        }
    }

    void ShowDamage(int damage)
    {
        if (damageSpawnPoint == null)
        {
            Debug.LogError("ダメージ表示位置が設定されていません！");
            return;
        }

        GameObject obj = Instantiate(
            damageTextPrefab,
            damageSpawnPoint.position,
            Quaternion.identity
        );

        Damagedesu damageScript = obj.GetComponent<Damagedesu>();

        if (damageScript != null)
        {
            damageScript.SetDamage(damage);
        }
    }
}
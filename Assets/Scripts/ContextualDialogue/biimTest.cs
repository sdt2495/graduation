using UnityEngine;

public class biimTest : MonoBehaviour
{
    [Header("GameMessageManager(メッセージ表示スクリプト)")]
    [SerializeField] GameMessageManager gameMessageManager;

    [Header("キャラデータ(誰が喋るのか)")]
    [SerializeField] private GameMessageCharacter thisCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        // 1：攻撃
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Attack);
        }

        // 2：ダメージ
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Damage);
        }

        // 3：ヒール
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Heal);
        }
    }
}

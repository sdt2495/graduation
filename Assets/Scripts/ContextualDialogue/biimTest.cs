using UnityEngine;

public class biimTest : MonoBehaviour
{
    [Header("biimテスト")]
    [SerializeField] GameMessageManager gameMessageManager;

    [Header("キャラデータ")]
    [SerializeField] private GameMessageCharacter thisCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        // Aキー：攻撃
        if (Input.GetKeyDown(KeyCode.A))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Attack);
        }

        // Dキー：ダメージ
        if (Input.GetKeyDown(KeyCode.D))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Damage);
        }

        // Hキー：ヒール
        if (Input.GetKeyDown(KeyCode.H))
        {
            gameMessageManager.ShowMessage(thisCharacter, GameMessageType.Heal);
        }
    }
}

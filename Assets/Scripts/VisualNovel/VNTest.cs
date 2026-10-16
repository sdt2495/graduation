using UnityEngine;

public class VNTest : MonoBehaviour
{
    [Header("NovelManager")]
    [SerializeField] private NovelManager novelManager;


    private void Start()
    {
        // 会話終了イベントを登録
        novelManager.OnConversationFinished += OnConversationFinished;
    }
    /// <summary>
    /// 会話終了時
    /// </summary>
    private void OnConversationFinished()
    {
        // イベント解除
        novelManager.OnConversationFinished -= OnConversationFinished;

        // バトル開始の処理とか？
        StartBattle();
    }



    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {

            // 会話開始
            novelManager.StartConversation("Sample");
        }
    }


    /// <summary>
    /// バトル開始
    /// </summary>
    private void StartBattle()
    {
        Debug.Log("バトル開始！！！！！！！！！！！！！！！！！！！！");
    }
}
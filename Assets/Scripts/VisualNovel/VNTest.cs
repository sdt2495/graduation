using UnityEngine;

public class VNTest : MonoBehaviour
{
    [Header("CSV名")]
    [SerializeField] private string csvName;

    private void Update()
    {
        // [1]を押すとSample.csvを再生し、終了後にDebug.Logを実行する。
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {


            // 会話を開始
            GameFlowManager.Instance.PlayConversation(csvName, () =>
            {
                // 会話終了後に実行
                Debug.Log("会話が終了しました！");

                // 次の処理
                Debug.Log("次の処理を開始します！");
            });



        }
    }
}
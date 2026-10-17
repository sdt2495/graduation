using UnityEngine;

public class VNTest : MonoBehaviour
{
    [Header("NovelManager")]
    [SerializeField] private NovelManager novelManager;

    [Header("CSV名")]
    [SerializeField] private string csvName;

    private void Update()
    {
        // [1]を押すとSample.csvを再生し、終了後にDebug.Logを実行する。
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            novelManager.StartConversation(csvName, () => { Debug.Log("会話終了！"); });
        }
    }
}
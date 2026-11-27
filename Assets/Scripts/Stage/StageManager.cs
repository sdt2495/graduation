using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ステージ設定")]
    [SerializeField] private StageData[] stages;

    // 現在のステージ番号（0から開始）
    private int currentStageIndex = 0;

    private void Awake()
    {
        // 既にStageManagerが存在する場合は重複を削除
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーン切り替え後も保持
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 現在のステージ情報を取得
    public StageData GetCurrentStageData()
    {
        if (stages == null ||
            currentStageIndex < 0 ||
            currentStageIndex >= stages.Length)
        {
            Debug.LogError("現在のステージ情報がありません。");
            return null;
        }

        return stages[currentStageIndex];
    }

    // 現在のステージの敵データを取得
    public EnemyData GetCurrentEnemyData()
    {
        StageData stage = GetCurrentStageData();

        if (stage == null)
            return null;

        return stage.enemyData;
    }

    // 現在の会話CSV名を取得
    public string GetCurrentDialogue()
    {
        StageData stage = GetCurrentStageData();

        if (stage == null)
            return null;

        return stage.dialogueCsvName;
    }

    // 次のステージが存在するか
    public bool HasNextStage()
    {
        return stages != null &&
               currentStageIndex + 1 < stages.Length;
    }

    // 次のステージへ進む
    public bool NextStage()
    {
        if (!HasNextStage())
        {
            Debug.Log("最終ステージです。");
            return false;
        }

        currentStageIndex++;
        return true;
    }

    // 現在のステージ番号を取得（1始まり）
    public int GetCurrentStageNumber()
    {
        return currentStageIndex + 1;
    }

    // ステージ数を取得
    public int GetStageCount()
    {
        return stages == null ? 0 : stages.Length;
    }

    // 強化シーンから戻ってきたか
    public bool IsReturningFromUpgrade { get; private set; }

    // 強化シーンへ移動する前に呼ぶ
    public void SetReturningFromUpgrade()
    {
        IsReturningFromUpgrade = true;
    }

    // 復帰状態を取得して解除する
    public bool ConsumeReturningFromUpgrade()
    {
        if (!IsReturningFromUpgrade)
            return false;

        IsReturningFromUpgrade = false;
        return true;
    }
}
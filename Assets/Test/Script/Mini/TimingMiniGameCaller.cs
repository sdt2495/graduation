
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ミニゲームPrefabを生成し、開始と結果通知を管理するクラス。
/// </summary>
public class TimingMiniGameCaller : MonoBehaviour
{
    [Header("ミニゲームPrefab")]
    [SerializeField] private GameObject timingMiniGamePrefab;

    [Header("生成する親オブジェクト")]
    [SerializeField] private Transform miniGameParent;

    [Header("結果通知")]
    [SerializeField] private UnityEvent onClear;
    [SerializeField] private UnityEvent onMiss;

    private TimingMiniGame timingMiniGame;
    private bool waitingForResult;
    private bool resultNotified;

    /// <summary>
    /// ミニゲームPrefabを生成して開始する。
    /// </summary>

    /// <summary>
    /// 敵の攻撃時に呼び出され、
    /// ミニゲームPrefabを生成して開始する。
    /// </summary>

    public void StartTimingMiniGame()
    {
        Debug.Log("[TimingMiniGameCaller] ミニゲーム呼び出し");

        // Prefabの設定確認
        if (timingMiniGamePrefab == null)
        {
            Debug.LogError("ミニゲームPrefabが設定されていません。", this);
            return;
        }

        // 二重起動を防止
        if (waitingForResult)
        {
            Debug.LogWarning("ミニゲームはすでに実行中です。", this);
            return;
        }

        // 生成先の確認
        if (miniGameParent == null)
        {
            Debug.LogError("Mini Game Parentが設定されていません。", this);
            return;
        }

        // 呼び出された瞬間にPrefabを生成
        GameObject miniGameObject = Instantiate(
            timingMiniGamePrefab,
            miniGameParent,
            false
        );

        // ★ 生成したミニゲームのActiveをオンにする
        miniGameObject.SetActive(true);

        Debug.Log(
            "[TimingMiniGameCaller] Prefab生成完了・Activeをオンにしました: "
            + miniGameObject.name,
            miniGameObject
        );

        // ミニゲーム本体を取得
        timingMiniGame = miniGameObject.GetComponent<TimingMiniGame>();

        if (timingMiniGame == null)
        {
            Debug.LogError(
                "PrefabのルートにTimingMiniGameがありません。",
                miniGameObject
            );

            Destroy(miniGameObject);
            return;
        }

        // 結果待ち状態を初期化
        waitingForResult = true;
        resultNotified = false;

        // ミニゲームを開始
        timingMiniGame.StartMiniGame();

        Debug.Log("[TimingMiniGameCaller] ミニゲーム開始");
    }


    /// <summary>
    /// ミニゲームの終了を監視する。
    /// </summary>
    private void Update()
    {
        // 結果待ち中でない、または結果通知済みなら何もしない
        if (!waitingForResult || resultNotified)
        {
            return;
        }

        // ミニゲームが破棄されていないか確認
        if (timingMiniGame == null)
        {
            Debug.LogError(
                "[TimingMiniGameCaller] 実行中のミニゲームが見つかりません。",
                this
            );

            waitingForResult = false;
            return;
        }

        // ミニゲームが終了するまで待つ
        if (!timingMiniGame.IsFinished())
        {
            return;
        }

        // 結果の二重通知を防止
        resultNotified = true;
        waitingForResult = false;

        // 成功・失敗を判定して通知
        if (timingMiniGame.IsClear())
        {
            Debug.Log(
                "[TimingMiniGameCaller] ミニゲーム成功！ onClearを実行します。",
                this
            );

            onClear?.Invoke();
        }
        else if (timingMiniGame.IsMiss())
        {
            Debug.LogWarning(
                "[TimingMiniGameCaller] ミニゲーム失敗。onMissを実行します。",
                this
            );

            onMiss?.Invoke();
        }

        // 成功・失敗のどちらでもミニゲームを非アクティブにする
        timingMiniGame.gameObject.SetActive(false);

        Debug.Log(
            "[TimingMiniGameCaller] ミニゲームを非アクティブにしました。",
            this
        );
    }
}
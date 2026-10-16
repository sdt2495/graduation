
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ミニゲームPrefabを生成し、開始と結果通知を管理するクラス。
/// </summary>
public class TimingMiniGameCaller : MonoBehaviour
{
    [Header("ミニゲームPrefab")]
    [SerializeField] private GameObject timingMiniGamePrefab;

    [Header("縦ゲージミニゲーム")]
    [SerializeField] private VerticalGauge verticalGaugePrefab;

    [Header("生成する親オブジェクト")]
    [SerializeField] private Transform miniGameParent;

    [Header("結果通知")]
    [SerializeField] private UnityEvent onClear;
    [SerializeField] private UnityEvent onMiss;

    private TimingMiniGame timingMiniGame;
    private bool waitingForResult;
    private bool resultNotified;

    // 生成された縦ゲージミニゲーム
    private VerticalGauge verticalGaugeInstance;
    // 今回、縦ゲージを選択したかどうか
    private bool isVerticalGaugeGame = false;

    /// <summary>
    /// ミニゲームPrefabを生成して開始する。
    /// </summary>

    /// <summary>
    /// 敵の攻撃時に呼び出され、
    /// ミニゲームPrefabを生成して開始する。
    /// </summary>

    public void StartTimingMiniGame()
    {
        Debug.Log("[TimingMiniGameCaller] ミニゲーム呼び出し", this);

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

        // 2種類ともPrefabが設定されているか確認
        if (timingMiniGamePrefab == null || verticalGaugePrefab == null)
        {
            Debug.LogError(
                "円形ミニゲームと縦ゲージミニゲームのPrefabを両方設定してください。",
                this
            );
            return;
        }

        // 前回の結果をリセット
        timingMiniGame = null;
        verticalGaugeInstance = null;
        resultNotified = false;
        waitingForResult = true;

        // 0なら円形、1なら縦ゲージ
        isVerticalGaugeGame = Random.Range(0, 2) == 1;

        if (isVerticalGaugeGame)
        {
            // 縦ゲージミニゲームを生成
            GameObject miniGameObject = Instantiate(
                verticalGaugePrefab.gameObject,
                miniGameParent,
                false
            );

            miniGameObject.SetActive(true);

            verticalGaugeInstance =
                miniGameObject.GetComponent<VerticalGauge>();

            if (verticalGaugeInstance == null)
            {
                Debug.LogError(
                    "縦ゲージPrefabにVerticalGaugeがありません。",
                    miniGameObject
                );

                Destroy(miniGameObject);
                waitingForResult = false;
                return;
            }

            // 縦ゲージを開始
            verticalGaugeInstance.StartMiniGame();

            Debug.Log("[TimingMiniGameCaller] 縦ゲージを開始", this);
        }
        else
        {
            // 円形ミニゲームを生成
            GameObject miniGameObject = Instantiate(
                timingMiniGamePrefab.gameObject,
                miniGameParent,
                false
            );

            miniGameObject.SetActive(true);

            timingMiniGame =
                miniGameObject.GetComponent<TimingMiniGame>();

            if (timingMiniGame == null)
            {
                Debug.LogError(
                    "円形ミニゲームPrefabにTimingMiniGameがありません。",
                    miniGameObject
                );

                Destroy(miniGameObject);
                waitingForResult = false;
                return;
            }

            // 円形ミニゲームを開始
            timingMiniGame.StartMiniGame();

            Debug.Log("[TimingMiniGameCaller] 円形ミニゲームを開始", this);
        }
    }

    /// <summary>
    /// ミニゲームの終了を監視する。
    /// </summary>
    private void Update()
    {
        // 結果待ち中でない、または通知済みなら何もしない
        if (!waitingForResult || resultNotified)
        {
            return;
        }

        bool isFinished = false;
        bool isClear = false;
        GameObject finishedGameObject = null;

        if (isVerticalGaugeGame)
        {
            // 縦ゲージの存在確認
            if (verticalGaugeInstance == null)
            {
                Debug.LogError(
                    "[TimingMiniGameCaller] 縦ゲージが見つかりません。",
                    this
                );

                waitingForResult = false;
                return;
            }

            // 縦ゲージの結果を取得
            isFinished = verticalGaugeInstance.TryGetResult(
                out isClear
            );

            finishedGameObject = verticalGaugeInstance.gameObject;
        }
        else
        {
            // 円形ミニゲームの存在確認
            if (timingMiniGame == null)
            {
                Debug.LogError(
                    "[TimingMiniGameCaller] 円形ミニゲームが見つかりません。",
                    this
                );

                waitingForResult = false;
                return;
            }

            // 円形ミニゲームの終了を確認
            isFinished = timingMiniGame.IsFinished();

            if (isFinished)
            {
                isClear = timingMiniGame.IsClear();
            }

            finishedGameObject = timingMiniGame.gameObject;
        }

        // まだ終了していなければ待つ
        if (!isFinished)
        {
            return;
        }

        // 結果の二重通知を防止
        resultNotified = true;
        waitingForResult = false;

        // 成功・失敗を通知
        if (isClear)
        {
            Debug.Log("[TimingMiniGameCaller] ミニゲーム成功！", this);
            onClear?.Invoke();
        }
        else
        {
            Debug.LogWarning("[TimingMiniGameCaller] ミニゲーム失敗！", this);
            onMiss?.Invoke();
        }

        // 成功・失敗どちらでも非アクティブにする
        if (finishedGameObject != null)
        {
            finishedGameObject.SetActive(false);
        }

        Debug.Log("[TimingMiniGameCaller] ミニゲームを非アクティブにしました。", this);
    }
}
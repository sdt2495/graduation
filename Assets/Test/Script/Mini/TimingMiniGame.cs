
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 円形のタイミングミニゲームを管理するクラス。
///
/// 【種類1：頂点から3回】
/// ・成功すると針を頂点に戻す
/// ・成功後に少し停止して再開
/// ・3回成功でクリア
///
/// 【種類2：連続で3回】
/// ・針は回転し続ける
/// ・成功するとマークだけをランダム移動
/// ・3回成功でクリア
///
/// 【種類3：成功したら反転】
/// ・成功するたびに針の回転方向を反転
/// ・針は止めずに回転し続ける
/// ・3回成功でクリア
///
/// 【共通仕様】
/// ・円とマークを重ねる
/// ・Spaceキーで判定
/// ・ミスすると針が灰色になって停止
/// </summary>
public class TimingMiniGame : MonoBehaviour
{
    /// <summary>
    /// スキルチェックの種類。
    /// </summary>
    public enum SkillCheckType
    {
        /// <summary>
        /// 円形の頂点から3回挑戦する。
        /// 成功すると針を頂点に戻す。
        /// </summary>
        VertexStartThreeTimes,

        /// <summary>
        /// 針を回し続けながら3回成功する。
        /// </summary>
        ContinuousThreeTimes,

        /// <summary>
        /// 成功するたびに針の回転方向を反転する。
        /// </summary>
        ReverseDirectionThreeTimes
    }

    private enum GameState
    {
        Waiting,
        Playing,
        SuccessPause,
        Clear,
        Miss
    }

    [Header("スキルチェックの種類")]
    [Tooltip("使用するスキルチェックの種類")]
    [SerializeField]
    private SkillCheckType skillCheckType =
        SkillCheckType.VertexStartThreeTimes;

    [Header("画像")]
    [Tooltip("円のベース画像")]
    [SerializeField]
    private RectTransform circle;

    [Tooltip("判定マークの画像")]
    [SerializeField]
    private RectTransform targetMark;

    [Tooltip("回転する針の画像")]
    [SerializeField]
    private RectTransform needle;

    [Header("マーク設定")]
    [Tooltip("マークの角度。20なら360度中20度がマークになる")]
    [SerializeField, Range(1f, 180f)]
    private float markAngle = 20f;

    [Tooltip("マークの位置をランダムにする")]
    [SerializeField]
    private bool randomizeTargetAngle = true;

    [Tooltip("ランダムな開始角度の最小値")]
    [SerializeField, Range(0f, 360f)]
    private float randomAngleMin = 0f;

    [Tooltip("ランダムな開始角度の最大値")]
    [SerializeField, Range(0f, 360f)]
    private float randomAngleMax = 360f;

    [Tooltip("マークの塗りつぶし方向。ONなら時計回り")]
    [SerializeField]
    private bool markClockwise = true;

    [Header("針の設定")]
    [Tooltip("針が回転する速度。1秒あたりの角度")]
    [SerializeField]
    private float needleSpeed = 500f;

    [Tooltip("針画像の向きを補正する角度")]
    [SerializeField]
    private float needleAngleOffset = 0f;

    [Tooltip("最初の回転方向。ONなら時計回り")]
    [SerializeField]
    private bool clockwise = true;

    [Header("判定")]
    [Tooltip("マークの範囲に追加する許容角度")]
    [SerializeField, Range(0f, 30f)]
    private float successAngleRange = 0f;

    [Tooltip("Spaceキーによる入力を有効にする")]
    [SerializeField]
    private bool enableSpaceInput = true;

    [Header("連続成功")]
    [Tooltip("クリアに必要な成功回数")]
    [SerializeField, Min(1)]
    private int requiredSuccessCount = 3;

    [Tooltip("頂点から3回タイプで、成功後に再開するまでの停止時間")]
    [SerializeField, Min(0f)]
    private float successPauseDuration = 0.7f;

    [Header("ミス時の針の色")]
    [Tooltip("ミスしたときに針を灰色にする")]
    [SerializeField]
    private bool changeNeedleColorOnMiss = true;

    [Tooltip("ミスしたときの針の色")]
    [SerializeField]
    private Color missNeedleColor = Color.gray;

    [Header("結果表示")]
    [Tooltip("CLEARを表示するテキスト")]
    [SerializeField]
    private Text clearText;

    [Tooltip("MISSを表示するテキスト")]
    [SerializeField]
    private Text missText;

    [Header("ゲーム開始")]
    [Tooltip("Start時に自動的にゲームを開始する")]
    [SerializeField]
    private bool playOnStart = true;

    // 現在のゲーム状態
    private GameState currentState = GameState.Waiting;

    // 針の現在角度
    private float currentNeedleAngle;

    // マークの開始角度
    private float targetStartAngle;

    // 現在までの成功回数
    private int successCount;

    // 現在の回転方向。trueなら時計回り
    private bool currentClockwise;

    // 針のImageコンポーネント
    private Image needleImage;

    // 針の通常時の色
    private Color originalNeedleColor = Color.white;

    // 成功後の待機処理
    private Coroutine successCoroutine;

    // 円の角度
    private const float FullCircleAngle = 360f;

    // 円の頂点から開始する角度
    private const float StartNeedleAngle = 0f;

    private void Start()
    {
        // 針のImageを取得する
        if (needle != null)
        {
            needleImage = needle.GetComponent<Image>();

            if (needleImage != null)
            {
                originalNeedleColor = needleImage.color;
            }
            else
            {
                Debug.LogWarning(
                    "NeedleにImageコンポーネントがありません。"
                );
            }
        }

        InitializeGame();

        if (playOnStart)
        {
            StartMiniGame();
        }
    }

    private void Update()
    {
        // プレイ中以外は針を動かさない
        if (currentState != GameState.Playing)
        {
            return;
        }

        UpdateNeedle();
        CheckInput();
    }

    /// <summary>
    /// ゲームを初期化する。
    /// </summary>
    private void InitializeGame()
    {
        currentState = GameState.Waiting;

        currentNeedleAngle = StartNeedleAngle;
        targetStartAngle = 0f;
        successCount = 0;

        // Inspectorで指定された最初の方向にする
        currentClockwise = clockwise;

        HideResultText();
        RestoreNeedleColor();

        AlignTargetMarkToCircle();

        UpdateNeedleTransform();
        UpdateTargetMark();
    }

    /// <summary>
    /// ミニゲームを最初から開始する。
    /// UIボタンなどから呼び出せる。
    /// </summary>
    public void StartMiniGame()
    {
        // 残っている待機処理を停止する
        if (successCoroutine != null)
        {
            StopCoroutine(successCoroutine);
            successCoroutine = null;
        }

        successCount = 0;

        // 毎回、最初の回転方向に戻す
        currentClockwise = clockwise;

        HideResultText();
        RestoreNeedleColor();

        // 最初の挑戦を開始する
        StartNextRound();
    }

    /// <summary>
    /// 次の挑戦を開始する。
    /// </summary>
    private void StartNextRound()
    {
        currentState = GameState.Playing;

        // 針を円の頂点に戻す
        currentNeedleAngle = StartNeedleAngle;

        HideResultText();
        RestoreNeedleColor();

        // 新しいマークを設定する
        SetTargetAngle();

        // 針の表示を更新する
        UpdateNeedleTransform();
    }

    /// <summary>
    /// マークの開始角度を決定する。
    /// </summary>
    private void SetTargetAngle()
    {
        if (randomizeTargetAngle)
        {
            float minAngle = Mathf.Min(
                randomAngleMin,
                randomAngleMax
            );

            float maxAngle = Mathf.Max(
                randomAngleMin,
                randomAngleMax
            );

            targetStartAngle = Random.Range(
                minAngle,
                maxAngle
            );
        }

        targetStartAngle = NormalizeAngle(targetStartAngle);

        UpdateTargetMark();
    }

    /// <summary>
    /// 円とマークを同じ位置・大きさにする。
    /// </summary>
    private void AlignTargetMarkToCircle()
    {
        if (circle == null || targetMark == null)
        {
            return;
        }

        targetMark.anchoredPosition = circle.anchoredPosition;
        targetMark.localScale = circle.localScale;
        targetMark.sizeDelta = circle.sizeDelta;
    }

    /// <summary>
    /// マークの表示を更新する。
    /// </summary>
    private void UpdateTargetMark()
    {
        if (targetMark == null)
        {
            return;
        }

        AlignTargetMarkToCircle();

        Image targetImage = targetMark.GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.LogWarning(
                "Target MarkにImageコンポーネントがありません。"
            );

            return;
        }

        // 円の360度のうち指定した角度だけ表示する
        targetImage.type = Image.Type.Filled;
        targetImage.fillMethod = Image.FillMethod.Radial360;
        targetImage.fillAmount = markAngle / FullCircleAngle;

        // マークの塗りつぶし方向
        targetImage.fillClockwise = markClockwise;

        // マークの開始位置
        targetMark.localRotation = Quaternion.Euler(
            0f,
            0f,
            targetStartAngle
        );
    }

    /// <summary>
    /// 針を回転させる。
    /// </summary>
    private void UpdateNeedle()
    {
        // 時計回りはZ軸のマイナス方向
        float direction = currentClockwise ? -1f : 1f;

        currentNeedleAngle +=
            needleSpeed *
            direction *
            Time.deltaTime;

        currentNeedleAngle = NormalizeAngle(currentNeedleAngle);

        UpdateNeedleTransform();
    }

    /// <summary>
    /// 針のTransformを更新する。
    /// </summary>
    private void UpdateNeedleTransform()
    {
        if (needle == null)
        {
            return;
        }

        float rotationAngle =
            currentNeedleAngle +
            needleAngleOffset;

        needle.localRotation = Quaternion.Euler(
            0f,
            0f,
            rotationAngle
        );
    }

    /// <summary>
    /// Spaceキー入力を確認する。
    /// </summary>
    private void CheckInput()
    {
        if (!enableSpaceInput)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Judge();
        }
    }

    /// <summary>
    /// 針がマークの範囲内にいるか判定する。
    /// </summary>
    private void Judge()
    {
        float angleFromStart;

        if (markClockwise)
        {
            angleFromStart = GetClockwiseAngle(
                targetStartAngle,
                currentNeedleAngle
            );
        }
        else
        {
            angleFromStart = GetCounterClockwiseAngle(
                targetStartAngle,
                currentNeedleAngle
            );
        }

        // マークの範囲と許容角度を合わせて判定する
        float allowedAngle = markAngle + successAngleRange;

        if (angleFromStart <= allowedAngle)
        {
            Clear();
        }
        else
        {
            Miss();
        }
    }

    /// <summary>
    /// 時計回り方向の角度差を取得する。
    /// </summary>
    private float GetClockwiseAngle(
        float startAngle,
        float currentAngle)
    {
        float difference = startAngle - currentAngle;

        if (difference < 0f)
        {
            difference += FullCircleAngle;
        }

        return difference;
    }

    /// <summary>
    /// 反時計回り方向の角度差を取得する。
    /// </summary>
    private float GetCounterClockwiseAngle(
        float startAngle,
        float currentAngle)
    {
        float difference = currentAngle - startAngle;

        if (difference < 0f)
        {
            difference += FullCircleAngle;
        }

        return difference;
    }

    /// <summary>
    /// 成功時の処理。
    /// 種類ごとに次の動きを決定する。
    /// </summary>
    private void Clear()
    {
        successCount++;

        Debug.Log(
            "タイミングゲーム成功！ " +
            successCount +
            " / " +
            requiredSuccessCount
        );

        // 必要回数に達したらクリアする
        if (successCount >= requiredSuccessCount)
        {
            FinishClear();
            return;
        }

        // 成功表示を更新する
        if (clearText != null)
        {
            clearText.text =
                "CLEAR " +
                successCount +
                " / " +
                requiredSuccessCount;

            clearText.gameObject.SetActive(true);
        }

        if (missText != null)
        {
            missText.gameObject.SetActive(false);
        }

        // 成功後の動作を種類ごとに切り替える
        switch (skillCheckType)
        {
            case SkillCheckType.VertexStartThreeTimes:
                // 種類1：
                // 一度停止して、次の挑戦で針を頂点へ戻す
                currentState = GameState.SuccessPause;

                successCoroutine = StartCoroutine(
                    WaitAndStartNextRound()
                );
                break;

            case SkillCheckType.ContinuousThreeTimes:
                // 種類2：
                // 針を止めずにマークだけ移動する
                SetTargetAngle();
                break;

            case SkillCheckType.ReverseDirectionThreeTimes:
                // 種類3：
                // 針を止めずに回転方向を反転する
                currentClockwise = !currentClockwise;

                // マークも次の位置に移動する
                SetTargetAngle();
                break;
        }
    }

    /// <summary>
    /// 成功後に待機して次の挑戦を開始する。
    /// 種類1でのみ使用する。
    /// </summary>
    private IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(successPauseDuration);

        successCoroutine = null;

        // 次の挑戦を頂点から開始する
        StartNextRound();
    }

    /// <summary>
    /// 完全クリア時の処理。
    /// </summary>
    private void FinishClear()
    {
        currentState = GameState.Clear;

        if (clearText != null)
        {
            clearText.text = "CLEAR";
            clearText.gameObject.SetActive(true);
        }

        if (missText != null)
        {
            missText.gameObject.SetActive(false);
        }

        Debug.Log("タイミングゲーム完全クリア！");
    }

    /// <summary>
    /// MISS処理。
    /// 動く線を灰色にして停止する。
    /// </summary>
    private void Miss()
    {
        currentState = GameState.Miss;

        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }

        if (missText != null)
        {
            missText.gameObject.SetActive(true);
        }

        // 設定が有効なら針を灰色にする
        if (changeNeedleColorOnMiss && needleImage != null)
        {
            needleImage.color = missNeedleColor;
        }

        Debug.Log("タイミングゲーム MISS！");
    }

    /// <summary>
    /// 針の色を通常時の色に戻す。
    /// </summary>
    private void RestoreNeedleColor()
    {
        if (needleImage != null)
        {
            needleImage.color = originalNeedleColor;
        }
    }

    /// <summary>
    /// 角度を0～360度に収める。
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle %= FullCircleAngle;

        if (angle < 0f)
        {
            angle += FullCircleAngle;
        }

        return angle;
    }

    /// <summary>
    /// 結果表示を消す。
    /// </summary>
    private void HideResultText()
    {
        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }

        if (missText != null)
        {
            missText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ゲームが終了しているか取得する。
    /// </summary>
    public bool IsFinished()
    {
        return
            currentState == GameState.Clear ||
            currentState == GameState.Miss;
    }

    /// <summary>
    /// クリアしているか取得する。
    /// </summary>
    public bool IsClear()
    {
        return currentState == GameState.Clear;
    }

    /// <summary>
    /// ミスしているか取得する。
    /// </summary>
    public bool IsMiss()
    {
        return currentState == GameState.Miss;
    }

    /// <summary>
    /// 現在の成功回数を取得する。
    /// </summary>
    public int GetSuccessCount()
    {
        return successCount;
    }

    /// <summary>
    /// 針の現在角度を取得する。
    /// </summary>
    public float GetNeedleAngle()
    {
        return currentNeedleAngle;
    }

    /// <summary>
    /// マークの開始角度を取得する。
    /// </summary>
    public float GetTargetAngle()
    {
        return targetStartAngle;
    }
}
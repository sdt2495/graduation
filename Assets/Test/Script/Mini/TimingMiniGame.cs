using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 円形のタイミングミニゲームを管理するクラス。
///
/// ・円とマークは同じ位置・同じ大きさで重ねる
/// ・円の360度の中から指定した角度分だけマークを表示
/// ・マークの開始位置はランダム
/// ・針がマークの範囲に入ったときSpaceキーでCLEAR
/// </summary>
public class TimingMiniGame : MonoBehaviour
{
    private enum GameState
    {
        Waiting,
        Playing,
        Clear,
        Miss
    }

    [Header("画像")]
    [Tooltip("円のベース画像")]
    [SerializeField] private RectTransform circle;

    [Tooltip("判定マークの画像")]
    [SerializeField] private RectTransform targetMark;

    [Tooltip("回転する針の画像")]
    [SerializeField] private RectTransform needle;

    [Header("マーク設定")]
    [Tooltip("マークの角度。20なら360度中20度がマークになる")]
    [SerializeField, Range(1f, 180f)]
    private float markAngle = 20f;

    [Tooltip("ゲーム開始時にマークの位置をランダムにする")]
    [SerializeField]
    private bool randomizeTargetAngle = true;

    [Tooltip("ランダムな開始角度の最小値")]
    [SerializeField, Range(0f, 360f)]
    private float randomAngleMin = 0f;

    [Tooltip("ランダムな開始角度の最大値")]
    [SerializeField, Range(0f, 360f)]
    private float randomAngleMax = 360f;

    [Tooltip("マークの回転方向。ONなら時計回り")]
    [SerializeField]
    private bool markClockwise = true;

    [Header("針の設定")]
    [Tooltip("針が回転する速度。1秒あたりの角度")]
    [SerializeField]
    private float needleSpeed = 500f;

    [Tooltip("針画像の向きを補正する角度")]
    [SerializeField]
    private float needleAngleOffset = 0f;

    [Tooltip("針の回転方向。ONなら時計回り")]
    [SerializeField]
    private bool clockwise = true;

    [Header("判定")]
    [Tooltip("マークの端から何度まで許容するか")]
    [SerializeField, Range(0f, 30f)]
    private float successAngleRange = 0f;

    [Tooltip("Spaceキーを押したときに判定する")]
    [SerializeField]
    private bool enableSpaceInput = true;

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

    private GameState currentState = GameState.Waiting;

    // 針の現在角度
    private float currentNeedleAngle;

    // マークの開始角度
    private float targetStartAngle;

    private const float FullCircleAngle = 360f;

    private void Start()
    {
        InitializeGame();

        if (playOnStart)
        {
            StartMiniGame();
        }
    }

    private void Update()
    {
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

        currentNeedleAngle = 0f;
        targetStartAngle = 0f;

        HideResultText();

        // 円とマークを完全に同じ位置にする
        AlignTargetMarkToCircle();

        UpdateNeedleTransform();
        UpdateTargetMark();
    }

    /// <summary>
    /// ミニゲームを開始する。
    /// </summary>
    public void StartMiniGame()
    {
        currentState = GameState.Playing;

        // 針を最初の位置に戻す
        currentNeedleAngle = 0f;

        HideResultText();

        SetTargetAngle();

        UpdateNeedleTransform();
    }

    /// <summary>
    /// マークの開始角度を決定する。
    /// </summary>
    private void SetTargetAngle()
    {
        if (randomizeTargetAngle)
        {
            targetStartAngle = Random.Range(
                randomAngleMin,
                randomAngleMax
            );
        }

        targetStartAngle = NormalizeAngle(targetStartAngle);

        UpdateTargetMark();
    }

    /// <summary>
    /// 円とマークを完全に重ねる。
    /// </summary>
    private void AlignTargetMarkToCircle()
    {
        if (circle == null || targetMark == null)
        {
            return;
        }

        targetMark.anchoredPosition = circle.anchoredPosition;

        targetMark.localScale = circle.localScale;
    }

    /// <summary>
    /// マークを更新する。
    /// </summary>
    private void UpdateTargetMark()
    {
        if (targetMark == null)
        {
            return;
        }

        // 円とマークを同じ位置にする
        AlignTargetMarkToCircle();

        Image targetImage = targetMark.GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.LogWarning(
                "Target MarkにImageコンポーネントがありません。"
            );

            return;
        }

        // 円の360度のうち、指定した角度だけ表示する
        targetImage.type = Image.Type.Filled;
        targetImage.fillMethod = Image.FillMethod.Radial360;

        // マークの大きさ
        targetImage.fillAmount = markAngle / FullCircleAngle;

        // マークの回転方向
        targetImage.fillClockwise = markClockwise;

        // マークの開始位置
        targetMark.localRotation =
            Quaternion.Euler(
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
        float direction;

        if (clockwise)
        {
            direction = -1f;
        }
        else
        {
            direction = 1f;
        }

        currentNeedleAngle +=
            needleSpeed *
            direction *
            Time.deltaTime;

        currentNeedleAngle =
            NormalizeAngle(currentNeedleAngle);

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

        needle.localRotation =
            Quaternion.Euler(
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
            // 時計回りの場合
            angleFromStart =
                GetClockwiseAngle(
                    targetStartAngle,
                    currentNeedleAngle
                );
        }
        else
        {
            // 反時計回りの場合
            angleFromStart =
                GetCounterClockwiseAngle(
                    targetStartAngle,
                    currentNeedleAngle
                );
        }

        // マークの範囲内か確認
        float allowedAngle =
            markAngle + successAngleRange;

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
        float difference =
            startAngle - currentAngle;

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
        float difference =
            currentAngle - startAngle;

        if (difference < 0f)
        {
            difference += FullCircleAngle;
        }

        return difference;
    }

    /// <summary>
    /// CLEAR処理。
    /// </summary>
    private void Clear()
    {
        currentState = GameState.Clear;

        if (clearText != null)
        {
            clearText.gameObject.SetActive(true);
        }

        if (missText != null)
        {
            missText.gameObject.SetActive(false);
        }

        Debug.Log("タイミングゲーム CLEAR！");
    }

    /// <summary>
    /// MISS処理。
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

        Debug.Log("タイミングゲーム MISS！");
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
    /// ゲームが終了しているか。
    /// </summary>
    public bool IsFinished()
    {
        return
            currentState == GameState.Clear ||
            currentState == GameState.Miss;
    }

    /// <summary>
    /// CLEARしているか。
    /// </summary>
    public bool IsClear()
    {
        return currentState == GameState.Clear;
    }

    /// <summary>
    /// MISSしているか。
    /// </summary>
    public bool IsMiss()
    {
        return currentState == GameState.Miss;
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
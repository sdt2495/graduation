
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// 4方向の回避ミニゲーム。
///
/// ・通常攻撃：3拍
/// ・広範囲攻撃：3拍
/// ・フェイント攻撃：4拍
/// ・連続攻撃：通常速度3拍＋高速3拍
///
/// 赤いターゲットが表示されている方向は危険。
/// 危険方向以外を選択すると回避成功。
///
/// カメラは既存のBattleCameraControllerを呼び出すだけ。
/// カメラ側の処理は変更しない。
/// </summary>
public class DodgeMiniGame : MonoBehaviour
{
    public enum AttackPattern
    {
        Normal,
        Wide,
        Feint,
        Chain
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("基本設定")]

    [SerializeField]
    private GameObject miniGamePanel;

    [SerializeField]
    private AttackPattern attackPattern = AttackPattern.Normal;

    [SerializeField]
    private bool startOnEnable = false;

    [Header("攻撃設定")]

    [SerializeField, Min(1)]
    private int requiredBeats = 3;

    [SerializeField, Min(0.1f)]
    private float beatInterval = 0.74f;

    [SerializeField, Min(0.1f)]
    private float chainBeatInterval = 0.4f;

    [SerializeField, Min(0f)]
    private float chainGap = 0.31f;

    [SerializeField]
    private int normalDamage = 18;

    [SerializeField]
    private int wideDamage = 22;

    [Header("方向矢印")]

    [SerializeField]
    private Image upArrow;

    [SerializeField]
    private Image downArrow;

    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [Header("危険ターゲット（赤）")]

    [Tooltip("Up / Down / Left / Right の順番で登録")]
    [SerializeField]
    private Image[] dangerTargetImages = new Image[4];

    [Header("フェイントターゲット（橙）")]

    [Tooltip("Up / Down / Left / Right の順番で登録")]
    [SerializeField]
    private Image[] feintTargetImages = new Image[4];

    [Header("広範囲ターゲット")]

    [Tooltip("必要な場合だけ設定。未設定なら個別ターゲットを使用")]
    [SerializeField]
    private Image wideTargetImage;

    [Header("表示テキスト")]

    [SerializeField]
    private TMP_Text beatText;

    [SerializeField]
    private TMP_Text resultText;

    [SerializeField]
    private TMP_Text instructionText;

    [Header("タイマー")]

    [SerializeField]
    private Image timerGauge;

    [SerializeField, Min(0.1f)]
    private float inputTimeLimit = 1.25f;

    [Header("カメラ")]

    [SerializeField]
    private BattleCameraController battleCameraController;

    [Header("イベント")]

    [SerializeField]
    private UnityEvent onDodgeSuccess;

    [SerializeField]
    private UnityEvent onDodgeFailure;

    private readonly bool[] dangerDirections = new bool[4];
    private readonly bool[] fakeDirections = new bool[4];

    private Direction selectedDirection = Direction.Up;

    private bool hasSelectedDirection;
    private bool isRunning;
    private bool isFinished;

    /// <summary>
    /// ミニゲームがプレイ中かどうかを取得する。
    /// </summary>
    public bool IsPlaying
    {
        get { return isRunning && !isFinished; }
    }

    private Coroutine miniGameCoroutine;

    private float remainingTime;

    private const int DirectionCount = 4;
    private const int NormalBeatCount = 3;
    private const int FeintBeatCount = 4;
    private const int ChainSeriesCount = 2;

    private void Start()
    {
        HideAllTargets();

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }

        if (startOnEnable)
        {
            StartDodgeMiniGame();
        }
    }

    private void Update()
    {
        if (!isRunning || isFinished)
        {
            return;
        }

        CheckDirectionInput();

        if (hasSelectedDirection)
        {
            UpdateTimer();
        }
    }

    /// <summary>
    /// 外部のBattleManagerなどから呼び出す開始関数。
    /// </summary>
    public void StartDodgeMiniGame()
    {
        if (isRunning)
        {
            return;
        }

        if (miniGameCoroutine != null)
        {
            StopCoroutine(miniGameCoroutine);
        }

        ResetMiniGame();

        isRunning = true;

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(true);
        }

        // 左方向への滑らかなズームを開始
        if (battleCameraController != null)
        {
            battleCameraController.StartZoom();
        }

        miniGameCoroutine = StartCoroutine(PlayAttack());
    }
    /// <summary>
    /// 攻撃パターンを指定して開始する。
    /// </summary>
    public void StartDodgeMiniGame(int patternIndex)
    {
        if (patternIndex < 0 ||
            patternIndex > (int)AttackPattern.Chain)
        {
            Debug.LogWarning("DodgeMiniGame：攻撃パターンの指定が不正です。", this);
            return;
        }

        attackPattern = (AttackPattern)patternIndex;
        StartDodgeMiniGame();
    }

    private void ResetMiniGame()
    {
        isFinished = false;
        hasSelectedDirection = false;

        remainingTime = inputTimeLimit;

        selectedDirection = Direction.Up;

        HideAllTargets();

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (timerGauge != null)
        {
            timerGauge.fillAmount = 1f;
        }

        UpdateSelectedArrow();
    }

    private IEnumerator PlayAttack()
    {
        if (attackPattern == AttackPattern.Chain)
        {
            yield return PlayBeatSeries(0, beatInterval);

            if (isFinished)
            {
                yield break;
            }

            if (beatText != null)
            {
                beatText.text = "連続攻撃！ 次の攻撃に備えろ！";
            }

            HideAllTargets();

            yield return new WaitForSeconds(chainGap);

            if (isFinished)
            {
                yield break;
            }

            hasSelectedDirection = false;
            remainingTime = inputTimeLimit;

            yield return PlayBeatSeries(1, chainBeatInterval);
        }
        else
        {
            yield return PlayBeatSeries(0, beatInterval);
        }

        if (!isFinished)
        {
            JudgeDodge();
        }
    }

    /// <summary>
    /// 1回分の攻撃拍を進める。
    /// </summary>
    private IEnumerator PlayBeatSeries(int seriesIndex, float interval)
    {
        int beatCount = attackPattern == AttackPattern.Feint
            ? FeintBeatCount
            : NormalBeatCount;

        SetupDangerDirections(seriesIndex);

        for (int beat = 0; beat < beatCount; beat++)
        {
            if (isFinished)
            {
                yield break;
            }

            ShowBeatTargets(beat);

            if (beatText != null)
            {
                beatText.text =
                    "BEAT " + (beat + 1) + " / " + beatCount;
            }

            float elapsed = 0f;

            while (elapsed < interval)
            {
                if (isFinished)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 攻撃パターンごとに危険方向を決める。
    /// </summary>
    private void SetupDangerDirections(int seriesIndex)
    {
        ClearDirectionArrays();

        switch (attackPattern)
        {
            case AttackPattern.Normal:
                SetRandomDangerDirections(2);
                break;

            case AttackPattern.Wide:
                SetWideDangerDirections();
                break;

            case AttackPattern.Feint:
                SetRandomDirections(fakeDirections, 2);
                SetRandomDangerDirections(2);
                break;

            case AttackPattern.Chain:
                SetRandomDangerDirections(seriesIndex == 0 ? 2 : 3);
                break;
        }
    }

    private void ClearDirectionArrays()
    {
        for (int i = 0; i < DirectionCount; i++)
        {
            dangerDirections[i] = false;
            fakeDirections[i] = false;
        }
    }

    private void SetRandomDangerDirections(int count)
    {
        SetRandomDirections(dangerDirections, count);
    }

    private void SetRandomDirections(bool[] destination, int count)
    {
        int selectedCount = 0;

        while (selectedCount < count)
        {
            int index = Random.Range(0, DirectionCount);

            if (destination[index])
            {
                continue;
            }

            destination[index] = true;
            selectedCount++;
        }
    }

    /// <summary>
    /// 広範囲攻撃は3方向、または反対側の2方向を危険にする。
    /// </summary>
    private void SetWideDangerDirections()
    {
        if (Random.Range(0, 2) == 0)
        {
            int startIndex = Random.Range(0, DirectionCount);

            for (int i = 0; i < 3; i++)
            {
                int index = (startIndex + i) % DirectionCount;
                dangerDirections[index] = true;
            }
        }
        else
        {
            int startIndex = Random.Range(0, 2);

            dangerDirections[startIndex] = true;
            dangerDirections[startIndex + 2] = true;
        }
    }

    /// <summary>
    /// 拍数に合わせてターゲットを表示する。
    /// </summary>
    private void ShowBeatTargets(int beat)
    {
        HideAllTargets();

        if (attackPattern == AttackPattern.Feint)
        {
            if (beat < 2)
            {
                // 最初の2拍はフェイントの橙色マーク。
                ShowDirectionTargets(fakeDirections, feintTargetImages);
            }
            else
            {
                // 3拍目から本物の危険方向に切り替える。
                ShowDirectionTargets(dangerDirections, dangerTargetImages);
            }

            return;
        }

        if (beat == 0)
        {
            // 最初の拍は危険方向を1つだけ表示。
            ShowFirstDangerTarget();
        }
        else
        {
            // 2拍目以降は危険方向をすべて表示。
            ShowDirectionTargets(dangerDirections, dangerTargetImages);
        }
    }

    private void ShowFirstDangerTarget()
    {
        for (int i = 0; i < DirectionCount; i++)
        {
            if (dangerDirections[i])
            {
                SetTargetVisible(dangerTargetImages, i, true);
                break;
            }
        }
    }

    private void ShowDirectionTargets(bool[] directions, Image[] images)
    {
        for (int i = 0; i < DirectionCount; i++)
        {
            if (directions[i])
            {
                SetTargetVisible(images, i, true);
            }
        }

        if (attackPattern == AttackPattern.Wide &&
            wideTargetImage != null)
        {
            wideTargetImage.gameObject.SetActive(true);
        }
    }

    private void SetTargetVisible(Image[] images, int index, bool visible)
    {
        if (images == null ||
            index < 0 ||
            index >= images.Length ||
            images[index] == null)
        {
            return;
        }

        images[index].gameObject.SetActive(visible);
    }

    private void HideAllTargets()
    {
        SetAllImagesInactive(dangerTargetImages);
        SetAllImagesInactive(feintTargetImages);

        if (wideTargetImage != null)
        {
            wideTargetImage.gameObject.SetActive(false);
        }
    }

    private void SetAllImagesInactive(Image[] images)
    {
        if (images == null)
        {
            return;
        }

        foreach (Image image in images)
        {
            if (image != null)
            {
                image.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// キーボードの方向入力を確認する。
    /// </summary>
    private void CheckDirectionInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            SelectDirection(Direction.Up);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
                 Keyboard.current.sKey.wasPressedThisFrame)
        {
            SelectDirection(Direction.Down);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                 Keyboard.current.aKey.wasPressedThisFrame)
        {
            SelectDirection(Direction.Left);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                 Keyboard.current.dKey.wasPressedThisFrame)
        {
            SelectDirection(Direction.Right);
        }
    }

    /// <summary>
    /// UIボタンからも呼び出せる方向選択関数。
    /// </summary>
    public void SelectUp()
    {
        SelectDirection(Direction.Up);
    }

    public void SelectDown()
    {
        SelectDirection(Direction.Down);
    }

    public void SelectLeft()
    {
        SelectDirection(Direction.Left);
    }

    public void SelectRight()
    {
        SelectDirection(Direction.Right);
    }

    private void SelectDirection(Direction direction)
    {
        if (!isRunning || isFinished)
        {
            return;
        }

        selectedDirection = direction;
        hasSelectedDirection = true;

        remainingTime = inputTimeLimit;

        UpdateSelectedArrow();
    }

    private void UpdateSelectedArrow()
    {
        SetArrowSelected(upArrow, selectedDirection == Direction.Up);
        SetArrowSelected(downArrow, selectedDirection == Direction.Down);
        SetArrowSelected(leftArrow, selectedDirection == Direction.Left);
        SetArrowSelected(rightArrow, selectedDirection == Direction.Right);
    }

    private void SetArrowSelected(Image arrow, bool selected)
    {
        if (arrow == null)
        {
            return;
        }

        // 矢印画像の色は変更せず、選択状態を拡大率で表現する。
        arrow.transform.localScale = selected
            ? Vector3.one * 1.15f
            : Vector3.one;
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (timerGauge != null)
        {
            timerGauge.fillAmount =
                Mathf.Clamp01(remainingTime / inputTimeLimit);
        }

        if (remainingTime <= 0f)
        {
            JudgeDodge();
        }
    }

    /// <summary>
    /// 最終的な回避判定。
    /// </summary>
    private void JudgeDodge()
    {
        if (isFinished)
        {
            return;
        }

        bool success = false;

        if (hasSelectedDirection)
        {
            int index = (int)selectedDirection;
            success = !dangerDirections[index];
        }

        if (success)
        {
            FinishMiniGame(true);
        }
        else
        {
            FinishMiniGame(false);
        }
    }

    private void FinishMiniGame(bool success)
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        isRunning = false;

        HideAllTargets();

        if (resultText != null)
        {
            resultText.text = success ? "DODGE!" : "HIT!";
        }

        if (instructionText != null)
        {
            instructionText.text = success
                ? "回避成功！"
                : "回避失敗！";
        }

        if (success)
        {
            onDodgeSuccess?.Invoke();
        }
        else
        {
            onDodgeFailure?.Invoke();
        }

        StartCoroutine(CloseMiniGame());
    }

    private IEnumerator CloseMiniGame()
    {
        yield return new WaitForSeconds(0.8f);

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }

        if (battleCameraController != null)
        {
            battleCameraController.EndZoom();
        }

        miniGameCoroutine = null;
    }

    /// <summary>
    /// 外部から強制終了したい場合に使用。
    /// </summary>
    public void CancelMiniGame()
    {
        if (miniGameCoroutine != null)
        {
            StopCoroutine(miniGameCoroutine);
            miniGameCoroutine = null;
        }

        isRunning = false;
        isFinished = true;

        HideAllTargets();

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }

        if (battleCameraController != null)
        {
            battleCameraController.EndZoom();
        }
    }
}

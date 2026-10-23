
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// 右側から左へ並んだ矢印を、方向キーで順番に入力するミニゲーム。
///
/// ・開始時に矢印をランダム生成して表示する
/// ・現在入力する矢印だけを拡大表示する
/// ・正解入力ごとに矢印の列を左へ1つ分スライドする
/// ・矢印1個ごとに入力制限時間を設定する
/// ・間違った入力、または制限時間切れで失敗する
/// ・全入力成功／失敗を UnityEvent で外部へ通知する
/// ・外部から StartArrowMiniGame() で開始できる
/// </summary>
public class ArrowInputMiniGame1 : MonoBehaviour
{
    private enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("矢印画像")]
    [SerializeField] private Sprite upArrowSprite;
    [SerializeField] private Sprite downArrowSprite;
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;

    [Header("表示設定")]
    [Tooltip("矢印を並べる親オブジェクト")]
    [SerializeField] private RectTransform arrowContainer;

    [Tooltip("矢印1個分のPrefab。未設定なら実行時に生成")]
    [SerializeField] private Image arrowImagePrefab;

    [Tooltip("矢印の個数")]
    [Min(1)]
    [SerializeField] private int arrowCount = 5;

    [Tooltip("矢印1個の基本サイズ")]
    [Min(1f)]
    [SerializeField] private float arrowSize = 80f;

    [Tooltip("隣り合う矢印の中心間隔")]
    [Min(1f)]
    [SerializeField] private float arrowSpacing = 110f;

    [Tooltip("入力対象の矢印に適用する拡大倍率")]
    [Min(1f)]
    [SerializeField] private float activeArrowScale = 1.35f;

    [Tooltip("正解時に列が左へ移動するアニメーション時間")]
    [Min(0f)]
    [SerializeField] private float slideDuration = 0.18f;

    [Tooltip("矢印列の開始位置")]
    [SerializeField] private float startCenterX = 0f;

    [Header("入力・時間設定")]
    [Tooltip("1個の矢印を入力する制限時間（秒）")]
    [Min(0.01f)]
    [SerializeField] private float inputTimeLimit = 1.25f;

    [Tooltip("開始時にミニゲームを自動開始するか")]
    [SerializeField] private bool startAutomatically = false;

    [Header("外部通知")]
    [Tooltip("全矢印の入力に成功したときに呼ばれます")]
    [SerializeField] private UnityEvent onSuccess = new UnityEvent();

    [Tooltip("入力ミスまたは時間切れで失敗したときに呼ばれます")]
    [SerializeField] private UnityEvent onFailure = new UnityEvent();

    [Header("デバッグ")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ArrowDirection> arrowSequence =
        new List<ArrowDirection>();

    private readonly List<RectTransform> arrowTransforms =
        new List<RectTransform>();

    private readonly List<Image> arrowImages =
        new List<Image>();

    private Coroutine slideCoroutine;

    private int currentArrowIndex;
    private float remainingInputTime;

    private bool isRunning;
    private bool isFinished;
    private bool isSliding;

    // 今回のミニゲームが成功して終了したかどうか。
    private bool isClear;

    /// <summary>ミニゲームが入力受付中か。</summary>
    public bool IsRunning => isRunning;

    /// <summary>ミニゲームが成功または失敗で終了したか。</summary>
    public bool IsFinished => isFinished;

    /// <summary>ミニゲームがクリアされたか。</summary>
    public bool IsClear => isFinished && isClear;

    /// <summary>現在入力している矢印の0始まりの番号。</summary>
    public int CurrentArrowIndex => currentArrowIndex;

    private void Awake()
    {
        if (!ValidateSettings())
        {
            enabled = false;
            return;
        }

        CreateArrowObjects();

        // 矢印の準備ができるまでは矢印だけ非表示にする。
        arrowContainer.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (startAutomatically)
        {
            StartArrowMiniGame();
        }
    }

    private void Update()
    {
        if (!isRunning || isFinished || isSliding)
        {
            return;
        }

        // 現在の矢印に対する制限時間を減らす。
        remainingInputTime -= Time.deltaTime;

        if (remainingInputTime <= 0f)
        {
            FailMiniGame("制限時間切れ");
            return;
        }

        ReadDirectionInput();
    }

    /// <summary>
    /// ミニゲームを開始する。
    /// メインゲームや呼び出し側から使用する。
    /// </summary>
    public void StartArrowMiniGame()
    {
        if (!ValidateSettings())
        {
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        StopSlideAnimation();
        ClearSequence();

        currentArrowIndex = 0;
        remainingInputTime = inputTimeLimit;

        isRunning = true;
        isFinished = false;
        isSliding = false;
        isClear = false;

        GenerateRandomSequence();
        ArrangeArrowsAtStart();
        UpdateArrowVisuals();

        // 矢印の準備ができてから表示する。
        arrowContainer.gameObject.SetActive(true);

        if (showDebugLogs)
        {
            Debug.Log(
                $"[ArrowInputMiniGame1] 開始。矢印数：{arrowSequence.Count}",
                this
            );
        }
    }

    /// <summary>
    /// 実行中のミニゲームを中断する。
    /// 成功・失敗イベントは呼ばない。
    /// </summary>
    public void StopArrowMiniGame()
    {
        StopSlideAnimation();

        isRunning = false;
        isFinished = true;
        isSliding = false;
        isClear = false;

        if (showDebugLogs)
        {
            Debug.Log("[ArrowInputMiniGame1] 中断しました。", this);
        }
    }

    /// <summary>
    /// ミニゲームを終了して非表示にする。
    /// </summary>
    public void HideArrowMiniGame()
    {
        StopArrowMiniGame();

        if (arrowContainer != null)
        {
            arrowContainer.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Inspectorの設定を検証する。
    /// </summary>
    private bool ValidateSettings()
    {
        if (arrowContainer == null)
        {
            Debug.LogError(
                "[ArrowInputMiniGame1] Arrow Container が設定されていません。",
                this
            );
            return false;
        }

        if (upArrowSprite == null ||
            downArrowSprite == null ||
            leftArrowSprite == null ||
            rightArrowSprite == null)
        {
            Debug.LogError(
                "[ArrowInputMiniGame1] 上下左右の矢印画像をすべて設定してください。",
                this
            );
            return false;
        }

        if (arrowCount < 1 ||
            arrowSize <= 0f ||
            arrowSpacing <= 0f ||
            activeArrowScale < 1f ||
            slideDuration < 0f ||
            inputTimeLimit <= 0f)
        {
            Debug.LogError(
                "[ArrowInputMiniGame1] Inspectorの数値設定が不正です。",
                this
            );
            return false;
        }

        // 生成済みの場合、矢印数の変更は許可しない。
        if (arrowTransforms.Count > 0 &&
            arrowTransforms.Count != arrowCount)
        {
            Debug.LogError(
                "[ArrowInputMiniGame1] 実行中に矢印数を変更することはできません。",
                this
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// 矢印オブジェクトを生成する。
    /// </summary>
    private void CreateArrowObjects()
    {
        for (int i = 0; i < arrowCount; i++)
        {
            Image arrowImage;

            if (arrowImagePrefab != null)
            {
                arrowImage = Instantiate(
                    arrowImagePrefab,
                    arrowContainer
                );
            }
            else
            {
                GameObject arrowObject = new GameObject(
                    $"Arrow_{i + 1}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

                arrowObject.transform.SetParent(
                    arrowContainer,
                    false
                );

                arrowImage = arrowObject.GetComponent<Image>();
            }

            arrowImage.name = $"Arrow_{i + 1}";
            arrowImage.preserveAspect = true;
            arrowImage.raycastTarget = false;

            RectTransform rectTransform = arrowImage.rectTransform;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                arrowSize,
                arrowSize
            );
            rectTransform.localScale = Vector3.one;

            arrowImages.Add(arrowImage);
            arrowTransforms.Add(rectTransform);
        }
    }

    /// <summary>
    /// ランダムな矢印列を作成する。
    /// </summary>
    private void GenerateRandomSequence()
    {
        arrowSequence.Clear();

        ArrowDirection[] directions =
        {
            ArrowDirection.Up,
            ArrowDirection.Down,
            ArrowDirection.Left,
            ArrowDirection.Right
        };

        for (int i = 0; i < arrowCount; i++)
        {
            int randomIndex = Random.Range(0, directions.Length);

            arrowSequence.Add(directions[randomIndex]);
            arrowImages[i].sprite = GetSprite(arrowSequence[i]);
        }
    }

    /// <summary>
    /// 矢印を開始位置に並べる。
    /// </summary>
    private void ArrangeArrowsAtStart()
    {
        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            float xPosition = startCenterX + (i * arrowSpacing);

            arrowTransforms[i].anchoredPosition = new Vector2(
                xPosition,
                0f
            );
        }
    }

    /// <summary>
    /// キーボード入力を読み取る。
    /// </summary>
    private void ReadDirectionInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        ArrowDirection? pressedDirection = null;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            pressedDirection = ArrowDirection.Up;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
                 Keyboard.current.sKey.wasPressedThisFrame)
        {
            pressedDirection = ArrowDirection.Down;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                 Keyboard.current.aKey.wasPressedThisFrame)
        {
            pressedDirection = ArrowDirection.Left;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                 Keyboard.current.dKey.wasPressedThisFrame)
        {
            pressedDirection = ArrowDirection.Right;
        }

        if (pressedDirection.HasValue)
        {
            CheckInput(pressedDirection.Value);
        }
    }

    /// <summary>
    /// 入力された方向が正しいか判定する。
    /// </summary>
    private void CheckInput(ArrowDirection inputDirection)
    {
        if (currentArrowIndex < 0 ||
            currentArrowIndex >= arrowSequence.Count)
        {
            return;
        }

        if (inputDirection != arrowSequence[currentArrowIndex])
        {
            FailMiniGame(
                $"入力ミス。正解：{arrowSequence[currentArrowIndex]}、入力：{inputDirection}"
            );
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log(
                $"[ArrowInputMiniGame1] 正解：{inputDirection}",
                this
            );
        }

        currentArrowIndex++;

        if (currentArrowIndex >= arrowSequence.Count)
        {
            CompleteMiniGame();
            return;
        }

        // 次の矢印の制限時間をリセットする。
        remainingInputTime = inputTimeLimit;

        UpdateArrowVisuals();
        StartSlideAnimation();
    }

    /// <summary>
    /// 入力対象の矢印を拡大し、入力済みの矢印を暗くする。
    /// </summary>
    private void UpdateArrowVisuals()
    {
        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            bool isCurrentArrow = i == currentArrowIndex;

            float targetScale = isCurrentArrow
                ? activeArrowScale
                : 1f;

            arrowTransforms[i].localScale =
                Vector3.one * targetScale;

            if (i < currentArrowIndex)
            {
                arrowImages[i].color = new Color(
                    0.65f,
                    0.65f,
                    0.65f,
                    1f
                );
            }
            else
            {
                arrowImages[i].color = Color.white;
            }
        }
    }

    /// <summary>
    /// 矢印列のスライドアニメーションを開始する。
    /// </summary>
    private void StartSlideAnimation()
    {
        StopSlideAnimation();

        if (slideDuration <= 0f)
        {
            MoveArrowLineLeft();
            return;
        }

        slideCoroutine = StartCoroutine(SlideArrowLineLeft());
    }

    /// <summary>
    /// 矢印列を左へスライドさせる。
    /// </summary>
    private IEnumerator SlideArrowLineLeft()
    {
        isSliding = true;

        Vector2[] startPositions =
            new Vector2[arrowTransforms.Count];

        Vector2[] targetPositions =
            new Vector2[arrowTransforms.Count];

        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            startPositions[i] = arrowTransforms[i].anchoredPosition;

            targetPositions[i] = startPositions[i] -
                new Vector2(arrowSpacing, 0f);
        }

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(
                elapsedTime / slideDuration
            );

            float easedTime = Mathf.SmoothStep(
                0f,
                1f,
                normalizedTime
            );

            for (int i = 0; i < arrowTransforms.Count; i++)
            {
                arrowTransforms[i].anchoredPosition = Vector2.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    easedTime
                );
            }

            yield return null;
        }

        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            arrowTransforms[i].anchoredPosition = targetPositions[i];
        }

        isSliding = false;
        slideCoroutine = null;
    }

    /// <summary>
    /// アニメーションなしで矢印列を左へ移動する。
    /// </summary>
    private void MoveArrowLineLeft()
    {
        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            arrowTransforms[i].anchoredPosition -=
                new Vector2(arrowSpacing, 0f);
        }
    }

    /// <summary>
    /// スライドアニメーションを停止する。
    /// </summary>
    private void StopSlideAnimation()
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        isSliding = false;
    }

    /// <summary>
    /// 全入力成功時の処理。
    /// </summary>
    private void CompleteMiniGame()
    {
        if (isFinished)
        {
            return;
        }

        isRunning = false;
        isFinished = true;
        isClear = true;

        StopSlideAnimation();

        if (showDebugLogs)
        {
            Debug.Log(
                "[ArrowInputMiniGame1] 全入力成功。",
                this
            );
        }

        onSuccess.Invoke();
    }

    /// <summary>
    /// 入力ミスまたは時間切れ時の処理。
    /// </summary>
    private void FailMiniGame(string reason)
    {
        if (isFinished)
        {
            return;
        }

        isRunning = false;
        isFinished = true;
        isClear = false;

        StopSlideAnimation();

        if (showDebugLogs)
        {
            Debug.Log(
                $"[ArrowInputMiniGame1] 失敗：{reason}",
                this
            );
        }

        onFailure.Invoke();
    }

    /// <summary>
    /// 矢印列を初期化する。
    /// </summary>
    private void ClearSequence()
    {
        arrowSequence.Clear();

        for (int i = 0; i < arrowImages.Count; i++)
        {
            arrowImages[i].sprite = null;
            arrowImages[i].color = Color.white;
        }
    }

    /// <summary>
    /// 方向に対応する画像を返す。
    /// </summary>
    private Sprite GetSprite(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:
                return upArrowSprite;

            case ArrowDirection.Down:
                return downArrowSprite;

            case ArrowDirection.Left:
                return leftArrowSprite;

            case ArrowDirection.Right:
                return rightArrowSprite;

            default:
                return null;
        }
    }
}

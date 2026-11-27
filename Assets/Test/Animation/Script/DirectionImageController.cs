using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 上下左右の方向を表すenum。
/// </summary>
public enum ArrowDirection
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

/// <summary>
/// 拍の進行状態。
/// </summary>
public enum BeatPhase
{
    Idle,
    Beat1,
    Beat2,
    Beat3,
    Attack
}

/// <summary>
/// Playerの周囲に4方向の画像を表示し、拍に合わせて
/// 危険表示・選択・着弾の演出を行う。
///
/// ・PlayerをTagから取得
/// ・4方向の画像をPlayerの周囲に配置
/// ・Direction Material（UI/DirectionGlitch）を設定した場合は
///   危険方向の赤グリッチ・選択方向の水色枠線/明るさをシェーダーで描画
/// ・Direction Materialが未設定の場合は従来のSprite重ね+Outline方式で描画
/// ・赤色・青色の光るエフェクトPrefabに対応
/// ・選択時に拡大演出
/// ・着弾時に拡大して小さくなる演出
/// ・被弾時はPlayer本体だけを赤く点滅
/// </summary>
public class DirectionImageController : MonoBehaviour
{
    [Header("Player設定")]
    [SerializeField] private string playerTag = "Player";

    [Header("Canvas設定")]
    [SerializeField] private Canvas targetCanvas;

    [Header("4方向の画像")]
    [SerializeField] private RectTransform upImage;
    [SerializeField] private RectTransform downImage;
    [SerializeField] private RectTransform leftImage;
    [SerializeField] private RectTransform rightImage;

    [Header("Playerからの初期距離")]
    [SerializeField] private float initialOffset = 100f;

    [Header("画像の広がり設定")]
    [SerializeField] private float spreadDistance = 50f;
    [SerializeField] private float moveDuration = 0.4f;

    [Header("シェーダー表示")]
    [Tooltip("UI/DirectionGlitchシェーダーのマテリアル。設定すると赤グリッチ/水色枠線をシェーダーで描く。空なら従来方式")]
    [SerializeField] private Material directionMaterial;

    [Header("選択時の明るさ")]
    [Tooltip("元画像の色を白に近づける割合。0で変化なし")]
    [Range(0f, 1f)]
    [SerializeField] private float selectedBrighten = 0.2f;

    [Header("危険表示（赤系グリッチ）※従来方式用")]
    [SerializeField] private Sprite glitchSprite;

    [SerializeField]
    private Color[] dangerColors =
    {
        new Color(0.95f, 0.10f, 0.30f, 1f),
        new Color(1.00f, 0.20f, 0.55f, 1f),
        new Color(0.75f, 0.05f, 0.20f, 1f),
        new Color(1.00f, 0.45f, 0.60f, 1f)
    };

    [SerializeField] private float noiseInterval = 0.04f;
    [SerializeField] private float jitterAmount = 2.5f;

    [Header("光るエフェクト")]
    [Tooltip("危険方向に表示する赤色エフェクトPrefab")]
    [SerializeField] private GameObject dangerEffectPrefab;

    [Tooltip("選択方向に表示する青色エフェクトPrefab")]
    [SerializeField] private GameObject selectedEffectPrefab;

    [Header("選択枠線（白に近い水色）")]
    [SerializeField]
    private Color frameColorA = new Color(0.70f, 0.92f, 1f, 1f);

    [SerializeField]
    private Color frameColorB = new Color(0.92f, 0.99f, 1f, 1f);

    [Tooltip("従来方式（Outline）の太さ。シェーダー方式ではマテリアルのFrame Widthで調整")]
    [SerializeField] private float frameThickness = 6f;
    [SerializeField] private float frameBlinkSpeed = 6f;

    [Header("選択した瞬間の演出")]
    [Tooltip("選択した瞬間に一瞬大きくなる倍率。最後は1倍(普通の大きさ)に戻る")]
    [SerializeField] private float selectPunchScale = 1.35f;
    [SerializeField] private float selectPunchDuration = 0.18f;

    [Header("着弾演出")]
    [SerializeField] private float impactPunchScale = 1.5f;
    [SerializeField] private float impactFinalScale = 0.8f;
    [SerializeField] private float impactPunchDuration = 0.25f;

    [Header("Player被弾時の赤色点滅")]
    [Tooltip("空の場合はPlayerから自動取得")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [SerializeField] private Color playerHitColor = Color.red;

    [SerializeField] private float playerHitFlashDuration = 0.6f;

    [SerializeField] private float playerHitFlashSpeed = 12f;

    [Header("入力")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private bool useKeyboardInput = false;

    [Header("結果通知")]
    public UnityEvent onHit = new UnityEvent();
    public UnityEvent onAvoid = new UnityEvent();

    [SerializeField] private bool noSelectionIsHit = true;

    [Header("BattleManager連携")]
    [SerializeField] private BattleManager battleManager;

    [Header("敵の攻撃シーケンス")]
    [SerializeField] private int dangerDirectionCount = 2;
    [SerializeField] private float beatInterval = 0.5f;
    [SerializeField] private float startWaitTimeout = 15f;

    [Header("攻撃終了後の非表示")]
    [SerializeField] private float hideDelayAfterAttack = 0.3f;

    public UnityEvent onImagesHidden = new UnityEvent();

    public event Action OnHit;
    public event Action OnAvoid;

    private bool waitingAttackFinish;
    private bool pendingHit;

    public BeatPhase Phase { get; private set; } = BeatPhase.Idle;

    // シェーダープロパティID
    private static readonly int DangerId = Shader.PropertyToID("_Danger");
    private static readonly int SelectedId = Shader.PropertyToID("_Selected");
    private static readonly int BrightenId = Shader.PropertyToID("_Brighten");
    private static readonly int FrameColorAId = Shader.PropertyToID("_FrameColorA");
    private static readonly int FrameColorBId = Shader.PropertyToID("_FrameColorB");

    // ------------------------------------------------------------
    // 1方向ぶんの画像情報
    // ------------------------------------------------------------
    private class Slot
    {
        public RectTransform rect;
        public Image image;

        // 危険方向表示用の複製画像（従来方式）
        public Image overlayImage;

        // シェーダー方式用に複製したマテリアル（nullなら従来方式）
        public Material material;

        // 光るエフェクト
        public GameObject dangerEffectObject;
        public GameObject selectedEffectObject;

        public Outline outline;

        public Sprite defaultSprite;
        public Color defaultColor;
        public Vector3 defaultScale;

        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 basePos;
        public Vector2 jitter;

        public bool isDanger;
        public bool isSelected;
        public Color dangerColor;

        public float scale = 1f;
        public float restScale = 1f;

        public bool punching;
        public float punchTimer;
        public float punchDuration;
        public float punchPeak;
        public float punchEnd;
    }

    private readonly Slot[] slots = new Slot[4];

    private readonly HashSet<ArrowDirection> dangerDirections =
        new HashSet<ArrowDirection>();

    private ArrowDirection? selectedDirection;

    private Transform player;
    private RectTransform canvasRect;

    private float timer;
    private bool isMoving;
    private bool hasStarted;

    private float noiseTimer;
    private float coyoteRemaining;

    private Coroutine attackRoutine;

    // Player点滅用
    private Coroutine playerHitFlashCoroutine;
    private Color playerDefaultColor = Color.white;

    // ------------------------------------------------------------
    // 初期化
    // ------------------------------------------------------------
    private void Awake()
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();

            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }
        }

        if (targetCanvas != null)
        {
            canvasRect = targetCanvas.GetComponent<RectTransform>();
        }

        FindPlayer();

        // PlayerのSpriteRendererを取得
        FindPlayerSpriteRenderer();

        CreateSlot(ArrowDirection.Up, upImage);
        CreateSlot(ArrowDirection.Down, downImage);
        CreateSlot(ArrowDirection.Left, leftImage);
        CreateSlot(ArrowDirection.Right, rightImage);

        SetImagesActive(false);
    }

    private void OnDestroy()
    {
        // 方向ごとに複製したマテリアルを破棄
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].material != null)
            {
                Destroy(slots[i].material);
            }
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        UpdateSpreadMove(dt);
        UpdateCoyoteTime(dt);

        if (useKeyboardInput)
        {
            HandleKeyboardInput();
        }

        noiseTimer -= dt;

        bool reroll = noiseTimer <= 0f;

        if (reroll)
        {
            noiseTimer = Mathf.Max(0.005f, noiseInterval);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];

            if (slot == null) continue;

            UpdateScale(slot, dt);
            UpdateVisual(slot, reroll);

            if (isMoving || slot.isDanger || slot.isSelected)
            {
                slot.rect.anchoredPosition =
                    slot.basePos + slot.jitter;
            }
        }

        CheckAttackFinished();
    }

    // ------------------------------------------------------------
    // PlayerのSpriteRenderer取得
    // ------------------------------------------------------------
    private void FindPlayerSpriteRenderer()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
            return;

        // Inspectorで設定されていなければPlayerから取得
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer =
                player.GetComponent<SpriteRenderer>();

            // Player本体にない場合は子オブジェクトから取得
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer =
                    player.GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (playerSpriteRenderer != null)
        {
            playerDefaultColor = playerSpriteRenderer.color;
        }
        else
        {
            Debug.LogWarning(
                "PlayerのSpriteRendererが見つかりません。"
            );
        }
    }

    // ------------------------------------------------------------
    // Player被弾時の赤色点滅
    // ------------------------------------------------------------
    /// <summary>
    /// Playerが攻撃を受けたときに赤色点滅を開始する。
    /// </summary>
    private void StartPlayerHitFlash()
    {
        if (playerSpriteRenderer == null)
        {
            FindPlayerSpriteRenderer();
        }

        if (playerSpriteRenderer == null)
            return;

        // すでに点滅中なら一度停止する
        if (playerHitFlashCoroutine != null)
        {
            StopCoroutine(playerHitFlashCoroutine);

            playerSpriteRenderer.color = playerDefaultColor;

            playerHitFlashCoroutine = null;
        }

        playerHitFlashCoroutine =
            StartCoroutine(PlayerHitFlashEffect());
    }

    /// <summary>
    /// Player本体だけを赤く点滅させる。
    /// </summary>
    private IEnumerator PlayerHitFlashEffect()
    {
        if (playerSpriteRenderer == null)
        {
            playerHitFlashCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < playerHitFlashDuration)
        {
            elapsed += Time.deltaTime;

            float flash = Mathf.PingPong(
                elapsed * playerHitFlashSpeed,
                1f
            );

            playerSpriteRenderer.color = Color.Lerp(
                playerDefaultColor,
                playerHitColor,
                flash
            );

            yield return null;
        }

        // 最後に元の色へ戻す
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = playerDefaultColor;
        }

        playerHitFlashCoroutine = null;
    }

    // ------------------------------------------------------------
    // エフェクト生成
    // ------------------------------------------------------------
    private GameObject CreateEffect(
        GameObject prefab,
        RectTransform parent,
        string objectName)
    {
        if (prefab == null || parent == null)
            return null;

        GameObject effect = Instantiate(
            prefab,
            parent,
            false
        );

        effect.name = objectName;

        RectTransform effectRect =
            effect.GetComponent<RectTransform>();

        if (effectRect != null)
        {
            effectRect.anchorMin = Vector2.zero;
            effectRect.anchorMax = Vector2.one;

            effectRect.offsetMin = Vector2.zero;
            effectRect.offsetMax = Vector2.zero;

            effectRect.localPosition = Vector3.zero;
            effectRect.localRotation = Quaternion.identity;
            effectRect.localScale = Vector3.one;
        }

        effect.SetActive(false);

        return effect;
    }

    private void SetEffectActive(GameObject effect, bool active)
    {
        if (effect == null)
            return;

        if (effect.activeSelf != active)
        {
            effect.SetActive(active);
        }
    }

    // ------------------------------------------------------------
    // 攻撃終了確認
    // ------------------------------------------------------------
    private void CheckAttackFinished()
    {
        if (!waitingAttackFinish)
            return;

        foreach (ArrowDirection dir in dangerDirections)
        {
            Slot slot = slots[(int)dir];

            if (slot != null && slot.punching)
                return;
        }

        waitingAttackFinish = false;

        if (pendingHit)
        {
            // Player本体だけを赤く点滅
            StartPlayerHitFlash();

            onHit?.Invoke();
            OnHit?.Invoke();
        }
        else
        {
            onAvoid?.Invoke();
            OnAvoid?.Invoke();
        }

        if (battleManager != null)
        {
            battleManager.OnEnemyAttackFinished();
        }
    }

    // ------------------------------------------------------------
    // 攻撃シーケンス
    // ------------------------------------------------------------
    public void StartEnemyAttack()
    {
        if (attackRoutine != null)
            return;

        MonoBehaviour host = battleManager != null
            ? (MonoBehaviour)battleManager
            : this;

        if (!host.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "DirectionImageController: 攻撃シーケンスを開始できません。"
            );

            return;
        }

        attackRoutine = host.StartCoroutine(EnemyAttackSequence());
    }

    private IEnumerator EnemyAttackSequence()
    {
        float waited = 0f;

        while (!hasStarted || isMoving)
        {
            waited += Time.deltaTime;

            if (waited >= startWaitTimeout)
            {
                Debug.LogWarning(
                    "4方向の画像が開始されませんでした。攻撃を中止します。"
                );

                attackRoutine = null;

                if (battleManager != null)
                {
                    battleManager.OnEnemyAttackFinished();
                }

                yield break;
            }

            yield return null;
        }

        ArrowDirection[] dangers =
            PickRandomDirections(dangerDirectionCount);

        int firstCount = Mathf.CeilToInt(dangers.Length / 2f);

        ArrowDirection[] beat1 =
            new ArrowDirection[firstCount];

        ArrowDirection[] beat2 =
            new ArrowDirection[dangers.Length - firstCount];

        Array.Copy(dangers, 0, beat1, 0, beat1.Length);
        Array.Copy(dangers, firstCount, beat2, 0, beat2.Length);

        BeginBeat1(beat1);

        yield return new WaitForSeconds(beatInterval);

        BeginBeat2(beat2);

        yield return new WaitForSeconds(beatInterval);

        BeginBeat3();

        while ((Phase != BeatPhase.Attack || waitingAttackFinish)
               && Phase != BeatPhase.Idle)
        {
            yield return null;
        }

        if (hideDelayAfterAttack > 0f)
        {
            yield return new WaitForSeconds(hideDelayAfterAttack);
        }

        HideImages();

        attackRoutine = null;
    }

    public void HideImages()
    {
        ResetBeat();

        isMoving = false;
        hasStarted = false;

        SetImagesActive(false);

        onImagesHidden?.Invoke();
    }

    private ArrowDirection[] PickRandomDirections(int count)
    {
        ArrowDirection[] all =
        {
            ArrowDirection.Up,
            ArrowDirection.Down,
            ArrowDirection.Left,
            ArrowDirection.Right
        };

        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            ArrowDirection tmp = all[i];
            all[i] = all[j];
            all[j] = tmp;
        }

        count = Mathf.Clamp(count, 1, all.Length);

        ArrowDirection[] result = new ArrowDirection[count];

        Array.Copy(all, result, count);

        return result;
    }

    // ------------------------------------------------------------
    // 拍の進行
    // ------------------------------------------------------------
    public void BeginBeat1(params ArrowDirection[] dangers)
    {
        ClearBeatState();

        Phase = BeatPhase.Beat1;

        AddDangers(dangers);
    }

    public void BeginBeat2(params ArrowDirection[] dangers)
    {
        Phase = BeatPhase.Beat2;

        AddDangers(dangers);
    }

    public void BeginBeat3()
    {
        Phase = BeatPhase.Beat3;

        coyoteRemaining = Mathf.Max(0f, coyoteTime);

        if (coyoteRemaining <= 0f)
        {
            TriggerAttack();
        }
    }

    public void SelectDirection(ArrowDirection direction)
    {
        if (!IsInputOpen)
        {
            Debug.Log("[DIC] 入力受付外のため無視: Phase=" + Phase);
            return;
        }

        if (selectedDirection.HasValue &&
            selectedDirection.Value == direction)
        {
            return;
        }

        if (selectedDirection.HasValue)
        {
            SetSelected(selectedDirection.Value, false);
        }

        selectedDirection = direction;
        Debug.Log("[DIC] 選択: " + direction + " / brighten=" + selectedBrighten);

        SetSelected(direction, true);

        // 選択した瞬間に一瞬大きくなって、普通の大きさ(1倍)に戻る
        Slot slot = slots[(int)direction];

        if (slot != null)
        {
            StartPunch(
                slot,
                selectPunchScale,
                1f,
                selectPunchDuration
            );
        }
    }

    public void ResetBeat()
    {
        ClearBeatState();

        Phase = BeatPhase.Idle;
    }

    public bool IsInputOpen
    {
        get
        {
            switch (Phase)
            {
                case BeatPhase.Beat1:
                case BeatPhase.Beat2:
                    return true;

                case BeatPhase.Beat3:
                    return coyoteRemaining > 0f;

                default:
                    return false;
            }
        }
    }

    // ------------------------------------------------------------
    // 危険方向の設定
    // ------------------------------------------------------------
    private void AddDangers(ArrowDirection[] dangers)
    {
        if (dangers == null) return;

        foreach (ArrowDirection dir in dangers)
        {
            if (!dangerDirections.Add(dir))
                continue;

            Slot slot = slots[(int)dir];

            if (slot == null) continue;

            slot.isDanger = true;
            slot.dangerColor = PickDangerColor();

            // 従来方式のときだけ複製Imageで赤色表示
            if (slot.material == null)
            {
                slot.overlayImage.sprite =
                    glitchSprite != null
                    ? glitchSprite
                    : slot.defaultSprite;

                slot.overlayImage.color = slot.dangerColor;
                slot.overlayImage.enabled = true;
            }

            // 赤色の危険エフェクトを表示
            SetEffectActive(
                slot.dangerEffectObject,
                true
            );
        }
    }

    private void UpdateCoyoteTime(float dt)
    {
        if (Phase != BeatPhase.Beat3)
            return;

        coyoteRemaining -= dt;

        if (coyoteRemaining <= 0f)
        {
            coyoteRemaining = 0f;

            TriggerAttack();
        }
    }

    private void TriggerAttack()
    {
        Phase = BeatPhase.Attack;

        foreach (ArrowDirection dir in dangerDirections)
        {
            Slot slot = slots[(int)dir];

            if (slot == null) continue;

            StartPunch(
                slot,
                impactPunchScale,
                impactFinalScale,
                impactPunchDuration
            );
        }

        bool selectedDanger =
            selectedDirection.HasValue &&
            dangerDirections.Contains(selectedDirection.Value);

        pendingHit = selectedDirection.HasValue
            ? selectedDanger
            : noSelectionIsHit;

        waitingAttackFinish = true;

        if (dangerDirections.Count == 0)
        {
            waitingAttackFinish = false;

            if (pendingHit)
            {
                StartPlayerHitFlash();

                onHit?.Invoke();
                OnHit?.Invoke();
            }
            else
            {
                onAvoid?.Invoke();
                OnAvoid?.Invoke();
            }

            if (battleManager != null)
            {
                battleManager.OnEnemyAttackFinished();
            }
        }
    }

    // ------------------------------------------------------------
    // 拍状態の初期化
    // ------------------------------------------------------------
    private void ClearBeatState()
    {
        dangerDirections.Clear();

        selectedDirection = null;

        coyoteRemaining = 0f;

        waitingAttackFinish = false;

        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];

            if (slot == null) continue;

            slot.isDanger = false;
            slot.isSelected = false;
            slot.jitter = Vector2.zero;

            slot.punching = false;
            slot.scale = 1f;
            slot.restScale = 1f;

            slot.image.sprite = slot.defaultSprite;
            slot.image.color = slot.defaultColor;

            // シェーダー方式の状態をリセット
            if (slot.material != null)
            {
                slot.material.SetFloat(DangerId, 0f);
                slot.material.SetFloat(SelectedId, 0f);
            }

            // 赤色表示用の複製画像を非表示
            slot.overlayImage.enabled = false;
            slot.overlayImage.sprite = slot.defaultSprite;
            slot.overlayImage.color = Color.white;

            // 赤色・青色エフェクトを非表示
            SetEffectActive(
                slot.dangerEffectObject,
                false
            );

            SetEffectActive(
                slot.selectedEffectObject,
                false
            );

            // 枠線を非表示
            slot.outline.enabled = false;

            slot.rect.localScale = slot.defaultScale;

            if (hasStarted)
            {
                slot.rect.anchoredPosition = slot.basePos;
            }
        }
    }

    // ------------------------------------------------------------
    // 選択状態
    // ------------------------------------------------------------
    private void SetSelected(ArrowDirection dir, bool selected)
    {
        Slot slot = slots[(int)dir];

        if (slot == null)
        {
            Debug.LogWarning("[DIC] Slotが未作成です: " + dir);
            return;
        }

        slot.isSelected = selected;

        // 従来方式のときだけOutlineで枠線を表示
        if (slot.material == null)
        {
            slot.outline.effectDistance =
                new Vector2(frameThickness, frameThickness);
            slot.outline.effectColor = frameColorA;
            slot.outline.enabled = selected;
        }

        // 青色エフェクトの表示・非表示
        SetEffectActive(
            slot.selectedEffectObject,
            selected
        );

        if (!selected)
        {
            slot.jitter = Vector2.zero;
        }
    }

    // ------------------------------------------------------------
    // 見た目の更新
    // ------------------------------------------------------------
    private void UpdateVisual(Slot slot, bool reroll)
    {
        // ---------- シェーダー方式 ----------
        if (slot.material != null)
        {
            // 小刻みな震えはC#側で位置をずらす
            if (slot.isDanger && reroll)
            {
                slot.jitter = new Vector2(
                    UnityEngine.Random.Range(-jitterAmount, jitterAmount),
                    UnityEngine.Random.Range(-jitterAmount, jitterAmount)
                );
            }

            slot.material.SetFloat(DangerId, slot.isDanger ? 1f : 0f);
            slot.material.SetFloat(SelectedId, slot.isSelected ? 1f : 0f);
            slot.material.SetFloat(BrightenId, selectedBrighten);
            slot.material.SetColor(FrameColorAId, frameColorA);
            slot.material.SetColor(FrameColorBId, frameColorB);

            slot.overlayImage.enabled = false;

            return;
        }

        // ---------- 従来方式（Sprite重ね + Outline） ----------

        // 選択中は元の色を白に近づけて少し明るくする
        if (slot.isSelected)
        {
            Color bright = Color.Lerp(
                slot.defaultColor,
                Color.white,
                selectedBrighten
            );

            bright.a = slot.defaultColor.a;

            slot.image.color = bright;
        }
        else
        {
            slot.image.color = slot.defaultColor;
        }

        if (slot.isDanger)
        {
            if (reroll)
            {
                slot.dangerColor = PickDangerColor();

                slot.jitter = new Vector2(
                    UnityEngine.Random.Range(-jitterAmount, jitterAmount),
                    UnityEngine.Random.Range(-jitterAmount, jitterAmount)
                );
            }

            slot.overlayImage.color = slot.dangerColor;
            slot.overlayImage.enabled = true;
        }
        else
        {
            slot.overlayImage.enabled = false;
        }

        if (slot.isSelected)
        {
            float k = Mathf.PingPong(
                Time.time * frameBlinkSpeed,
                1f
            );

            slot.outline.effectDistance =
                new Vector2(frameThickness, frameThickness);

            slot.outline.effectColor =
                Color.Lerp(frameColorA, frameColorB, k);
        }
    }

    private Color PickDangerColor()
    {
        if (dangerColors == null || dangerColors.Length == 0)
            return Color.red;

        return dangerColors[
            UnityEngine.Random.Range(0, dangerColors.Length)
        ];
    }

    // ------------------------------------------------------------
    // 拡大・縮小演出
    // ------------------------------------------------------------
    private void StartPunch(
        Slot slot,
        float peak,
        float endScale,
        float duration)
    {
        slot.punching = true;
        slot.punchTimer = 0f;
        slot.punchPeak = peak;
        slot.punchEnd = endScale;
        slot.punchDuration = Mathf.Max(0.01f, duration);
    }

    private void UpdateScale(Slot slot, float dt)
    {
        if (slot.punching)
        {
            slot.punchTimer += dt;

            float t = Mathf.Clamp01(
                slot.punchTimer / slot.punchDuration
            );

            const float peakTime = 0.3f;

            if (t < peakTime)
            {
                float k = t / peakTime;

                k = 1f - (1f - k) * (1f - k);

                slot.scale = Mathf.Lerp(
                    slot.restScale,
                    slot.punchPeak,
                    k
                );
            }
            else
            {
                float k = (t - peakTime) / (1f - peakTime);

                slot.scale = Mathf.Lerp(
                    slot.punchPeak,
                    slot.punchEnd,
                    Mathf.SmoothStep(0f, 1f, k)
                );
            }

            if (t >= 1f)
            {
                slot.punching = false;

                slot.restScale = slot.punchEnd;

                slot.scale = slot.punchEnd;
            }
        }

        if (slot.punching || slot.scale != 1f)
        {
            slot.rect.localScale =
                slot.defaultScale * slot.scale;
        }
    }

    // ------------------------------------------------------------
    // 入力
    // ------------------------------------------------------------
    private void HandleKeyboardInput()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W))
        {
            SelectDirection(ArrowDirection.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                 Input.GetKeyDown(KeyCode.S))
        {
            SelectDirection(ArrowDirection.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                 Input.GetKeyDown(KeyCode.A))
        {
            SelectDirection(ArrowDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) ||
                 Input.GetKeyDown(KeyCode.D))
        {
            SelectDirection(ArrowDirection.Right);
        }
#endif
    }

    // ------------------------------------------------------------
    // Player取得
    // ------------------------------------------------------------
    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "TagがPlayerのオブジェクトが見つかりません。"
            );
        }
    }

    // ------------------------------------------------------------
    // Player座標をCanvas座標へ変換
    // ------------------------------------------------------------
    private bool GetPlayerCanvasPosition(
        out Vector2 canvasPosition)
    {
        canvasPosition = Vector2.zero;

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null || canvasRect == null)
            return false;

        Camera worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogWarning("Camera.main が見つかりません。");
            return false;
        }

        Camera uiCamera = null;

        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = targetCanvas.worldCamera;

            if (uiCamera == null)
            {
                uiCamera = worldCamera;
            }
        }

        Vector3 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                worldCamera,
                player.position
            );

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out canvasPosition
        );
    }

    // ------------------------------------------------------------
    // 4方向画像の開始
    // ------------------------------------------------------------
    public void StartDirectionImages()
    {
        if (hasStarted)
            return;

        if (!GetPlayerCanvasPosition(out Vector2 centerPosition))
        {
            Debug.LogWarning(
                "PlayerまたはCanvasの取得に失敗しました。"
            );

            return;
        }

        SetInitialPositions(centerPosition);

        SetImagesActive(true);

        timer = 0f;

        isMoving = true;

        hasStarted = true;

        Debug.Log("4方向の画像演出を開始しました。");
    }

    // ------------------------------------------------------------
    // 広がり移動
    // ------------------------------------------------------------
    private void UpdateSpreadMove(float dt)
    {
        if (!isMoving)
            return;

        timer += dt;

        float t = moveDuration <= 0f
            ? 1f
            : Mathf.Clamp01(timer / moveDuration);

        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];

            if (slot == null) continue;

            slot.basePos = Vector2.Lerp(
                slot.startPos,
                slot.endPos,
                smoothT
            );
        }

        if (t >= 1f)
        {
            isMoving = false;

            Debug.Log("4方向の画像の移動が完了しました。");
        }
    }

    private void SetInitialPositions(Vector2 centerPosition)
    {
        SetSlotPositions(
            ArrowDirection.Up,
            centerPosition,
            Vector2.up
        );

        SetSlotPositions(
            ArrowDirection.Down,
            centerPosition,
            Vector2.down
        );

        SetSlotPositions(
            ArrowDirection.Left,
            centerPosition,
            Vector2.left
        );

        SetSlotPositions(
            ArrowDirection.Right,
            centerPosition,
            Vector2.right
        );
    }

    private void SetSlotPositions(
        ArrowDirection dir,
        Vector2 center,
        Vector2 direction)
    {
        Slot slot = slots[(int)dir];

        if (slot == null) return;

        slot.startPos = center + direction * initialOffset;

        slot.endPos = slot.startPos + direction * spreadDistance;

        slot.basePos = slot.startPos;

        slot.rect.anchoredPosition = slot.startPos;
    }

    // ------------------------------------------------------------
    // スロット作成・赤色表示用複製画像・エフェクト生成
    // ------------------------------------------------------------
    private void CreateSlot(
        ArrowDirection dir,
        RectTransform rect)
    {
        if (rect == null) return;

        Image image = rect.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogWarning(
                rect.name + " に Image コンポーネントがありません。"
            );

            return;
        }

        // 選択枠線を準備（従来方式用）
        Outline outline = rect.GetComponent<Outline>();

        if (outline == null)
        {
            outline = rect.gameObject.AddComponent<Outline>();
        }

        outline.effectDistance =
            new Vector2(frameThickness, frameThickness);

        outline.effectColor = frameColorA;

        outline.useGraphicAlpha = false;

        outline.enabled = false;

        // 赤色表示用の複製画像を作成（従来方式用）
        GameObject overlayObject = new GameObject(
            rect.name + "_ColorOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform overlayRect =
            overlayObject.GetComponent<RectTransform>();

        overlayRect.SetParent(rect, false);

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;

        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        overlayRect.localScale = Vector3.one;
        overlayRect.localRotation = Quaternion.identity;
        overlayRect.localPosition = Vector3.zero;

        Image overlayImage =
            overlayObject.GetComponent<Image>();

        overlayImage.sprite = image.sprite;
        overlayImage.color = Color.white;

        overlayImage.type = image.type;
        overlayImage.preserveAspect = image.preserveAspect;

        overlayImage.raycastTarget = false;
        overlayImage.maskable = image.maskable;

        // ※マテリアルの複製より前に元のマテリアルをコピーする
        overlayImage.material = image.material;

        overlayImage.enabled = false;

        // シェーダー方式：方向ごとにマテリアルを複製して差し替える
        Material slotMaterial = null;

        if (directionMaterial != null)
        {
            slotMaterial = new Material(directionMaterial);
            image.material = slotMaterial;
        }

        // 赤色の危険エフェクトを生成
        GameObject dangerEffect = CreateEffect(
            dangerEffectPrefab,
            rect,
            rect.name + "_DangerEffect"
        );

        // 青色の選択エフェクトを生成
        GameObject selectedEffect = CreateEffect(
            selectedEffectPrefab,
            rect,
            rect.name + "_SelectedEffect"
        );

        slots[(int)dir] = new Slot
        {
            rect = rect,
            image = image,
            overlayImage = overlayImage,

            material = slotMaterial,

            dangerEffectObject = dangerEffect,
            selectedEffectObject = selectedEffect,

            outline = outline,

            defaultSprite = image.sprite,
            defaultColor = image.color,
            defaultScale = rect.localScale
        };
    }

    // ------------------------------------------------------------
    // 画像の表示切り替え
    // ------------------------------------------------------------
    private void SetImagesActive(bool active)
    {
        if (upImage != null)
            upImage.gameObject.SetActive(active);

        if (downImage != null)
            downImage.gameObject.SetActive(active);

        if (leftImage != null)
            leftImage.gameObject.SetActive(active);

        if (rightImage != null)
            rightImage.gameObject.SetActive(active);
    }
}
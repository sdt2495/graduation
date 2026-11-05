
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuButtonSelectEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("拡大設定")]
    [SerializeField] private float scaleSelected = 1.15f;
    [SerializeField] private float scaleNormal = 1.0f;
    [SerializeField] private float scaleDecided = 1.4f;
    [SerializeField] private float lerpSpeed = 10f;

    [Header("決定時の演出")]
    [SerializeField] private float rotateDecided = 15f;
    [SerializeField] private float decideDuration = 0.3f;
    [SerializeField] private float stayDuration = 0.1f;
    [SerializeField] private float returnDuration = 0.25f;

    [Header("EXIT専用：振動設定")]
    [SerializeField] private bool shakeInsteadOfDecide = false;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAngle = 10f;
    [SerializeField] private float shakeSpeed = 30f;

    [Header("EXIT専用：表示テキスト")]
    [SerializeField] private TMP_Text exitMessageText;
    [SerializeField] private string exitMessage = "使えません";

    [Header("メッセージ表示時間")]
    [SerializeField] private float messageDuration = 2f;

    [Header("色設定")]
    [SerializeField] private Image visualImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;

    [Header("設定画面")]
    [SerializeField] private bool openConfigAfterEffect = false;
    [SerializeField] private TitleManager titleManager;

    private Vector3 targetScale;

    private bool isDecided;
    private bool isHovered;
    private bool isSelected;

    private Button button;

    private Coroutine decideCoroutine;
    private Coroutine messageCoroutine;

    private void Start()
    {
        button = GetComponent<Button>();

        ResetButtonState();

        if (button != null)
        {
            button.onClick.AddListener(OnDecided);
        }
    }

    private void Update()
    {
        if (isDecided) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            lerpSpeed * Time.unscaledDeltaTime
        );
    }

   public void OnPointerEnter(PointerEventData eventData)
{
    if (isDecided || shakeInsteadOfDecide) return;

    isHovered = true;
    targetScale = Vector3.one * scaleSelected;

    if (visualImage != null)
        visualImage.color = selectedColor;
}

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDecided || shakeInsteadOfDecide) return;

        isHovered = false;

        if (!isSelected)
        {
            targetScale = Vector3.one * scaleNormal;

            if (visualImage != null)
                visualImage.color = normalColor;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (isDecided || shakeInsteadOfDecide) return;

        isSelected = true;
        targetScale = Vector3.one * scaleSelected;

        if (visualImage != null)
            visualImage.color = selectedColor;
    }

    // ボタンの選択が解除されたとき
    public void OnDeselect(BaseEventData eventData)
    {
        if (isDecided || shakeInsteadOfDecide) return;

        isSelected = false;

        if (!isHovered)
        {
            targetScale = Vector3.one * scaleNormal;

            if (visualImage != null)
                visualImage.color = normalColor;
        }
    }

    // ボタン決定時
    private void OnDecided()
    {
        if (isDecided) return;

        isDecided = true;

        // EXIT専用：振動演出
        if (shakeInsteadOfDecide)
        {
            DecideEffect(ShowExitMessage, true);
            return;
        }

        // CONFIG専用：演出後に設定画面へ
        if (openConfigAfterEffect)
        {
            DecideEffect(() =>
            {
                if (titleManager != null)
                {
                    titleManager.OpenConfigAfterEffect();
                }
            });

            return;
        }

        // 通常の決定演出
        DecideEffect();
    }

    // 演出の開始
    public void DecideEffect(
        System.Action onComplete = null,
        bool useShake = false)
    {
        if (decideCoroutine != null)
        {
            StopCoroutine(decideCoroutine);
        }

        if (useShake)
        {
            decideCoroutine = StartCoroutine(
                PlayShakeEffect(onComplete)
            );
        }
        else
        {
            decideCoroutine = StartCoroutine(
                PlayDecideEffect(onComplete)
            );
        }
    }

    // 通常の決定演出
    private IEnumerator PlayDecideEffect(
        System.Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;

        Vector3 decidedScale = Vector3.one * scaleDecided;

        Quaternion decidedRotation =
            Quaternion.Euler(0f, 0f, rotateDecided);

        float elapsed = 0f;

        // 拡大しながら傾く
        while (elapsed < decideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = decideDuration > 0f
                ? Mathf.Clamp01(elapsed / decideDuration)
                : 1f;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(
                startScale,
                decidedScale,
                smoothT
            );

            transform.localRotation = Quaternion.Slerp(
                startRotation,
                decidedRotation,
                smoothT
            );

            yield return null;
        }

        transform.localScale = decidedScale;
        transform.localRotation = decidedRotation;

        // 少し維持
        yield return new WaitForSecondsRealtime(stayDuration);

        // 元に戻す
        elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = returnDuration > 0f
                ? Mathf.Clamp01(elapsed / returnDuration)
                : 1f;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(
                decidedScale,
                Vector3.one * scaleNormal,
                smoothT
            );

            transform.localRotation = Quaternion.Slerp(
                decidedRotation,
                Quaternion.identity,
                smoothT
            );

            yield return null;
        }

        transform.localScale = Vector3.one * scaleNormal;
        transform.localRotation = Quaternion.identity;

        decideCoroutine = null;

        onComplete?.Invoke();
    }

    // EXIT専用：左右に振動
    private IEnumerator PlayShakeEffect(
        System.Action onComplete)
    {
        Quaternion originalRotation = transform.localRotation;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float angle = Mathf.Sin(
                elapsed * shakeSpeed
            ) * shakeAngle;

            transform.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // 元の角度に戻す
        transform.localRotation = originalRotation;

      
        // 再度決定できるようにする
        isDecided = false;

        decideCoroutine = null;

        // 振動終了後にメッセージ表示
        onComplete?.Invoke();
    }

    // EXIT決定後にメッセージ表示
    private void ShowExitMessage()
    {
        if (exitMessageText == null) return;

        // 前回の消去処理を停止
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        exitMessageText.text = exitMessage;
        exitMessageText.gameObject.SetActive(true);

        // 一定時間後に消す
        messageCoroutine = StartCoroutine(
            HideExitMessage()
        );
    }

    // メッセージを一定時間後に消す
    private IEnumerator HideExitMessage()
    {
        yield return new WaitForSecondsRealtime(
            messageDuration
        );

        if (exitMessageText != null)
        {
            exitMessageText.gameObject.SetActive(false);
        }

        messageCoroutine = null;
    }

    // ボタン状態を初期化
    public void ResetButtonState()
    {
        if (decideCoroutine != null)
        {
            StopCoroutine(decideCoroutine);
            decideCoroutine = null;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        isDecided = false;
        isHovered = false;
        isSelected = false;

        targetScale = Vector3.one * scaleNormal;

        transform.localScale = targetScale;
        transform.localRotation = Quaternion.identity;

        if (visualImage != null)
        {
            visualImage.color = normalColor;
        }

        if (exitMessageText != null)
        {
            exitMessageText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnDecided);
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MenuButtonSelectEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("ägëÂê›íË")]
    [SerializeField] private float scaleSelected = 1.15f;
    [SerializeField] private float scaleNormal = 1.0f;
    [SerializeField] private float scaleDecided = 1.4f;
    [SerializeField] private float lerpSpeed = 10f;

    [Header("åàíËéûÇÃââèo")]
    [SerializeField] private float rotateDecided = 15f;
    [SerializeField] private float decideDuration = 0.3f;
    [SerializeField] private float stayDuration = 0.1f;
    [SerializeField] private float returnDuration = 0.25f;

    [Header("êFê›íË")]
    [SerializeField] private Image visualImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;

    [SerializeField] private bool openConfigAfterEffect = false;
    [SerializeField] private TitleManager titleManager;

    private Vector3 targetScale;
    private bool isDecided;
    private bool isHovered;
    private bool isSelected;

    private Button button;
    private Coroutine decideCoroutine;

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
        if (isDecided) return;

        isHovered = true;
        targetScale = Vector3.one * scaleSelected;

        if (visualImage != null)
            visualImage.color = selectedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDecided) return;

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
        if (isDecided) return;

        isSelected = true;
        targetScale = Vector3.one * scaleSelected;

        if (visualImage != null)
            visualImage.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isDecided) return;

        isSelected = false;

        if (!isHovered)
        {
            targetScale = Vector3.one * scaleNormal;

            if (visualImage != null)
                visualImage.color = normalColor;
        }
    }

    private void OnDecided()
    {
        if (isDecided) return;

        isDecided = true;

        if (openConfigAfterEffect)
        {
            DecideEffect(() =>
            {
                if (titleManager != null)
                {
                    titleManager.OpenConfigAfterEffect();
                }
            });
        }
        else
        {
            DecideEffect();
        }
    }

    public void DecideEffect(System.Action onComplete = null)
    {
        if (decideCoroutine != null)
            StopCoroutine(decideCoroutine);

        decideCoroutine = StartCoroutine(
            PlayDecideEffect(onComplete)
        );
    }

    private IEnumerator PlayDecideEffect(System.Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;

        Vector3 decidedScale = Vector3.one * scaleDecided;
        Quaternion decidedRotation =
            Quaternion.Euler(0f, 0f, rotateDecided);

        float elapsed = 0f;

        // ägëÂÇµÇ»Ç™ÇÁåXÇ≠
        while (elapsed < decideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = decideDuration > 0f
                ? Mathf.Clamp01(elapsed / decideDuration)
                : 1f;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(startScale, decidedScale, smoothT);

            transform.localRotation =
                Quaternion.Slerp(startRotation, decidedRotation, smoothT);

            yield return null;
        }

        transform.localScale = decidedScale;
        transform.localRotation = decidedRotation;

        // åXÇ¢ÇΩèÛë‘Çè≠Çµà€éù
        yield return new WaitForSecondsRealtime(stayDuration);

        // å≥ÇÃëÂÇ´Ç≥ÅEäpìxÇ…ñﬂÇÈ
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

    // É{É^ÉìèÛë‘Çèâä˙âª
    public void ResetButtonState()
    {
        if (decideCoroutine != null)
        {
            StopCoroutine(decideCoroutine);
            decideCoroutine = null;
        }

        isDecided = false;
        isHovered = false;
        isSelected = false;

        targetScale = Vector3.one * scaleNormal;

        transform.localScale = targetScale;
        transform.localRotation = Quaternion.identity;

        if (visualImage != null)
            visualImage.color = normalColor;
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnDecided);
        }
    }
}
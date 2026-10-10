
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

    private Vector3 targetScale;
    private bool isDecided = false;
    private bool isHovered = false;
    private bool isSelected = false;

    private Button button;
    private Coroutine decideCoroutine;

    private void Start()
    {
        targetScale = Vector3.one * scaleNormal;
        button = GetComponent<Button>();

        if (visualImage != null)
        {
            visualImage.color = normalColor;
        }

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
            lerpSpeed * Time.deltaTime
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
        DecideEffect();
    }

    public void DecideEffect()
    {
        if (decideCoroutine != null)
            StopCoroutine(decideCoroutine);

        decideCoroutine = StartCoroutine(PlayDecideEffect());
    }

    private IEnumerator PlayDecideEffect()
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;

        Vector3 targetScale = Vector3.one * scaleDecided;
        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, rotateDecided);

        // ägëÂÇµÇ»Ç™ÇÁåXÇ≠
        float elapsed = 0f;

        while (elapsed < decideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / decideDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(startScale, targetScale, smoothT);

            transform.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, smoothT);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.localRotation = targetRotation;

        // åXÇ¢ÇΩèÛë‘Çè≠Çµà€éù
        yield return new WaitForSecondsRealtime(stayDuration);

        // å≥ÇÃëÂÇ´Ç≥ÅEäpìxÇ…ñﬂÇÈ
        elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / returnDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(targetScale, Vector3.one * scaleNormal, smoothT);

            transform.localRotation =
                Quaternion.Slerp(targetRotation, Quaternion.identity, smoothT);

            yield return null;
        }

        transform.localScale = Vector3.one * scaleNormal;
        transform.localRotation = Quaternion.identity;

        decideCoroutine = null;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnDecided);
    }
}

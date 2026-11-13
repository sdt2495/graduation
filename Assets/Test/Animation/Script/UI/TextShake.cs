
using UnityEngine;

/// <summary>
/// テキストを小さく震わせる演出。
/// </summary>
public class TextShake : MonoBehaviour
{
    [Header("震えの設定")]
    [SerializeField] private float shakeAmount = 2f;
    [SerializeField] private float shakeSpeed = 30f;

    private RectTransform rectTransform;
    private Vector3 initialPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
    }

    private void Update()
    {
        float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;

        rectTransform.localPosition = initialPosition +
            new Vector3(offsetX, offsetY, 0f);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.localPosition = initialPosition;
        }
    }
}

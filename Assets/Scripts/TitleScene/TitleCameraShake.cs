using System.Collections;
using UnityEngine;

public class TitleCameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private float magnitude = 10f;

    private RectTransform rectTransform;
    private Vector2 originalPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransformÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            enabled = false;
            return;
        }

        originalPos = rectTransform.anchoredPosition;
    }

    public IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rectTransform.anchoredPosition =
                originalPos + new Vector2(x, y);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;
    }
}
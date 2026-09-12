using UnityEngine;

public class MenuButtonIdleMotion : MonoBehaviour
{
    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float phaseOffset = 0f;
    [SerializeField] private float delay = 1f;

    private RectTransform rectTransform;
    private Vector2 defaultPosition;
    private float timer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransformÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            enabled = false;
            return;
        }

        defaultPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < delay)
        {
            return;
        }

        float t = timer - delay;

        float y = Mathf.Sin(t * speed + phaseOffset) * moveAmount;

        rectTransform.anchoredPosition =
            defaultPosition + new Vector2(0f, y);
    }
}
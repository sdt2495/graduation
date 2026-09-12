using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonSelectEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private float scaleSelected = 1.15f;
    [SerializeField] private float scaleNormal = 1.0f;
    [SerializeField] private float lerpSpeed = 10f;

    [SerializeField] private Image visualImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;

    private Vector3 targetScale;

    private void Start()
    {
        targetScale = Vector3.one * scaleNormal;

        if (visualImage != null)
        {
            visualImage.color = normalColor;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            lerpSpeed * Time.deltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * scaleSelected;

        if (visualImage != null)
        {
            visualImage.color = selectedColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * scaleNormal;

        if (visualImage != null)
        {
            visualImage.color = normalColor;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonDescription : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("表示するテキスト")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("このボタンの説明")]
    [TextArea(2, 4)]
    [SerializeField] private string description;

    // マウスを合わせたとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowText();
    }

    // マウスが離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        HideText();
    }

    // ボタンが選択されたとき
    public void OnSelect(BaseEventData eventData)
    {
        ShowText();
    }

    // ボタンの選択が解除されたとき
    public void OnDeselect(BaseEventData eventData)
    {
        HideText();
    }

    private void ShowText()
    {
        if (descriptionText == null) return;

        descriptionText.text = description;
        descriptionText.gameObject.SetActive(true);
    }

    private void HideText()
    {
        if (descriptionText == null) return;

        descriptionText.gameObject.SetActive(false);
    }
}

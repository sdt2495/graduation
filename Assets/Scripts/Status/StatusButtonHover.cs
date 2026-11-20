
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    private PageSwitcher pageSwitcher;
    private bool isMain;

    public void Initialize(PageSwitcher switcher, bool main)
    {
        pageSwitcher = switcher;
        isMain = main;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pageSwitcher == null)
            return;

        // 選択中はホバーだけでは切り替えない
        if (TradeSelectionUI.HasAnySelection)
            return;

        pageSwitcher.SwitchPage(isMain);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pageSwitcher == null)
            return;

        // 実際にクリックしたときはページを切り替える
        pageSwitcher.SwitchPage(isMain);
    }
}








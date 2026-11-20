
using UnityEngine;
using UnityEngine.EventSystems;

public class StatusButtonHover : MonoBehaviour, IPointerClickHandler
{
    private PageSwitcher pageSwitcher;
    private bool isMain;

    public void Initialize(PageSwitcher switcher, bool main)
    {
        pageSwitcher = switcher;
        isMain = main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pageSwitcher == null)
            return;

        // クリックしたときだけページを切り替える
        pageSwitcher.SwitchPage(isMain);
    }
}









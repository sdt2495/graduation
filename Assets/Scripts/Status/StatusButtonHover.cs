
using UnityEngine;
using UnityEngine.EventSystems;

public class StatusButtonHover : MonoBehaviour, IPointerEnterHandler
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
        if (pageSwitcher != null)
        {
            pageSwitcher.SwitchPage(isMain);
        }
    }
}


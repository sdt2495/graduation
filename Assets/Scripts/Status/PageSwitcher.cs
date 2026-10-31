
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PageSwitcher : MonoBehaviour
{
    [Header("ページ")]
    public GameObject page1;
    public GameObject page2;

    [Header("メインステータスのボタン（Page1）")]
    public Button[] mainStatusButtons;

    [Header("サブステータスのボタン（Page2）")]
    public Button[] subStatusButtons;

    [Header("ボタンの大きさ")]
    public float selectedScale = 1.2f;
    public float normalScale = 1f;
    public float animationDuration = 0.2f;

    private readonly HashSet<Button> registeredButtons =
        new HashSet<Button>();

    private void Start()
    {
        // 最初はメインページ
        ShowPage1();

        // Inspectorで登録したボタンにイベントを設定
        RegisterButtons(mainStatusButtons, ShowPage1);
        RegisterButtons(subStatusButtons, ShowPage2);
    }

    private void RegisterButtons(
        Button[] buttons,
        UnityEngine.Events.UnityAction action)
    {
        if (buttons == null) return;

        foreach (Button button in buttons)
        {
            if (button == null || !registeredButtons.Add(button))
                continue;

            EventTrigger trigger =
                button.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            if (trigger.triggers == null)
                trigger.triggers =
                    new List<EventTrigger.Entry>();

            EventTrigger.Entry entry =
                new EventTrigger.Entry();

            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => action());

            trigger.triggers.Add(entry);
        }
    }

    public void ShowPage1()
    {
        if (page1 != null)
            page1.SetActive(true);

        if (page2 != null)
            page2.SetActive(false);
    }

    public void ShowPage2()
    {
        if (page1 != null)
            page1.SetActive(false);

        if (page2 != null)
            page2.SetActive(true);
    }
}






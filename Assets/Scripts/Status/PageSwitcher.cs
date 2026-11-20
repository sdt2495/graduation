
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [Header("ページ")]
    public GameObject page1;
    public GameObject page2;

    [Header("メインページへ切り替えるボタン")]
    public Button[] mainStatusButtons;

    [Header("サブページへ切り替えるボタン")]
    public Button[] subStatusButtons;

    [Header("サブステータス管理")]
    public SubStatusTrade subStatusTrade;

    private void Start()
    {
        RegisterButtons(mainStatusButtons, true);
        RegisterButtons(subStatusButtons, false);

        ShowPage1();
        RefreshSubStatusLabels();
    }

    private void RegisterButtons(Button[] buttons, bool isMain)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            StatusButtonHover hover =
                button.GetComponent<StatusButtonHover>();

            if (hover == null)
                hover = button.gameObject.AddComponent<StatusButtonHover>();

            hover.Initialize(this, isMain);
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

        RefreshSubStatusLabels();
    }

    public void SwitchPage(bool isMain)
    {
        if (isMain)
            ShowPage1();
        else
            ShowPage2();
    }

    // 選択されたボタンがどのページに属するか判定する
    public bool SwitchToPageContaining(Button button)
    {
        if (button == null)
            return false;

        // まず、登録済みのボタン配列を確認する
        if (ContainsButton(mainStatusButtons, button))
        {
            ShowPage1();
            return true;
        }

        if (ContainsButton(subStatusButtons, button))
        {
            ShowPage2();
            return true;
        }

        // 配列にない場合は親オブジェクトから判定する
        if (page1 != null &&
            (button.transform == page1.transform ||
             button.transform.IsChildOf(page1.transform)))
        {
            ShowPage1();
            return true;
        }

        if (page2 != null &&
            (button.transform == page2.transform ||
             button.transform.IsChildOf(page2.transform)))
        {
            ShowPage2();
            return true;
        }

        Debug.LogWarning(
            "ボタン「" + button.name +
            "」が Page1 / Page2 のどちらにも判定されません。");

        return false;
    }

    private bool ContainsButton(Button[] buttons, Button target)
    {
        if (buttons == null)
            return false;

        foreach (Button button in buttons)
        {
            if (button == target)
                return true;
        }

        return false;
    }

    private void RefreshSubStatusLabels()
    {
        if (subStatusTrade == null)
            subStatusTrade = FindFirstObjectByType<SubStatusTrade>();

        if (subStatusTrade != null)
            subStatusTrade.RefreshSubStatusButtonLabels();
    }
}










using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickGuideUI : MonoBehaviour
{
    [System.Serializable]
    public class StatusInfo
    {
        public string statusName;

        [TextArea(3, 6)]
        public string description;

        public Sprite image;
    }

    [Header("説明ウィンドウ")]
    public GameObject quickGuidePanel;
    public Button closeButton;

    [Header("切り替えボタン")]
    public Button StatusTabButton;
    public Button ActionTabButton;

    [Header("スクロール")]
    public ScrollRect statusScrollRect;
    public GameObject StatusList;
    public GameObject ActionList;

    [Header("表示欄")]
    public TMP_Text statusNameText;
    public TMP_Text statusDescriptionText;
    public Image statusImage;

    [Header("ステータス")]
    public Button[] StatusButtons;
    public StatusInfo[] StatusInfos;

    [Header("アクション")]
    public Button[] ActionButtons;
    public StatusInfo[] ActionInfos;

    private bool showingMain = true;

    private void Start()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseGuide);

        if (StatusTabButton != null)
            StatusTabButton.onClick.AddListener(ShowMainStatuses);

        if (ActionTabButton != null)
            ActionTabButton.onClick.AddListener(ShowSubStatuses);

        RegisterStatusButtons(StatusButtons, true);
        RegisterStatusButtons(ActionButtons, false);

        UpdateStatusLists();
    }

    private void RegisterStatusButtons(Button[] buttons, bool isMain)
    {
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            int index = i;

            buttons[i].onClick.AddListener(() =>
            {
                if (isMain)
                    ShowStatus(StatusInfos, index);
                else
                    ShowStatus(ActionInfos, index);
            });
        }
    }

    public void OpenGuide()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(true);

        ShowMainStatuses();
    }

    public void CloseGuide()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);
    }

    public void ShowMainStatuses()
    {
        showingMain = true;
        UpdateStatusLists();

        ResetScroll();

        ShowStatus(StatusInfos, 0);
    }

    public void ShowSubStatuses()
    {
        showingMain = false;
        UpdateStatusLists();

        ResetScroll();

        ShowStatus(ActionInfos, 0);
    }

    private void UpdateStatusLists()
    {
        if (StatusList != null)
            StatusList.SetActive(showingMain);

        if (ActionList != null)
            ActionList.SetActive(!showingMain);

        // 選択中のタブを少し大きくする
        SetTabScale(StatusTabButton, showingMain);
        SetTabScale(ActionTabButton, !showingMain);
    }

    private void SetTabScale(Button button, bool selected)
    {
        if (button == null) return;

        float scale = selected ? 1.1f : 1.0f;
        button.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void ResetScroll()
    {
        if (statusScrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        statusScrollRect.StopMovement();
        statusScrollRect.verticalNormalizedPosition = 1f;
    }

    private void ShowStatus(StatusInfo[] infos, int index)
    {
        if (infos == null ||
            index < 0 ||
            index >= infos.Length)
            return;

        StatusInfo info = infos[index];

        if (statusNameText != null)
            statusNameText.text = info.statusName;

        if (statusDescriptionText != null)
            statusDescriptionText.text = info.description;

        if (statusImage != null)
        {
            statusImage.sprite = info.image;
            statusImage.enabled = info.image != null;
        }
    }
}




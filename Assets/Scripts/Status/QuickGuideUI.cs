
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

    [Header("スクロール一覧")]
    public ScrollRect statusScrollRect;

    [Header("表示欄")]
    public TMP_Text statusNameText;
    public TMP_Text statusDescriptionText;
    public Image statusImage;

    [Header("ステータス選択ボタン")]
    public Button[] statusButtons;

    [Header("説明データ（ボタンと同じ順番）")]
    public StatusInfo[] statusInfos;

    private void Start()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseGuide);

        if (statusButtons != null)
        {
            for (int i = 0; i < statusButtons.Length; i++)
            {
                int index = i;

                if (statusButtons[i] != null)
                {
                    statusButtons[i].onClick.AddListener(
                        () => ShowStatus(index)
                    );
                }
            }
        }
    }

    public void OpenGuide()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(true);

        // スクロール位置を一番上に戻す
        if (statusScrollRect != null)
        {
            statusScrollRect.StopMovement();
            statusScrollRect.verticalNormalizedPosition = 1f;
        }

        ShowStatus(0);
    }

    public void CloseGuide()
    {
        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);
    }

    public void ShowStatus(int index)
    {
        if (statusInfos == null ||
            index < 0 ||
            index >= statusInfos.Length)
            return;

        StatusInfo info = statusInfos[index];

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



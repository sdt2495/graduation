using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    [Header("選択肢全体")]
    [SerializeField] private GameObject choicePanel;

    [Header("選択肢1")]
    [SerializeField] private Button choiceButton1;
    [SerializeField] private TMP_Text choiceText1;

    [Header("選択肢2")]
    [SerializeField] private Button choiceButton2;
    [SerializeField] private TMP_Text choiceText2;

    private string choice1ID;
    private string choice2ID;


    public bool IsShowing => choicePanel != null && choicePanel.activeSelf;     // 現在、選択肢を表示しているか
    public event Action<string> OnChoiceSelected;    // 選択肢が選ばれたときにIDを通知する

    private void Awake()
    {
        choiceButton1?.onClick.AddListener(OnClickChoice1);
        choiceButton2?.onClick.AddListener(OnClickChoice2);

        HideChoices();
    }

    private void OnDestroy()
    {
        choiceButton1?.onClick.RemoveListener(OnClickChoice1);
        choiceButton2?.onClick.RemoveListener(OnClickChoice2);
    }

    /// <summary>
    /// 選択肢を表示する
    /// </summary>
    public void ShowChoices(
        string text1,
        string id1,
        string text2,
        string id2)
    {
        choice1ID = id1;
        choice2ID = id2;

        bool hasChoice1 = !string.IsNullOrEmpty(text1);
        bool hasChoice2 = !string.IsNullOrEmpty(text2);


        if (choiceText1 != null)
        {
            choiceText1.text = text1;
        }
        if (choiceText2 != null)
        {
            choiceText2.text = text2;
        }

        if (choiceButton1 != null)
        {
            choiceButton1.gameObject.SetActive(hasChoice1);
        }
        if (choiceButton2 != null)
        {
            choiceButton2.gameObject.SetActive(hasChoice2);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(hasChoice1 || hasChoice2);
        }
            
    }

    /// <summary>
    /// 選択肢を非表示にする
    /// </summary>
    public void HideChoices()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }

    private void OnClickChoice1()
    {
        if (!string.IsNullOrEmpty(choice1ID))
        {
            OnChoiceSelected?.Invoke(choice1ID);
        }
    }

    private void OnClickChoice2()
    {
        if (!string.IsNullOrEmpty(choice2ID))
        {
            OnChoiceSelected?.Invoke(choice2ID);
        }
    }
}
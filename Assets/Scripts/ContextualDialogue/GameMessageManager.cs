using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMessageManager : MonoBehaviour
{
    [Header("メッセージUI")]
    [SerializeField] private GameObject messagePanel;
    [Space(5)]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text messageText;

    [Header("──────────────────────────────")]
    [Header("表示時間")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine hideCoroutine;

    #region メッセージを表示する

    /// <summary>
    /// メッセージを表示する
    /// </summary> 
    public void ShowMessage(string message)
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
            
        if (messageText != null)
        {
            messageText.text = message;
        }
            

        // 前の消去処理があれば止める
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 一定時間後に消す
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }
    #endregion


    #region キャラ画像を変更してメッセージを表示

    /// <summary>
    /// キャラ画像を変更してメッセージを表示
    /// </summary>
    public void ShowMessage(Sprite character, string message)
    {
        if (characterImage != null)
        {
            characterImage.sprite = character;
        }

        ShowMessage(message);
    }
    #endregion


    #region メッセージを消す

    /// <summary>
    /// メッセージを消す
    /// </summary>
    public void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
    #endregion

    /// <summary>
    /// メッセージを表示してから一定時間待って、自動的に非表示にする
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        hideCoroutine = null;
    }
}
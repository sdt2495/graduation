using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バックログ1件分の表示
/// </summary>
public class BackLogEntry : MonoBehaviour
{
    [Header("テキスト")]
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI messageText;
    [Header("ボタン")]
    [SerializeField] private Button voiceButton;

    /// <summary>
    /// 表示内容を設定
    /// </summary>
    public void Set(BackLogData log, NovelVoiceManager voiceManager)
    {
        // テキスト
        speakerText.text = log.Speaker;
        messageText.text = log.Message;

        // ボイスの有無
        bool hasVoice = !string.IsNullOrEmpty(log.Voice);
        // ボイス有ならボタンを有効
        voiceButton.gameObject.SetActive(hasVoice);

        // (以前の)登録を全部消す
        voiceButton.onClick.RemoveAllListeners();

        if (hasVoice)
        {
            // 「ボタンが押されたら、この処理を実行」
            voiceButton.onClick.AddListener(() =>
            {
                voiceManager.PlayVoice(log.Voice);
            });
        }
    }
}
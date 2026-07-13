using TMPro;
using UnityEngine;

/// <summary>
/// バックログ1件分の表示
/// </summary>
public class BackLogEntry : MonoBehaviour
{
    [Header("テキスト")]
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI messageText;


    /// <summary>
    /// 表示内容を設定
    /// </summary>
    public void Set(BackLogData log)
    {
        speakerText.text = log.Speaker;
        messageText.text = log.Message;
    }
}
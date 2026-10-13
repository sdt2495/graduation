using UnityEngine;

/// <summary>
/// 文字送り速度・オート速度の設定を管理するクラス
/// </summary>
public class TextSettingsManager : MonoBehaviour
{
    public static TextSettingsManager Instance { get; private set; }

    // PlayerPrefsキー
    private const string TEXT_SPEED_KEY = "TextSpeed"; // テキスト表示速度
    private const string AUTO_SPEED_KEY = "AutoSpeed"; // オートモード待機時間

    private const string PAUSE_AT_PUNCTUATION_KEY = "PauseAtPunctuation"; // 句読点での待機
    private const string MessageWindowOpacityKey = "MessageWindowOpacity"; // メッセージウィンドウ不透明度


    // 不透明度変更通知
    public event System.Action<float> OnMessageWindowOpacityChanged;


    #region 設定取得

    /// <summary>
    /// 文字送り速度を取得
    /// </summary>
    public float GetTextSpeed()
    {
        return PlayerPrefs.GetFloat(TEXT_SPEED_KEY, GameDefaultSettings.TextSpeed);
    }

    /// <summary>
    /// オート速度を取得
    /// </summary>
    public float GetAutoSpeed()
    {
        return PlayerPrefs.GetFloat(AUTO_SPEED_KEY, GameDefaultSettings.AutoSpeed);
    }


    /// <summary>
    /// 句読点で一時停止するか取得
    /// </summary>
    public bool GetPauseAtPunctuation()
    {
        return PlayerPrefs.GetInt(PAUSE_AT_PUNCTUATION_KEY, GameDefaultSettings.PauseAtPunctuation ? 1 : 0) == 1;
    }

    /// <summary>
    /// メッセージウィンドウ背景の不透明度を取得
    /// </summary>
    public float GetMessageWindowOpacity()
    {
        return PlayerPrefs.GetFloat(MessageWindowOpacityKey, GameDefaultSettings.MessageWindowOpacity);
    }
    #endregion


    // 常駐オブジェクト
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    #region 設定保存

    /// <summary>
    /// 文字送り速度を設定
    /// </summary>
    public void SetTextSpeed(float value)
    {
        PlayerPrefs.SetFloat(TEXT_SPEED_KEY, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// オート速度を設定
    /// </summary>
    public void SetAutoSpeed(float value)
    {
        PlayerPrefs.SetFloat(AUTO_SPEED_KEY, value);
        PlayerPrefs.Save();
    }


    /// <summary>S
    /// 「、/。」句読点で一時停止するか設定
    /// </summary>
    public void SetPauseAtPunctuation(bool value)
    {
        PlayerPrefs.SetInt(PAUSE_AT_PUNCTUATION_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// メッセージウィンドウ背景の不透明度を設定・保存
    /// </summary>
    public void SetMessageWindowOpacity(float opacity)
    {
        opacity = Mathf.Clamp01(opacity);

        PlayerPrefs.SetFloat(MessageWindowOpacityKey, opacity);
        PlayerPrefs.Save();

        OnMessageWindowOpacityChanged?.Invoke(opacity);
    }
    #endregion
}
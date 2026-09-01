using UnityEngine;

/// <summary>
/// 文字送り速度・オート速度の設定を管理するクラス
/// </summary>
public class TextSettingsManager : MonoBehaviour
{
    public static TextSettingsManager Instance { get; private set; }

    // PlayerPrefsキー
    private const string TEXT_SPEED_KEY = "TextSpeed";
    private const string AUTO_SPEED_KEY = "AutoSpeed";

    private const string PAUSE_AT_PUNCTUATION_KEY = "PauseAtPunctuation";


    #region 設定取得

    /// <summary>
    /// 文字送り速度を取得
    /// </summary>
    public float GetTextSpeed()
    {
        return PlayerPrefs.GetFloat(TEXT_SPEED_KEY, 0.05f);
    }

    /// <summary>
    /// オート速度を取得
    /// </summary>
    public float GetAutoSpeed()
    {
        return PlayerPrefs.GetFloat(AUTO_SPEED_KEY, 1.5f);
    }


    /// <summary>
    /// 句読点で一時停止するか取得
    /// </summary>
    public bool GetPauseAtPunctuation()
    {
        return PlayerPrefs.GetInt(PAUSE_AT_PUNCTUATION_KEY, 1) == 1;
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
    #endregion
}
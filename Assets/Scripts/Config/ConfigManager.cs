using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// "ConfigScene"のスライダーを管理
/// </summary>
public class ConfigManager : MonoBehaviour
{
    [Header("ConfigSEManager")]
    [SerializeField] private ConfigSEManager configSEManager; // コンフィグでのSEを再生するスクリプト

    [Header("──────────────────────────────")]
    [Header("AudioSetting")]
    [SerializeField] private Slider masterSlider;    // 全体音量
    [SerializeField] private Slider bgmSlider;       // BGM音量
    [SerializeField] private Slider systemSESlider;  // システムSE音量
    [SerializeField] private Slider voiceSlider;     // ボイス音量
    [SerializeField] private Slider seSlider;        // SE音量

    [Header("──────────────────────────────")]
    [Header("TextSetting")]
    [SerializeField] private Slider textSpeedSlider;             // テキスト表示速度
    [SerializeField] private Slider autoSpeedSlider;             // オート待機時間
    [SerializeField] private Toggle pauseAtPunctuationToggle;    // 句読点での待機
    [Header("MessageWindowSetting")]
    [SerializeField] private Slider messageWindowOpacitySlider;  // ウィンドウ不透明度


    private void Start()
    {
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogError("AudioSettingsManagerが存在しません");
            return;
        }
        if (TextSettingsManager.Instance == null)
        {
            Debug.LogError("TextSettingsManagerが存在しません");
            return;
        }

        // 保存済みの設定をSliderへ反映
        LoadSliderValue();

        // Slider変更イベント登録
        AddListener();
    }

    #region Sliderの値に反映

    /// <summary>
    /// 保存されている音量をSliderへ設定
    /// </summary>
    private void LoadSliderValue()
    {
        // 保存した値を、スライダーに設定
        // Audio
        masterSlider.value = AudioSettingsManager.Instance.GetMasterVolume(); // マスター音量
        bgmSlider.value = AudioSettingsManager.Instance.GetBGMVolume(); // BGM音量
        systemSESlider.value = AudioSettingsManager.Instance.GetSystemSEVolume(); // システムSE音量
        voiceSlider.value = AudioSettingsManager.Instance.GetVoiceVolume(); // ボイス音量
        seSlider.value = AudioSettingsManager.Instance.GetSEVolume(); // SE音量
        // Text
        textSpeedSlider.value = TextSettingsManager.Instance.GetTextSpeed(); // テキスト表示速度
        autoSpeedSlider.value = TextSettingsManager.Instance.GetAutoSpeed(); // オート待機時間
        pauseAtPunctuationToggle.isOn = TextSettingsManager.Instance.GetPauseAtPunctuation(); // 句読点待機
        messageWindowOpacitySlider.value = TextSettingsManager.Instance.GetMessageWindowOpacity(); // ウィンドウ不透明度
    }
    #endregion


    #region イベント
    /// <summary>
    /// Slider変更時のイベント登録
    /// </summary>
    private void AddListener()
    {
        // 値変更イベント登録 (Sliderの値が変わったら関数が呼び出しされる)
        // Audio
        masterSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetMasterVolume); // マスター音量
        bgmSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetBGMVolume); // BGM音量
        systemSESlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSystemSEVolume); // システムSE音量
        voiceSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetVoiceVolume); // ボイス音量
        seSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSEVolume); // SE音量
        // Text
        textSpeedSlider.onValueChanged.AddListener(TextSettingsManager.Instance.SetTextSpeed); // テキスト表示速度
        autoSpeedSlider.onValueChanged.AddListener(TextSettingsManager.Instance.SetAutoSpeed); // オート待機時間
        pauseAtPunctuationToggle.onValueChanged.AddListener(TextSettingsManager.Instance.SetPauseAtPunctuation); // 句読点待機
        messageWindowOpacitySlider.onValueChanged.AddListener(TextSettingsManager.Instance.SetMessageWindowOpacity); // ウィンドウ不透明度
    }

    /// <summary>
    /// イベント解除
    /// </summary>
    private void OnDestroy()
    {
        // Audio
        if (AudioSettingsManager.Instance != null)
        {
            masterSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetMasterVolume); // マスター音量
            bgmSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetBGMVolume); // BGM音量
            systemSESlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSystemSEVolume); // システムSE音量
            voiceSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetVoiceVolume); // ボイス音量
            seSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSEVolume); // SE音量
        }
        // Text
        if (TextSettingsManager.Instance != null)
        {
            textSpeedSlider.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetTextSpeed); // テキスト表示速
            autoSpeedSlider.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetAutoSpeed); // オート待機時間
            pauseAtPunctuationToggle.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetPauseAtPunctuation); // 句読点待機
            messageWindowOpacitySlider.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetMessageWindowOpacity); // ウィンドウ不透明度
        }
    }
    #endregion


    #region 設定画面を閉じる

    /// <summary>
    /// 設定画面を閉じる
    /// </summary>
    public void OnClickCloseConfigButton()
    {
        ConfigSceneController.Instance.CloseConfig();
    }
    #endregion
}
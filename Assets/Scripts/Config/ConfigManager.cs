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
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider systemSESlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider seSlider;

    [Header("──────────────────────────────")]
    [Header("TextSetting")]
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private Slider autoSpeedSlider;
    [SerializeField] private Toggle pauseAtPunctuationToggle;


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


    /// <summary>
    /// 保存されている音量をSliderへ設定
    /// </summary>
    private void LoadSliderValue()
    {
        // 保存した値を、スライダーに設定
        // Audio
        masterSlider.value = AudioSettingsManager.Instance.GetMasterVolume();
        bgmSlider.value = AudioSettingsManager.Instance.GetBGMVolume();
        systemSESlider.value = AudioSettingsManager.Instance.GetSystemSEVolume();
        voiceSlider.value = AudioSettingsManager.Instance.GetVoiceVolume();
        seSlider.value = AudioSettingsManager.Instance.GetSEVolume();
        // Text
        textSpeedSlider.value = TextSettingsManager.Instance.GetTextSpeed();
        autoSpeedSlider.value = TextSettingsManager.Instance.GetAutoSpeed();

        pauseAtPunctuationToggle.isOn = TextSettingsManager.Instance.GetPauseAtPunctuation();
    }

    #region イベント
    /// <summary>
    /// Slider変更時のイベント登録
    /// </summary>
    private void AddListener()
    {
        // 値変更イベント登録 (Sliderの値が変わったら関数が呼び出しされる)
        // Audio
        masterSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetBGMVolume);
        systemSESlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSystemSEVolume);
        voiceSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetVoiceVolume);
        seSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSEVolume);
        // Text
        textSpeedSlider.onValueChanged.AddListener(TextSettingsManager.Instance.SetTextSpeed);
        autoSpeedSlider.onValueChanged.AddListener(TextSettingsManager.Instance.SetAutoSpeed);

        pauseAtPunctuationToggle.onValueChanged.AddListener(TextSettingsManager.Instance.SetPauseAtPunctuation);
    }

    /// <summary>
    /// イベント解除
    /// </summary>
    private void OnDestroy()
    {
        // Audio
        if (AudioSettingsManager.Instance != null)
        {
            masterSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetMasterVolume);
            bgmSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetBGMVolume);
            systemSESlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSystemSEVolume);
            voiceSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetVoiceVolume);
            seSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSEVolume);
        }
        // Text
        if (TextSettingsManager.Instance != null)
        {
            textSpeedSlider.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetTextSpeed);
            autoSpeedSlider.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetAutoSpeed);

            pauseAtPunctuationToggle.onValueChanged.RemoveListener(TextSettingsManager.Instance.SetPauseAtPunctuation);
        }
    }
    #endregion

    #region ボタン (仮)

    /// <summary>
    /// Scene変更 (ボタン)
    /// </summary>
    public void OnClickChangeSceneButton(string sceneName)
    {
        // シーン移動
        StartCoroutine(ChangeScene(sceneName));
    }
    private IEnumerator ChangeScene(string sceneName)
    {
        float seLength = 0f;

        if (configSEManager != null)
        {
            // SE再生
            seLength = configSEManager.PlayDecisionSE();
        }
        else
        {
            Debug.LogWarning("ConfigSEManagerが設定されていません");
        }

        // SE終了待ち
        yield return new WaitForSeconds(seLength);

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }



    public void OnClickCloseConfigButton()
    {
        ConfigSceneController.Instance.CloseConfig();
    }
    #endregion
}
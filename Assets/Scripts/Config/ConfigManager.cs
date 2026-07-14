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
    [Header("スライダーたち")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider systemSESlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider seSlider;


    private void Start()
    {
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogError("AudioSettingsManagerが存在しません");
            return;
        }

        // 保存済みの音量をSliderへ反映
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
        masterSlider.value = AudioSettingsManager.Instance.GetMasterVolume();
        bgmSlider.value = AudioSettingsManager.Instance.GetBGMVolume();
        systemSESlider.value = AudioSettingsManager.Instance.GetSystemSEVolume();
        voiceSlider.value = AudioSettingsManager.Instance.GetVoiceVolume();
        seSlider.value = AudioSettingsManager.Instance.GetSEVolume();
    }

    #region イベント
    /// <summary>
    /// Slider変更時のイベント登録
    /// </summary>
    private void AddListener()
    {
        // 値変更イベント登録 (Sliderの値が変わったら関数が呼び出しされる)
        masterSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetBGMVolume);
        systemSESlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSystemSEVolume);
        voiceSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetVoiceVolume);
        seSlider.onValueChanged.AddListener(AudioSettingsManager.Instance.SetSEVolume);
    }

    /// <summary>
    /// イベント解除
    /// </summary>
    private void OnDestroy()
    {
        // AudioSettingsManagerが存在しなければ処理をやめる
        if (AudioSettingsManager.Instance == null)
        {
            return;
        }

        masterSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetBGMVolume);
        systemSESlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSystemSEVolume);
        voiceSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetVoiceVolume);
        seSlider.onValueChanged.RemoveListener(AudioSettingsManager.Instance.SetSEVolume);
    }
    #endregion

    #region ボタン (仮)

    /// <summary>
    /// Scene変更 (ボタン)
    /// </summary>
    public void OnClickChangeSneceButton(string sceneName)
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
    #endregion
}
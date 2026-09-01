using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音量設定を管理するクラス
/// </summary>
public class AudioSettingsManager : MonoBehaviour
{
    // > AudioSettingsManager を GameManager と同じように最初のシーンで生成する形にすると完成形に近づきますわ。

    public static AudioSettingsManager Instance { get; private set; }

    // PlayerPrefsキー
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SYSTEM_SE_VOLUME_KEY = "SystemSEVolume";
    private const string VOICE_VOLUME_KEY = "VoiceVolume";
    private const string SE_VOLUME_KEY = "SEVolume";

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;


    #region 音量取得

    /// <summary>
    /// 音量：マスター取得
    /// </summary>
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// 音量：BGM取得
    /// </summary>
    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// 音量：システムSE取得
    /// </summary>
    public float GetSystemSEVolume()
    {
        return PlayerPrefs.GetFloat(SYSTEM_SE_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// 音量：ボイス取得
    /// </summary>
    public float GetVoiceVolume()
    {
        return PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// 音量：SE取得
    /// </summary>
    public float GetSEVolume()
    {
        return PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1f);
    }
    #endregion

    private void Awake()
    {
        // インスタンス
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(this.gameObject);

            LoadVolume();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    #region 音量調整の値を保存

    /// <summary>
    /// 音量：マスター
    /// </summary>
    public void SetMasterVolume(float value)
    {
        SetVolume(MASTER_VOLUME_KEY, value);
    }

    /// <summary>
    /// 音量：BGM
    /// </summary>
    public void SetBGMVolume(float value)
    {
        SetVolume(BGM_VOLUME_KEY, value);
    }

    /// <summary>
    /// 音量：システムSE
    /// </summary>
    public void SetSystemSEVolume(float value)
    {
        SetVolume(SYSTEM_SE_VOLUME_KEY, value);
    }

    /// <summary>
    /// 音量：ボイス
    /// </summary>
    public void SetVoiceVolume(float value)
    {
        SetVolume(VOICE_VOLUME_KEY, value);
    }

    /// <summary>
    /// 音量：SE
    /// </summary>
    public void SetSEVolume(float value)
    {
        SetVolume(SE_VOLUME_KEY, value);
    }
    #endregion


    /// <summary>
    /// 音量保存 (どの項目を,どのくらい)
    /// </summary>
    private void SetVolume(string key, float value)
    {
        audioMixer.SetFloat(key, ConvertVolume(value));

        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }



    /// <summary>
    /// ボリュームを読み込み
    /// </summary>
    private void LoadVolume()
    {
        SetVolume(MASTER_VOLUME_KEY, PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f));

        SetVolume(BGM_VOLUME_KEY, PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f));

        SetVolume(SYSTEM_SE_VOLUME_KEY, PlayerPrefs.GetFloat(SYSTEM_SE_VOLUME_KEY, 1f));

        SetVolume(VOICE_VOLUME_KEY, PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1f));

        SetVolume(SE_VOLUME_KEY, PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1f));
    }


    /// <summary>
    /// 現在のボリューム (Sliderの値（0～1）を、AudioMixerが使う音量単位（dB）に変換している処理)
    /// </summary>
    private float ConvertVolume(float value)
    {
        // 0の場合はミュート
        if (value <= 0.001f)
        {
            // -８０は、ほぼ無音にする値。(-80dB は ほぼ無音)
            return -80f;
        }
        // ２０は、音量をdBに変換するための定数。
        return Mathf.Log10(value) * 20f;
    }
}
using UnityEngine;

/// <summary>
/// ノベルゲーム用SE管理クラス
/// </summary>
public class NovelSystemSEManager : MonoBehaviour
{
    [Header("AudioSource (システムSE)")]
    [SerializeField] private AudioSource audioSource;

    [Header("SE音量")]
    [SerializeField][Range(0f, 1f)] private float systemVolume = 1f;

    [Space(5)]

    [Header("文字送りSE")]
    [SerializeField] private AudioClip textSE;

    [Header("次の行へ進むSE")]
    [SerializeField] private AudioClip nextLineSE;

    [Header("Auto切替SE")]
    [SerializeField] private AudioClip autoOnSE;
    [SerializeField] private AudioClip autoOffSE;

    [Header("Skip切替SE")]
    [SerializeField] private AudioClip skipOnSE;
    [SerializeField] private AudioClip skipOffSE;


    [Header("決定")]
    [SerializeField] private AudioClip decideSE;

    [Header("キャンセル")]
    [SerializeField] private AudioClip cancelSE;

    #region SE呼び出し関数
    /// <summary>
    /// 文字送り時SE
    /// </summary>
    public void PlayTextSE()
    {
        PlaySystemSE(textSE);
    }

    /// <summary>
    /// 次の文章へ進むSE
    /// </summary>
    public void PlayNextLineSE()
    {
        PlaySystemSE(nextLineSE);
    }

    /// <summary>
    /// Auto切替SE
    /// </summary>
    public void PlayAutoSE(bool enable)
    {
        if (enable)
        {
            PlaySystemSE(autoOnSE);
        }
        else
        {
            PlaySystemSE(autoOffSE);
        }
    }

    /// <summary>
    /// Skip切替SE
    /// </summary>
    public void PlaySkipSE(bool enable)
    {
        if (enable)
        {
            PlaySystemSE(skipOnSE);
        }
        else
        {
            PlaySystemSE(skipOffSE);
        }
    }
    #endregion

    #region UI

    public void PlayDecideSE()
    {
        PlaySystemSE(decideSE);
    }


    public void PlayCancelSE()
    {
        PlaySystemSE(cancelSE);
    }

    #endregion

    /// <summary>
    /// 実際のSE再生処理
    /// </summary>
    void PlaySystemSE(AudioClip clip)
    {
        // SEが設定されていなければ終了
        if (clip == null)
            return;

        // AudioSourceで再生
        audioSource.PlayOneShot(clip, systemVolume);
    }
}
using UnityEngine;

/// <summary>
/// ノベルゲーム用SE管理クラス
/// </summary>
/// 
[RequireComponent(typeof(AudioSource))]
public class NovelSEManager : MonoBehaviour
{
    [Header("SE音量")]
    [SerializeField][Range(0f, 1f)] private float seVolume = 1f;

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

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    #region SE呼び出し関数
    /// <summary>
    /// 文字送り時SE
    /// </summary>
    public void PlayTextSE()
    {
        PlaySE(textSE);
    }

    /// <summary>
    /// 次の文章へ進むSE
    /// </summary>
    public void PlayNextLineSE()
    {
        PlaySE(nextLineSE);
    }

    /// <summary>
    /// Auto切替SE
    /// </summary>
    public void PlayAutoSE(bool enable)
    {
        if (enable)
        {
            PlaySE(autoOnSE);
        }
        else
        {
            PlaySE(autoOffSE);
        }
    }

    /// <summary>
    /// Skip切替SE
    /// </summary>
    public void PlaySkipSE(bool enable)
    {
        if (enable)
        {
            PlaySE(skipOnSE);
        }
        else
        {
            PlaySE(skipOffSE);
        }
    }
    #endregion

    /// <summary>
    /// 実際のSE再生処理
    /// </summary>
    void PlaySE(AudioClip clip)
    {
        // SEが設定されていなければ終了
        if (clip == null)
            return;

        // AudioSourceで再生
        audioSource.PlayOneShot(clip, seVolume);
    }
}
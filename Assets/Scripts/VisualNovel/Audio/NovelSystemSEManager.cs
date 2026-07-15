using UnityEngine;

/// <summary>
/// ノベルゲーム用SE管理クラス
/// </summary>
public class NovelSystemSEManager : MonoBehaviour
{
    [Header("AudioSource (システムSE)")]
    [SerializeField] private AudioSource audioSource;

    [Header("SE音量 (AudioMixserあるので不要?)")]
    [SerializeField][Range(0f, 1f)] private float systemVolume = 1f;

    [Space(5)]

    [Header("文字送りSE (処理作ってない)")]
    [SerializeField] private AudioClip textSE;

    [Header("次の行へ進むSE")]
    [SerializeField] private AudioClip nextLineSE;

    [Header("Auto切替SE")]
    [SerializeField] private AudioClip autoOnSE;
    [SerializeField] private AudioClip autoOffSE;

    [Header("Skip切替SE")]
    [SerializeField] private AudioClip skipOnSE;
    [SerializeField] private AudioClip skipOffSE;

    [Header("バックログ")]
    [SerializeField] private AudioClip backLogOpenSE;
    [SerializeField] private AudioClip backLogCloseSE;

    [Header("ウィンドウ表示切替")]
    [SerializeField] private AudioClip messageShowSE;
    [SerializeField] private AudioClip messageHideSE;


    [Header("決定 / キャンセル")]
    [SerializeField] private AudioClip decideSE;
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


    /// <summary>
    /// バックログを開くSE
    /// </summary>
    public void PlayBackLogOpenSE()
    {
        PlaySystemSE(backLogOpenSE);
    }
    /// <summary>
    /// バックログを閉じるSE
    /// </summary>
    public void PlayBackLogCloseSE()
    {
        PlaySystemSE(backLogCloseSE);
    }


    /// <summary>
    /// メッセージウィンドウ表示SE
    /// </summary>
    public void PlayMessageShowSE()
    {
        PlaySystemSE(messageShowSE);
    }
    /// <summary>
    /// メッセージウィンドウ非表示SE
    /// </summary>
    public void PlayMessageHideSE()
    {
        PlaySystemSE(messageHideSE);
    }


    /// <summary>
    /// 決定SE
    /// </summary>
    public void PlayDecideSE()
    {
        PlaySystemSE(decideSE);
    }
    /// <summary>
    /// キャンセルSE
    /// </summary>
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
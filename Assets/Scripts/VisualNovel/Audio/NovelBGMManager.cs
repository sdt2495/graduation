using System.Collections;
using UnityEngine;

/// <summary>
/// ノベルゲーム用BGM管理
/// </summary>
public class NovelBGMManager : MonoBehaviour
{
    [Header("AudioSource (BGM)")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("BGM音量")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 1f;

    [Header("フェード時間")]
    [SerializeField] private float fadeTime = 1f;

    private AudioSource currentSource; // 現在再生しているAudioSource


    private void Awake()
    {
        // ループ再生
        sourceA.loop = true;
        sourceB.loop = true;

        // 片方は消音
        sourceA.volume = bgmVolume;
        sourceB.volume = 0f;

        currentSource = sourceA;
    }



    #region BGM再生

    /// <summary>
    /// CSV指定BGM再生
    /// </summary>
    /// <param name="bgmName">BGMファイル名</param>
    public void PlayBGM(string bgmName)
    {
        // 空なら何もしない
        if (string.IsNullOrEmpty(bgmName))
            return;

        // 再生停止 (CSVでSTOPと指定された場合)
        if (bgmName == "STOP")
        {
            StopBGM();
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>("BGM/" + bgmName);


        if (clip == null)
        {
            Debug.LogWarning("BGMが見つかりません : " + bgmName);
        }

        // 同じ曲ならそのまま
        if (currentSource.clip == clip)
            return;

        // BGM再生
        StartCoroutine(CrossFade(clip));
    }


    /// <summary>
    /// クロスフェード
    /// </summary>
    private IEnumerator CrossFade(AudioClip newClip)
    {
        AudioSource nextSource;

        // nextSource
        if (currentSource == sourceA)
        {
            nextSource = sourceB;
        }
        else
        {
            nextSource = sourceA;
        }


        // 新しいBGM設定 (初期化)
        nextSource.clip = newClip;
        nextSource.time = 0f;
        nextSource.volume = 0f;
        nextSource.Play();

        float time = 0f;

        // クロスフェード
        while (time < fadeTime)
        {
            time += Time.deltaTime;

            float rate = time / fadeTime;

            // 徐々に音を小さく (フェードアウト)
            currentSource.volume = Mathf.Lerp(bgmVolume, 0f, rate);
            // 徐々に音を大きく (フェードイン)
            nextSource.volume = Mathf.Lerp(0f, bgmVolume, rate);

            yield return null;
        }

        // 古いBGM停止
        currentSource.Stop();
        currentSource.volume = 0f;

        // currentSourceを切替
        currentSource = nextSource;
    }
    #endregion


    #region BGM停止

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM()
    {
        StartCoroutine(FadeOut());
    }


    /// <summary>
    /// フェードアウト
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startVolume = currentSource.volume;

        float time = 0f;


        // 徐々に音を小さく (フェードアウト)
        while (time < fadeTime)
        {
            time += Time.deltaTime;

            currentSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);

            yield return null;
        }

        // 再生停止
        currentSource.Stop();
        currentSource.clip = null;
    }
    #endregion
}
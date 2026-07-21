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

    
    private Coroutine crossFadeCoroutine;    // 現在実行中のクロスフェード
    private Coroutine fadeOutCoroutine;      // 現在実行中のフェードアウト


    private void Awake()
    {
        // ループ再生
        sourceA.loop = true;
        sourceB.loop = true;

        // 初期状態
        sourceA.volume = 0f;
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
        // 空欄なら何もしない (現在のBGMをそのまま再生)
        if (string.IsNullOrEmpty(bgmName))
            return;

        // 再生停止 (CSVでSTOPと指定された場合)
        if (bgmName == "STOP")
        {
            StopBGM();
            return;
        }

        // BGM読み込み
        AudioClip clip = Resources.Load<AudioClip>("BGM/" + bgmName);
        if (clip == null)
        {
            Debug.LogWarning("BGMが見つかりません : " + bgmName);
            return;
        }

        // 同じ曲なら何もしない
        if (currentSource.clip == clip && currentSource.isPlaying)
            return;

        // 既存のクロスフェードを停止
        if (crossFadeCoroutine != null)
        {
            StopCoroutine(crossFadeCoroutine);
            crossFadeCoroutine = null;
        }

        // フェードアウト中なら停止
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // BGM変更
        crossFadeCoroutine = StartCoroutine(CrossFade(clip));
    }


    /// <summary>
    /// クロスフェード
    /// </summary>
    private IEnumerator CrossFade(AudioClip newClip)
    {
        AudioSource nextSource;

        // 現在使っていないAudioSource(nextSource)を取得 
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
        // 再生開始
        nextSource.Play();

        // 現在の音量を保存
        float currentStartVolume = currentSource.volume;

        // フェード時間0なら即時切替
        if (fadeTime <= 0f)
        {
            currentSource.Stop();
            currentSource.volume = 0f;

            nextSource.volume = bgmVolume;

            currentSource = nextSource;
            crossFadeCoroutine = null;

            yield break;
        }

        // クロスフェード
        float time = 0f;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / fadeTime);

            // 古いBGMをフェードアウト
            currentSource.volume = Mathf.Lerp(currentStartVolume, 0f, rate);
            // 新しいBGMをフェードイン
            nextSource.volume = Mathf.Lerp(0f, bgmVolume, rate);

            yield return null;
        }

        // 古いBGM停止
        currentSource.Stop();
        currentSource.volume = 0f;
        // 新しいBGMを完全再生
        nextSource.volume = bgmVolume;
        // 現在のAudioSourceを切り替え
        currentSource = nextSource;
        crossFadeCoroutine = null;
    }
    #endregion


    #region BGM停止

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM()
    {
        // クロスフェード中なら停止
        if (crossFadeCoroutine != null)
        {
            StopCoroutine(crossFadeCoroutine);
            crossFadeCoroutine = null;
        }
        // すでにフェードアウト中なら停止
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }
        // フェードアウト開始
        fadeOutCoroutine = StartCoroutine(FadeOut());
    }


    /// <summary>
    /// フェードアウト
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startVolume = currentSource.volume;

        // フェード時間0以下
        if (fadeTime <= 0f)
        {
            currentSource.Stop();
            currentSource.clip = null;
            currentSource.volume = 0f;

            fadeOutCoroutine = null;

            yield break;
        }

        // フェードアウト
        float time = 0f;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / fadeTime);
            currentSource.volume = Mathf.Lerp(startVolume, 0f, rate);

            yield return null;
        }
        // 完全停止
        currentSource.Stop();
        currentSource.clip = null;
        currentSource.volume = 0f;

        fadeOutCoroutine = null;
    }
    #endregion
}
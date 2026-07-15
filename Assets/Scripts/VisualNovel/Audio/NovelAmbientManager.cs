using UnityEngine;

/// <summary>
/// ノベルゲーム用環境音管理
/// </summary>
public class NovelAmbientManager : MonoBehaviour
{
    [Header("AudioSource (環境音)")]
    [SerializeField] private AudioSource audioSource;

    [Header("環境音音量")]
    [SerializeField][Range(0f, 1f)] private float ambientVolume = 1f;


    private void Awake()
    {
        if (audioSource != null)
        {
            // 環境音は基本ループ
            audioSource.loop = true;
        }
        else
        {
            Debug.LogWarning("AudioSource (Ambient) がありません");
        }
    }


    #region CSVから呼び出し

    /// <summary>
    /// CSV指定環境音再生
    /// </summary>
    /// <param name="ambientName">環境音ファイル名</param>
    public void PlayAmbient(string ambientName)
    {
        // 空欄なら何もしない
        if (string.IsNullOrEmpty(ambientName))
            return;

        // 停止
        if (ambientName == "STOP")
        {
            StopAmbient();
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>("Ambient/" + ambientName);

        if (clip == null)
        {
            Debug.LogWarning("環境音が見つかりません : " + ambientName);
            return;
        }

        // 同じ環境音なら再生しない
        if (audioSource.clip == clip)
            return;

        // 環境音再生
        audioSource.clip = clip;
        audioSource.volume = ambientVolume;
        audioSource.Play();
    }


    /// <summary>
    /// 環境音停止
    /// </summary>
    public void StopAmbient()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    #endregion
}
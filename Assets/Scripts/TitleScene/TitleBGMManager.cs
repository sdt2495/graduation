using UnityEngine;

public class TitleBGMManager : MonoBehaviour
{
    [Header("AudioSource (BGM)")]
    [SerializeField] private AudioSource audioSource;

    [Header("タイトルBGM")]
    [SerializeField] private AudioClip bgm;

    private void Start()
    {
        PlayBGM();
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    public void PlayBGM()
    {
        // BGMが設定されていない場合
        if (bgm == null)
        {
            Debug.LogWarning("タイトルBGMが設定されていません");
            return;
        }
        // AudioSourceがない場合
        if (audioSource == null)
        {
            Debug.LogWarning("BGM用AudioSourceが設定されていません");
            return;
        }

        // 初期化
        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
    }
}
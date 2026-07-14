using UnityEngine;

/// <summary>
/// ノベルゲーム演出用SE管理
/// </summary>
public class NovelSEManager : MonoBehaviour
{
    [Header("AudioSource (SE)")]
    [SerializeField] private AudioSource audioSource;

    [Header("SE音量")]
    [SerializeField][Range(0f, 1f)] private float seVolume = 1f;


    #region CSVから呼び出すSE

    /// <summary>
    /// CSV指定SE再生
    /// </summary>
    /// <param name="seName">SEファイル名</param>
    public void PlaySE(string seName)
    {
        // 空なら何もしない
        if (string.IsNullOrEmpty(seName))
            return;

        AudioClip clip = Resources.Load<AudioClip>("SE/" + seName);

        if (clip != null)
        {
            // SE再生
            audioSource.PlayOneShot(clip, seVolume);
        }
        else
        {
            Debug.LogWarning("SEが見つかりません : " + seName);
        }
    }

    #endregion
}
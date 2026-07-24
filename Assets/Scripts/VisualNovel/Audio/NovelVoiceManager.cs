using UnityEngine;

public class NovelVoiceManager : MonoBehaviour
{
    [Header("AudioSource (‰¹º)")]
    [SerializeField] private AudioSource voiceSource;

    public bool IsPlayingVoice => voiceSource.isPlaying; // ‰¹º‚ªÄ¶’†‚©

    /// <summary>
    /// ‰¹º‚ğÄ¶
    /// </summary>
    public void PlayVoice(string voiceName)
    {
        // ‰½‚à‘‚©‚ê‚Ä‚È‚¯‚ê‚Îreturn
        if (string.IsNullOrEmpty(voiceName))
            return;

        AudioClip clip = Resources.Load<AudioClip>("Voice/" + voiceName);
        if (clip != null)
        {
            voiceSource.clip = clip;
            // ‰¹ºÄ¶
            voiceSource.Play();
        }
    }


    /// <summary>
    /// Œ»İÄ¶’†‚Ìƒ{ƒCƒX‚ğ’â~
    /// </summary>
    public void StopVoice()
    {
        if (voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }
        voiceSource.clip = null;
    }
}
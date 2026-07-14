using UnityEngine;

public class NovelVoiceManager : MonoBehaviour
{
    [Header("AudioSource (SE)")]
    [SerializeField] private AudioSource voiceSource;


    public bool IsPlayingVoice => voiceSource.isPlaying; // ‰¹º‚ªÄ¶’†‚©


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
}
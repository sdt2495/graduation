using UnityEngine;

public class ConfigSEManager : MonoBehaviour
{
    [Header("AudioSource (İ’è‰æ–Ê)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Œˆ’èSE")]
    [SerializeField] private AudioClip decisionSE;

    /// <summary>
    /// Œˆ’èSE‚ğÄ¶
    /// </summary>
    public void PlayDecisionSE()
    {
        if (decisionSE == null)
        {
            Debug.LogWarning("Œˆ’èSE‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        audioSource.PlayOneShot(decisionSE);
    }
}
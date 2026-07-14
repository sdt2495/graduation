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
    public float PlayDecisionSE()
    {
        // SE‚ªİ’è‚³‚ê‚Ä‚¢‚È‚¢ê‡
        if (decisionSE == null)
        {
            Debug.LogWarning("Œˆ’èSE‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return 0f;
        }
        // AudioSource‚ª‚È‚¢ê‡
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return 0f;
        }


        // SEÄ¶
        audioSource.PlayOneShot(decisionSE);

        // SE‚Ì’·‚³‚ğ•Ô‚·
        return decisionSE.length;
    }
}
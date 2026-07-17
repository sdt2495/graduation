using UnityEngine;
using UnityEngine.UI;

public class PlayerHPSlot : MonoBehaviour
{
    [SerializeField] private Image HPImage;
    [SerializeField] private Image brokenImage;
    [SerializeField] private Image crossImage;

    private void Awake()
    {
        HPImage.enabled = true;
        brokenImage.enabled = false;
        crossImage.enabled = false;
    }

    /// <summary>
    /// Damage‚ğó‚¯‚½‚Æ‚«‚Ì‰æ‘œ‚ÌØ‚è‘Ö‚¦
    /// </summary>
    public void BreakHaert()
    {
        HPImage.enabled = false;
        brokenImage.enabled = true;
        crossImage.enabled = true;
    }

    /// <summary>
    /// HP‰æ‘œ‚ÌƒŠƒZƒbƒg
    /// </summary>
    public void ResetHP()
    {
        HPImage.enabled = true;
        brokenImage.enabled = false;
        crossImage.enabled = false;
    }
}

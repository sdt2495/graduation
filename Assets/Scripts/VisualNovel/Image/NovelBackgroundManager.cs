using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ƒmƒxƒ‹ƒQ[ƒ€”wŒiŠÇ—
/// </summary>
public class NovelBackgroundManager : MonoBehaviour
{
    [Header("”wŒiImage")]
    [SerializeField] private Image backgroundImage;

    /// <summary>
    /// ”wŒi•ÏX
    /// </summary>
    public void ChangeBackground(string bgName)
    {
        // ‹ó—“‚È‚ç•ÏX‚µ‚È‚¢
        if (string.IsNullOrEmpty(bgName))
            return;

        // ”wŒi‚È‚µ
        if (bgName == "NONE")
        {
            // ”ñ•\¦
            backgroundImage.sprite = null;
            backgroundImage.enabled = false;
            return;
        }

        Sprite sprite = Resources.Load<Sprite>("Background/" + bgName);

        if (sprite == null)
        {
            Debug.LogWarning($"”wŒi‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ : {bgName}");
            return;
        }

        // ”wŒi‰æ‘œ•ÏX
        backgroundImage.enabled = true;
        backgroundImage.sprite = sprite;
    }
}
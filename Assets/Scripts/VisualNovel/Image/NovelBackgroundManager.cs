using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// ƒmƒxƒ‹ƒQ[ƒ€”wŒiŠÇ—
/// </summary>
public class NovelBackgroundManager : MonoBehaviour
{
    [Header("Œ³‚Ì”wŒiImage")]
    [SerializeField] private Image backgroundImage;
    [Header("Ÿ‚Ì”wŒiImage")]
    [SerializeField] private Image nextBackgroundImage;

    [Header("„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ")]
    [Header("Ø‘ÖŠÔ")]
    [SerializeField] private float transitionTime = 0.5f;


    /// <summary>
    /// ”wŒi•ÏX
    /// </summary>
    public IEnumerator ChangeBackground(string bgName, TransitionType transition)
    {
        // ‹ó—“‚È‚ç•ÏX‚µ‚È‚¢
        if (string.IsNullOrEmpty(bgName))
            yield break;

        // ”wŒi‚È‚µ
        if (bgName == "NONE")
        {
            ClearBackground();
            yield break;
        }

        // “Ç‚İ‚İ
        Sprite sprite = Resources.Load<Sprite>("Background/" + bgName);

        if (sprite == null)
        {
            Debug.LogWarning($"”wŒi‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ : {bgName}");
            yield break;
        }

        // •ÏX‰‰o•ªŠò
        switch (transition)
        {
            case TransitionType.Instant:
                ChangeInstant(sprite);
                break;

            case TransitionType.Fade:
                yield return Fade(sprite);
                break;

            case TransitionType.Clock:
                yield return Clock(sprite);
                break;
        }
    }

    #region ‹¤’Êˆ—

    /// <summary>
    /// Œ»İ‚Ì”wŒi‚ğİ’è
    /// </summary>
    void SetCurrentBackground(Sprite sprite)
    {
        backgroundImage.enabled = true;
        backgroundImage.sprite = sprite;
        backgroundImage.color = Color.white;
    }
    /// <summary>
    /// Ÿ‚Ì”wŒi‚ğİ’è
    /// </summary>
    void SetNextBackground(Sprite sprite)
    {
        nextBackgroundImage.enabled = true;
        nextBackgroundImage.sprite = sprite;
        nextBackgroundImage.color = Color.white;
    }

    /// <summary>
    /// ”wŒi‚ğÁ‚·
    /// </summary>
    void ClearBackground()
    {
        // Œ»İ‚Ì”wŒi‚ğÁ‚·
        backgroundImage.sprite = null;
        backgroundImage.enabled = false;
        // Ÿ‚Ì”wŒi‚ğÁ‚·
        nextBackgroundImage.sprite = null;
        nextBackgroundImage.enabled = false;
    }

    /// <summary>
    /// Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚É‚·‚é
    /// </summary>
    void ApplyNextBackground()
    {
        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Éİ’è
        backgroundImage.sprite = nextBackgroundImage.sprite;
        backgroundImage.color = Color.white;
        backgroundImage.enabled = true;
        // Ÿ‚Ì”wŒi‚ğÁ‚·
        nextBackgroundImage.sprite = null;
        nextBackgroundImage.enabled = false;
        nextBackgroundImage.color = Color.white;
        // Image‚Ìİ’è‚ğ‰Šú‰»
        nextBackgroundImage.type = Image.Type.Simple;
        nextBackgroundImage.fillAmount = 1f;
    }
    #endregion


    #region ‰‰oˆ—

    /// <summary>
    /// ˆêu‚Å•\¦
    /// </summary>
    void ChangeInstant(Sprite sprite)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);
        // ‘¦À‚ÉŒ»İ‚Ì”wŒi‚Ö”½‰f
        ApplyNextBackground();
    }


    /// <summary>
    /// ƒtƒF[ƒh
    /// </summary>
    IEnumerator Fade(Sprite sprite)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // Ÿ‚Ì”wŒi‚ğ“§–¾‚É‚·‚é
        Color color = nextBackgroundImage.color;
        color.a = 0f;
        nextBackgroundImage.color = color;

        // ™X‚É•\¦
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / transitionTime);
            nextBackgroundImage.color = color;

            yield return null;
        }
        // •\¦Š®—¹
        color.a = 1f;
        nextBackgroundImage.color = color;

        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();
    }


    /// <summary>
    /// Œv‰ñ‚è
    /// </summary>
    IEnumerator Clock(Sprite sprite)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);
        
        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Radial360;
        nextBackgroundImage.fillOrigin = 2;
        nextBackgroundImage.fillClockwise = true;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / transitionTime);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;

        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();
    }
    #endregion
}
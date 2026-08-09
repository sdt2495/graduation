using System.Collections;
using UnityEngine;
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

    [Header("Character")]
    [SerializeField] private NovelCharacterManager characterManager;

    [Header("„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ")]
    [Header("Ø‘ÖŠÔ")]
    [SerializeField] private float transitionTime = 0.5f;

    private int originalSiblingIndex; // nextBackgroundImage‚ÌŒ³‚ÌHierarchyˆÊ’u


    private void Awake()
    {
        // nextBackgroundImage‚ªŒ³X‚¢‚½Hierarchyã‚ÌˆÊ’u‚ğ‹L‰¯
        originalSiblingIndex = nextBackgroundImage.transform.GetSiblingIndex();
    }

    /// <summary>
    /// ”wŒi•ÏX
    /// </summary>
    public IEnumerator ChangeBackground(string bgName, TransitionType transition, float? customTransitionTime = null)
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

        // ‰‰oŠÔ (CSV‚ÉŠÔw’è‚ª‚ ‚ê‚Î‚»‚ê‚ğg—p,‹ó—“‚È‚çInspector‚ÌƒfƒtƒHƒ‹ƒg’l‚ğg—p)
        // ¦customTransitionTime ‚ª null ‚È‚ç transitionTimeBnull ‚Å‚È‚¯‚ê‚Î customTransitionTimeB
        float time = customTransitionTime ?? transitionTime;

        // •ÏX‰‰o•ªŠò
        switch (transition)
        {
            case TransitionType.Instant:
                ChangeInstant(sprite);
                break;

            case TransitionType.Fade:
                yield return Fade(sprite, time);
                break;

            case TransitionType.Clock:
                yield return Clock(sprite, time);
                break;

            // ¶¨‰E
            case TransitionType.LeftToRight:
                yield return LeftToRight(sprite, time);
                break;
            // ‰E¨¶
            case TransitionType.RightToLeft:
                yield return RightToLeft(sprite, time);
                break;
            // ã¨‰º
            case TransitionType.TopToBottom:
                yield return TopToBottom(sprite, time);
                break;
            // ‰º¨ã
            case TransitionType.BottomToTop:
                yield return BottomToTop(sprite, time);
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


    #region ƒqƒGƒ‰ƒ‹ƒL[ˆÊ’u‚Ì•ÏX


    /// <summary>
    /// ”wŒi‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ‘O–Ê‚Éo‚·
    /// </summary>
    void MoveNextBackgroundToFront()
    {
        // nextBackgroundImage‚ğHierarchy‚Ìˆê”Ô‘O‚ÖˆÚ“®
        nextBackgroundImage.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Ÿ‚Ì”wŒi‚ğ’Êí‚ÌˆÊ’u‚Ö–ß‚·
    /// </summary>
    void RestoreNextBackgroundPosition()
    {
        // nextBackgroundImage‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        nextBackgroundImage.transform.SetSiblingIndex(originalSiblingIndex);
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
    IEnumerator Fade(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // Ÿ‚Ì”wŒi‚ğ“§–¾‚É‚·‚é
        Color color = nextBackgroundImage.color;
        color.a = 0f;
        nextBackgroundImage.color = color;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / duration);
            nextBackgroundImage.color = color;

            yield return null;
        }
        // •\¦Š®—¹
        color.a = 1f;
        nextBackgroundImage.color = color;

        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }


    /// <summary>
    /// Œv‰ñ‚è
    /// </summary>
    IEnumerator Clock(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Radial360;
        nextBackgroundImage.fillOrigin = 2;
        nextBackgroundImage.fillClockwise = true;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;

        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }





    /// <summary>
    /// ¶‚©‚ç‰E‚Ö•\¦
    /// </summary>
    IEnumerator LeftToRight(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Horizontal;
        // ¶‚©‚çŠJn
        nextBackgroundImage.fillOrigin = 0;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;
        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }


    /// <summary>
    /// ‰E‚©‚ç¶‚Ö•\¦
    /// </summary>
    IEnumerator RightToLeft(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Horizontal;
        // ‰E‚©‚çŠJn
        nextBackgroundImage.fillOrigin = 1;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;
        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }


    /// <summary>
    /// ã‚©‚ç‰º‚Ö•\¦
    /// </summary>
    IEnumerator TopToBottom(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Vertical;
        // ã‚©‚çŠJn
        nextBackgroundImage.fillOrigin = 1;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;
        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }


    /// <summary>
    /// ‰º‚©‚çã‚Ö•\¦
    /// </summary>
    IEnumerator BottomToTop(Sprite sprite, float duration)
    {
        // Ÿ‚Ì”wŒi‚Éİ’è
        SetNextBackground(sprite);

        // ‰‰o’†‚¾‚¯Ÿ‚Ì”wŒi‚ğ—§‚¿ŠG‚æ‚è‘O–Ê‚É‚·‚é
        MoveNextBackgroundToFront();

        // ‰æ‘œƒ^ƒCƒv‚ğu“h‚è‚Â‚Ô‚µv‚É•ÏX
        nextBackgroundImage.type = Image.Type.Filled;
        nextBackgroundImage.fillMethod = Image.FillMethod.Vertical;
        // ‰º‚©‚çŠJn
        nextBackgroundImage.fillOrigin = 0;
        // “h‚è‚Â‚Ô‚µ0‚©‚çŠJn
        nextBackgroundImage.fillAmount = 0f;

        // ™X‚É•\¦
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            nextBackgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // •\¦Š®—¹
        nextBackgroundImage.fillAmount = 1f;
        // Ÿ‚Ì”wŒi‚ğŒ»İ‚Ì”wŒi‚Ö
        ApplyNextBackground();

        // Ÿ‚Ì”wŒi‚ğŒ³‚ÌHierarchyˆÊ’u‚Ö–ß‚·
        RestoreNextBackgroundPosition();
        // ”wŒi•ÏXŠ®—¹ŒãA—§‚¿ŠG‚ğ”ñ•\¦
        characterManager?.SetCharactersVisible(false);
    }
    #endregion
}
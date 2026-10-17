
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TradeModeUI : MonoBehaviour
{
    [Header("Buy/Sellボタン")]
    public GameObject modeButtons;
    public GameObject buyButtons;
    public GameObject sellButtons;

    [Header("ボタン本体")]
    public Button buyModeButton;
    public Button sellModeButton;

    [Header("選択中・通常の大きさ")]
    public float selectedScale = 1.2f;
    public float normalScale = 1.0f;

    [Header("アニメーション")]
    public float animationDuration = 0.2f;

    private Coroutine buyAnimation;
    private Coroutine sellAnimation;

    private void Start()
    {
        ShowModeSelect();
    }

    public void ShowModeSelect()
    {
        if (modeButtons != null) modeButtons.SetActive(true);
        if (buyButtons != null) buyButtons.SetActive(false);
        if (sellButtons != null) sellButtons.SetActive(false);

        // 初期状態では両方とも通常サイズ
        AnimateButton(buyModeButton, false, ref buyAnimation);
        AnimateButton(sellModeButton, false, ref sellAnimation);
    }

    public void ShowBuy()
    {
        if (modeButtons != null) modeButtons.SetActive(true);
        if (buyButtons != null) buyButtons.SetActive(true);
        if (sellButtons != null) sellButtons.SetActive(false);

        AnimateButton(buyModeButton, true, ref buyAnimation);
        AnimateButton(sellModeButton, false, ref sellAnimation);
    }

    public void ShowSell()
    {
        if (modeButtons != null) modeButtons.SetActive(true);
        if (buyButtons != null) buyButtons.SetActive(false);
        if (sellButtons != null) sellButtons.SetActive(true);

        AnimateButton(buyModeButton, false, ref buyAnimation);
        AnimateButton(sellModeButton, true, ref sellAnimation);
    }

    private void AnimateButton(
        Button button,
        bool selected,
        ref Coroutine animation)
    {
        if (button == null) return;

        if (animation != null)
        {
            StopCoroutine(animation);
        }

        float targetScale = selected ? selectedScale : normalScale;

        animation = StartCoroutine(
            ScaleAnimation(button.transform, targetScale, animation)
        );
    }

    private IEnumerator ScaleAnimation(
        Transform target,
        float targetScale,
        Coroutine unused)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = new Vector3(
            targetScale,
            targetScale,
            startScale.z
        );

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);

            // なめらかに加速して、なめらかに止まる
            t = t * t * (3f - 2f * t);

            target.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        target.localScale = endScale;
    }
}





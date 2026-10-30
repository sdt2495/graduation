using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TradeModeUI : MonoBehaviour
{
    [Header("共通スクロール")]
    public ScrollRect statusScrollRect;

    [Header("Buy / Sell の一覧")]
    public GameObject modeButtons;
    public GameObject buyButtons;
    public GameObject sellButtons;

    [Header("切り替えボタン")]
    public Button buyModeButton;
    public Button sellModeButton;

    [Header("ボタンの大きさ")]
    public float selectedScale = 1.2f;
    public float normalScale = 1.0f;

    [Header("アニメーション時間")]
    public float animationDuration = 0.2f;

    private Coroutine buyAnimation;
    private Coroutine sellAnimation;

    private void Start()
    {
        ShowModeSelect();
    }

    // Buy / Sell の選択画面
    public void ShowModeSelect()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (buyButtons != null)
            buyButtons.SetActive(false);

        if (sellButtons != null)
            sellButtons.SetActive(false);

        AnimateButton(buyModeButton, false, ref buyAnimation);
        AnimateButton(sellModeButton, false, ref sellAnimation);
    }

    // Buy の一覧を表示
    public void ShowBuy()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (sellButtons != null)
            sellButtons.SetActive(false);

        if (buyButtons != null)
            buyButtons.SetActive(true);

        RefreshScroll();

        AnimateButton(buyModeButton, true, ref buyAnimation);
        AnimateButton(sellModeButton, false, ref sellAnimation);
    }

    // Sell の一覧を表示
    public void ShowSell()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (buyButtons != null)
            buyButtons.SetActive(false);

        if (sellButtons != null)
            sellButtons.SetActive(true);

        RefreshScroll();

        AnimateButton(buyModeButton, false, ref buyAnimation);
        AnimateButton(sellModeButton, true, ref sellAnimation);
    }

    // 一覧切り替え後にレイアウトとスクロール位置を更新
    private void RefreshScroll()
    {
        Canvas.ForceUpdateCanvases();

        if (statusScrollRect != null)
        {
            statusScrollRect.StopMovement();
            statusScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }
    }

    private void AnimateButton(
        Button button,
        bool selected,
        ref Coroutine animation)
    {
        if (button == null)
            return;

        if (animation != null)
            StopCoroutine(animation);

        float targetScale =
            selected ? selectedScale : normalScale;

        animation = StartCoroutine(
            ScaleAnimation(button.transform, targetScale)
        );
    }

    private IEnumerator ScaleAnimation(
        Transform target,
        float targetScale)
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

            float t = Mathf.Clamp01(
                elapsed / animationDuration
            );

            // なめらかに拡大・縮小
            t = t * t * (3f - 2f * t);

            target.localScale =
                Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        target.localScale = endScale;
    }
}






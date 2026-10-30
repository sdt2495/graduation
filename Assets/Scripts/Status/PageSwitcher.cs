
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [Header("ページ")]
    public GameObject page1;
    public GameObject page2;

    [Header("ページ切り替えボタン")]
    public Button mainModeButton;
    public Button subModeButton;

    [Header("ボタンの大きさ")]
    public float selectedScale = 1.2f;
    public float normalScale = 1.0f;

    [Header("アニメーション")]
    public float animationDuration = 0.2f;

    private Coroutine mainAnimation;
    private Coroutine subAnimation;

    private void Start()
    {
        ShowPage1();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowPage1();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowPage2();
        }
    }

    public void ShowPage1()
    {
        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);

        AnimateButton(mainModeButton, true, ref mainAnimation);
        AnimateButton(subModeButton, false, ref subAnimation);
    }

    public void ShowPage2()
    {
        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);

        AnimateButton(mainModeButton, false, ref mainAnimation);
        AnimateButton(subModeButton, true, ref subAnimation);
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

            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = t * t * (3f - 2f * t);

            target.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        target.localScale = endScale;
    }
}



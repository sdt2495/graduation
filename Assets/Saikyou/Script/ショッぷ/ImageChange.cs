
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageChange : MonoBehaviour
{
    public Image spriteImage;
    public RectTransform panel;

    public float duration = 0.5f;
    public float moveDistance = 300f;

    private RectTransform spriteRect;

    private Vector2 spriteOriginalPos;
    private Vector2 panelOriginalPos;

    private bool isChanging = false;
    private bool showingPanel = false;

    [Header("登場位置調整")]
    public float startOffsetX = 150f;

    void Start()
    {
        spriteRect = spriteImage.GetComponent<RectTransform>();

        spriteOriginalPos = spriteRect.anchoredPosition;
        panelOriginalPos = panel.anchoredPosition;

        panel.gameObject.SetActive(false);
    }

    public void ChangeToPanel()
    {
        if (!isChanging && !showingPanel)
            StartCoroutine(SlideChange(true));
    }

    public void ChangeToSprite()
    {
        if (!isChanging && showingPanel)
            StartCoroutine(SlideChange(false));
    }

    public void ChangeImage()
    {
        if (isChanging) return;

        StartCoroutine(SlideChange(!showingPanel));
    }

    IEnumerator SlideChange(bool toPanel)
    {
        isChanging = true;

        if (toPanel)
        {
            panel.gameObject.SetActive(true);
            panel.anchoredPosition =
                panelOriginalPos + new Vector2(
                    moveDistance - startOffsetX, 0
                );
        }
        else
        {
            spriteImage.gameObject.SetActive(true);
            spriteRect.anchoredPosition =
                spriteOriginalPos + new Vector2(
                    -moveDistance - startOffsetX, 0
                );
        }

        float time = 0f;

        Color spriteColor = spriteImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            if (toPanel)
            {
                // Spriteは左へ
                spriteRect.anchoredPosition =
                    Vector2.Lerp(
                        spriteOriginalPos,
                        spriteOriginalPos + Vector2.left * moveDistance,
                        t
                    );

                spriteColor.a = 1f - t;
                spriteImage.color = spriteColor;

                // Panelは右から中央へ
                panel.anchoredPosition =
                    Vector2.Lerp(
                        panelOriginalPos + new Vector2(
                            moveDistance - startOffsetX, 0
                        ),
                        panelOriginalPos,
                        t
                    );

                SetPanelAlpha(t);
            }
            else
            {
                // Panelは右へ
                panel.anchoredPosition =
                    Vector2.Lerp(
                        panelOriginalPos,
                        panelOriginalPos + Vector2.right * moveDistance,
                        t
                    );

                SetPanelAlpha(1f - t);

                // Spriteは左から中央へ
                spriteRect.anchoredPosition =
                    Vector2.Lerp(
                        spriteOriginalPos + new Vector2(
                            -moveDistance - startOffsetX, 0
                        ),
                        spriteOriginalPos,
                        t
                    );

                spriteColor.a = t;
                spriteImage.color = spriteColor;
            }

            yield return null;
        }

        // 最終位置
        spriteRect.anchoredPosition = spriteOriginalPos;
        panel.anchoredPosition = panelOriginalPos;

        spriteColor.a = 1f;
        spriteImage.color = spriteColor;

        SetPanelAlpha(1f);

        // 不要な側を非表示
        spriteImage.gameObject.SetActive(!toPanel);
        panel.gameObject.SetActive(toPanel);

        showingPanel = toPanel;
        isChanging = false;
    }

    void SetPanelAlpha(float alpha)
    {
        Graphic g = panel.GetComponent<Graphic>();

        if (g != null)
        {
            Color c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }
}

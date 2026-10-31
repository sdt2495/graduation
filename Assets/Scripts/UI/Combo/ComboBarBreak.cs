using System;
using System.Collections;
using UnityEngine;

public class ComboBarBreak : MonoBehaviour
{
    [Header("左右のバー")]
    [SerializeField] private RectTransform leftPiece;
    [SerializeField] private RectTransform rightPiece;

    [Header("透明度")]
    [SerializeField] private CanvasGroup leftGroup;
    [SerializeField] private CanvasGroup rightGroup;

    [Header("演出全体")]
    [SerializeField] private GameObject breakEffect;
    [SerializeField] private float duration = 0.55f;

    private Vector2 leftStart;
    private Vector2 rightStart;
    private Coroutine currentAnimation;

    private void Awake()
    {
        leftStart = leftPiece.anchoredPosition;
        rightStart = rightPiece.anchoredPosition;

        ResetVisual();
        breakEffect.SetActive(false);
    }

    public void PlayBreak()
    {
        if(currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        ResetVisual();
        breakEffect.SetActive(true);

        currentAnimation = StartCoroutine(BreakAnimation());
    }

    private IEnumerator BreakAnimation()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            // 左側:左上へ
            leftPiece.anchoredPosition = Vector2.Lerp(leftStart, leftStart + new Vector2(-70f, 40f), ease);

            // 右側:右上へ
            rightPiece.anchoredPosition = Vector2.Lerp(rightStart, rightStart + new Vector2(70f, 35f), ease);

            // 回転
            leftPiece.localRotation = Quaternion.Euler(0f, 0f, -20f * ease);
            rightPiece.localRotation = Quaternion.Euler(0f, 0f, 18f * ease);

            // フェードアウト
            float alpha = 1f - t;
            leftGroup.alpha = alpha;
            rightGroup.alpha = alpha;

            yield return null;
        }

        breakEffect.SetActive(false);
        ResetVisual();

        currentAnimation = null;
    }

    private void ResetVisual()
    {
        leftPiece.anchoredPosition = leftStart;
        rightPiece.anchoredPosition = rightStart;

        leftPiece.localRotation = Quaternion.identity;
        rightPiece.localRotation = Quaternion.identity;

        leftGroup.alpha = 1f;
        rightGroup.alpha = 1f;
    }
}

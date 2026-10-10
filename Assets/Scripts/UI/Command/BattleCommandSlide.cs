using System;
using System.Collections;
using UnityEngine;

public class BattleCommandSlide : MonoBehaviour
{
    [Header("スライド設定")]
    [SerializeField] private RectTransform commandPanel;
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("スライド調整")]
    [SerializeField] private float slideEase = 3f;

    private Vector2 targetPosition;

    private void Start()
    {
        targetPosition = commandPanel.anchoredPosition;

        // 初期位置を画面下にする
        commandPanel.anchoredPosition = targetPosition + Vector2.down * slideDistance;

        // 出現アニメーション
        ShowCommand();
    }

    /// <summary>
    /// 選択コマンドを下からスライドさせて表示する
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ShowCommand()
    {
        StopAllCoroutines();
        StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        float time = 0;

        Vector2 startPosition = commandPanel.anchoredPosition;

        while (time < slideDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / slideDuration);

            // 最初は速く、目標位置に近づくほど遅くする
            t = 1f - Mathf.Pow(1f - t , slideEase);

            commandPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        commandPanel.anchoredPosition = targetPosition;
    }
}

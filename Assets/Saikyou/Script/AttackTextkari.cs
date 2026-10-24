
using UnityEngine;
using TMPro;
using System.Collections;

public class AttackTextkari : MonoBehaviour
{
    public TextMeshProUGUI text;

    [Header("文字の色")]
    public Color criticalColor = Color.yellow;
    public Color counterColor = Color.red;
    public Color finishBlowColor = Color.blue;

    [Header("アニメーション設定")]
    public float showDuration = 0.3f;

    [Tooltip("文字を表示しておく時間")]
    public float displayDuration = 1.5f;

    [Tooltip("元の位置に戻る時間")]
    public float returnDuration = 0.3f;

    [Header("移動設定")]
    public Transform enemyPoint;
    public RectTransform targetPoint;

    private RectTransform rect;
    private Coroutine animationCoroutine;

    public void SetEnemyPoint(Transform point)
    {
        enemyPoint = point;
    }

    public void ShowText(string message)
    {
        if (text == null)
        {
            Debug.LogError("文字表示用Textが設定されていません！");
            return;
        }

        if (enemyPoint == null || targetPoint == null)
        {
            Debug.LogError("移動元または移動先が設定されていません！");
            return;
        }

        rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogError("RectTransformがありません！");
            return;
        }

        text.gameObject.SetActive(true);
        text.text = message;

        switch (message)
        {
            case "CRITICAL!":
                text.color = criticalColor;
                break;

            case "COUNTER!":
                text.color = counterColor;
                break;

            case "FINISHBLOW!":
                text.color = finishBlowColor;
                break;

            default:
                text.color = Color.white;
                break;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 敵の位置をCanvas内の座標に変換
        Vector2 enemyScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                enemyPoint.position
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            enemyScreenPosition,
            null,
            out Vector2 startPosition
        );

        // 指定したUI位置をCanvas内の座標に変換
        Vector2 targetScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                targetPoint.position
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            targetScreenPosition,
            null,
            out Vector2 endPosition
        );

        // 敵の位置からスタート
        rect.anchoredPosition = startPosition;

        Color color = text.color;
        color.a = 0f;
        text.color = color;

        float timer = 0f;

        // 登場：敵から指定位置へ移動しながらフェードイン
        while (timer < showDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / showDuration);

            // 滑らかに減速
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                eased
            );

            color.a = t;
            text.color = color;

            yield return null;
        }

        rect.anchoredPosition = endPosition;
        color.a = 1f;
        text.color = color;

        // 指定した秒数だけ表示
        yield return new WaitForSeconds(displayDuration);

        // 退場：登場時と逆方向に戻りながらフェードアウト
        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / returnDuration);

            // 滑らかに加速して戻る
            float eased = t * t;

            rect.anchoredPosition = Vector2.Lerp(
                endPosition,
                startPosition,
                eased
            );

            color.a = 1f - t;
            text.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}

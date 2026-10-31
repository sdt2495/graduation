
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

    [Header("出現位置設定")]
    public float spawnOffsetX = -200f;
    public float criticalOffsetY = 30f;

    [Header("サイズアニメーション")]
    public float normalScale = 1.0f;
    public float maxScale = 1.2f;
    public float popDuration = 0.15f;
    public float stopDuration = 0.1f;

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

        // 敵の位置から右側にずらす
        startPosition += new Vector2(spawnOffsetX, 0f);

        // クリティカルの場合だけ少し上にずらす
        if (text.text == "CRITICAL!")
        {
            startPosition.y += criticalOffsetY;
        }

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

        // 移動先のY座標を出現位置と統一する
        endPosition.y = startPosition.y;

        // 敵の右側からスタート
        rect.anchoredPosition = startPosition;

        // 通常サイズでスタート
        rect.localScale = Vector3.one * normalScale;

        yield return new WaitForSeconds(stopDuration);

        Color color = text.color;
        color.a = 1f;
        text.color = color;

        float timer = 0f;

        // その場で大きくなって元に戻る
        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / popDuration);

            float scale;

            if (t < 0.3f)
            {
                float scaleT = t / 0.3f;
                scale = Mathf.Lerp(normalScale, maxScale, scaleT);
            }
            else
            {
                float scaleT = (t - 0.3f) / 0.7f;
                scale = Mathf.Lerp(maxScale, normalScale, scaleT);
            }

            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        // 元のサイズに戻す
        rect.localScale = Vector3.one * normalScale;

        // 元のサイズで一瞬停止
        yield return new WaitForSeconds(stopDuration);

        rect.localScale = Vector3.one * normalScale;

        timer = 0f;

        while (timer < showDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / showDuration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                eased
            );

            yield return null;
        }

        // yield return new WaitForSeconds(displayDuration); 左に移動するやつ消すなら上のWhile文消してこれ入れる

        rect.anchoredPosition = endPosition;
        // 指定した秒数だけ表示
        yield return new WaitForSeconds(displayDuration);

        // 退場：左に移動しながらフェードアウト
        timer = 0f;

        // 現在の位置を保存
        Vector2 exitStartPosition = rect.anchoredPosition;

        // 左上に移動する距離
        Vector2 exitEndPosition = exitStartPosition + new Vector2(-200f, 150f);

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / returnDuration);

            // 滑らかに加速
            float eased = t * t;

            // 左に移動
            rect.anchoredPosition = Vector2.Lerp(
                exitStartPosition,
                exitEndPosition,
                eased
            );

            // フェードアウト
            color.a = 1f - t;
            text.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}

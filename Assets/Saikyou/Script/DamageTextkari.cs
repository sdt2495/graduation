using UnityEngine;
using TMPro;
using System.Collections;

public class DamageTextkari : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI attackTypeText;

    [Header("移動設定")]
    public float moveDuration = 0.6f;
    // 左右の移動距離の区間
    public float minMoveDistanceX = -40f;
    public float maxMoveDistanceX = 40f;

    public float moveHeight = 80f;

    private Vector2 startPosition;
    private Color startColor;
    private float randomMoveX;
    private bool currentIsCritical;
    private int currentDamage;

    [Header("数字の大きさ")]
    public float baseFontSize = 30f;
    public float fontSizePerDamage = 2f;
    public float maxFontSize = 80f;

    public float criticalStartScale = 0.4f; // 本来のサイズの%
    public float criticalScaleDuration = 0.2f; // 大きくなる時間
    public float criticalPunchScale = 1.2f; // 最大で本来のサイズの%
    public float criticalPunchDuration = 0.1f; // 大きくなって戻る時間

    [Header("ダメージの色")]
    public Color normalColor = Color.white;
    public Color criticalColor = Color.yellow;
    public Color counterColor = Color.red;
    public Color finishBlowColor = Color.cyan;


    public void ShowDamage(int damage, bool isCritical, string attackType = "Normal")
    {
        currentDamage = damage;
        currentIsCritical = isCritical;

        // 数字は数字用TextMeshProに表示
        text.text = damage.ToString();

        // 文字は別のTextMeshProに表示
        if (attackTypeText != null)
        {
            attackTypeText.gameObject.SetActive(true);

            if (attackType == "Counter")
            {
                attackTypeText.text = "Counter!";
                attackTypeText.color = counterColor;
            }
            else if (attackType == "FinishBlow")
            {
                attackTypeText.text = "Finish Blow!";
                attackTypeText.color = finishBlowColor;
            }
            else if (isCritical)
            {
                attackTypeText.text = "Critical!";
                attackTypeText.color = criticalColor;
            }
            else
            {
                attackTypeText.text = "";
            }

            // 文字を不透明に戻す
            Color labelColor = attackTypeText.color;
            labelColor.a = 1f;
            attackTypeText.color = labelColor;

            attackTypeText.fontSize = 24f;
        }
        else
        {
            Debug.LogError("AttackTypeTextが設定されていません！");
        }

        // 数字の色
        switch (attackType)
        {
            case "Counter":
                text.color = counterColor;
                break;

            case "FinishBlow":
                text.color = finishBlowColor;
                break;

            default:
                text.color = isCritical ? criticalColor : normalColor;
                break;
        }

        // ダメージに応じた数字のサイズ
        text.fontSize = Mathf.Min(
            baseFontSize + damage * fontSizePerDamage,
            maxFontSize
        );

        startColor = text.color;

        RectTransform rect = GetComponent<RectTransform>();
        startPosition = rect.anchoredPosition;

        randomMoveX = Random.Range(minMoveDistanceX, maxMoveDistanceX);

        StartCoroutine(AnimateDamageText(rect));
    }

    private IEnumerator AnimateDamageText(RectTransform rect)
    {
        float timer = 0f;

        // クリティカルなら最初は小さくする
        if (currentIsCritical)
        {
            text.fontSize *= criticalStartScale;
        }

        float targetFontSize = Mathf.Min(baseFontSize + currentDamage * fontSizePerDamage,maxFontSize);

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);

            float x = randomMoveX * t;
            float y = moveHeight * 4f * t * (1f - t);

            rect.anchoredPosition =
                startPosition + new Vector2(x, y);
            // クリティカル時の拡大演出
            if (currentIsCritical)
            {
                if (timer < criticalScaleDuration)
                {
                    // 小さいサイズから本来のサイズへ
                    float scaleT = Mathf.Clamp01(
                        timer / criticalScaleDuration
                    );

                    scaleT = 1f - Mathf.Pow(1f - scaleT, 3f);

                    text.fontSize = Mathf.Lerp(
                        targetFontSize * criticalStartScale,
                        targetFontSize,
                        scaleT
                    );
                }
                else if (timer < criticalScaleDuration + criticalPunchDuration)
                {
                    // 本来のサイズから少し大きくなり、元に戻る
                    float punchT = Mathf.Clamp01(
                        (timer - criticalScaleDuration) / criticalPunchDuration
                    );

                    // 0 → 1 → 0 と変化させる
                    float punch = Mathf.Sin(punchT * Mathf.PI);

                    text.fontSize = targetFontSize * Mathf.Lerp(
                        1f,
                        criticalPunchScale,
                        punch
                    );
                }
                else
                {
                    // 本来のサイズを維持
                    text.fontSize = targetFontSize;
                }
            }

            Color color = startColor;
            color.a = 1f - t;
            text.color = color;

            if (attackTypeText != null)
            {
                Color labelColor = attackTypeText.color;
                labelColor.a = 1f - t;
                attackTypeText.color = labelColor;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
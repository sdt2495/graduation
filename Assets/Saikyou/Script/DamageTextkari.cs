
using UnityEngine;
using TMPro;
using System.Collections;

public class DamageTextkari : MonoBehaviour
{
    public TextMeshProUGUI text;

    [Header("移動設定")]
    public float moveDuration = 0.6f;
    public float moveDistanceX = 40f;
    public float moveHeight = 80f;

    private Vector2 startPosition;
    private Color startColor;

    [Header("数字の大きさ")]
    public float baseFontSize = 30f;  //　基本の文字サイズ
    public float fontSizePerDamage = 2f;  //　ダメージに応じて増えるサイズ
    public float maxFontSize = 80f;  //　最大サイズ

    public void ShowDamage(int damage, bool isCritical)
    {
        text.text = damage.ToString();

        if (isCritical)
        {
            text.color = Color.yellow;
        }

        text.fontSize = Mathf.Min(baseFontSize + damage *  fontSizePerDamage, maxFontSize);

        startColor = text.color;

        RectTransform rect = GetComponent<RectTransform>();
        startPosition = rect.anchoredPosition;

        StartCoroutine(AnimateDamageText(rect));
    }

    private IEnumerator AnimateDamageText(RectTransform rect)
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / moveDuration);

            float x = moveDistanceX * t;
            float y = moveHeight * 4f * t * (1f - t);

            rect.anchoredPosition =
                startPosition + new Vector2(x, y);

            Color color = startColor;
            color.a = 1f - t;
            text.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}

using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 100f;
    public float upwardSpeed = 80f;

    [Header("表示時間")]
    public float lifeTime = 0.7f;

    [Header("サイズ")]
    public float normalScale = 1.0f;
    public float specialScale = 1.5f;

    private TextMeshProUGUI damageText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private float timer = 0f;

    // ダメージが飛んでいく方向
    private float moveDirection = 1f;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // CanvasGroupがなければ自動追加
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        // 左右＋上方向へ移動
        rectTransform.anchoredPosition += new Vector2(
            moveDirection * moveSpeed,
            upwardSpeed
        ) * Time.deltaTime;

        // 経過時間
        timer += Time.deltaTime;

        // 徐々に透明にする
        float alpha = 1f - (timer / lifeTime);
        canvasGroup.alpha = Mathf.Clamp01(alpha);

        // 表示時間終了
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(
        int damage,
        DamageType damageType,
        float direction
    )
    {
        // ダメージ数字
        damageText.text = damage.ToString();

        // キャラクターの進行方向とは逆方向
        moveDirection = -Mathf.Sign(direction);

        // 種類ごとの設定
        switch (damageType)
        {
            case DamageType.Normal:

                // 白
                damageText.color = Color.white;

                // 通常サイズ
                rectTransform.localScale =
                    Vector3.one * normalScale;

                break;

            case DamageType.Critical:

                // 黄色
                damageText.color = Color.yellow;

                // 大きい
                rectTransform.localScale =
                    Vector3.one * specialScale;

                break;

            case DamageType.Counter:

                // 赤
                damageText.color = Color.red;

                // 大きい
                rectTransform.localScale =
                    Vector3.one * specialScale;

                break;

            case DamageType.Finisher:

                // 青
                damageText.color = Color.blue;

                // 大きい
                rectTransform.localScale =
                    Vector3.one * specialScale;

                break;
        }
    }

    // ==========================================
    // 追加クリティカルダメージ表記
    // ==========================================

    public void SetupAdditionalCritical(
        int damage,
        float direction
    )
    {
        // 「+」を付ける
        damageText.text =
            "+" + damage.ToString();

        // キャラクターの進行方向とは逆方向
        moveDirection = -Mathf.Sign(direction);

        // 黄色
        damageText.color = Color.yellow;

        // 大きいサイズ
        rectTransform.localScale =
            Vector3.one * specialScale;
    }
}
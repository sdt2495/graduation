using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public enum CommandType
{
    Up,
    Down,
    Left,
    Right
}

public enum CheckResult
{
    Success,   // 途中成功
    Complete,  // 全部成功
    Miss       // 失敗
}


public class Enemy : MonoBehaviour
{
    private List<CommandType> currentCommaneds = new List<CommandType>();
    private int currentIndex = 0;

    // ミスしたコマンドのindex
    private int missIndex = -1;

    [Header("HP")]
    [SerializeField] private int maxHP = 10;

    [Header("ダメージ表示")]

    public GameObject damageTextPrefab;

    public Transform damagePoint;

    public Canvas targetCanvas;

    [Header("クリティカル設定")]
    [Range(0, 100)]
    public float criticalChance = 20f;
    public float criticalMultiplier = 1.2f;

    private int currentHP;

    [Header("ダメージフラッシュ")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color flashColor = Color.red;

    private Coroutine coroutine;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // 外部参照用
    public int CurrentHP {  get { return currentHP; } }
    public int MaxHP {  get { return maxHP; } }

    /// <summary>
    /// 敵がダメージを受ける処理
    /// </summary>
    /// <param name="damage"></param>
    public void Damage(int damage)
    {
        // クリティカル判定（敵側で実行）
        bool isCritical = Random.Range(0f, 100f) < criticalChance;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
        }

        currentHP -= damage;

        if(currentHP < 0)
        {
            currentHP = 0;
        }

        // 白くフラッシュ
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(FlashWhite());

        // ダメージ表示
        if (damageTextPrefab != null && damagePoint != null && targetCanvas != null)
        {
            GameObject obj = Instantiate(
                damageTextPrefab,
                targetCanvas.transform,
                false
            );

            RectTransform rect = obj.GetComponent<RectTransform>();

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                null,
                damagePoint.position
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.GetComponent<RectTransform>(),
                screenPos,
                null,
                out Vector2 localPos
            );

            rect.anchoredPosition = localPos;

            DamageTextkari damageText = obj.GetComponent<DamageTextkari>();

            if (damageText != null)
            {
                damageText.ShowDamage(damage, isCritical);
            }
        }
    }

    /// <summary>
    /// 敵が死亡しているか
    /// </summary>
    /// <returns></returns>
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    /// <summary>
    /// 敵の死亡処理
    /// </summary>
    public void Die()
    {
        // 敵の死亡処理
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = normalColor;

        coroutine = null;
    }

    public void ResetCommand()
    {
        currentIndex = 0;
        missIndex = -1;
    }

    /// <summary>
    /// 正しい入力がされたらクリア
    /// </summary>
    /// <param name="inputcommaned"></param>
    /// <returns></returns>
    public CheckResult Check(CommandType inputcommaned)
    {
        if (inputcommaned == currentCommaneds[currentIndex])
        {
            currentIndex++;

            // 全成功
            if (currentIndex >= currentCommaneds.Count)
            {
                return CheckResult.Complete;
            }

            // 途中成功
            return CheckResult.Success;
        }

        // ミスした場所を記録
        missIndex = currentIndex;

        // ミスしたコマンドをスキップ
        currentIndex++;

        return CheckResult.Miss;
    }

    /// <summary>
    /// ダメージポップアップ用キャンバス
    /// </summary>
    public void SetDamageCanvas(Canvas canvas)
    {
        targetCanvas = canvas;
    }

    public void SetRandomCommands()
    {
        currentCommaneds.Clear();
        currentIndex = 0;
        missIndex = -1;

        int count = Random.Range(1, 4);

        for(int i = 0; i < count; i++)
        {
            currentCommaneds.Add((CommandType)Random.Range(0, 4));
        }
    }

    public void SetCommands(List<CommandType> commands)
    {
        currentCommaneds.Clear();
        currentCommaneds.AddRange(commands);

        currentIndex = 0;
        missIndex = -1;
    }

    public List<CommandType> GetCommands() { return currentCommaneds; }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public int GetMissIndex()
    {
        return missIndex;
    }

    public bool IsCommandFinished()
    {
        return currentIndex >= currentCommaneds.Count;
    }
}

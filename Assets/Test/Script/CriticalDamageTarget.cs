using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CriticalDamageTarget : MonoBehaviour
{
    [Header("テスト用ダメージ設定")]
    [Tooltip("DamageTestControllerからダメージを受け取る間隔")]
    public float damageInterval = 1.0f;

    [Header("テスト用 ON / OFF")]
    [Tooltip("現在はテスト用。Player完成後は不要になります")]
    public DamageTestController damageTestController;

    [Header("通常ダメージ")]
    [Tooltip("通常ダメージを表示するDamageTarget")]
    public DamageTarget damageTarget;

    [Header("ダメージ表記")]
    [Tooltip("追加クリティカルのDamageText Prefab")]
    public GameObject damageTextPrefab;

    [Tooltip("ダメージ数字を生成する親")]
    public Transform damageTextParent;

    [Header("ダメージ制限")]
    [Tooltip("追加クリティカルダメージの最大値")]
    public int maxDamage = 50;

    [Header("クリティカル設定")]
    [Tooltip("追加クリティカルが発生する確率（％）")]
    [Range(0f, 100f)]
    public float criticalChance = 30f;

    [Tooltip("通常ダメージから追加クリティカルが出るまでの時間")]
    public float criticalDelay = 0.2f;

    [Header("進行方向")]
    [Tooltip("1 = 右方向、-1 = 左方向")]
    public float moveDirection = 1f;


    private float damageTimer = 0f;


    private void Update()
    {
        // テスト用ダメージ処理

        if (damageTestController != null)
        {
            UpdateDamageTest();
        }
    }


    // ==========================================
    // テスト用ダメージ処理
    // ==========================================

    private void UpdateDamageTest()
    {
        // OFFの場合
        if (!damageTestController.IsDamageTestEnabled())
        {
            damageTimer = 0f;
            return;
        }


        // ONの場合
        damageTimer += Time.deltaTime;


        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;


            // 仮の通常ダメージ値
            int normalDamage =
                damageTestController.GetDamage();


            // ------------------------------
            // まず通常ダメージを表示
            // ------------------------------

            if (damageTarget != null)
            {
                damageTarget.TakeDamage(
                    normalDamage,
                    DamageType.Normal
                );
            }


            // ------------------------------
            // クリティカル判定
            // ------------------------------

            if (Random.Range(0f, 100f) < criticalChance)
            {
                // 仮の追加クリティカルダメージ
                int criticalDamage =
                    damageTestController.GetDamage();


                // 少し遅れて表示
                StartCoroutine(
                    DelayedCriticalDamage(
                        criticalDamage
                    )
                );
            }
        }
    }


    // ==========================================
    // 遅れて追加クリティカルを表示
    // ==========================================

    private IEnumerator DelayedCriticalDamage(
        int damageAmount
    )
    {
        // 通常ダメージが表示されるまで待つ
        yield return new WaitForSeconds(
            criticalDelay
        );


        // 追加クリティカルを表示
        TakeCriticalDamage(
            damageAmount
        );
    }


    // ==========================================
    // 追加クリティカルダメージ
    // ==========================================

    public void TakeCriticalDamage(
        int damageAmount
    )
    {
        // ダメージ値を1～50に制限

        damageAmount = Mathf.Clamp(
            damageAmount,
            1,
            maxDamage
        );


        // ダメージ表記を生成

        CreateCriticalDamageText(
            damageAmount
        );
    }


    // ==========================================
    // クリティカルダメージ表記生成
    // ==========================================

    private void CreateCriticalDamageText(
        int damageAmount
    )
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning(
                "CriticalDamageTarget : " +
                "DamageTextPrefabが設定されていません。"
            );

            return;
        }


        Transform parent = damageTextParent;


        if (parent == null)
        {
            parent = transform.parent;
        }


        GameObject damageTextObject =
            Instantiate(
                damageTextPrefab,
                transform.position,
                Quaternion.identity,
                parent
            );


        DamageText damageText =
            damageTextObject.GetComponent<DamageText>();


        if (damageText == null)
        {
            Debug.LogWarning(
                "CriticalDamageTarget : " +
                "DamageTextPrefabに" +
                "DamageTextコンポーネントがありません。"
            );

            return;
        }


        // 追加クリティカル専用の表示

        damageText.SetupAdditionalCritical(
            damageAmount,
            moveDirection
        );
    }
}
using UnityEngine;
using UnityEngine.UI;

public class DamageTarget : MonoBehaviour
{
    [Header("テスト用ダメージ設定")]
    [Tooltip("DamageTestControllerからダメージを受け取る間隔")]
    public float damageInterval = 1.0f;

    [Header("テスト用 ON / OFF")]
    [Tooltip("現在はテスト用。Player完成後は不要になります")]
    public DamageTestController damageTestController;

    [Header("テスト用ダメージ種類")]
    [Tooltip("Player完成後はTakeDamage()から直接指定します")]
    public DamageType damageType = DamageType.Normal;

    [Header("ダメージ表記")]
    [Tooltip("ダメージ数字のPrefab")]
    public GameObject damageTextPrefab;

    [Tooltip("ダメージ数字を生成する親")]
    public Transform damageTextParent;

    [Header("ダメージ制限")]
    [Tooltip("ダメージ表記の最大値")]
    public int maxDamage = 50;

    [Header("進行方向")]
    [Tooltip("1 = 右方向、-1 = 左方向")]
    public float moveDirection = 1f;

    [Header("ダメージ演出")]
    [Tooltip("ダメージを受けたときに赤く光る時間")]
    public float damageFlashTime = 0.1f;


    private float damageTimer = 0f;

    //キャラクターのImage
    private Image targetImage;

    //元の色
    private Color originalColor;

    //赤く光っている残り時間
    private float damageFlashTimer = 0f;


    private void Start()
    {
        targetImage = GetComponent<Image>();

        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }


    private void Update()
    {
        // テスト用ダメージ処理

        if (damageTestController != null)
        {
            UpdateDamageTest();
        }

        UpdateDamageFlash();
    }

    // テスト用ダメージ処理

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

            //仮のダメージ値を取得
            int damageAmount =
                damageTestController.GetDamage();

            //仮のダメージを表示
            TakeDamage(
                damageAmount,
                damageType
            );
        }
    }

    // ダメージを受け取る

    /// <param name="damageAmount">ダメージ量</param>
    /// <param name="type">ダメージ種類</param>
    public void TakeDamage(
        int damageAmount,
        DamageType type
    )
    {

        // ダメージ値を1～50に制限

        damageAmount = Mathf.Clamp(
            damageAmount,
            1,
            maxDamage
        );

        DamageFlash();

        // ダメージ表記を生成

        CreateDamageText(
            damageAmount,
            type
        );
    }

    // ダメージフラッシュ

    private void DamageFlash()
    {
        if (targetImage == null)
        {
            return;
        }

        targetImage.color = Color.red;
        damageFlashTimer = damageFlashTime;
    }

    // ダメージフラッシュ更新

    private void UpdateDamageFlash()
    {
        if (damageFlashTimer <= 0f)
        {
            return;
        }


        damageFlashTimer -= Time.deltaTime;


        if (damageFlashTimer <= 0f)
        {
            damageFlashTimer = 0f;


            if (targetImage != null)
            {
                // 元の色に戻す
                targetImage.color = originalColor;
            }
        }
    }

    // ダメージ表記生成
    private void CreateDamageText(
        int damageAmount,
        DamageType type
    )
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning(
                "DamageTarget : " +
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
                "DamageTarget : " +
                "DamageTextPrefabに" +
                "DamageTextコンポーネントがありません。"
            );

            return;
        }

        damageText.Setup(
            damageAmount,
            type,
            moveDirection
        );
    }

}
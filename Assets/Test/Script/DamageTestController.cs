using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageTestController : MonoBehaviour
{
    [Header("ON / OFF")]
    public Button toggleButton;
    public TextMeshProUGUI toggleText;

    [Header("ダメージ設定")]
    public int damage = 100;
    public float damageInterval = 1.0f;

    private bool isDamageTestEnabled = false;
    private float damageTimer = 0f;

    private void Start()
    {
        UpdateButtonUI();
    }

    private void Update()
    {
        if (!isDamageTestEnabled)
        {
            return;
        }

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;

            TakeDamage();
        }
    }

    public void ToggleDamageTest()
    {
        isDamageTestEnabled = !isDamageTestEnabled;

        damageTimer = 0f;

        UpdateButtonUI();
    }

    private void TakeDamage()
    {
        Debug.Log(
            "ダメージを受けた！ ダメージ量：" + damage
        );
    }

    public bool IsDamageTestEnabled()
    {
        return isDamageTestEnabled;
    }

    public int GetDamage()
    {
        return damage;
    }

    public float GetDamageInterval()
    {
        return damageInterval;
    }

    private void UpdateButtonUI()
    {
        if (toggleText != null)
        {
            toggleText.text =
                isDamageTestEnabled ? "ON" : "OFF";
        }
    }
}
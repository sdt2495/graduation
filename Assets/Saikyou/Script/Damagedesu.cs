using TMPro;
using UnityEngine;

public class Damagedesu : MonoBehaviour
{
    public float lifeTime = 2f;

    private TextMeshProUGUI textComponent;
    private float timer;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textComponent == null)
        {
            Debug.LogError("子オブジェクトに TextMeshProUGUI がありません！");
        }
    }

    public void SetDamage(int damage)
    {
        if (textComponent == null) return;

        textComponent.text = damage.ToString();
        textComponent.color = Color.red;
        textComponent.fontSize = 48;
        textComponent.enabled = true;

        timer = lifeTime;

        Debug.Log("ダメージ表示: " + textComponent.text);
    }

    void Update()
    {
        if (textComponent == null) return;

        timer -= Time.deltaTime;

        Color c = textComponent.color;
        c.a = Mathf.Clamp01(timer / lifeTime);
        textComponent.color = c;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float moveSpeed = 50f;
    public float lifeTime = 1f;

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void SetDamage(int damage)
    {
        if (text != null)
        {
            text.text = "-" + damage;
            text.color = Color.red;
        }
    }

    void Update()
    {
        transform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
            Destroy(gameObject);
    }
}
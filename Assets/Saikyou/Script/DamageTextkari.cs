using UnityEngine;
using TMPro;

public class DamageTextkari : MonoBehaviour
{
    public TMP_Text text;

    void Awake()
    {
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void ShowDamage(int damage)
    {
        if (text == null)
        {
            Debug.LogError("TextMeshProÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
            return;
        }

        text.text = damage.ToString();
        text.color = Color.red;

        Destroy(gameObject, 1f);
    }
}
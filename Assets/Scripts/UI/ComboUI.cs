using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;

    public void UpdateCombo(int combo)
    {
        comboText.text = $"{combo}";
    }
}

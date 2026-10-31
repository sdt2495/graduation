using UnityEngine;
using UnityEngine.UI;

public class ComboBar : MonoBehaviour
{
    [Header("バー")]
    [SerializeField] private Image[] bars;

    [Header("カラー")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    /// <summary>
    /// コンボ数に応じてバーを更新
    /// </summary>
    public void UpdateComboBar(int combo)
    {
        int activeCount = GetActiveBarCount(combo);

        for (int i = 0; i < bars.Length; i++)
        {
            if (i < activeCount)
            {
                bars[i].color = activeColor;
            }
            else
            {
                bars[i].color = defaultColor;
            }
        }
    }

    /// <summary>
    /// コンボ数から色を付けるバーの数を取得
    /// </summary>
    private int GetActiveBarCount(int combo)
    {
        if (combo <= 0)
        {
            return 0;
        }

        // 1～5コンボ：2個ずつ
        if (combo <= 5)
        {
            return combo * 2;
        }

        // 6～10コンボ：先頭から2個ずつ
        if (combo <= 10)
        {
            return (combo - 5) * 2;
        }

        // 11～20コンボ：1個ずつ
        if (combo <= 20)
        {
            return combo - 10;
        }

        // 21コンボ以上：満タン
        return 10;
    }
}

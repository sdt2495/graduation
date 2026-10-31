using System;
using UnityEngine;
using UnityEngine.UI;

public class ComboBar : MonoBehaviour
{
    [Header("バー")]
    [SerializeField] private Image[] bars;

    [Header("カラー")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color firstColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color secoundColor = Color.cyan;
    [SerializeField] private Color thirdColor = Color.yellow;
    [SerializeField] private Color fourthColor = new Color(1f, 0.75f, 0.85f);

    /// <summary>
    /// コンボ数に応じてバーを更新
    /// </summary>
    public void UpdateComboBar(int combo)
    {
        int activeCount = GetActiveBarCount(combo);
        Color activeColor = GetActiveColor(combo);

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
    /// コンボ数に応じてバーの色を変える
    /// </summary>
    /// <param name="combo"></param>
    /// <returns></returns>
    private Color GetActiveColor(int combo)
    {
        if(combo < 5)
        {
            return firstColor;
        }

        if(combo < 10)
        {
            return secoundColor;
        }

        if( combo < 20)
        {
            return thirdColor;
        }

        return fourthColor;
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

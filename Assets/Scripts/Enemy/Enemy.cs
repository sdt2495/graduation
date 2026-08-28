using NUnit.Framework;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

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
    private List<CommandType> commaneds = new List<CommandType>();
    private int currentIndex = 0;

    // ミスしたコマンドのindex
    private int missIndex = -1;

    /// <summary>
    /// 正しい入力がされたらクリア
    /// </summary>
    /// <param name="inputcommaned"></param>
    /// <returns></returns>
    public CheckResult Check(CommandType inputcommaned)
    {
        if (inputcommaned == commaneds[currentIndex])
        {
            currentIndex++;

            // 全成功
            if (currentIndex >= commaneds.Count)
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

    public void SetRandomCommands()
    {
        commaneds.Clear();
        currentIndex = 0;
        missIndex = -1;

        int count = Random.Range(1, 4);

        for(int i = 0; i < count; i++)
        {
            commaneds.Add((CommandType)Random.Range(0, 4));
        }
    }

    public List<CommandType> GetCommands() { return commaneds; }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public int GetMissIndex()
    {
        return missIndex;
    }
}

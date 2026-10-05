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
    private List<CommandType> currentCommaneds = new List<CommandType>();
    private int currentIndex = 0;

    // ミスしたコマンドのindex
    private int missIndex = -1;

    [Header("HP")]
    [SerializeField] private int maxHP = 10;

    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // 外部参照用
    public int CurrentHP {  get { return currentHP; } }
    public int MaxHP {  get { return maxHP; } }

    public void Damage(int damage)
    {
        currentHP -= damage;

        if(currentHP < 0)
        {
            currentHP = 0;
        }
    }

    public void ResetCommand()
    {
        currentIndex = 0;
        missIndex = -1;
    }

    /// <summary>
    /// 正しい入力がされたらクリア
    /// </summary>
    /// <param name="inputcommaned"></param>
    /// <returns></returns>
    public CheckResult Check(CommandType inputcommaned)
    {
        if (inputcommaned == currentCommaneds[currentIndex])
        {
            currentIndex++;

            // 全成功
            if (currentIndex >= currentCommaneds.Count)
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
        currentCommaneds.Clear();
        currentIndex = 0;
        missIndex = -1;

        int count = Random.Range(1, 4);

        for(int i = 0; i < count; i++)
        {
            currentCommaneds.Add((CommandType)Random.Range(0, 4));
        }
    }

    public void SetCommands(List<CommandType> commands)
    {
        currentCommaneds.Clear();
        currentCommaneds.AddRange(commands);

        currentIndex = 0;
        missIndex = -1;
    }

    public List<CommandType> GetCommands() { return currentCommaneds; }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public int GetMissIndex()
    {
        return missIndex;
    }

    public bool IsCommandFinished()
    {
        return currentIndex >= currentCommaneds.Count;
    }
}

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

public class Enemy : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI textMeshPro;
    private List<CommandType> commaneds = new List<CommandType>();
    private int currentIndex = 0;

    /// <summary>
    /// ³‚µ‚¢“ü—Í‚ª‚³‚ê‚½‚çƒNƒŠƒA
    /// </summary>
    /// <param name="inputcommaned"></param>
    /// <returns></returns>
    public bool Check(CommandType inputcommaned)
    {
       if(inputcommaned == commaneds[currentIndex])
        {
            currentIndex++;
            UpdateCommanedText();

            // ‘S•”“ü—Í‚Å‚«‚½
            if(currentIndex >= commaneds.Count)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public void SetRandomCommands()
    {
        commaneds.Clear();
        currentIndex = 0;

        int count = Random.Range(2, 4);

        for(int i = 0; i < count; i++)
        {
            commaneds.Add((CommandType)Random.Range(0, 4));
        }

        UpdateCommanedText();
    }


    public void UpdateCommanedText()
    {
        textMeshPro.text = "";

        foreach (var command in commaneds)
        {
            switch (command)
            {
                case CommandType.Up:
                    textMeshPro.text += "ª";
                    break;
                case CommandType.Down:
                    textMeshPro.text += "«";
                    break;
                case CommandType.Left:
                    textMeshPro.text += "©";
                    break;
                case CommandType.Right:
                    textMeshPro.text += "¨";
                    break;
            }
        }
    }
}

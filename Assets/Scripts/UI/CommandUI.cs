using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class CommandUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI commandText;
    [SerializeField] private TextMeshProUGUI nextcommandText;

    public void UpdateCommanedText(Enemy battleEnemy, Enemy nextEnemy)
    {
        commandText.text = CreateCommandString(battleEnemy.GetCommands());

        if (nextEnemy != null)
        {
            nextcommandText.text = CreateCommandString(nextEnemy.GetCommands());
        }
        else
        {
            nextcommandText.text = "";
        }
    }

    private string CreateCommandString(List<CommandType> commands)
    {
        string text = "";

        foreach(CommandType command in commands) 
        {
            switch (command)
            {
                case CommandType.Up:
                    text += "Å™";
                    break;
                case CommandType.Down:
                    text += "Å´";
                    break;
                case CommandType.Left:
                    text += "Å©";
                    break;
                case CommandType.Right:
                    text += "Å®";
                    break;
            }
            text += "";
        }

        return text;
    }
}

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
    public CommandType correctCommand;
    [SerializeField] private TMPro.TextMeshProUGUI textMeshPro;

    public bool Check(CommandType inputcommaned)
    {
        if(inputcommaned == correctCommand)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public void UpdateCommanedText()
    {
        switch(correctCommand)
        {
            case CommandType.Up:
                textMeshPro.text = "Å™";
                break;
            case CommandType.Down:
                textMeshPro.text = "Å´";
                break;
            case CommandType.Left:
                textMeshPro.text = "Å©";
                break;
            case CommandType.Right:
                textMeshPro.text = "Å®";
                break;
        }
    }
}

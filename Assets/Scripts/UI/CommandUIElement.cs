using TMPro;
using UnityEngine;

// ===================================
// コマンド１個の表示・状態管理
// ===================================

public class CommandUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI arrowText;

    /// <summary>
    /// コマンドの中身をランダムに選択
    /// </summary>
    /// <param name="command"></param>
    public void SetCommand(CommandType command)
    {
        switch (command)
        {
            case CommandType.Up:
                arrowText.text = "↑";
                break;
            case CommandType.Down:
                arrowText.text = "↓";
                break;
            case CommandType.Left:
                arrowText.text = "←";
                break;
            case CommandType.Right:
                arrowText.text = "→";
                break;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===================================
// コマンド１個の表示・状態管理
// ===================================

public class CommandUIElement : MonoBehaviour
{
    [SerializeField] private Image diamondImage;
    [SerializeField] private Image arrowImage;

    [Header("矢印画像")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    /// <summary>
    /// コマンドの中身をランダムに選択
    /// </summary>
    /// <param name="command"></param>
    public void SetCommand(CommandType command)
    {
        switch (command)
        {
            case CommandType.Up:
                arrowImage.sprite = upSprite;
                break;
            case CommandType.Down:
                arrowImage.sprite = downSprite;
                break;
            case CommandType.Left:
                arrowImage.sprite = leftSprite;
                break;
            case CommandType.Right:
                arrowImage.sprite = rightSprite;
                break;
        }
    }

    /// <summary>
    /// ひし形の表示・非表示
    /// </summary>
    /// <param name="visible"></param>
    public void SetDiamondVisible(bool visible)
    {
        diamondImage.gameObject.SetActive(visible);
    }
}

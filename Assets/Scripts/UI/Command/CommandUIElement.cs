using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===================================
// コマンド１個の表示・状態管理
// ===================================

public class CommandUIElement : MonoBehaviour
{
    // 黒いひし形
    [SerializeField] private Image diamondImage;

    // ピンクのひし形
    [SerializeField] private Image activeDiamondImage;
    
    // 矢印
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
    /// 黒いひし形の表示・非表示
    /// </summary>
    /// <param name="visible"></param>
    public void SetDiamondVisible(bool visible)
    {
        diamondImage.gameObject.SetActive(visible);
    }

    /// <summary>
    /// ピンクのひし形の表示・非表示
    /// </summary>
    /// <param name="visible"></param>
    public void SetActiveDiamondVisible(bool visible)
    {
        activeDiamondImage.gameObject.SetActive(visible);
    }

    /// <summary>
    /// ピンクのひし形の位置を取得
    /// </summary>
    /// <returns></returns>
    public RectTransform GetActiveDiamondTransform()
    {
        return activeDiamondImage.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Scaleを設定
    /// ピンク色のひし形ののみサイズ固定
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale(float scale)
    {
        diamondImage.transform.localScale = Vector3.one * scale;
        arrowImage.transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// 色を設定
    /// </summary>
    /// <param name="color"></param>
    public void SetDiamondColor(Color color)
    {
        diamondImage.color = color;
    }
}

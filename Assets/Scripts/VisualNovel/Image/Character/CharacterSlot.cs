using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1か所分の立ち絵を管理するクラス
/// </summary>
public class CharacterSlot
{
    private Image currentImage;      // 現在表示しているImage
    private Image nextImage;         // 次に表示するImage

    private string currentCharacterName = "";    // 現在表示している立ち絵名


    /// <summary>
    /// CharacterSlotを作成 (立ち絵の型)
    /// </summary>
    public CharacterSlot(Image imageA, Image imageB, float transitionTime)
    {
        currentImage = imageA;
        nextImage = imageB;

        // 初期状態では非表示
        imageA.gameObject.SetActive(false);
        imageB.gameObject.SetActive(false);
    }


    /// <summary>
    /// 立ち絵を更新
    /// </summary>
    public void SetCharacter(string characterName)
    {
        // 空欄なら何もしない (現在表示している立ち絵をそのまま維持)
        if (string.IsNullOrEmpty(characterName))
            return;

        // NONEなら立ち絵を非表示
        if (characterName == "NONE")
        {
            ClearImage(currentImage);
            // 現在表示している立ち絵名を削除
            currentCharacterName = "";

            return;
        }

        // 同じ立ち絵なら何もしない
        if (currentCharacterName == characterName)
            return;

        // 立ち絵を読み込む
        Sprite sprite = Resources.Load<Sprite>("Character/" + characterName);

        // 立ち絵が見つからなかった場合
        if (sprite == null)
        {
            Debug.LogWarning($"立ち絵が見つかりません : {characterName}");
            return;
        }

        // 現在の立ち絵を即座に変更
        currentImage.sprite = sprite;
        currentImage.color = Color.white;
        currentImage.gameObject.SetActive(true);

        // 次のImageは使用しないので非表示
        nextImage.sprite = null;
        nextImage.gameObject.SetActive(false);
        nextImage.color = Color.white;

        // 現在の立ち絵名を更新
        currentCharacterName = characterName;
    }


    #region 画像の表示・非表示

    /// <summary>
    /// 立ち絵を完全に消す
    /// </summary>
    private void ClearImage(Image image)
    {
        image.sprite = null;
        image.gameObject.SetActive(false);

        // 透明度を初期状態に戻す
        Color color = image.color;
        color.a = 1f;
        image.color = color;
    }


    /// <summary>
    /// 立ち絵を表示・非表示
    /// </summary>
    public void SetVisible(bool visible)
    {
        // 現在の立ち絵を表示・非表示
        currentImage.gameObject.SetActive(visible);

    }
    #endregion
}
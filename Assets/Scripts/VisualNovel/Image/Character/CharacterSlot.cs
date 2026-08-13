using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1か所分の立ち絵を管理するクラス
/// </summary>
public class CharacterSlot
{
    private Image image;         // Image
    private Animator animator;

    private string currentCharacter = "";    // 現在表示している立ち絵名

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public CharacterSlot(Image image, Animator animator)
    {
        this.image = image;
        this.animator = animator;
    }


    /// <summary>
    /// 立ち絵を設定
    /// </summary>
    public void SetCharacter(string character)
    {
        // 空欄なら変更しない
        if (string.IsNullOrEmpty(character))
            return;

        // NONEなら非表示
        if (character == "NONE")
        {
            SetVisible(false);
            currentCharacter = "";
            return;
        }

        // 同じ立ち絵なら変更しない
        if (currentCharacter == character && image.enabled)
            return;

        // 立ち絵を読み込む
        Sprite sprite = Resources.Load<Sprite>("Character/" + character);

        if (sprite == null)
        {
            Debug.LogWarning($"立ち絵が見つかりません : {character}");
            return;
        }

        // 立ち絵を設定
        image.sprite = sprite;

        // 表示
        image.enabled = true;

        // 現在の立ち絵を記録
        currentCharacter = character;
    }


    /// <summary>
    /// Active状態を変更 (Animatorのフラグ)
    /// </summary>
    public void SetActive(bool active)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animatorが設定されていません。");
            return;
        }
        // AnimatorのActiveパラメータを変更
        animator.SetBool("Active", active);
    }


    /// <summary>
    /// 表示・非表示
    /// </summary>
    public void SetVisible(bool visible)
    {
        image.enabled = visible;
    }
}
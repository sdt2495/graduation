using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ノベルゲームの立ち絵を管理するクラス
/// </summary>
public class NovelCharacterManager : MonoBehaviour
{
    [Header("Left")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Animator leftAnimator;

    [Header("Right")]
    [SerializeField] private Image rightImage;
    [SerializeField] private Animator rightAnimator;


    // 左右2つの立ち絵表示場所
    private CharacterSlot leftSlot;
    private CharacterSlot rightSlot;

    private void Awake()
    {
        // 左の立ち絵スロットを作成
        leftSlot = new CharacterSlot(leftImage, leftAnimator);
        // 右の立ち絵スロットを作成
        rightSlot = new CharacterSlot(rightImage, rightAnimator);
    }


    /// <summary>
    /// 立ち絵を更新
    /// </summary>
    /// <param name="left">左に表示する立ち絵</param>
    /// <param name="right">右に表示する立ち絵</param>
    /// <param name="active">明るくする立ち絵 (LEFT / RIGHT / BOTH / NONE)</param>
    public void UpdateCharacters(string left, string right, string active)
    {
        // 左の立ち絵を更新
        leftSlot.SetCharacter(left);

        // 右の立ち絵を更新
        rightSlot.SetCharacter(right);


        // Activeの内容によってアニメーションを変更
        switch (active.ToUpper())
        {
            // 左だけActive
            case "LEFT":
                leftSlot.SetActive(true);
                rightSlot.SetActive(false);
                break;

            // 右だけActive
            case "RIGHT":
                leftSlot.SetActive(false);
                rightSlot.SetActive(true);
                break;

            // 左右ともActive
            case "BOTH":
                leftSlot.SetActive(true);
                rightSlot.SetActive(true);
                break;

            // 左右とも非Active
            case "NONE":
                leftSlot.SetActive(false);
                rightSlot.SetActive(false);
                break;

            // 空欄なら明るさを変更しない
            case "":
                break;

            default:
                Debug.LogWarning($"Activeの値が正しくありません : {active}");
                break;
        }
    }


    /// <summary>
    /// すべての立ち絵を表示・非表示
    /// </summary>
    public void SetCharactersVisible(bool visible)
    {
        leftSlot.SetVisible(visible);
        rightSlot.SetVisible(visible);
    }
}
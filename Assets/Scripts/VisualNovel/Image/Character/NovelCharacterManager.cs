using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ノベルゲームの立ち絵を管理するクラス
/// </summary>
public class NovelCharacterManager : MonoBehaviour
{
    [Header("MessageLeft")]
    [SerializeField] private Image messageLeftA;
    [SerializeField] private Image messageLeftB;

    [Header("Left")]
    [SerializeField] private Image leftA;
    [SerializeField] private Image leftB;
    [Header("Center")]
    [SerializeField] private Image centerA;
    [SerializeField] private Image centerB;
    [Header("Right")]
    [SerializeField] private Image rightA;
    [SerializeField] private Image rightB;

    [Header("──────────────────────────────")]
    [Header("立ち絵切替時間")]
    [SerializeField] private float transitionTime = 0.5f;

    // 4つの立ち絵表示場所
    private CharacterSlot messageLeftSlot;
    private CharacterSlot leftSlot;
    private CharacterSlot centerSlot;
    private CharacterSlot rightSlot;


    private void Awake()
    {
        // 各場所の立ち絵スロットを作成
        messageLeftSlot = new CharacterSlot(messageLeftA, messageLeftB, transitionTime);

        leftSlot = new CharacterSlot(leftA, leftB, transitionTime);
        centerSlot = new CharacterSlot(centerA, centerB, transitionTime);
        rightSlot = new CharacterSlot(rightA, rightB, transitionTime);
    }


    /// <summary>
    /// 立ち絵を更新
    /// </summary>
    public void UpdateCharacters(string left, string center, string right, string messageLeft)
    {
        // 左
        leftSlot.SetCharacter(left);
        // 中央
        centerSlot.SetCharacter(center);
        // 右
        rightSlot.SetCharacter(right);

        // メッセージウィンドウ左
        messageLeftSlot.SetCharacter(messageLeft);
    }
}
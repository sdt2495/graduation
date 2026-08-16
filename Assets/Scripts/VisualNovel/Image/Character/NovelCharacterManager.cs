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
    public IEnumerator UpdateCharacters(string left, string right, string active)
    {
        // 左右を同時に変更開始
        Coroutine leftCoroutine = StartCoroutine(leftSlot.SetCharacter(left));
        Coroutine rightCoroutine = StartCoroutine(rightSlot.SetCharacter(right));

        // 左右の変更を同時に実行
        yield return leftCoroutine;
        yield return rightCoroutine;


        // Activeの内容によって明るさを変更
        switch (active.ToUpper())
        {
            case "LEFT":
                leftSlot.SetActive(true);
                rightSlot.SetActive(false);
                break;

            case "RIGHT":
                leftSlot.SetActive(false);
                rightSlot.SetActive(true);
                break;

            case "BOTH":
                leftSlot.SetActive(true);
                rightSlot.SetActive(true);
                break;

            case "NONE":
                leftSlot.SetActive(false);
                rightSlot.SetActive(false);
                break;

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


    /// <summary>
    /// Skipモードを設定
    /// </summary>
    public void SetSkipMode(bool enable)
    {
        // 左のAnimator速度を変更
        leftSlot.SetSkipMode(enable);
        // 右のAnimator速度を変更
        rightSlot.SetSkipMode(enable);
    }
}
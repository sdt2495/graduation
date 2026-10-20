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

    [Space(5)]
    [Header("Message Balloon Tails")]
    [SerializeField] private Image leftTail;
    [SerializeField] private Image rightTail;


    // 左右2つの立ち絵表示場所
    private CharacterSlot leftSlot;
    private CharacterSlot rightSlot;

    private void Awake()
    {
        // 左右の立ち絵スロットを作成
        leftSlot = new CharacterSlot(leftImage, leftAnimator);
        rightSlot = new CharacterSlot(rightImage, rightAnimator);

        // 初期状態ではしっぽを非表示
        leftTail.gameObject.SetActive(false);
        rightTail.gameObject.SetActive(false);
    }


    #region 立ち絵を更新

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
                // 立ち絵
                leftSlot.SetActive(true);
                rightSlot.SetActive(false);
                // 吹き出しの口
                leftTail.gameObject.SetActive(true);
                rightTail.gameObject.SetActive(false);
                break;

            case "RIGHT":
                // 立ち絵
                leftSlot.SetActive(false);
                rightSlot.SetActive(true);
                // 吹き出しの口
                leftTail.gameObject.SetActive(false);
                rightTail.gameObject.SetActive(true);
                break;

            case "BOTH":
                // 立ち絵
                leftSlot.SetActive(true);
                rightSlot.SetActive(true);
                // 吹き出しの口
                leftTail.gameObject.SetActive(true);
                rightTail.gameObject.SetActive(true);
                break;

            case "NONE":
                // 立ち絵
                leftSlot.SetActive(false);
                rightSlot.SetActive(false);
                // 吹き出しの口
                leftTail.gameObject.SetActive(false);
                rightTail.gameObject.SetActive(false);
                break;

            case "":
                // 空欄なら現在の状態を維持
                break;

            default:
                Debug.LogWarning($"Activeの値が正しくありません : {active}");
                break;
        }
    }
    #endregion


    #region すべての立ち絵を表示・非表示

    /// <summary>
    /// すべての立ち絵を表示・非表示
    /// </summary>
    public void SetCharactersVisible(bool visible)
    {
        leftSlot.SetVisible(visible);
        rightSlot.SetVisible(visible);
    }
    #endregion


    #region すべての立ち絵を高速で退場させる

    /// <summary>
    /// すべての立ち絵を高速で退場させる
    /// </summary>
    public IEnumerator ExitAllCharactersFast()
    {
        // 左右の立ち絵を同時に退場開始
        Coroutine leftCoroutine = StartCoroutine(leftSlot.ExitCharacterFast());
        Coroutine rightCoroutine = StartCoroutine(rightSlot.ExitCharacterFast());

        // 左右の退場が完了するまで待機
        yield return leftCoroutine;
        yield return rightCoroutine;

        // 立ち絵を非表示状態にする
        leftSlot.SetActive(false);
        rightSlot.SetActive(false);

        // 吹き出しのしっぽを非表示にする
        leftTail.gameObject.SetActive(false);
        rightTail.gameObject.SetActive(false);
    }
    #endregion


    #region Skipモードを設定

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
    #endregion
}
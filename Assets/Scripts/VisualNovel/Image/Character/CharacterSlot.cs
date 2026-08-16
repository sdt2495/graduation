using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1か所分の立ち絵を管理するクラス
/// </summary>
public class CharacterSlot
{
    // 通常時のアニメーション速度
    private const float NORMAL_ANIMATION_SPEED = 1f;

    // スキップモード中のアニメーション速度 (外から弄れるようにしたいね☆ミ)
    private float skipAnimationSpeed = 2f;

    // 現在Skipモードか
    private bool isSkipMode = false;

    private Image image;
    private Animator animator;

    private string currentCharacter = "";     // 現在表示している立ち絵

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public CharacterSlot(Image image, Animator animator)
    {
        this.image = image;
        this.animator = animator;
        // 最初は非表示
        image.enabled = false;

        // Animatorの速度を通常速度にする
        if (animator != null)
        {
            animator.speed = NORMAL_ANIMATION_SPEED;
        }
    }


    #region Skipモード

    /// <summary>
    /// Skipモードを設定
    /// </summary>
    public void SetSkipMode(bool enable)
    {
        // Skipモード状態を保存
        isSkipMode = enable;

        // Animatorの速度を変更
        if (animator != null)
        {
            if (isSkipMode)
            {
                // Skip中は高速再生
                animator.speed = skipAnimationSpeed;
            }
            else
            {
                // 通常時は通常速度
                animator.speed = NORMAL_ANIMATION_SPEED;
            }
        }
    }


    /// <summary>
    /// Skipモード中のアニメーション速度を設定
    /// </summary>
    public void SetSkipAnimationSpeed(float speed)
    {
        // 0以下にならないようにする
        skipAnimationSpeed = Mathf.Max(0.01f, speed);

        // 現在Skip中なら即座に反映
        if (isSkipMode && animator != null)
        {
            animator.speed = skipAnimationSpeed;
        }
    }
    #endregion


    #region キャラクター変更

    /// <summary>
    /// 立ち絵を変更する
    /// </summary>
    public IEnumerator SetCharacter(string character)
    {
        // 空欄なら変更しない
        if (string.IsNullOrEmpty(character))
            yield break;

        // NONEなら退場
        if (character == "NONE")
        {
            // 現在キャラクターが表示されている場合
            if (image.enabled)
            {
                yield return ExitCharacter();
            }

            currentCharacter = "";
            yield break;
        }

        // CSVの立ち絵名からキャラクター名を取得
        string newCharacter = GetCharacterName(character);

        // 同じキャラクターなら差分変更 (例：Alice/normal → Alice/smile)
        // ※この場合は退場・登場アニメーションを行わない
        if (currentCharacter == newCharacter && image.enabled)
        {
            SetSprite(character);
            yield break;
        }

        // 別のキャラクターが現在表示されている場合、まず現在のキャラクターを退場させる
        if (image.enabled)
        {
            yield return ExitCharacter();
        }
        // 新しい立ち絵を設定
        SetSprite(character);

        // 新しいキャラクターとして記録
        currentCharacter = newCharacter;

        // 画面外から登場
        yield return EnterCharacter();
    }


    /// <summary>
    /// 立ち絵をImageに設定する
    /// </summary>
    private void SetSprite(string character)
    {
        // 立ち絵を読み込む
        Sprite sprite = Resources.Load<Sprite>("Character/" + character);
        if (sprite == null)
        {
            Debug.LogWarning($"立ち絵が見つかりません : {character}");
            return;
        }

        // Spriteを変更
        image.sprite = sprite;

        //// 表示
        //image.enabled = true;
    }


    /// <summary>
    /// 立ち絵名からキャラクター名を取得
    /// </summary>
    private string GetCharacterName(string character)
    {
        // 例：Alice/normal → Alice (パスを参照)

        int slashIndex = character.IndexOf('/');
        // "/" がある場合
        if (slashIndex >= 0)
        {
            return character.Substring(0, slashIndex);
        }

        // "/" がない場合は、そのままキャラクター名として扱う
        return character;
    }
    #endregion


    #region 登場・退場

    /// <summary>
    /// 画面外から登場
    /// </summary>
    private IEnumerator EnterCharacter()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animatorが設定されていません。");
            image.enabled = true;
            yield break;
        }

        // 現在のAnimator速度を設定
        SetAnimatorSpeed();

        // まだ立ち絵を表示しない
        image.enabled = false;

        // 登場アニメーション開始
        animator.ResetTrigger("Exit");
        animator.SetTrigger("Enter");

        // AnimatorにTriggerを反映させる
        yield return null;

        // Enterアニメーションが開始された状態で表示
        image.enabled = true;

        // 現在再生しているアニメーション情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 実際のアニメーションクリップの長さ
        float animationLength = stateInfo.length;
        // Animatorの速度を考慮した実際の再生時間
        float waitTime = animationLength / animator.speed;

        // アニメーション終了まで待つ
        yield return new WaitForSeconds(waitTime);

        // 通常速度へ戻す
        animator.speed = NORMAL_ANIMATION_SPEED;
    }


    /// <summary>
    /// 画面外へ退場
    /// </summary>
    private IEnumerator ExitCharacter()
    {
        if (animator == null)
        {
            image.enabled = false;
            yield break;
        }

        // 現在のAnimator速度を設定
        SetAnimatorSpeed();

        // 退場アニメーション開始
        animator.ResetTrigger("Enter");
        animator.SetTrigger("Exit");

        // Animatorにアニメーションを反映
        yield return null;

        // 現在再生しているアニメーション情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 実際のアニメーションクリップの長さ
        float animationLength = stateInfo.length;
        // Animatorの速度を考慮した実際の再生時間
        float waitTime = animationLength / animator.speed;

        // アニメーション終了まで待つ
        yield return new WaitForSeconds(waitTime);

        // アニメーション終了後に非表示
        image.enabled = false;

        // 通常速度へ戻す
        animator.speed = NORMAL_ANIMATION_SPEED;
    }


    /// <summary>
    /// Animatorの速度を現在のモードに合わせる
    /// </summary>
    private void SetAnimatorSpeed()
    {
        if (isSkipMode)
        {
            animator.speed = skipAnimationSpeed;
        }
        else
        {
            animator.speed = NORMAL_ANIMATION_SPEED;
        }
    }
    #endregion


    #region Active

    /// <summary>
    /// Active状態を変更
    /// </summary>
    public void SetActive(bool active)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animatorが設定されていません。");
            return;
        }
        // AnimatorのActive Boolを変更
        animator.SetBool("Active", active);
    }
    #endregion


    #region 表示・非表示

    /// <summary>
    /// 表示・非表示
    /// </summary>
    public void SetVisible(bool visible)
    {
        image.enabled = visible;
    }
    #endregion
}
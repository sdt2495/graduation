
using System.Collections;
using UnityEngine;

/// <summary>
/// Enemyの頭上にアニメーションを表示する。
///
/// ・BattleManagerから呼び出されたときだけ開始
/// ・Enemyの頭上にアニメーションを表示
/// ・アニメーション終了まで待機
/// ・終了後にUIPanelShrinkの縮小演出を開始
/// </summary>
public class ExclamationMark : MonoBehaviour
{
    [Header("アニメーションPrefab")]
    [SerializeField] private GameObject animationPrefab;

    [Header("表示先Canvas")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Enemyを映すカメラ")]
    [SerializeField] private Camera targetCamera;

    [Header("パネル縮小")]
    [SerializeField] private UIPanelShrink uiPanelShrink;

    [Header("表示位置")]
    [SerializeField]
    private Vector3 positionOffset = new Vector3(0f, 1.5f, 0f);

    [Header("次の演出までの待機時間")]
    [SerializeField] private float nextSequenceDelay = 0.5f;

    // 二重起動防止
    private bool isPlaying;

    /// <summary>
    /// BattleManagerから呼び出す開始処理。
    /// </summary>
    public void StartExclamation()
    {
        if (isPlaying)
            return;

        isPlaying = true;

        ShowAnimation();
    }

    /// <summary>
    /// Enemyの頭上にアニメーションを表示する。
    /// </summary>
    private void ShowAnimation()
    {
        GameObject enemy =
            GameObject.FindGameObjectWithTag("Enemy");

        if (enemy == null ||
            animationPrefab == null ||
            targetCanvas == null)
        {
            Debug.LogWarning(
                "Enemy、Prefab、Canvasの設定を確認してください。"
            );

            isPlaying = false;
            return;
        }

        Camera worldCamera = targetCamera != null
            ? targetCamera
            : Camera.main;

        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
            worldCamera == null)
        {
            Debug.LogWarning("カメラが見つかりません。");
            isPlaying = false;
            return;
        }

        // Enemyの頭上の座標を取得
        Vector3 worldPosition =
            enemy.transform.position + positionOffset;

        // スクリーン座標に変換
        Vector3 screenPosition = worldCamera != null
            ? worldCamera.WorldToScreenPoint(worldPosition)
            : worldPosition;

        if (screenPosition.z < 0f)
        {
            isPlaying = false;
            return;
        }

        Camera canvasCamera =
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : targetCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.transform as RectTransform,
            screenPosition,
            canvasCamera,
            out Vector2 localPosition))
        {
            isPlaying = false;
            return;
        }

        // PrefabをCanvas内に生成
        GameObject effect = Instantiate(
            animationPrefab,
            targetCanvas.transform,
            false
        );

        RectTransform rectTransform =
            effect.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogWarning(
                "PrefabにRectTransformがありません。"
            );

            Destroy(effect);
            isPlaying = false;
            return;
        }

        // アニメーション開始前に位置を確定
        rectTransform.anchoredPosition = localPosition;

        // Animatorを取得
        Animator animator = effect.GetComponent<Animator>();

        if (animator == null ||
            animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning(
                "AnimatorまたはControllerがありません。"
            );

            Destroy(effect);
            isPlaying = false;
            return;
        }

        // Animatorを初期状態から再生
        animator.enabled = false;
        animator.enabled = true;
        animator.Play(0, 0, 0f);
        animator.Update(0f);

        // アニメーションクリップを取得
        AnimationClip[] clips =
            animator.runtimeAnimatorController.animationClips;

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning(
                "アニメーションクリップがありません。"
            );

            Destroy(effect);
            isPlaying = false;
            return;
        }

        // アニメーション終了まで待機
        StartCoroutine(
            WaitForAnimationEnd(effect, clips[0].length)
        );
    }

    /// <summary>
    /// アニメーション終了後にパネル縮小を開始する。
    /// </summary>
    private IEnumerator WaitForAnimationEnd(
        GameObject effect,
        float animationLength)
    {
        // 頭上アニメーション終了まで待機
        yield return new WaitForSeconds(animationLength);

        // アニメーションオブジェクトを削除
        if (effect != null)
        {
            Destroy(effect);
        }

        // 次の演出まで待機
        if (nextSequenceDelay > 0f)
        {
            yield return new WaitForSeconds(nextSequenceDelay);
        }

        // パネル縮小を開始
        if (uiPanelShrink != null)
        {
            uiPanelShrink.StartShrink();

            Debug.Log("待機終了：パネル縮小開始");
        }
        else
        {
            Debug.LogWarning(
                "UIPanelShrinkが設定されていません。"
            );
        }

        isPlaying = false;
    }
}


using UnityEngine;

/// <summary>
/// Enemyタグのオブジェクトを検索し、
/// その頭上にアニメーションPrefabを表示するクラス。
/// </summary>
public class EnemyAnimationDisplay : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private GameObject animationPrefab;

    [Header("表示位置")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 1.5f, 0f);

    private GameObject currentAnimation;

    /// <summary>
    /// Enemyを検索してアニメーションを表示する
    /// </summary>
    public void ShowAnimation()
    {
        // Enemyタグのオブジェクトを検索
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

        if (enemy == null)
        {
            Debug.LogWarning("Enemyタグのオブジェクトが見つかりません。");
            return;
        }

        if (animationPrefab == null)
        {
            Debug.LogWarning("AnimationPrefabが設定されていません。");
            return;
        }

        // すでに表示されているアニメーションを削除
        if (currentAnimation != null)
        {
            Destroy(currentAnimation);
        }

        // Enemyの子オブジェクトとして生成
        currentAnimation = Instantiate(
            animationPrefab,
            enemy.transform
        );

        // 頭上の位置に設定
        currentAnimation.transform.localPosition = positionOffset;
        currentAnimation.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 表示中のアニメーションを削除
    /// </summary>
    public void HideAnimation()
    {
        if (currentAnimation != null)
        {
            Destroy(currentAnimation);
            currentAnimation = null;
        }
    }
}

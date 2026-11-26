using TMPro;
using UnityEngine;

public class EnemyHintUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Animator animator;

    private static readonly int ShowHash =
        Animator.StringToHash("Show");

    private void Awake()
    {
        gameObject.SetActive(false);
    }


    public void ShowHint(string message)
    {
        if (hintText == null || animator == null)
        {
            Debug.LogWarning(
                "EnemyHintUIのTextまたはAnimatorが設定されていません。"
            );

            return;
        }

        // UIを表示
        hintText.text = message;
        gameObject.SetActive(true);

        // Animatorを初期状態に戻してから再生する
        animator.Rebind();
        animator.Update(0f);

        animator.ResetTrigger(ShowHash);
        animator.SetTrigger(ShowHash);
    }

    /// ヒントUIを非表示にする。

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
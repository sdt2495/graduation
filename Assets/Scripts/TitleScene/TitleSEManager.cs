using UnityEngine;

public class TitleSEManager : MonoBehaviour
{
    [Header("AudioSource (タイトル)")]
    [SerializeField] private AudioSource audioSource;

    [Header("決定SE")]
    [SerializeField] private AudioClip decisionSE;

    // SEonnryouこうもくは？

    /// <summary>
    /// 決定SEを再生
    /// </summary>
    public float PlayDecisionSE()
    {
        // SEが設定されていない場合
        if (decisionSE == null)
        {
            Debug.LogWarning("決定SEが設定されていません");
            return 0f;
        }
        // AudioSourceがない場合
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSourceが設定されていません");
            return 0f;
        }


        // SE再生
        audioSource.PlayOneShot(decisionSE);

        // SEの長さを返す
        return decisionSE.length;
    }
}
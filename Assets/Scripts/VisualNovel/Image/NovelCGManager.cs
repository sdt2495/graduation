using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ノベルゲームCG管理
/// </summary>
public class NovelCGManager : MonoBehaviour
{
    // ★MEMO：イベントCGが表示されたら、立ち絵を自動で隠すようにすると、市販ノベルゲームに近い挙動になります！（まだ）


    [Header("CG Image")]
    [SerializeField] private Image cgImage;

    private void Awake()
    {
        // 非表示
        cgImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// CG表示
    /// </summary>
    public void ShowCG(string cgName)
    {
        // 空欄なら何もしない
        if (string.IsNullOrEmpty(cgName))
            return;

        // 非表示
        if (cgName == "NONE")
        {
            HideCG();
            return;
        }

        Sprite sprite = Resources.Load<Sprite>("CG/" + cgName);

        if (sprite == null)
        {
            Debug.LogWarning("CGが見つかりません : " + cgName);
            return;
        }

        // CG表示
        cgImage.sprite = sprite;
        cgImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// CGを消す
    /// </summary>
    public void HideCG()
    {
        // CG非表示
        cgImage.sprite = null;
        cgImage.gameObject.SetActive(false);
    }
}
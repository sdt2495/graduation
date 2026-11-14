
using UnityEngine;

/// <summary>
/// ゲーム全体で使用する常駐オブジェクトを管理するクラス
/// </summary>
public class PersistentRoot : MonoBehaviour
{
    // 常駐オブジェクトのインスタンス
    private static PersistentRoot Instance;

    private void Awake()
    {
        // 既に別のPersistentRootが存在する場合
        if (Instance != null && Instance != this)
        {
            // 重複したオブジェクトを破棄
            Destroy(gameObject);
            return;
        }

        // このオブジェクトをインスタンスとして登録
        Instance = this;

        // シーンを移動しても、このオブジェクトと子オブジェクトを維持する
        DontDestroyOnLoad(gameObject);
    }
}

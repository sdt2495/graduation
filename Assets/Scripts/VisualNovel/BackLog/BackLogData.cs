using UnityEngine;

/// <summary>
/// バックログ1件分のデータ
/// </summary>
[System.Serializable]
public class BackLogData
{

    #region 項目 (読み取り専用)

    public string Speaker { get; } // 発言者
    public string Message { get; } // セリフ

    #endregion

    /// <summary>
    /// コンストラクタ (初期化)
    /// </summary>
    public BackLogData(string speaker, string message)
    {
        Speaker = speaker;
        Message = message;


    }
}
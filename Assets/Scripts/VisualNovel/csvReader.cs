using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// CSV読み込み君
/// </summary>
public class CSVReader : MonoBehaviour
{
    private List<string[]> csvData = new List<string[]>(); // CSVファイルの中身(string)を入れるリスト

    #region Start()

    /// <summary>
    /// ファイルの読み込みを行う
    /// </summary>
    void Start()
    {
        // 最初に読み込むCSV
        LoadCSV("Sample");
    }
    #endregion


    #region CSVを読み込む

    /// <summary>
    /// CSVを読み込む
    /// </summary>
    public void LoadCSV(string csvName)
    {
        // 前回読み込んだCSVを削除
        csvData.Clear();

        // CSV読み込み
        TextAsset csv = Resources.Load<TextAsset>("csv/" + csvName);
        if (csv == null)
        {
            Debug.LogError($"CSVが見つかりません : {csvName}");
            return;
        }

        StringReader reader = new StringReader(csv.text);
        // CSVを1行ずつ読み込む
        while (reader.Peek() != -1)
        {
            csvData.Add(reader.ReadLine().Split(','));
        }
    }
    #endregion


    #region 取得
    /// <summary>
    /// 全行数を取得
    /// </summary>
    public int GetCount()
    {
        return csvData.Count;
    }

    /// <summary>
    /// i行目を返す
    /// </summary>
    public string[] GetLine(int index)
    {
        return csvData[index];
    }
    #endregion
}
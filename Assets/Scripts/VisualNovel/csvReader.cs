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
        TextAsset csv = Resources.Load<TextAsset>("csv/Sample");    // Resourcesのcsvのsample.csvを格納
        StringReader reader = new StringReader(csv.text);           // TextAssetをStringReaderに変換

        // CSVを1行ずつ読む (ファイルが終わるまで繰り返す)
        while (reader.Peek() != -1)
        {
            csvData.Add(reader.ReadLine().Split(','));   // 1行ずつ読み込み、csvDataリストに追加する
        }
    }
    #endregion



    #region 取得
    /// <summary>
    /// 行数
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
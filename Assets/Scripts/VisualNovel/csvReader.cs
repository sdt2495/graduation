using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;    // ファイルの読み込みを行うための名前空間
using TMPro;

/// <summary>
/// CSV読み込み君
/// </summary>
public class csvReader : MonoBehaviour
{
    private TextAsset csvFile; // CSVファイル (TextAsset:Unityでテキストファイルを扱うためのクラス)
    private List<string[]> csvData = new List<string[]>(); // CSVファイルの中身(string)を入れるリスト

    int i = 0; // インデックスカウンター

    [Header("テキスト")]
    [SerializeField] private TextMeshProUGUI Englishtext;
    [SerializeField] private TextMeshProUGUI Japanesetext;

    #region Start()
    void Start()
    {
        csvFile = Resources.Load("csv/Sample") as TextAsset;　　　//Resourcesのcsvのsample.csvを格納
        StringReader reader = new StringReader(csvFile.text); // TextAssetをStringReaderに変換

        // CSVを1行ずつ読む (ファイルが終わるまで繰り返す)
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine(); // 1行ずつ読み込む
            csvData.Add(line.Split(',')); // csvDataリストに追加する
        }


        // 表示部分 (デバッグ)
        for (int i = 0; i < csvData.Count; i++) // csvDataリストの条件を満たす値の数（全て）
        {
            // データの表示
            Debug.Log("日本語：" + csvData[i][0] + ", English：" + csvData[i][1]);

            // [?] 「csvData[i][0]」と「csvData[i][1]」は、それぞれi番目の行の１列目と２列目を表します。
        }
    }
    #endregion


    #region Update()
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // マウス左ボタンがクリック
        {
            //TextMeshProにcsvテキストを代入
            Englishtext.text = csvData[i][0];
            Japanesetext.text = csvData[i][1];

            if (i < csvData.Count - 1) // インデックスiがCSVデータの要素数未満の場合
            {
                i++; // インデックスをインクリメントする
            }
            Debug.Log(csvData[i][0] + csvData[i][1]);
        }
    }
    #endregion
}
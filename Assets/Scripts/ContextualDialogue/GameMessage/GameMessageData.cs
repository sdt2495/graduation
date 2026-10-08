using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクター＆セリフ集 (1キャラ分)
/// </summary>
[CreateAssetMenu(fileName = "GameMessageData", menuName = "Game/Game Message Data")] 
public class GameMessageData : ScriptableObject
{
    [Header("キャラクター")]
    [SerializeField] private GameMessageCharacter character;
    [Header("メッセージ")]
    [SerializeField] private List<GameMessageEntry> messages = new();


    #region 読み取り専用

    public GameMessageCharacter Character => character;
    #endregion


    #region 指定されたタイプのメッセージを取得する

    /// <summary>
    /// 指定されたメッセージタイプからランダムに1つ取得する
    /// </summary>
    public GameMessageEntry GetRandomMessage(GameMessageType type)
    {
        // 指定されたタイプに一致するメッセージを一時的に保存するリスト
        List<GameMessageEntry> candidates = new();

        // 登録されているすべてのメッセージを確認
        foreach (GameMessageEntry entry in messages)
        {
            // メッセージタイプが指定されたタイプと一致する場合
            if (entry.Type == type)
            {
                // 候補リストに追加
                candidates.Add(entry);
            }
        }

        // 該当するメッセージが1つもない場合
        if (candidates.Count == 0)
        {
            return null;
        }

        // 候補の中からランダムに1つ選ぶ
        int index = Random.Range(0, candidates.Count);

        // 選ばれたメッセージを返す
        return candidates[index];
    }
    #endregion
}
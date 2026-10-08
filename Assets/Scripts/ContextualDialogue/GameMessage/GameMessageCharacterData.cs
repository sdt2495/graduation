using System;
using UnityEngine;

/// <summary>
/// キャラクターと、そのキャラクターのメッセージデータをまとめる
/// </summary>
[Serializable]
public class GameMessageCharacterData
{
    [Header("キャラクター")]
    [SerializeField] private GameMessageCharacter character;

    [Header("メッセージデータ")]
    [SerializeField] private GameMessageData messageData;


    #region 読み取り専用

    public GameMessageCharacter Character => character; // キャラクターを取得
    public GameMessageData MessageData => messageData; // キャラクターに対応するメッセージデータを取得
    #endregion
}
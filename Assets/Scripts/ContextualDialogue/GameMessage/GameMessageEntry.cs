using System;
using UnityEngine;

/// <summary>
/// セリフ1個
/// </summary>
[Serializable]
public class GameMessageEntry
{
    [Header("行動")]
    public GameMessageType type;

    [Header("表情")]
    public GameCharacterFace face;

    [Header("セリフ")][TextArea(2, 4)]
    public string message;


    #region 読み取り専用
    public GameMessageType Type => type;
    public GameCharacterFace Face => face;
    public string Message => message;
    #endregion
}
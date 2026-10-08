using UnityEngine;

/// <summary>
/// 表情集 (1キャラ分)
/// </summary>
[CreateAssetMenu(fileName = "GameMessageCharacter", menuName = "Game/Message Character")]
public class GameMessageCharacter : ScriptableObject
{
    [Header("キャラクター")]
    [SerializeField] private string characterName;

    [Header("表情")]
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite smile;
    [SerializeField] private Sprite damage;
    [SerializeField] private Sprite angry;

    #region 読み取り専用
    public string CharacterName => characterName;

    public Sprite Normal => normal;
    public Sprite Smile => smile;
    public Sprite Damage => damage;
    public Sprite Angry => angry;
    #endregion

    #region 指定された表情の画像を取得する

    /// <summary>
    /// 指定された表情の画像を取得する
    /// </summary>
    public Sprite GetFace(GameCharacterFace face)
    {
        switch (face)
        {
            case GameCharacterFace.Normal:
                return normal;

            case GameCharacterFace.Smile:
                return smile;

            case GameCharacterFace.Damage:
                return damage;

            case GameCharacterFace.Angry:
                return angry;

            default:
                return normal;
        }
    }
    #endregion
}
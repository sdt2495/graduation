using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// セリフをを表示
/// </summary>
public class GameMessageManager : MonoBehaviour
{
    [Header("メッセージUI")]
    [SerializeField] private GameObject messagePanel;
    [Space(5)]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text messageText;

    [Header("キャラクター")]
    [SerializeField] private List<GameMessageCharacterData> characters = new();

    [Header("──────────────────────────────")]
    [Header("表示時間")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine hideCoroutine; // 現在実行中の自動非表示コルーチン


    #region タイプからメッセージを表示

    /// <summary>
    /// 指定したキャラクターの指定したタイプのセリフを表示する
    /// </summary>
    public void ShowMessage(GameMessageCharacter character, GameMessageType type)
    {
        // 指定したキャラクターに対応するメッセージデータを取得
        GameMessageData data = FindCharacterData(character);
        // メッセージデータが見つからない場合は処理しない
        if (data == null)
        {
            Debug.LogWarning("指定されたキャラクターのメッセージデータが見つかりません: " + character.CharacterName);
            return;
        }

        // 指定したタイプのセリフからランダムに1つ取得
        GameMessageEntry entry = data.GetRandomMessage(type);
        // 対応するセリフが見つからない場合は処理しない
        if (entry == null)
        {
            Debug.LogWarning("指定されたメッセージが見つかりません: " + character.CharacterName + " / " + type);
            return;
        }

        // セリフに設定されている表情の画像を取得
        Sprite face = character.GetFace(entry.Face);

        // 表情とセリフを設定してメッセージを表示
        DisplayMessage(face, entry.Message);
    }
    #endregion


    #region キャラクター検索

    /// <summary>
    /// 指定したキャラクターに対応するメッセージデータを探す
    /// </summary>
    private GameMessageData FindCharacterData(GameMessageCharacter character)
    {
        // 登録されているキャラクターを順番に確認
        foreach (GameMessageCharacterData data in characters)
        {
            // 指定したキャラクターと一致するか確認
            if (data.Character == character)
            {
                // 対応するメッセージデータを返す
                return data.MessageData;
            }
        }
        // 対応するキャラクターが見つからなかった場合
        return null;
    }
    #endregion


    #region メッセージを表示

    /// <summary>
    /// キャラクター画像とセリフを表示する
    /// </summary>
    private void DisplayMessage(Sprite character, string message)
    {
        // メッセージパネルを表示
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }

        // キャラクター画像を設定
        if (characterImage != null)
        {
            characterImage.sprite = character;
        }

        // メッセージ本文を設定
        if (messageText != null)
        {
            messageText.text = message;
        }

        // 前の消去処理があれば止める
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 新しいメッセージの自動非表示処理を開始
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }
    #endregion


    #region メッセージを消す

    /// <summary>
    /// メッセージを消す
    /// </summary>
    public void HideMessage()
    {
        // メッセージパネルを非表示
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        // 自動非表示処理が実行中なら停止
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
    #endregion

    /// <summary>
    /// メッセージを表示してから一定時間待って、自動的に非表示にする
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        // 設定した表示時間だけ待機
        yield return new WaitForSeconds(displayTime);
        // 待機後、メッセージパネルを非表示
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        // コルーチンが終了したので参照を解除
        hideCoroutine = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// セリフをを表示
/// </summary>
public class GameMessageManager : MonoBehaviour
{
    /// <summary>
    /// 文字に適用する特殊エフェクトの種類
    /// </summary>
    private enum CharacterEffect
    {
        Normal, // 通常
        Wavy,   // 波揺れ
        Shaky   // 震え
    }

    [Header("メッセージUI")]
    [SerializeField] private GameObject messagePanel;
    [Space(5)]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text messageText;

    [Header("キャラクター")]
    [SerializeField] private List<GameMessageCharacterData> characters = new();

    [Header("──────────────────────────────")]
    [Header("文字送り")]
    [SerializeField] private float characterInterval = 0.05f;
    [Header("1文字が完全に表示されるまでの時間")]
    [SerializeField] private float characterFadeTime = 0.15f;

    [Header("表示時間")]
    [SerializeField] private float displayTime = 2f;

    [Header("──────────────────────────────")]
    [Header("特殊文字(</wavy>)")]
    [SerializeField] private float wavyAmplitude = 5f;
    [SerializeField] private float wavySpeed = 5f;
    [Header("特殊文字(</shaky>)")]
    [SerializeField] private float shakyAmplitude = 3f;
    [SerializeField] private float shakySpeed = 20f;

    
    private CharacterEffect[] characterEffects; // 各文字に適用するエフェクト

    private Vector3[][] originalVertices;    // 各文字の元の頂点位置

    private Coroutine specialCharacterCoroutine; // セリフ表示と自動非表示を管理するコルーチン
    private Coroutine messageCoroutine; // 現在実行中の自動非表示コルーチン


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
    /// キャラクター画像とセリフを設定し、表示を開始する
    /// </summary>
    private void DisplayMessage(Sprite character, string message)
    {
        // 前の文字送り・自動非表示処理があれば停止
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        // 前の特殊文字アニメーションがあれば停止
        if (specialCharacterCoroutine != null)
        {
            StopCoroutine(specialCharacterCoroutine);
            specialCharacterCoroutine = null;
        }

        // メッセージパネルを表示
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }

        // キャラクターの表情画像を設定
        if (characterImage != null)
        {
            characterImage.sprite = character;
            // 画像が設定されている場合だけ表示
            characterImage.enabled = character != null;
        }

        // テキストUIが設定されていなければ警告を出して終了
        if (messageText == null)
        {
            Debug.LogWarning("Message Text が設定されていません。");
            return;
        }

        // 特殊文字タグを解析して、表示用テキストからタグを取り除く
        ParseSpecialCharacterTags(ref message);

        // タグを取り除いたセリフを設定
        messageText.text = message;

        // 文字送りを開始する前は文字数を0にする
        messageText.maxVisibleCharacters = 0;

        // TextMeshProの文字情報を更新
        messageText.ForceMeshUpdate();

        // 各文字の元の頂点位置を保存する
        InitializeSpecialCharacters();

        // 特殊文字のアニメーションを開始
        specialCharacterCoroutine = StartCoroutine(AnimateSpecialCharacters());

        // 文字送りと自動非表示を開始
        messageCoroutine = StartCoroutine(TypeAndHide());
    }
    #endregion


    #region 特殊文字タグの解析

    /// <summary>
    /// WavyとShakyのタグを解析し、タグ自体をテキストから取り除く
    /// 対象の文字に対応するエフェクト番号を保存する
    /// </summary>
    private void ParseSpecialCharacterTags(ref string message)
    {
        // タグを取り除いた文字列を作成する
        StringBuilder result = new StringBuilder();

        // 文字ごとのエフェクト番号を保存するリスト
        List<CharacterEffect> effects = new List<CharacterEffect>();
        // 現在有効になっているエフェクト(0 = 通常、1 = Wavy、2 = Shaky)
        CharacterEffect currentEffect = CharacterEffect.Normal;

        // 元の文字列を先頭から解析する
        for (int i = 0; i < message.Length;)
        {
            // タグの開始位置を見つけた場合
            if (message[i] == '<')
            {
                // タグの終わりを探す
                int tagEnd = message.IndexOf('>', i);

                // 正しいタグ形式の場合
                if (tagEnd >= 0)
                {
                    // タグ全体を取得
                    string tag = message.Substring(i, tagEnd - i + 1);

                    if (tag == "<wavy>")
                        currentEffect = CharacterEffect.Wavy;
                    else if (tag == "</wavy>")
                        currentEffect = CharacterEffect.Normal;
                    else if (tag == "<shaky>")
                        currentEffect = CharacterEffect.Shaky;
                    else if (tag == "</shaky>")
                        currentEffect = CharacterEffect.Normal;
                    else
                    {
                        // colorなど、TextMeshProの通常のリッチテキストタグは残す
                        result.Append(tag);
                    }
                    // 処理済みのタグの次から解析を続ける
                    i = tagEnd + 1;
                    continue;
                }
            }
            // 通常の文字を追加
            result.Append(message[i]);
            // その文字に適用するエフェクトを保存
            effects.Add(currentEffect);

            i++;
        }
        // 特殊文字タグを取り除いた文字列に置き換える
        message = result.ToString();

        // 文字ごとのエフェクトを配列に変換
        characterEffects = effects.ToArray();
    }
    #endregion


    #region 特殊文字の初期化

    /// <summary>
    /// 各文字の元の頂点位置を保存する
    /// アニメーションで動かした文字を元の位置に戻せるようにする
    /// </summary>
    private void InitializeSpecialCharacters()
    {
        // TextMeshProの文字情報を取得
        TMP_TextInfo textInfo = messageText.textInfo;

        // 文字数を取得
        int characterCount = textInfo.characterCount;

        // 各文字の頂点位置を保存する配列を用意
        originalVertices = new Vector3[characterCount][];

        // 文字ごとに処理
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 空白など、頂点を持たない文字はスキップ
            if (!charInfo.isVisible)
                continue;

            // 文字が属するメッシュと頂点の位置を取得
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 1文字につき4頂点分の位置を保存
            originalVertices[i] = new Vector3[4];

            for (int j = 0; j < 4; j++)
            {
                originalVertices[i][j] = vertices[vertexIndex + j];
            }
        }

        // 特殊文字のアニメーションで使用する配列を文字数に合わせる
        // 解析した文字数とTextMeshProの文字数に差がある場合にも対応
        if (characterEffects == null ||
            characterEffects.Length != characterCount)
        {
            CharacterEffect[] adjustedEffects = new CharacterEffect[characterCount];

            if (characterEffects != null)
            {
                int copyCount = Mathf.Min(characterEffects.Length, characterCount);
                System.Array.Copy(characterEffects, adjustedEffects, copyCount);
            }
            characterEffects = adjustedEffects;
        }
    }
    #endregion


    #region 特殊文字アニメーション

    /// <summary>
    /// WavyとShakyの文字を毎フレーム動かす
    /// </summary>
    private IEnumerator AnimateSpecialCharacters()
    {
        // テキストが存在する間、毎フレームアニメーションする
        while (messageText != null)
        {
            TMP_TextInfo textInfo = messageText.textInfo;
            int characterCount = textInfo.characterCount;

            // 文字ごとに処理
            for (int i = 0; i < characterCount; i++)
            {
                // 元の頂点位置がない文字はスキップ
                if (originalVertices == null || i >= originalVertices.Length || originalVertices[i] == null)
                {
                    continue;
                }

                // エフェクトの種類を取得
                CharacterEffect effect = CharacterEffect.Normal;

                if (characterEffects != null &&
                    i < characterEffects.Length)
                {
                    effect = characterEffects[i];
                }

                // 通常の文字は動かさない
                if (effect == CharacterEffect.Normal)
                {
                    continue;
                }

                // 文字の頂点情報を取得
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // 文字を元の位置に戻す
                // 前フレームの移動量が累積するのを防ぐ
                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertices[i][j];
                }

                // 文字の移動量
                Vector3 offset = Vector3.zero;

                // Wavy：文字を波のように上下させる
                if (effect == CharacterEffect.Wavy)
                {
                    float y = Mathf.Sin(Time.time * wavySpeed + i * 0.5f) * wavyAmplitude;
                    offset = new Vector3(0f, y, 0f);
                }
                // Shaky：文字を細かく震わせる
                else if (effect == CharacterEffect.Shaky)
                {
                    float time = Time.time * shakySpeed;
                    float x = Mathf.Sin(time + i * 12.9898f) * shakyAmplitude;
                    float y = Mathf.Cos(time * 1.3f + i * 7.233f) * shakyAmplitude;

                    offset = new Vector3(x, y, 0f);
                }

                // 4頂点すべてに同じ移動量を適用
                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] += offset;
                }
            }

            // 頂点位置の変更をTextMeshProに反映
            messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            // 次のフレームまで待機
            yield return null;
        }
    }

    #endregion


    #region 文字送りとフェードイン

    /// <summary>
    /// 一定間隔で文字のフェードインを開始し、全文表示後に自動で消す
    /// 文字の表示開始間隔とフェード時間は別々に管理する
    /// </summary>
    private IEnumerator TypeAndHide()
    {
        // 全文字の頂点情報を生成する
        // 文字ごとの透明度を制御するため、いったん全文字を有効にする
        messageText.maxVisibleCharacters = int.MaxValue;
        messageText.ForceMeshUpdate();

        TMP_TextInfo textInfo = messageText.textInfo;
        int characterCount = textInfo.characterCount;

        // 各文字の元の色を保存する
        Color32[][] originalColors = new Color32[characterCount][];

        // すべての文字を最初は透明にする
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 空白など、表示用の頂点がない文字はスキップ
            if (!charInfo.isVisible)
                continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            // 1文字につき4頂点分の色を保存
            originalColors[i] = new Color32[4];

            for (int j = 0; j < 4; j++)
            {
                // 元の色を保存
                originalColors[i][j] = colors[vertexIndex + j];

                // アルファ値を0にして透明にする
                Color32 color = colors[vertexIndex + j];
                color.a = 0;
                colors[vertexIndex + j] = color;
            }
        }

        // 透明化した色を反映
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // 各文字のフェード開始時刻を記録
        float[] startTimes = new float[characterCount];

        for (int i = 0; i < characterCount; i++)
        {
            // 0文字目は即座に開始し、その後は一定間隔で開始する
            startTimes[i] = i * characterInterval;
        }

        // 全文のフェードが完了するまでの時間
        float totalDuration =
            (characterCount > 0 ? (characterCount - 1) * characterInterval : 0f) + characterFadeTime;

        float elapsed = 0f;

        // 全文字を並行してフェードインさせる
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < characterCount; i++)
            {
                // 表示用の頂点がない文字はスキップ
                if (originalColors[i] == null)
                    continue;

                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                int vertexIndex = charInfo.vertexIndex;
                int materialIndex = charInfo.materialReferenceIndex;

                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                // その文字のフェード進行度を計算
                // 文字ごとに開始時刻が異なるため、フェードが重なって進む
                float alpha = characterFadeTime > 0f ? Mathf.Clamp01((elapsed - startTimes[i]) / characterFadeTime) : 1f;

                // 元の色を維持しながら透明度だけを変更
                for (int j = 0; j < 4; j++)
                {
                    Color32 color = originalColors[i][j];

                    color.a = (byte)(originalColors[i][j].a * alpha);
                    colors[vertexIndex + j] = color;
                }
            }

            // 変更した透明度を反映
            messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null;
        }

        // 全文を完全に表示された状態にする
        messageText.maxVisibleCharacters = int.MaxValue;

        // 全文表示後、指定時間だけ待つ
        yield return new WaitForSeconds(displayTime);

        // 表示時間が終了したらメッセージを消す
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        // 不要になった特殊文字アニメーションを停止
        if (specialCharacterCoroutine != null)
        {
            StopCoroutine(specialCharacterCoroutine);
            specialCharacterCoroutine = null;
        }

        // コルーチンの実行状態をリセット
        messageCoroutine = null;
    }
    #endregion


    #region メッセージを消す

    /// <summary>
    /// 表示中のセリフを即座に消す
    /// </summary>
    public void HideMessage()
    {
        // 文字送り・自動非表示処理を停止
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        // 特殊文字アニメーションを停止
        if (specialCharacterCoroutine != null)
        {
            StopCoroutine(specialCharacterCoroutine);
            specialCharacterCoroutine = null;
        }

        // メッセージパネルを非表示にする
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        // 次に表示するセリフが前のセリフの続きにならないようにする
        if (messageText != null)
        {
            messageText.maxVisibleCharacters = 0;
        }
    }
    #endregion
}
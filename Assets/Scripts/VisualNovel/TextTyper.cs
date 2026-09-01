using System.Collections;
using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// セリフを一文字ずつ表示するクラス
/// </summary>
public class TextTyper : MonoBehaviour
{
    #region 特殊演出タグ
    private const string WAVY_TAG = "<wavy>";
    private const string WAVY_END_TAG = "</wavy>";

    private const string SHAKY_TAG = "<shaky>";
    private const string SHAKY_END_TAG = "</shaky>";
    #endregion

    [Header("表示するテキスト")]
    [SerializeField] private TextMeshProUGUI messageText;

    //[Header("文字送り速度（秒）")]
    //[SerializeField] private float textSpeed = 0.1f;

    [Header("──────────────────────────────")]
    [Header("文字フェード")]
    [SerializeField] private bool fadeInCharacter = true;

    [Header("──────────────────────────────")]
    [Header("Letter Pop")]
    [SerializeField] private bool letterPop = true;

    [Tooltip("表示開始時の文字サイズ")]
    [SerializeField] private float letterPopStartScale = 0.7f;

    [Tooltip("一瞬だけ大きくなる倍率")]
    [SerializeField] private float letterPopOvershootScale = 1.15f;

    [Tooltip("Letter Popの拡大が終わる位置（0～1）")]
    [SerializeField, Range(0f, 1f)] private float letterPopPeakPosition = 0.5f;

    [Header("──────────────────────────────")]

    [Header("「、」(読点) 一時停止機能")]
    //[SerializeField] private bool pauseAtComma = true;
    [Tooltip("「、」(読点) の待機時間倍率")]
    [SerializeField] private float commaPauseMultiplier = 5f;

    [Header("「。」(句点) 一時停止機能")]
    //[SerializeField] private bool pauseAtPeriod = true;
    [Tooltip("「。」(句点) の待機時間倍率")]
    [SerializeField] private float periodPauseMultiplier = 5f;

    [Header("──────────────────────────────")]
    [Header("Wavy")]
    [SerializeField] private bool wavy = true;

    [Tooltip("上下に動く幅")][SerializeField] private float wavyAmplitude = 5f;
    [Tooltip("波の速さ")][SerializeField] private float wavySpeed = 5f;

    [Header("Shaky")]
    [SerializeField] private bool shaky = true;

    [Tooltip("上下左右に動く最大幅")][SerializeField] private float shakyAmplitude = 3f;
    [Tooltip("揺れが変化する速さ")][SerializeField] private float shakySpeed = 20f;





    private bool isSkipMode = false; // Skipモード中か

    private string currentMessage;           // 現在表示している全文を保存
    private Coroutine typingCoroutine;       // 文字送りのCoroutineを保存

    public event Action OnTypingFinished;    // 文字送り終了時に呼ばれるイベント

    private readonly Vector3[] originalCharacterVertices = new Vector3[4];   // 現在アニメーションしている文字の元の頂点

    private readonly Dictionary<int, Vector3[]> originalVertices = new();    // 各文字の元の頂点位置を保存する

    private readonly List<int> wavyCharacters = new();   // Wavy対象の文字番号
    private readonly List<int> shakyCharacters = new();  // Shaky対象の文字番号

    private bool isWavy = false;     // 現在Wavy範囲の中か
    private bool isShaky = false;    // 現在Shaky範囲の中か


    #region Get:読み取り関数
    public bool IsTyping { get; private set; } // 現在文字送り中かどうか (true = 表示中│false = 表示完了)
    #endregion


    #region Skipモード

    /// <summary>
    /// Skipモードの状態を設定 (NovelManagerからSkipモードの状態を受け取る)
    /// </summary>
    public void SetSkipMode(bool skipMode)
    {
        isSkipMode = skipMode;
    }
    #endregion




    private void Update()
    {
        UpdateSpecialCharacters();
    }


    #region 特殊文字を毎フレーム動かす

    /// <summary>
    /// 特殊文字を毎フレーム動かす
    /// </summary>
    /// <summary>
    /// Wavy / Shaky対象の文字を毎フレーム動かす
    /// </summary>
    private void UpdateSpecialCharacters()
    {
        // TextMeshProが設定されていなければ何もしない
        if (messageText == null)
            return;
        // 特殊演出がなければ何もしない
        if (wavyCharacters.Count == 0 && shakyCharacters.Count == 0)
            return;

        // TextMeshProの情報を更新
        messageText.ForceMeshUpdate();


        // Wavy
        foreach (int characterIndex in wavyCharacters)
        {
            // 文字番号が存在しない場合はスキップ
            if (characterIndex >= messageText.textInfo.characterCount)
                continue;

            // 文字情報を取得
            TMP_CharacterInfo charInfo = messageText.textInfo.characterInfo[characterIndex];

            // 表示されない文字ならスキップ
            if (!charInfo.isVisible)
                continue;
            // 元の頂点が保存されていなければスキップ
            if (!originalVertices.ContainsKey(characterIndex))
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = messageText.textInfo.meshInfo[materialIndex].vertices;

            // 元の頂点を取得
            Vector3[] original = originalVertices[characterIndex];

            // 文字ごとに波の位置をずらす
            float offset = Mathf.Sin(Time.time * wavySpeed + characterIndex) * wavyAmplitude;
            // 元の位置から上下に移動
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = original[j] + Vector3.up * offset;
            }
        }


        // Shaky
        foreach (int characterIndex in shakyCharacters)
        {
            // 文字番号が存在しない場合はスキップ
            if (characterIndex >= messageText.textInfo.characterCount)
                continue;

            // 文字情報を取得
            TMP_CharacterInfo charInfo = messageText.textInfo.characterInfo[characterIndex];

            // 表示されない文字ならスキップ
            if (!charInfo.isVisible)
                continue;
            // 元の頂点が保存されていなければスキップ
            if (!originalVertices.ContainsKey(characterIndex))
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = messageText.textInfo.meshInfo[materialIndex].vertices;

            // 元の頂点を取得
            Vector3[] original = originalVertices[characterIndex];

            // 上下左右のランダムな移動量
            Vector2 offset = UnityEngine.Random.insideUnitCircle * shakyAmplitude;
            // 元の位置から移動
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = original[j] + (Vector3)offset;
            }
        }

        // 頂点情報をTextMeshProへ反映
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
    #endregion


    #region 1文字ずつ表示
    /// <summary>
    /// 新しい文章を表示開始する
    /// </summary>
    public void StartTyping(string message)
    {
        // 表示する文章を保存
        currentMessage = message;

        // 前の文章の特殊演出情報をリセット
        wavyCharacters.Clear();
        shakyCharacters.Clear();
        originalVertices.Clear();

        isWavy = false;
        isShaky = false;

        // もし前回の文字送りがまだ動いていたら止める
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 一文字ずつ表示開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 一文字ずつ表示するCoroutine
    /// </summary>
    IEnumerator TypeText()
    {
        // 文字送り速度を設定から取得
        float textSpeed = TextSettingsManager.Instance.GetTextSpeed();

        // 文字送り開始
        IsTyping = true;
        // 最初は空文字にする
        messageText.text = "";

        // 現在の文章を一文字ずつ処理
        for (int i = 0; i < currentMessage.Length; i++)
        {
            // Rich Text / 特殊演出タグを検出
            if (currentMessage[i] == '<')
            {
                // Wavy開始タグ
                if (currentMessage.Substring(i).StartsWith(WAVY_TAG))
                {
                    isWavy = true;

                    // タグを表示せずにスキップ
                    i += WAVY_TAG.Length - 1;

                    continue;
                }
                // Wavy終了タグ
                if (currentMessage.Substring(i).StartsWith(WAVY_END_TAG))
                {
                    isWavy = false;

                    // タグを表示せずにスキップ
                    i += WAVY_END_TAG.Length - 1;

                    continue;
                }

                // Shaky開始タグ
                if (currentMessage.Substring(i).StartsWith(SHAKY_TAG))
                {
                    isShaky = true;

                    // タグを表示せずにスキップ
                    i += SHAKY_TAG.Length - 1;

                    continue;
                }
                // Shaky終了タグ
                if (currentMessage.Substring(i).StartsWith(SHAKY_END_TAG))
                {
                    isShaky = false;

                    // タグを表示せずにスキップ
                    i += SHAKY_END_TAG.Length - 1;

                    continue;
                }

                // 通常のRich Textタグ
                int tagEnd = currentMessage.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    // タグ全体を一気に追加
                    messageText.text += currentMessage.Substring(i, tagEnd - i + 1);
                    // タグの最後まで進める
                    i = tagEnd;

                    // タグ自体では待機しない
                    continue;
                }
            }
            // 一文字追加
            messageText.text += currentMessage[i];
            // TextMeshProの情報を更新
            messageText.ForceMeshUpdate();

            // 今追加した文字のインデックス
            int characterIndex = messageText.textInfo.characterCount - 1;

            // 表示される文字の場合
            if (characterIndex >= 0 &&
                messageText.textInfo.characterInfo[characterIndex].isVisible)
            {
                // 元の頂点を保存
                SaveOriginalVertices(characterIndex);

                // Wavy範囲なら登録
                if (isWavy && wavy)
                {
                    if (!wavyCharacters.Contains(characterIndex))
                    {
                        wavyCharacters.Add(characterIndex);
                    }
                }
                // Shaky範囲なら登録
                if (isShaky && shaky)
                {
                    if (!shakyCharacters.Contains(characterIndex))
                    {
                        shakyCharacters.Add(characterIndex);
                    }
                }
            }


            // 表示される文字の場合
            if (characterIndex >= 0 && messageText.textInfo.characterInfo[characterIndex].isVisible)
            {
                // フェードを使う場合は透明にする
                if (fadeInCharacter)
                {
                    SetCharacterAlpha(characterIndex, 0f);
                }
                // Letter Popを使う場合は小さい状態から開始
                if (letterPop)
                {
                    // Letter Pop開始前の元の頂点を保存
                    int materialIndex = messageText.textInfo.characterInfo[characterIndex].materialReferenceIndex;
                    int vertexIndex = messageText.textInfo.characterInfo[characterIndex].vertexIndex;

                    Vector3[] vertices = messageText.textInfo.meshInfo[materialIndex].vertices;
                    for (int v = 0; v < 4; v++)
                    {
                        originalCharacterVertices[v] = vertices[vertexIndex + v];
                    }

                    // 小さい状態から開始
                    SetCharacterScale(characterIndex, letterPopStartScale);
                }

                // アニメーション
                float time = 0f;
                while (time < textSpeed)
                {
                    time += Time.deltaTime;
                    float progress = Mathf.Clamp01(time / textSpeed);

                    // フェード 
                    if (fadeInCharacter)
                    {
                        float alpha = progress;
                        SetCharacterAlpha(characterIndex, alpha);
                    }

                    // ポップ
                    if (letterPop)
                    {
                        float scale;
                        // 前半：小 → 最大
                        if (progress < letterPopPeakPosition)
                        {
                            float t = progress / letterPopPeakPosition;
                            scale = Mathf.Lerp(letterPopStartScale, letterPopOvershootScale, t);
                        }
                        // 後半：最大 → 通常
                        else
                        {
                            float t = (progress - letterPopPeakPosition) / letterPopPeakPosition;
                            scale = Mathf.Lerp(letterPopOvershootScale, 1f, t);
                        }
                        SetCharacterScale(characterIndex, scale);
                    }
                    // 次のフレームへ
                    yield return null;
                }

                // アニメーション終了
                if (fadeInCharacter)
                {
                    SetCharacterAlpha(characterIndex, 1f);
                }
                if (letterPop)
                {
                    SetCharacterScale(characterIndex, 1f);
                }
            }
            else
            {
                // 空白など表示されない文字
                yield return new WaitForSeconds(textSpeed);
            }

            // 句読点一時停止がONの場合
            if (TextSettingsManager.Instance.GetPauseAtPunctuation())
            {
                // 「、」の場合はTextSpeedに応じた待機
                if (currentMessage[i] == '、')
                {
                    yield return new WaitForSeconds(textSpeed * commaPauseMultiplier);
                }
                // 「。」の場合はTextSpeedに応じた待機
                else if (currentMessage[i] == '。')
                {
                    yield return new WaitForSeconds(textSpeed * periodPauseMultiplier);
                }
            }
        }

        // 文字送り終了
        IsTyping = false;
        // 「文字送り終了」を通知 (イベント)
        OnTypingFinished?.Invoke();
    }
    #endregion


    #region 文字送りスキップ
    /// <summary>
    /// 文字送りスキップ (クリック時など)
    /// </summary>
    public void Skip(bool callEvent = true)
    {
        // 文字送り中でなければ何もしない
        if (!IsTyping) return;

        // Coroutineを停止
        StopCoroutine(typingCoroutine);

        // 特殊演出タグを除去して全文表示
        string displayMessage = currentMessage;

        displayMessage = displayMessage.Replace(WAVY_TAG, "");
        displayMessage = displayMessage.Replace(WAVY_END_TAG, "");
        displayMessage = displayMessage.Replace(SHAKY_TAG, "");
        displayMessage = displayMessage.Replace(SHAKY_END_TAG, "");

        messageText.text = displayMessage;

        // 文字送り終了
        IsTyping = false;

        if (callEvent)
        {
            //「文字送り終了」を通知 (イベント)
            OnTypingFinished?.Invoke();
        }
    }
    #endregion


    #region 全文表示
    /// <summary>
    /// 文章を一瞬で表示する
    /// </summary>
    public void ShowInstant(string message)
    {
        // Coroutine停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentMessage = message;

        // 特殊演出タグを除去して全文表示
        string displayMessage = message;

        displayMessage = displayMessage.Replace(WAVY_TAG, "");
        displayMessage = displayMessage.Replace(WAVY_END_TAG, "");
        displayMessage = displayMessage.Replace(SHAKY_TAG, "");
        displayMessage = displayMessage.Replace(SHAKY_END_TAG, "");

        messageText.text = displayMessage;

        // 文字送り終了
        IsTyping = false;
        //「文字送り終了」を通知 (イベント)
        OnTypingFinished?.Invoke();
    }
    #endregion


    #region 指定した文字の透明度を変更

    /// <summary>
    /// 指定した文字の透明度を変更
    /// </summary>
    private void SetCharacterAlpha(int characterIndex, float alpha)
    {
        TMP_CharacterInfo charInfo = messageText.textInfo.characterInfo[characterIndex];

        if (!charInfo.isVisible)
            return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Color32[] vertexColors = messageText.textInfo.meshInfo[materialIndex].colors32;

        byte a = (byte)(alpha * 255f);

        vertexColors[vertexIndex + 0].a = a;
        vertexColors[vertexIndex + 1].a = a;
        vertexColors[vertexIndex + 2].a = a;
        vertexColors[vertexIndex + 3].a = a;

        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    #endregion


    #region ポップテキスト

    /// <summary>
    /// 指定した文字の大きさを変更
    /// </summary>
    private void SetCharacterScale(int characterIndex, float scale)
    {
        TMP_CharacterInfo charInfo = messageText.textInfo.characterInfo[characterIndex];

        // 表示されない文字なら何もしない
        if (!charInfo.isVisible)
            return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        // 文字の4頂点
        Vector3[] vertices = messageText.textInfo.meshInfo[materialIndex].vertices;

        // 文字の中心座標を計算
        Vector3 center = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) * 0.5f;

        // 保存した「元の頂点」から毎回計算する
        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i] = center + (originalCharacterVertices[i] - center) * scale;
        }

        // 頂点情報を更新
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
    #endregion


    #region 指定した文字の元の頂点を保存

    /// <summary>
    /// 指定した文字の元の頂点を保存する
    /// </summary>
    private void SaveOriginalVertices(int characterIndex)
    {
        TMP_CharacterInfo charInfo = messageText.textInfo.characterInfo[characterIndex];

        // 表示されない文字なら何もしない
        if (!charInfo.isVisible)
            return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Vector3[] vertices = messageText.textInfo.meshInfo[materialIndex].vertices;

        // すでに保存済みなら何もしない
        if (originalVertices.ContainsKey(characterIndex))
            return;

        Vector3[] original = new Vector3[4];

        for (int i = 0; i < 4; i++)
        {
            original[i] = vertices[vertexIndex + i];
        }

        originalVertices.Add(characterIndex, original);
    }
    #endregion
}
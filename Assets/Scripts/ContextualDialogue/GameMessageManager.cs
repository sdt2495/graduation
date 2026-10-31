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
    #region 文字に適用する特殊エフェクトの種類
    /// <summary>
    /// 文字に適用する特殊エフェクトの種類
    /// </summary>
    private enum CharacterEffect
    {
        Normal, // 通常
        Wavy,   // 波揺れ
        Shaky   // 震え
    }
    #endregion

    #region <wait=>対応
    /// <wait=>対応
    private class WaitPoint
    {
        public int characterIndex;
        public float waitTime;
    }
    private List<WaitPoint> waitPoints = new();
    #endregion

    [Header("フェード")]
    [SerializeField] private CanvasGroup messageCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.25f;


    [Header("──────────────────────────────")]
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

    [Header("──────────────────────────────")]
    [Header("Letter Pop")]
    [SerializeField] private bool letterPop = true;

    [Tooltip("表示開始時の文字サイズ")]
    [SerializeField] private float letterPopStartScale = 0f;

    [Tooltip("一瞬だけ大きくなる最大倍率")]
    [SerializeField] private float letterPopOvershootScale = 1.0f;

    [Header("拡大が最大になる位置（0～1）")]
    [SerializeField, Range(0f, 1f)] private float letterPopPeakPosition = 1.0f;


    private CharacterEffect[] characterEffects; // 各文字に適用するエフェクト

    private Vector3[][] originalVertices;    // 各文字の元の頂点位置

    private Coroutine specialCharacterCoroutine; // セリフ表示と自動非表示を管理するコルーチン
    private Coroutine messageCoroutine; // 現在実行中の自動非表示コルーチン


    private Coroutine fadeCoroutine;     // 現在実行中のフェード処理を管理するコルーチン
    private bool isMessageVisible;      // メッセージが表示状態かどうか
    private bool isFadingOut;            // フェードアウト中かどうか


    #region Awake()

    private void Awake()
    {
        // CanvasGroupが設定されている場合
        if (messageCanvasGroup != null)
        {
            // 初期状態では完全に透明にする
            messageCanvasGroup.alpha = 0f;

            // 初期状態ではUIへの操作を受け付けない
            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }

        // メッセージパネルが設定されている場合
        if (messagePanel != null)
        {
            // ゲーム開始時はメッセージパネルを非表示にする
            messagePanel.SetActive(false);
        }
        // メッセージが非表示の状態として初期化
        isMessageVisible = false;
        isFadingOut = false;
    }
    #endregion


    #region CANVASのフェード

    /// <summary>
    /// 現在の透明度から指定した透明度までフェードする
    /// </summary>
    /// <param name="targetAlpha">フェード終了時の透明度（0：透明、1：不透明）</param>
    /// <param name="duration">フェードにかける時間（秒）</param>
    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        // CanvasGroupが設定されていない場合は処理を終了する
        if (messageCanvasGroup == null)
            yield break;

        // フェード開始時点の透明度を取得する
        float startAlpha = messageCanvasGroup.alpha;

        // フェード時間が0以下の場合は、即座に目標の透明度に設定する
        if (duration <= 0f)
        {
            messageCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        // フェード開始からの経過時間を初期化する
        float elapsed = 0f;

        // 指定した時間が経過するまでフェードを続ける
        while (elapsed < duration)
        {
            // 前フレームからの経過時間を加算する
            elapsed += Time.deltaTime;

            // 経過時間からフェードの進行度を算出する（0～1）
            float progress = Mathf.Clamp01(elapsed / duration);

            // 開始時の透明度から目標の透明度まで徐々に変更する
            messageCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            // 次のフレームまで処理を待つ
            yield return null;
        }

        // 最後に透明度を目標値へ正確に設定する
        messageCanvasGroup.alpha = targetAlpha;
    }


    /// <summary>
    /// フェード処理を開始する
    /// 進行中のフェードがあれば停止し、現在の透明度から再開する
    /// </summary>
    private void StartFade(float targetAlpha, float duration)
    {
        // すでにフェード処理が実行中なら停止する
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // フェードの方向を記録する（true：フェードアウト中、false：フェードイン中）
        isFadingOut = targetAlpha <= 0f;

        // フェードインを開始した時点でメッセージを表示状態にする
        if (targetAlpha > 0f)
        {
            isMessageVisible = true;
        }

        // 指定した透明度と時間でフェード処理を開始する
        fadeCoroutine = StartCoroutine(FadeAndUpdateState(targetAlpha, duration));
    }


    /// <summary>
    /// フェード処理の完了後、メッセージの表示状態を更新する
    /// </summary>
    /// <param name="targetAlpha">フェード終了時の透明度</param>
    /// <param name="duration">フェードにかける時間</param>
    private IEnumerator FadeAndUpdateState(float targetAlpha, float duration)
    {
        // 指定した透明度までフェードさせる
        yield return FadeCanvasGroup(targetAlpha, duration);

        // フェード処理が終了したため、コルーチンの参照を解除
        fadeCoroutine = null;

        // 完全に透明になった場合は、メッセージを非表示にする
        if (targetAlpha <= 0f)
        {
            // メッセージが非表示になったことを記録
            isMessageVisible = false;

            // フェードアウトが終了したことを記録
            isFadingOut = false;

            // メッセージパネルを非表示にする
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }

            // 次回表示時に備えて、テキストの表示文字数をリセット
            if (messageText != null)
            {
                messageText.maxVisibleCharacters = 0;
            }
        }
        else
        {
            // メッセージが表示状態になったことを記録
            isMessageVisible = true;

            // フェードアウトが終了したことを記録
            isFadingOut = false;
        }
    }
    #endregion


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

        // 表示パネルが非表示なら有効化
        if (messagePanel != null && !messagePanel.activeSelf)
        {
            messagePanel.SetActive(true);
        }

        // 非表示、またはフェードアウト中ならフェードイン
        if (!isMessageVisible || isFadingOut)
        {
            StartFade(1f, fadeInDuration);
        }

        // キャラクターの表情画像を設定
        if (characterImage != null)
        {
            characterImage.sprite = character;
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

        // 各文字の元の頂点位置を保存
        InitializeSpecialCharacters();

        // 特殊文字のアニメーションを開始
        specialCharacterCoroutine = StartCoroutine(AnimateSpecialCharacters());

        // 文字送りと自動非表示を開始
        messageCoroutine = StartCoroutine(TypeAndHide());
    }
    #endregion


    #region 特殊文字タグの解析

    /// <summary>
    /// Wavy / Shaky / Waitタグを解析し、特殊タグをテキストから取り除く
    /// Wavy / Shakyは各文字のエフェクトを保存
    /// Waitは指定位置と待機時間を保存
    /// </summary>
    private void ParseSpecialCharacterTags(ref string message)
    {
        // タグを取り除いた文字列を作成する
        StringBuilder result = new StringBuilder();

        // 文字ごとのエフェクトを保存
        List<CharacterEffect> effects = new List<CharacterEffect>();

        // 待機情報を初期化
        waitPoints.Clear();

        // 現在有効になっているエフェクト
        CharacterEffect currentEffect = CharacterEffect.Normal;

        // 解析する文字列上の位置
        int displayCharacterIndex = 0;

        // 元の文字列を先頭から解析
        for (int i = 0; i < message.Length;)
        {
            // タグの開始位置
            if (message[i] == '<')
            {
                // タグの終わりを探す
                int tagEnd = message.IndexOf('>', i);

                // 正しいタグ形式の場合
                if (tagEnd >= 0)
                {
                    // タグ全体を取得
                    string tag = message.Substring(i, tagEnd - i + 1);

                    // Wavy開始
                    if (tag == "<wavy>")
                    {
                        currentEffect = CharacterEffect.Wavy;
                    }
                    // Wavy終了
                    else if (tag == "</wavy>")
                    {
                        currentEffect = CharacterEffect.Normal;
                    }
                    // Shaky開始
                    else if (tag == "<shaky>")
                    {
                        currentEffect = CharacterEffect.Shaky;
                    }
                    // Shaky終了
                    else if (tag == "</shaky>")
                    {
                        currentEffect = CharacterEffect.Normal;
                    }
                    // Wait
                    else if (tag.StartsWith("<wait=") && tag.EndsWith(">"))
                    {
                        // <wait= と > の間を取得
                        string waitTimeText = tag.Substring("<wait=".Length, tag.Length - "<wait=".Length - 1);

                        // 数値として読み取れた場合
                        if (float.TryParse(waitTimeText, out float waitTime))
                        {
                            if (waitTime > 0f)
                            {
                                waitPoints.Add(new WaitPoint
                                {
                                    characterIndex = displayCharacterIndex,
                                    waitTime = waitTime
                                });
                            }
                        }
                    }
                    else
                    {
                        // colorなど、通常のTextMeshProリッチテキストタグは残す
                        result.Append(tag);
                    }

                    // タグの次から解析
                    i = tagEnd + 1;
                    continue;
                }
            }

            // 通常の文字を追加
            result.Append(message[i]);

            // その文字に適用するエフェクトを保存
            effects.Add(currentEffect);

            // 表示文字数を1増やす
            displayCharacterIndex++;

            i++;
        }

        // タグを取り除いた文字列に置き換える
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

        // 各文字の元の頂点位置を保存
        Vector3[][] originalScaleVertices = new Vector3[characterCount][];

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
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 1文字につき4頂点分の色を保存
            originalColors[i] = new Color32[4];

            // 1文字につき4頂点分の元位置を保存
            originalScaleVertices[i] = new Vector3[4];

            for (int j = 0; j < 4; j++)
            {
                // 元の色を保存
                originalColors[i][j] = colors[vertexIndex + j];

                // アルファ値を0にして透明にする
                Color32 color = colors[vertexIndex + j];
                color.a = 0;
                colors[vertexIndex + j] = color;

                // 元の頂点位置を保存
                originalScaleVertices[i][j] = vertices[vertexIndex + j];
            }

            // Letter Popを使う場合
            if (letterPop)
            {
                // 文字の中心座標
                Vector3 center = (originalScaleVertices[i][0] + originalScaleVertices[i][2]) * 0.5f;

                // 最初のサイズに設定
                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = center + (originalScaleVertices[i][j] - center) * letterPopStartScale;
                }
            }
        }

        // 透明化した色を反映
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // 各文字のフェード開始時刻を記録
        float[] startTimes = new float[characterCount];

        // 現在までの文字送り時間
        float currentTime = 0f;

        // 文字ごとの開始時間を計算
        for (int i = 0; i < characterCount; i++)
        {
            // この文字の表示開始時間を保存
            startTimes[i] = currentTime;

            // 次の文字へ進む
            currentTime += characterInterval;

            // この文字の後に待機が設定されているか確認
            foreach (WaitPoint waitPoint in waitPoints)
            {
                if (waitPoint.characterIndex == i + 1)
                {
                    currentTime += waitPoint.waitTime;
                    break;
                }
            }
        }

        // 全文のフェードが完了するまでの時間
        float totalDuration = characterCount > 0 ? startTimes[characterCount - 1] + characterFadeTime : 0f;

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
                float alpha = characterFadeTime > 0f ? Mathf.Clamp01((elapsed - startTimes[i]) / characterFadeTime) : 1f;

                // Letter Pop：文字を拡大してから通常サイズへ戻す
                if (letterPop)
                {
                    float scale;

                    if (alpha < letterPopPeakPosition)
                    {
                        // 開始サイズ → 最大サイズ
                        float progress = letterPopPeakPosition > 0f ? alpha / letterPopPeakPosition : 1f;

                        scale = Mathf.Lerp(letterPopStartScale, letterPopOvershootScale, progress);
                    }
                    else
                    {
                        // 最大サイズ → 通常サイズ
                        float progress = letterPopPeakPosition < 1f ? (alpha - letterPopPeakPosition) / (1f - letterPopPeakPosition) : 1f;

                        scale = Mathf.Lerp(letterPopOvershootScale, 1f, progress);
                    }

                    // 文字の中心を基準に拡大・縮小
                    Vector3 center = (originalScaleVertices[i][0] + originalScaleVertices[i][2]) * 0.5f;

                    for (int j = 0; j < 4; j++)
                    {
                        Vector3 originalVertex = originalScaleVertices[i][j];

                        textInfo.meshInfo[materialIndex].vertices[vertexIndex + j] = center + (originalVertex - center) * scale;
                    }
                }

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

        // フェードアウト開始
        StartFade(0f, fadeOutDuration);

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

        // 現在の透明度からフェードアウト
        StartFade(0f, fadeOutDuration);
    }

    /// <summary>
    /// フェードアウトしてからメッセージを非表示にする
    /// </summary>
    private IEnumerator HideMessageCoroutine()
    {
        yield return FadeCanvasGroup(0f, fadeOutDuration);

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.maxVisibleCharacters = 0;
        }

        messageCoroutine = null;
    }
    #endregion
}
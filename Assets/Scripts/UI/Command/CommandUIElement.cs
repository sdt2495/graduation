using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===================================
// コマンド１個の表示・状態管理
// ===================================

public class CommandUIElement : MonoBehaviour
{
    // 黒いひし形
    [SerializeField] private Image diamondImage;

    // 状況に応じて画像を切り替える枠
    [SerializeField] private Image activeDiamondImage;
    
    // 矢印
    [SerializeField] private Image arrowImage;

    [Header("矢印画像")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("枠画像")]
    [SerializeField] private Sprite waitingFrameSprite;
    [SerializeField] private Sprite succesFrameSprite;
    [SerializeField] private Sprite missFrameSprite;

    [Header("色")]
    [SerializeField] private Color normalArrowColor = Color.white;
    [SerializeField] private Color succesColor = new Color(0.25f, 0.9f, 1f);
    [SerializeField] private Color missColor = Color.red;

    [Header("成功演出")]
    [SerializeField] private float succesScale = 1.25f;
    [SerializeField] private float succesDuration = 0.12f;

    private Vector3 diamondOriginalScale;
    private Vector3 arrowOriginalScale;
    private Coroutine successCoroutine;

    // 枠の状態
    private enum FrameState
    {
        Hidden,
        Waiting,
        Success,
        Miss
    }

    private FrameState frameState = FrameState.Hidden;

    private void Awake()
    {
        diamondOriginalScale = diamondImage.transform.localScale;
        arrowOriginalScale = arrowImage.transform.localScale;
        activeDiamondImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// コマンドの中身をランダムに選択
    /// </summary>
    /// <param name="command"></param>
    public void SetCommand(CommandType command)
    {
        switch (command)
        {
            case CommandType.Up:
                arrowImage.sprite = upSprite;
                break;
            case CommandType.Down:
                arrowImage.sprite = downSprite;
                break;
            case CommandType.Left:
                arrowImage.sprite = leftSprite;
                break;
            case CommandType.Right:
                arrowImage.sprite = rightSprite;
                break;
        }
        ResetVisualState();
    }

    /// <summary>
    /// 表示状態を初期化
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ResetVisualState()
    {
        if(successCoroutine != null)
        {
            StopCoroutine(successCoroutine);
            successCoroutine = null;
        }
        frameState = FrameState.Hidden;
        activeDiamondImage.gameObject.SetActive(false);

        arrowImage.color = normalArrowColor;
        arrowImage.transform.localScale = arrowOriginalScale;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 黒いひし形の表示・非表示
    /// </summary>
    /// <param name="visible"></param>
    public void SetDiamondVisible(bool visible)
    {
        diamondImage.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 枠を入力待ち状態にする
    /// </summary>
    public void SetWaitingFrame()
    {
        if (frameState != FrameState.Hidden) { return; }

        frameState = FrameState.Waiting;
        activeDiamondImage.sprite = waitingFrameSprite;

        activeDiamondImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 枠を表示・非表示にする
    /// </summary>
    /// <param name="visible"></param>
    public void SetActiveDiamondVisible(bool visible)
    {
        if (!visible)
        {
            // 成功・失敗した枠は残す
            if (frameState == FrameState.Success || frameState == FrameState.Miss) { return; }

            frameState = FrameState.Hidden;

            activeDiamondImage.gameObject.SetActive(false);
            return;
        }
        SetWaitingFrame();
    }    

    /// <summary>
    /// 成功状態にする
    /// </summary>
    public void SetSucces()
    {
        if(successCoroutine != null) { StopCoroutine(successCoroutine); }

        frameState = FrameState.Success;
        activeDiamondImage.sprite = succesFrameSprite;

        activeDiamondImage.gameObject.SetActive(true);

        arrowImage.color = succesColor;
        successCoroutine = StartCoroutine(SuccessPop());
    }

    /// <summary>
    /// ミス状態にする
    /// </summary>
    public void SetMiss()
    {
        if(successCoroutine != null) { StopCoroutine(successCoroutine); successCoroutine = null; }

        frameState = FrameState.Miss;
        activeDiamondImage.sprite = missFrameSprite;

        activeDiamondImage.gameObject.SetActive(true);

        arrowImage.color = missColor;
        arrowImage.transform.localScale = arrowOriginalScale;
    }

    /// <summary>
    /// 成功時に矢印を一瞬拡大して戻す
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerator SuccessPop()
    {
        float time = 0f;

        while(time < succesDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / succesDuration);

            // 前半で拡大して、後半で元に戻す
            float scale;
            if(t < 0.5f)
            {
                scale = Mathf.Lerp(1f, succesScale, t * 2f);
            }
            else
            {
                scale = Mathf.Lerp(succesScale, 1f, (t - 0.5f) * 2f);
            }
            arrowImage.transform.localScale = arrowOriginalScale * scale;
            yield return null;
        }
        arrowImage.transform.localScale = arrowOriginalScale;
        successCoroutine = null;
    }

    /// <summary>
    /// ピンクのひし形の位置を取得
    /// </summary>
    /// <returns></returns>
    public RectTransform GetActiveDiamondTransform()
    {
        return activeDiamondImage.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Scaleを設定
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale(float scale)
    {
        diamondImage.transform.localScale = diamondOriginalScale * scale;
        arrowImage.transform.localScale = arrowOriginalScale * scale;
    }

    /// <summary>
    /// コマンドを縦方向に縮める
    /// </summary>
    /// <param name="scale"></param>
    public void SetVerticalScale(float scale)
    {
        transform.localScale = new Vector3(1f, scale, 1f);
    }

    /// <summary>
    /// 色を設定
    /// </summary>
    /// <param name="color"></param>
    public void SetDiamondColor(Color color)
    {
        diamondImage.color = color;
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("演出")]
    
    [Header("Flash")]
    [SerializeField] private float flashAlpha = 0.3f;
    
    [Header("Scale")]
    [SerializeField] private float effectScale = 1.3f;
    
    [Header("Angle")]
    [SerializeField] private float effectAngle = 10.0f;

    [Header("Speed")]
    [SerializeField] private float effectSpeed = 1.5f;

    [Header("Timing")]
    [SerializeField] private float scaleUpDuration = 0.1f;
    [SerializeField] private float scaleDowntDuration = 0.15f;

    // Default関数
    private Vector3 defaultScale;
    private Quaternion defaultRotation;
    private Color defaultColor;

    private Coroutine effectCoroutine;

    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
        defaultScale = comboText.transform.localScale;
        defaultRotation = comboText.transform.localRotation;
        defaultColor = comboText.color;
    }

    /// <summary>
    /// Update処理
    /// </summary>
    /// <param name="combo"></param>
    public void UpdateCombo(int combo)
    {
        comboText.text = $"{combo}";
        
        // 0になった場合は演出しない
        if(combo <= 0)
        {
            ResetVisual();
            return;
        }

        PlayComboEffect();
    }

    private void PlayComboEffect()
    {
        // 前の演出が残っていたら止める
        if(effectCoroutine != null)
        {
            StopAllCoroutines();
        }

        // 必ず通常状態に戻してから新しい演出を開始
        ResetVisual();
        effectCoroutine = StartCoroutine(ComboEffect());
    }

    private IEnumerator ComboEffect()
    {
        Transform textTransform = comboText.transform;

        // =====================
        // フラッシュ
        // =====================
        Color flashColor = defaultColor;
        flashColor.a = flashAlpha;
        comboText.color = flashColor;

        // ======================
        // 拡大
        // ======================
        float timer = 0f;

        while (timer < scaleUpDuration)
        {
            timer += Time.deltaTime * effectSpeed;

            float t = Mathf.Clamp01(timer / scaleUpDuration);

            textTransform.localScale = Vector3.Lerp(defaultScale, defaultScale * effectScale, t);

            // 傾ける
            textTransform.localRotation = Quaternion.Lerp(defaultRotation, defaultRotation * Quaternion.Euler(0f, 0f, effectAngle), t);

            yield return null;
        }

        // 最大サイズ
        textTransform.localScale = defaultScale * effectScale;

        // ==========================
        // 縮小
        // ==========================

        timer = 0f;

        while(timer < scaleDowntDuration)
        { 
            timer += Time.deltaTime * effectSpeed;

            float t = Mathf.Clamp01(timer / scaleDowntDuration);

            textTransform.localScale = Vector3.Lerp(defaultScale * effectScale, defaultScale, t);
            textTransform.localRotation = Quaternion.Lerp(defaultRotation, defaultRotation* Quaternion.Euler(0f, 0f, effectAngle), t);

            // 透明度も元に戻す
            Color color = Color.Lerp(flashColor, defaultColor, t);

            comboText.color = color;

            yield return null;
        }

        // 元に戻す
        ResetVisual();

        effectCoroutine = null;
    }

    /// <summary>
    /// 初期状態に戻す
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ResetVisual()
    {
        comboText.transform.localScale = defaultScale;
        comboText.transform.localRotation = defaultRotation;
        comboText.color = defaultColor;
    }
}

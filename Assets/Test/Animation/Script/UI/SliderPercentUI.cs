
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// スキャンの進行度と結果表示を管理する。
///
/// ・外部からスキャン開始
/// ・外部から進行度を受け取る
/// ・Sliderとパーセント表示を同期
/// ・100%到達後に結果を表示
/// ・指定時間後にUIを非表示
/// </summary>
public class SliderPercentUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private TMP_Text statusText;

    [Header("Finish")]
    [SerializeField] private float hideDelay = 1f;

    private bool isScanning;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Sliderの初期設定
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 0f;
        }

        canvasGroup.alpha = 0f;

        UpdateUI();
    }

    /// <summary>
    /// 外部からスキャン表示を開始する。
    /// </summary>
    public void StartScan()
    {
        if (isScanning)
            return;

        isScanning = true;

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        if (slider != null)
        {
            slider.value = 0f;
        }

        UpdateUI();

        Debug.Log("Sliderスキャン開始");
    }

    /// <summary>
    /// 外部から受け取った進行度を反映する。
    /// progressは0～1。
    /// </summary>
    public void UpdateScanProgress(float progress)
    {
        if (!isScanning || slider == null)
            return;

        progress = Mathf.Clamp01(progress);

        // 0～1を0～100%に変換
        slider.value = progress * 100f;

        UpdateUI();
    }

    /// <summary>
    /// スキャン完了時に呼び出す。
    /// </summary>
    public void CompleteScan()
    {
        if (!isScanning)
            return;

        slider.value = 100f;

        UpdateUI();

        Debug.Log("DATA ACQUIRED");

        StartCoroutine(FinishRoutine());
    }

    /// <summary>
    /// 完了後の非表示処理。
    /// </summary>
    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        canvasGroup.alpha = 0f;

        isScanning = false;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI表示を更新する。
    /// </summary>
    private void UpdateUI()
    {
        if (slider == null)
            return;

        float percent = slider.value;

        if (percentText != null)
        {
            percentText.text = $"{percent:0}%";
        }

        if (statusText == null)
            return;

        if (percent <= 0f)
        {
            statusText.text = "";
        }
        else if (percent < 100f)
        {
            statusText.text = "SCANNING TARGET...";
        }
        else
        {
            statusText.text = "DATA ACQUIRED";
        }
    }
}

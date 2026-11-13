
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderPercentUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private TMP_Text statusText;

    [Header("Scan")]
    [SerializeField] private float scanDuration = 2f;

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
    }

    private void Start()
    {
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 0;

        canvasGroup.alpha = 0f;

        UpdateUI();
    }

    // 外部から呼び出してスキャンを開始
    public void StartScan()
    {
        if (isScanning)
            return;

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        StartCoroutine(ScanRoutine());
    }

    private IEnumerator ScanRoutine()
    {
        isScanning = true;

        slider.value = 0;

        float timer = 0f;

        while (timer < scanDuration)
        {
            timer += Time.deltaTime;

            float t = scanDuration <= 0f
                ? 1f
                : Mathf.Clamp01(timer / scanDuration);

            slider.value = Mathf.Lerp(0f, 100f, t);

            UpdateUI();

            yield return null;
        }

        slider.value = 100f;

        statusText.text = "DATA ACQUIRED";

        yield return new WaitForSeconds(hideDelay);

        gameObject.SetActive(false);

        isScanning = false;
    }

    private void UpdateUI()
    {
        if (slider == null)
            return;

        float percent = slider.value;

        percentText.text = $"{percent:0}%";

        if (percent <= 0)
        {
            statusText.text = "";
        }
        else if (percent < 100)
        {
            statusText.text = "SCANNING TARGET...";
        }
        else
        {
            statusText.text = "DATA ACQUIRED";
        }
    }
}


using UnityEngine;

/// <summary>
/// パネルの縮小演出を管理する。
///
/// ・ExclamationMarkから呼び出されて縮小開始
/// ・縮小開始と同時にカメラをズーム
/// ・縮小終了後にスキャンを開始
/// </summary>
public class UIPanelShrink : MonoBehaviour
{
    [Header("対象")]
    [SerializeField] private RectTransform target;

    [Header("サイズ")]
    [SerializeField] private float startWidth = 1200f;
    [SerializeField] private float endWidth = 700f;
    [SerializeField] private float height = 700f;

    [Header("収縮時間")]
    [SerializeField] private float duration = 0.5f;

    [Header("縮小基準")]
    [Range(0f, 1f)]
    [SerializeField] private float pivotX = 0.5f;

    [Header("カメラ制御")]
    [SerializeField]
    private BattleCameraController battleCameraController;

    [Header("スキャン制御")]
    [SerializeField]
    private SliderPercentUI sliderPercentUI;

    // 縮小経過時間
    private float timer;

    // 縮小中かどうか
    private bool shrinking;

    // 縮小が完了したかどうか
    private bool shrinkCompleted;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (!shrinking || target == null)
            return;

        // 経過時間を更新
        timer += Time.deltaTime;

        // 縮小進行度
        float t = duration <= 0f
            ? 1f
            : Mathf.Clamp01(timer / duration);

        // 横幅を補間
        float width = Mathf.Lerp(
            startWidth,
            endWidth,
            t
        );

        target.sizeDelta = new Vector2(width, height);

        // 縮小終了
        if (t >= 1f)
        {
            target.sizeDelta = new Vector2(
                endWidth,
                height
            );

            shrinking = false;
            shrinkCompleted = true;

            // 縮小終了後にスキャン開始
            if (sliderPercentUI != null)
            {
                sliderPercentUI.StartScan();

                Debug.Log(
                    "パネル縮小終了：スキャン開始"
                );
            }
            else
            {
                Debug.LogWarning(
                    "SliderPercentUIが設定されていません。"
                );
            }
        }
    }

    /// <summary>
    /// パネル縮小を開始する。
    /// ExclamationMarkから呼び出す。
    /// </summary>
    public void StartShrink()
    {
        // 二重起動防止
        if (shrinking || shrinkCompleted)
            return;

        if (target == null)
        {
            Debug.LogWarning(
                "縮小対象のRectTransformが設定されていません。"
            );

            return;
        }

        // 初期化
        timer = 0f;
        shrinking = true;

        // 縮小基準を設定
        target.pivot = new Vector2(
            pivotX,
            target.pivot.y
        );

        // 初期サイズに設定
        target.sizeDelta = new Vector2(
            startWidth,
            height
        );

        // 縮小開始と同時にカメラをズーム
        if (battleCameraController != null)
        {
            battleCameraController.StartZoom();
        }
        else
        {
            Debug.LogWarning(
                "BattleCameraControllerが設定されていません。"
            );
        }
    }
}

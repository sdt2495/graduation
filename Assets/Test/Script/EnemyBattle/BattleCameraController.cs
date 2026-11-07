using System.Collections;
using UnityEngine;

/// <summary>
/// 敵ターン中のカメラ移動とズームを管理するクラス
/// </summary>
public class BattleCameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [SerializeField] private Camera targetCamera;

    [Header("プレイヤー設定")]
    [SerializeField] private string playerTag = "Player";

    [Header("ズーム設定")]
    [Min(0.1f)]
    [SerializeField] private float zoomDistance = 5f;

    [Range(1f, 179f)]
    [SerializeField] private float zoomFieldOfView = 30f;

    [Min(0f)]
    [SerializeField] private float transitionDuration = 0.5f;

    [SerializeField] private Vector3 cameraOffset = Vector3.zero;

    private Transform playerTransform;

    private Vector3 originalPosition;
    private float originalFieldOfView;

    private bool isZoomed;

    private Coroutine cameraMoveCoroutine;


    public bool IsZoomed => isZoomed;
    // ★追加
    public float TransitionDuration => transitionDuration;

    // ★追加
    public bool IsMovingCamera
    {
        get
        {
            return cameraMoveCoroutine != null;
        }
    }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError(
                "BattleCameraController：カメラが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        if (targetCamera.orthographic)
        {
            Debug.LogError(
                "BattleCameraController：ProjectionをPerspectiveにしてください。",
                this
            );

            enabled = false;
            return;
        }

        SaveOriginalCameraState();
        FindPlayer();
    }

    private bool FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
        {
            Debug.LogError(
                "BattleCameraController：Playerタグのオブジェクトが見つかりません。",
                this
            );

            playerTransform = null;
            return false;
        }

        playerTransform = playerObject.transform;

        return true;
    }

    private void SaveOriginalCameraState()
    {
        originalPosition = targetCamera.transform.position;
        originalFieldOfView = targetCamera.fieldOfView;
    }

    public void FocusOnPlayer()
    {
        Debug.Log("FocusOnPlayer実行");

        if (playerTransform == null && !FindPlayer())
        {
            Debug.LogError("Playerが見つかりません");
            return;
        }

        if (!isZoomed)
        {
            SaveOriginalCameraState();
        }

        StopCameraMove();

        isZoomed = true;

        Vector3 destinationPosition =
            playerTransform.position
            - targetCamera.transform.forward * zoomDistance;

        destinationPosition += cameraOffset;

        Debug.Log("移動先:" + destinationPosition);

        cameraMoveCoroutine = StartCoroutine(
            MoveCamera(destinationPosition, zoomFieldOfView)
        );
    }

    public void RestoreCamera()
    {
        if (targetCamera == null)
        {
            return;
        }

        StopCameraMove();

        cameraMoveCoroutine = StartCoroutine(
            MoveCamera(originalPosition, originalFieldOfView)
        );

        isZoomed = false;
    }

    private IEnumerator MoveCamera(
        Vector3 destinationPosition,
        float destinationFieldOfView)
    {
        Debug.Log("MoveCamera開始");

        Transform cameraTransform = targetCamera.transform;

        Vector3 startPosition = cameraTransform.position;
        float startFieldOfView = targetCamera.fieldOfView;

        if (transitionDuration <= 0f)
        {
            cameraTransform.position = destinationPosition;
            targetCamera.fieldOfView = destinationFieldOfView;

            cameraMoveCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / transitionDuration);

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            cameraTransform.position = Vector3.Lerp(
                startPosition,
                destinationPosition,
                smoothProgress
            );

            targetCamera.fieldOfView = Mathf.Lerp(
                startFieldOfView,
                destinationFieldOfView,
                smoothProgress
            );

            yield return null;
        }

        cameraTransform.position = destinationPosition;
        targetCamera.fieldOfView = destinationFieldOfView;

        cameraMoveCoroutine = null;
    }

    private void StopCameraMove()
    {
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
            cameraMoveCoroutine = null;
        }
    }
}
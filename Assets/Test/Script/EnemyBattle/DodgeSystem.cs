
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DodgeSystem : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("方向画像")]
    [SerializeField] private Image upImage;
    [SerializeField] private Image downImage;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image rightImage;

    [Header("照準画像")]
    [SerializeField] private GameObject upCrosshair;
    [SerializeField] private GameObject downCrosshair;
    [SerializeField] private GameObject leftCrosshair;
    [SerializeField] private GameObject rightCrosshair;

    [Header("カメラ")]
    [SerializeField] private BattleCameraController battleCamera;

    [Tooltip("ズームが完了するまでの待機時間")]
    [SerializeField, Min(0f)]
    private float cameraZoomWaitTime = 1.0f;

    [Tooltip("カメラを元に戻した後の待機時間")]
    [SerializeField, Min(0f)]
    private float cameraReturnWaitTime = 1.0f;

    [Header("攻撃設定")]
    [SerializeField] private float beatInterval = 1f;

    [Header("選択色")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("結果通知")]
    [SerializeField]
    private UnityEvent onClear;

    [SerializeField]
    private UnityEvent onMiss;

    private Direction selectedDirection;
    private Direction firstAttackDirection;
    private Direction secondAttackDirection;

    private bool waitingForStart = true;
    private bool isRunning = false;
    private bool sequenceRunning = false;

    private void Start()
    {
        HideAllCrosshairs();
        ResetDirectionColors();

        Debug.Log("SPACEで開始");
    }

    private void Update()
    {
        if (waitingForStart)
        {
            if (Input.GetKeyDown(KeyCode.Space) &&
                !sequenceRunning)
            {
                sequenceRunning = true;
                StartCoroutine(StartBattleSequence());
            }

            return;
        }

        if (!isRunning)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            SelectDirection(Direction.Up);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            SelectDirection(Direction.Down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelectDirection(Direction.Left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelectDirection(Direction.Right);
    }

    private IEnumerator StartBattleSequence()
    {
        waitingForStart = false;

        Debug.Log("カメラズーム開始");

        if (battleCamera != null)
        {
            battleCamera.StartZoom();

            yield return new WaitForSeconds(cameraZoomWaitTime);
        }

        Debug.Log("ズーム完了");

        yield return new WaitForSeconds(beatInterval);

        yield return StartCoroutine(AttackPattern());
    }

    private IEnumerator AttackPattern()
    {
        isRunning = true;

        HideAllCrosshairs();

        // 1拍目
        firstAttackDirection = GetRandomDirection();

        ShowCrosshair(firstAttackDirection);

        Debug.Log("1拍目 : " + firstAttackDirection);

        yield return new WaitForSeconds(beatInterval);

        // 2拍目
        secondAttackDirection =
            GetRandomDirectionExcluding(firstAttackDirection);

        ShowCrosshair(secondAttackDirection);

        Debug.Log("2拍目 : " + secondAttackDirection);

        yield return new WaitForSeconds(beatInterval);

        // 3拍目
        ExecuteAttack();

        yield return new WaitForSeconds(1f);

        ResetBattle();
    }

    private void ExecuteAttack()
    {
        bool success =
            selectedDirection != firstAttackDirection &&
            selectedDirection != secondAttackDirection;

        if (success)
        {
            Debug.Log("回避成功");
            onClear?.Invoke();
        }
        else
        {
            Debug.Log("回避失敗");
            onMiss?.Invoke();
        }
    }

    private void ResetBattle()
    {
        HideAllCrosshairs();
        ResetDirectionColors();

        isRunning = false;
        waitingForStart = true;
        sequenceRunning = false;

        if (battleCamera != null)
        {
            battleCamera.EndZoom();
        }

        Debug.Log("SPACEで再開");
    }

    private void SelectDirection(Direction direction)
    {
        selectedDirection = direction;

        ResetDirectionColors();

        switch (direction)
        {
            case Direction.Up:
                if (upImage != null)
                    upImage.color = selectedColor;
                break;

            case Direction.Down:
                if (downImage != null)
                    downImage.color = selectedColor;
                break;

            case Direction.Left:
                if (leftImage != null)
                    leftImage.color = selectedColor;
                break;

            case Direction.Right:
                if (rightImage != null)
                    rightImage.color = selectedColor;
                break;
        }

        Debug.Log("選択方向 : " + direction);
    }

    private void ResetDirectionColors()
    {
        if (upImage != null)
            upImage.color = normalColor;

        if (downImage != null)
            downImage.color = normalColor;

        if (leftImage != null)
            leftImage.color = normalColor;

        if (rightImage != null)
            rightImage.color = normalColor;
    }

    private Direction GetRandomDirection()
    {
        return (Direction)Random.Range(0, 4);
    }

    private Direction GetRandomDirectionExcluding(Direction exclude)
    {
        Direction result;

        do
        {
            result = (Direction)Random.Range(0, 4);
        }
        while (result == exclude);

        return result;
    }

    private void ShowCrosshair(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                if (upCrosshair != null)
                    upCrosshair.SetActive(true);
                break;

            case Direction.Down:
                if (downCrosshair != null)
                    downCrosshair.SetActive(true);
                break;

            case Direction.Left:
                if (leftCrosshair != null)
                    leftCrosshair.SetActive(true);
                break;

            case Direction.Right:
                if (rightCrosshair != null)
                    rightCrosshair.SetActive(true);
                break;
        }
    }

    private void HideAllCrosshairs()
    {
        if (upCrosshair != null)
            upCrosshair.SetActive(false);

        if (downCrosshair != null)
            downCrosshair.SetActive(false);

        if (leftCrosshair != null)
            leftCrosshair.SetActive(false);

        if (rightCrosshair != null)
            rightCrosshair.SetActive(false);
    }
}

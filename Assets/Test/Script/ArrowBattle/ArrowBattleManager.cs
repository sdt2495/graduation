using System.Collections;

using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class ArrowBattleManager : MonoBehaviour

{

    public enum ArrowDirection

    {

        Left,

        Up,

        Right,

        Down

    }

    [Header("矢印UI")]

    [SerializeField] private Image arrowImage;

    [SerializeField] private Image arrowFrame;

    [Header("矢印画像")]

    [SerializeField] private Sprite leftSprite;

    [SerializeField] private Sprite upSprite;

    [SerializeField] private Sprite rightSprite;

    [SerializeField] private Sprite downSprite;

    [Header("時間ゲージ")]

    [SerializeField] private Image timeGauge;

    [Header("テキストUI")]

    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private TMP_Text comboText;

    [SerializeField] private TMP_Text resultText;

    [Header("入力設定")]

    [Tooltip("矢印1個あたりの受付時間（秒）")]

    [SerializeField, Min(0.1f)]

    private float inputTime = 1.25f;

    [Header("敵の攻撃設定")]

    [Tooltip("攻撃間隔の最小値（秒）")]

    [SerializeField, Min(0f)]

    private float minAttackInterval = 6f;

    [Tooltip("攻撃間隔の最大値（秒）")]

    [SerializeField, Min(0f)]

    private float maxAttackInterval = 7.5f;

    [Header("スコア設定")]

    [SerializeField] private int baseScore = 100;

    [SerializeField] private int comboBonus = 50;

    [Header("結果表示設定")]

    [SerializeField, Min(0f)]

    private float resultDisplayTime = 1f;

    private ArrowDirection currentDirection;

    private float remainingTime;

    private int score;

    private int combo;

    private bool isWaitingInput;

    private bool isRunning = true;

    private Coroutine attackCoroutine;

    private Coroutine resultCoroutine;

    public bool IsWaitingInput => isWaitingInput;

    public int Score => score;

    public int Combo => combo;

    private void Start()

    {

        if (!ValidateReferences())

        {

            enabled = false;

            return;

        }

        arrowImage.gameObject.SetActive(false);

        arrowFrame.gameObject.SetActive(false);

        timeGauge.fillAmount = 0f;

        resultText.text = "";

        UpdateUI();

        attackCoroutine = StartCoroutine(EnemyAttackLoop());

    }

    private bool ValidateReferences()

    {

        if (arrowImage == null ||

            arrowFrame == null ||

            timeGauge == null ||

            scoreText == null ||

            comboText == null ||

            resultText == null)

        {

            Debug.LogError(

                "ArrowBattleManager：UIの参照が設定されていません。",

                this

            );

            return false;

        }

        if (leftSprite == null ||

            upSprite == null ||

            rightSprite == null ||

            downSprite == null)

        {

            Debug.LogError(

                "ArrowBattleManager：4方向の矢印画像を設定してください。",

                this

            );

            return false;

        }

        if (inputTime <= 0f ||

            minAttackInterval < 0f ||

            maxAttackInterval < minAttackInterval)

        {

            Debug.LogError(

                "ArrowBattleManager：時間設定を確認してください。",

                this

            );

            return false;

        }

        return true;

    }

    private IEnumerator EnemyAttackLoop()

    {

        while (isRunning)

        {

            float waitTime = Random.Range(

                minAttackInterval,

                maxAttackInterval

            );

            yield return new WaitForSeconds(waitTime);

            if (!isRunning)

                yield break;

            StartArrowInput();

            // 入力判定が終わるまで次の攻撃を待つ

            while (isWaitingInput && isRunning)

            {

                yield return null;

            }

        }

    }

    private void StartArrowInput()

    {

        if (isWaitingInput || !isRunning)

            return;

        // 左・上・右・下からランダムに1つ選ぶ

        currentDirection =

            (ArrowDirection)Random.Range(0, 4);

        arrowImage.sprite = GetArrowSprite(currentDirection);

        arrowImage.gameObject.SetActive(true);

        arrowFrame.gameObject.SetActive(true);

        // 枠の位置と大きさを矢印に合わせる

        RectTransform arrowRect =

            arrowImage.rectTransform;

        RectTransform frameRect =

            arrowFrame.rectTransform;

        frameRect.position = arrowRect.position;

        frameRect.sizeDelta =

            arrowRect.sizeDelta + new Vector2(24f, 24f);

        // 1個あたりの受付時間を設定

        remainingTime = inputTime;

        timeGauge.fillAmount = 1f;

        isWaitingInput = true;

        ShowResult("入力してください！");

    }

    private void Update()

    {

        if (!isWaitingInput || !isRunning)

            return;

        // 制限時間を減らす

        remainingTime -= Time.deltaTime;

        timeGauge.fillAmount = Mathf.Clamp01(

            remainingTime / inputTime

        );

        // 時間切れ

        if (remainingTime <= 0f)

        {

            FinishInput(false);

            return;

        }

        // WASDキーによる入力判定

        if (Input.GetKeyDown(KeyCode.A))

        {

            CheckInput(ArrowDirection.Left);

        }

        else if (Input.GetKeyDown(KeyCode.W))

        {

            CheckInput(ArrowDirection.Up);

        }

        else if (Input.GetKeyDown(KeyCode.D))

        {

            CheckInput(ArrowDirection.Right);

        }

        else if (Input.GetKeyDown(KeyCode.S))

        {

            CheckInput(ArrowDirection.Down);

        }

    }

    private void CheckInput(ArrowDirection input)

    {

        if (!isWaitingInput)

            return;

        FinishInput(input == currentDirection);

    }

    private void FinishInput(bool success)

    {

        if (!isWaitingInput)

            return;

        // 二重判定を防止

        isWaitingInput = false;

        arrowImage.gameObject.SetActive(false);

        arrowFrame.gameObject.SetActive(false);

        timeGauge.fillAmount = 0f;

        if (success)

        {

            combo++;

            score += baseScore +

                     (combo - 1) * comboBonus;

            ShowResult("SUCCESS!");

        }

        else

        {

            combo = 0;

            ShowResult("MISS!");

        }

        UpdateUI();

    }

    private void ShowResult(string message)

    {

        if (resultCoroutine != null)

        {

            StopCoroutine(resultCoroutine);

            resultCoroutine = null;

        }

        resultText.text = message;

        if (message == "入力してください！")

            return;

        resultCoroutine =

            StartCoroutine(ClearResultAfterDelay());

    }

    private IEnumerator ClearResultAfterDelay()

    {

        yield return new WaitForSeconds(resultDisplayTime);

        if (!isWaitingInput)

        {

            resultText.text = "";

        }

        resultCoroutine = null;

    }

    private Sprite GetArrowSprite(ArrowDirection direction)

    {

        switch (direction)

        {

            case ArrowDirection.Left:

                return leftSprite;

            case ArrowDirection.Up:

                return upSprite;

            case ArrowDirection.Right:

                return rightSprite;

            case ArrowDirection.Down:

                return downSprite;

            default:

                return null;

        }

    }

    private void UpdateUI()

    {

        scoreText.text = "SCORE\n" + score;

        comboText.text = combo + " COMBO";

    }

    private void OnDisable()

    {

        isRunning = false;

        isWaitingInput = false;

        if (attackCoroutine != null)

        {

            StopCoroutine(attackCoroutine);

            attackCoroutine = null;

        }

        if (resultCoroutine != null)

        {

            StopCoroutine(resultCoroutine);

            resultCoroutine = null;

        }

    }

}

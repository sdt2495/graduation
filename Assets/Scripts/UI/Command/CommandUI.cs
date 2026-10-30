using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

// ===================================
// コマンド列全体の管理
// ===================================

public class CommandUI : MonoBehaviour
{
    [Header("入力矢印UI")]
    [SerializeField] private CommandUIElement commandPrefab;
    [SerializeField] private Transform commandParent;
    [SerializeField] private Transform nextCommandParent;
    [SerializeField] private float spacing = 150f;

    [Header("入力中のひし形")]
    [SerializeField] private float activeDiamondMoveDuration = 0.15f;
    [SerializeField] private AnimationCurve activeDiamondMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("コマンドの大きさ")]
    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private float activeScale = 1.1f;

    [Header("コマンドの色")]
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color missColor = Color.red;

    [Header("出現アニメーション")]
    [SerializeField] private float appearDuration = 0.2f;
    [SerializeField] private float appearInterval = 0.08f;
    [SerializeField] private float appearOvershot = 1.2f;

    [Header("ミス演出")]
    [SerializeField] private float missFlashDuration = 0.05f;
    [SerializeField] private float missTiltAngle = 15f;
    [SerializeField] private float missTiltDuration = 0.1f;

    [Header("完了演出")]
    [SerializeField] private float completeAnimationDuration = 0.2f;

    private List<CommandUIElement> commandElements = new List<CommandUIElement>();    
    private List<CommandUIElement> nextcommandElements = new List<CommandUIElement>();
    private List<CommandType> nextCommand = new List<CommandType>();

    private RectTransform acitiveDiamond;
    private Coroutine appearCoroutine;

    private int currentActiveIndex = 0;

    /// <summary>
    /// 現在のコマンドとNextコマンドのUIを更新する
    /// </summary>
    /// <param name="battleEnemy"></param>
    public void UpdateCommanedText(Enemy battleEnemy)
    {
        CreateCommandElements(battleEnemy.GetCommands(), commandParent, commandElements);

        CreateCommandElements(nextCommand, nextCommandParent, nextcommandElements);

        UpdateActiveComand(battleEnemy.GetCurrentIndex());
    }

    /// <summary>
    /// 戦闘開始時のNextコマンドを生成して表示する
    /// </summary>
    public void SetInitialNextCommands()
    {
        SetNextCommand();

        CreateCommandElements(nextCommand, nextCommandParent, nextcommandElements);
    }

    /// <summary>
    /// NextコマンドをEnemyの現在のコマンドとして設定する
    /// </summary>
    /// <param name="enemy"></param>
    public void ApplyNext(Enemy enemy)
    {
        enemy.SetCommands(nextCommand);
    }

    /// <summary>
    /// Nextコマンドを新しくランダム生成する
    /// </summary>
    public void UpdateNextCommand()
    {
        SetNextCommand();
    }

    /// <summary>
    /// Player入力コマンドの生成
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="parent"></param>
    /// <param name="elements"></param>
    private void CreateCommandElements(List<CommandType> commands, Transform parent, List<CommandUIElement> elements)
    {
        if(parent == commandParent && appearCoroutine != null)
        {
            StopCoroutine(appearCoroutine);
            appearCoroutine = null;
        }

        // 既存のUIを削除
        foreach(CommandUIElement element in elements)
        {
            Destroy(element.gameObject);
        }

        elements.Clear();

        if(parent == commandParent)
        {
            currentActiveIndex = 0;
        }

        int count = commands.Count;

        // 全体を中央揃えするための開始位置
        float startX = -(count - 1) * spacing / 2f;

        // 生成
        for(int i = 0; i < count; i++)
        {
            CommandUIElement element = Instantiate(commandPrefab, parent);

            element.SetCommand(commands[i]);
    
            bool isBattle = parent == commandParent;
            element.SetDiamondVisible(isBattle);

            // Battleの1個目だけをピンクのひし形の表示
            bool isFirst = i == 0 && parent == commandParent;
            element.SetActiveDiamondVisible(isFirst);

            if (parent == commandParent)
            {
                element.SetScale(normalScale);
                element.SetAppearScale(0.1f);
            }
            else
            {
                element.SetScale(normalScale);
                element.SetAppearScale(1f);
            }

            // 横方向に配置
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            float x = startX + i * spacing;
            rectTransform.anchoredPosition = new Vector2(x, 0f);

            elements.Add(element);
        }

        if(parent == commandParent)
        {
            StartAppearAnimation();
        }
    }

    /// <summary>
    /// 矢印出現アニメーションの開始関数
    /// </summary>
    private void StartAppearAnimation()
    {
        if(appearCoroutine != null)
        {
            StopCoroutine(appearCoroutine);
        }

        appearCoroutine = StartCoroutine(PlayeAppearAnimation());
    }

    /// <summary>
    /// 矢印出現アニメーション
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    private IEnumerator PlayeAppearAnimation()
    {
        for(int i = 0; i < commandElements.Count; i++)
        {
            float time = 0f;

            while(time < appearDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / appearDuration);

                float scale;

                if(t < 0.8f)
                {
                    scale = Mathf.Lerp(0.1f, appearOvershot, t / 0.8f);
                }
                else
                {
                    scale = Mathf.Lerp(appearOvershot, 1f, (t - 0.8f) / 0.2f);
                }

                commandElements[i].SetAppearScale(scale);

                yield return null;
            }

            commandElements[i].SetAppearScale(1f);

            yield return new WaitForSeconds(appearInterval);
        }

        appearCoroutine = null;
    }

    /// <summary>
    /// 入力対象のコマンドへひし形を移動する
    /// </summary>
    /// <param name="currentIndex"></param>
    public void UpdateActiveComand(int currentIndex)
    {
        if(currentIndex >= commandElements.Count)
        {
            return;
        }

        UpdateCommandScale(currentIndex);

        // 現在の矢印に入力待ち枠を表示する
        // 成功・失敗の枠は上書きしない
        commandElements[currentIndex].SetWaitingFrame();
    }

    private void UpdateCommandScale(int currentIndex)
    {
        currentActiveIndex = currentIndex;

        for (int i = 0; i < commandElements.Count; i++)
        {
            if(i == currentIndex)
            {
                commandElements[i].SetScale(activeScale);
            }
            else
            {
                commandElements[i].SetScale(normalScale);
            }
        }
    }

    /// <summary>
    /// 次のコマンドを設定する
    /// </summary>
    private void SetNextCommand()
    {
        nextCommand.Clear();

        int count = Random.Range(1, 4);

        for(int i = 0; i < count;i++)
        {
            nextCommand.Add((CommandType)Random.Range(0, 4));
        }
    }

    /// <summary>
    /// 指定したコマンドを成功状態にする
    /// </summary>
    /// <param name="index"></param>
    public void SetSuccessComand(int index)
    {
        if(index < 0 || index >= commandElements.Count) { return; }

        commandElements[index].SetSucces();
    }

    /// <summary>
    /// 指定したコマンドのひし形をミス色に変更する
    /// </summary>
    /// <param name="index"></param>
    public void SetMissCommand(int index)
    {
        if (index < 0 || index >= commandElements.Count)
        {
            return;
        }

        commandElements[index].SetMiss();
    }

    public IEnumerator  PlayMissAnimation(int missIndex, int nextIndex)
    {
       yield return StartCoroutine(MissAnimation(missIndex, nextIndex));
    }

    public IEnumerator PlayCompleteAnimation(int completeIndex)
    {
        yield return StartCoroutine(CompleteAnimation(completeIndex));
    }

    /// <summary>
    /// ミスしたときのアニメーション
    /// </summary>
    /// <param name="missIndex"></param>
    /// <param name="nextIndex"></param>
    /// <returns></returns>
    private IEnumerator MissAnimation(int missIndex, int nextIndex)
    {
        if(missIndex < 0 || missIndex >= commandElements.Count) { yield break; }

        CommandUIElement missElement = commandElements[missIndex];

        // 枠と矢印を赤色にする
        missElement.SetMiss();

        // 白くフラッシュ
        missElement.SetDiamondColor(Color.white);

        yield return new WaitForSeconds(missFlashDuration);

        // 赤色に変更
        missElement.SetDiamondColor(Color.red);

        // 少し傾ける
        RectTransform rect = missElement.GetComponent<RectTransform>();

        Quaternion startRotation = rect.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, missTiltAngle);

        float time = 0f;

        while( time < missTiltDuration)
        {
            time += Time.deltaTime;

            float t = time / missTiltDuration;

            rect.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        // 元の角度の戻す
        rect.localRotation = startRotation;

        // 次のコマンドへ移動
        UpdateActiveComand(nextIndex);
    }

    /// <summary>
    /// コマンドが最後まで入力されたあとのアニメーション
    /// </summary>
    /// <param name="completeIndex"></param>
    /// <returns></returns>
    private IEnumerator CompleteAnimation(int completeIndex)
    {
       if(appearCoroutine != null)
        {
            StopCoroutine(appearCoroutine);
            appearCoroutine = null;
        }

       // コマンド全体をグレーアウト
       foreach(CommandUIElement element in commandElements)
        {
            element.SetGrayOut();
        }

        // グレーアウトを一瞬見せる
        yield return new WaitForSeconds(0.1f);

        // 全コマンドを一斉に非表示
        foreach(CommandUIElement element1 in commandElements)
        {
            element1.gameObject.SetActive(false);
        }
    }
}
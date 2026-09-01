using System.Collections;
using System.Collections.Generic;
using System.Security;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    [Header("ミス演出")]
    [SerializeField] private float missFlashDuration = 0.05f;
    [SerializeField] private float missTiltAngle = 15f;
    [SerializeField] private float missTiltDuration = 0.1f;

    private List<CommandUIElement> commandElements = new List<CommandUIElement>();    
    private List<CommandUIElement> nextcommandElements = new List<CommandUIElement>();

    private RectTransform acitiveDiamond;

    public void UpdateCommanedText(Enemy battleEnemy, Enemy nextEnemy)
    {
        CreateCommandElements(battleEnemy.GetCommands(), commandParent, commandElements);

        if(nextEnemy != null)
        {
            CreateCommandElements(nextEnemy.GetCommands(), nextCommandParent, nextcommandElements);
        }

        UpdateActiveComand(battleEnemy.GetCurrentIndex());
    }

    /// <summary>
    /// Player入力コマンドの生成
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="parent"></param>
    /// <param name="elements"></param>
    private void CreateCommandElements(List<CommandType> commands, Transform parent, List<CommandUIElement> elements)
    {
        // 既存のUIを削除
        foreach(CommandUIElement element in elements)
        {
            Destroy(element.gameObject);
        }

        elements.Clear();

        int count = commands.Count;

        // 全体を中央揃えするための開始位置
        float startX = -(count - 1) * spacing / 2f;

        // 生成
        for(int i = 0; i < count; i++)
        {
            CommandUIElement element = Instantiate(commandPrefab, parent);

            element.SetCommand(commands[i]);

            // Battleはひし形表示・Nextは非表示
            element.SetDiamondVisible(parent != nextCommandParent);

            // Battleの1個目だけをピンクのひし形の表示
            bool isFirst = i == 0 && parent == commandParent;
            element.SetActiveDiamondVisible(isFirst);

            if(isFirst)
            {
                acitiveDiamond = element.GetActiveDiamondTransform();
            }

            // 横方向に配置
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            float x = startX + i * spacing;
            rectTransform.anchoredPosition = new Vector2(x, 0f);

            elements.Add(element);
        }
    }

    /// <summary>
    /// 選択されてるひし形を移動
    /// </summary>
    /// <param name="currentIndex"></param>
    public void UpdateActiveComand(int currentIndex)
    {
        if(acitiveDiamond == null)
        {
            return;
        }

        if(currentIndex >= commandElements.Count)
        {
            return;
        }

        StartCoroutine(MoveActiveDiamond(currentIndex));
    }

    private IEnumerator MoveActiveDiamond(int currentIndex)
    {
        UpdateCommandScale(currentIndex);

        // 移動先のElement
        CommandUIElement targetElement = commandElements[currentIndex];

        RectTransform target = targetElement.GetComponent<RectTransform>();

        // 現在位置
        Vector3 startPosition = acitiveDiamond.position;

        // 移動先
        Vector3 targetPosition = target.position;

        // 移動時間
        float time = 0f;

        while (time < activeDiamondMoveDuration)
        {
            time += Time.deltaTime;
            float t = time / activeDiamondMoveDuration;

            // スッと加速して、最後に減速
            float curveValue = activeDiamondMoveCurve.Evaluate(t);

            acitiveDiamond.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            yield return null;
        }

        // 最後に正確な位置へ
        acitiveDiamond.position = targetPosition;

        // ピンクのひし形を移動先のElementの子にする
        acitiveDiamond.SetParent(target);

        // Elementの中央に配置
        acitiveDiamond.localPosition = Vector3.zero;

        // 移動してきたピンクをArrowの直前にする
        acitiveDiamond.SetSiblingIndex(1);
    }

    private void UpdateCommandScale(int currentIndex)
    {
        for (int i = 0; i < commandElements.Count; i++)
        {
            if (i == currentIndex)
            {
                commandElements[i].SetScale(activeScale);
            }
            else
            {
                commandElements[i].SetScale(normalScale);
            }
        }
    }

    public void SetMissCommand(int index)
    {
        if (index < 0 || index >= commandElements.Count)
        {
            return;
        }

        commandElements[index].SetDiamondColor(missColor);
    }

    public void  PlayMissAnimation(int missIndex, int nextIndex)
    {
        StartCoroutine(MissAnimation(missIndex, nextIndex));
    }

    private IEnumerator MissAnimation(int missIndex)
    {
        if(missIndex < 0 || missIndex >= commandElements.Count) { yield break; }

        CommandUIElement missElement = commandElements[missIndex];

        // 白くフラッシュ
        missElement.SetDiamondColor(Color.whilte);

        yield return new WaitForSeceonds(missFlashDuration);

        // 赤色に変更
        missElement.SetDiamondColor(Color.red);

        // 少し傾ける
        RectTransform rect = missElement.GetCompornent<RectTransform>();

        Quaternion startRotation = rect.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, missTiltAngle);

        float time = 0f;

        while( time < missTiltAngle)
        {
            time += time.deltaTime;

            float t = time / missTiltDuration;

            rect.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return break;
        }

        // 元の角度の戻す
        rect.localRotation = startRotation;

        // 次のコマンドへ移動
        UpdateActiveComand(enemy.GetCurrentIndex());
    }
}
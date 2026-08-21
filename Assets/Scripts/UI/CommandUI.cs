using System.Collections;
using System.Collections.Generic;
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
        // 移動先のElement
        CommandUIElement targetElement = commandElements[currentIndex];

        RectTransform target = targetElement.GetComponent<RectTransform>();

        // 現在位置
        Vector3 startPosition = acitiveDiamond.position;

        // 移動先
        Vector3 targetPosition = target.position;

        // 移動時間
        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            acitiveDiamond.position = Vector3.Lerp(startPosition, targetPosition, t);

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
}

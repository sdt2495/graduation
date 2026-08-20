using System.Collections.Generic;
using NUnit.Framework;
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

    public void UpdateCommanedText(Enemy battleEnemy, Enemy nextEnemy)
    {
        CreateCommandElements(battleEnemy.GetCommands(), commandParent, commandElements);

        if(nextEnemy != null)
        {
            CreateCommandElements(nextEnemy.GetCommands(), nextCommandParent, nextcommandElements);
        }
    }

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

            // 横方向に配置
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            float x = startX + i * spacing;
            rectTransform.anchoredPosition = new Vector2(x, 0f);

            elements.Add(element);
        }
    }
}

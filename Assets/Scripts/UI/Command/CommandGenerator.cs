using UnityEngine;

public static class CommandGenerator
{
    /// <summary>
    /// TECに応じてコマンド数をランダムに決定する
    /// </summary>
    public static int GetCommandCount(float tec)
    {
        if (tec <= 0)
            return 1;

        if (tec < 25)
            return Random.Range(1, 3);

        if (tec < 50)
            return Random.Range(2, 4);

        if (tec < 75)
            return Random.Range(3, 5);

        if (tec < 100)
            return Random.Range(4, 6);

        return 5;
    }
}
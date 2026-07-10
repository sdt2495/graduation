using UnityEngine;

public enum CommandType
{
    Up,
    Down,
    Left,
    Right
}

public class Enemy : MonoBehaviour
{
    public CommandType correctCommand;

    public bool Check(CommandType inputcommaned)
    {
        if(inputcommaned == correctCommand)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}

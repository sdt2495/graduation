
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveButton : MonoBehaviour
{
    [Header("ˆÚ“®æ‚ÌƒV[ƒ“–¼")]
    [SerializeField] private string sceneName = "GameScene";

    public void MoveToGameScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}


using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BootSceneä«óù
/// </summary>
public class BootManager : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
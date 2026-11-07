using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleBGM : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Title Ç∆ Config ÇæÇØê∂ë∂
        if (scene.name != "TitleScene" && scene.name != "ConfigScene")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
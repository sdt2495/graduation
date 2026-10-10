
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleManager : MonoBehaviour
{
    [Header("画面切り替え演出")]
    [SerializeField] private RectTransform transitionImage;
    [SerializeField] private float buttonEffectDelay = 0.1f;
    [SerializeField] private float moveDuration = 0.7f;

    // 移動距離の調整
    [SerializeField] private float moveDistance = 1920f;

    private bool isLoading = false;

    private void Start()
    {
        if (transitionImage != null)
        {
            // 画像を画面の左外に配置
            RectTransform canvasRect =
                transitionImage.parent as RectTransform;

            float canvasWidth = canvasRect.rect.width;
            float imageWidth = transitionImage.rect.width;

            transitionImage.anchoredPosition = new Vector2(
                -canvasWidth / 2f - imageWidth / 2f,
                0f
            );

            // 最初は非表示
            transitionImage.gameObject.SetActive(false);
        }
    }

    // ゲーム開始
    public void OnClickStartGameButton()
    {
        StartSceneLoad("GameScene");
    }

    // 設定画面
    public void OnClickConfigButton()
    {
        ConfigSceneController.OpenFromGame();
    }

    // ゲーム終了
    public void OnClickQuitGameButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 指定したシーンへ移動
    public void OnClickLoadStringSneceButton(string sceneName)
    {
        StartSceneLoad(sceneName);
    }

    private void StartSceneLoad(string sceneName)
    {
        if (isLoading) return;

        isLoading = true;
        StartCoroutine(PlayTransition(sceneName));
    }

    private IEnumerator PlayTransition(string sceneName)
    {
        // ボタンの拡大演出を少し見せる
        yield return new WaitForSeconds(buttonEffectDelay);

        if (transitionImage != null)
        {
            transitionImage.gameObject.SetActive(true);

            RectTransform canvasRect =
                transitionImage.parent as RectTransform;

            float canvasWidth = canvasRect.rect.width;
            float imageWidth = transitionImage.rect.width;

            Vector2 startPosition = new Vector2(
                 -moveDistance,
                 0f
             );

            Vector2 endPosition = new Vector2(
                moveDistance,
                0f
            );

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / moveDuration);

                // なめらかに加速・減速
                float easedT = Mathf.SmoothStep(0f, 1f, t);

                transitionImage.anchoredPosition =
                    Vector2.Lerp(startPosition, endPosition, easedT);

                yield return null;
            }

            transitionImage.anchoredPosition = endPosition;
        }

        // 演出が終わってからシーン移動
        SceneManager.LoadScene(sceneName);
    }
}

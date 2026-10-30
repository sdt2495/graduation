using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleManager : MonoBehaviour
{
    [Header("画面切り替え演出")]
    [SerializeField] private RectTransform transitionImage;
    [SerializeField] private float buttonEffectDelay = 0.1f;
    [SerializeField] private float moveDuration = 0.7f;

    [Header("移動距離の調整")]
    [SerializeField] private float moveDistance = 1920f;

    private bool isLoading;

    private void Start()
    {
        ResetTransition();
    }

    // ゲーム開始
    public void OnClickStartGameButton()
    {
        StartSceneLoad("GameScene");
    }

    // 設定画面を開く
    public void OnClickConfigButton()
    {
        StartSceneLoad("ConfigScene");
    }

    // ボタンの決定演出後に設定画面を開く
    public void OpenConfigAfterEffect()
    {
        StartSceneLoad("ConfigScene");
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

    // シーン移動開始
    private void StartSceneLoad(string sceneName)
    {
        if (isLoading) return;

        isLoading = true;
        StartCoroutine(PlayTransition(sceneName));
    }

    // 画面切り替えアニメーション
    private IEnumerator PlayTransition(string sceneName)
    {
        yield return new WaitForSecondsRealtime(buttonEffectDelay);

        if (transitionImage != null)
        {
            RectTransform canvasRect =
                transitionImage.parent as RectTransform;

            if (canvasRect == null)
            {
                Debug.LogError(
                    "Transition Image の親にCanvasのRectTransformがありません。"
                );

                isLoading = false;
                yield break;
            }

            Vector2 startPosition = new Vector2(-moveDistance, 0f);
            Vector2 endPosition = new Vector2(moveDistance, 0f);

            transitionImage.anchoredPosition = startPosition;
            transitionImage.gameObject.SetActive(true);

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = moveDuration > 0f
                    ? Mathf.Clamp01(elapsed / moveDuration)
                    : 1f;

                float easedT = Mathf.SmoothStep(0f, 1f, t);

                transitionImage.anchoredPosition =
                    Vector2.Lerp(startPosition, endPosition, easedT);

                yield return null;
            }

            transitionImage.anchoredPosition = endPosition;
        }

        // アニメーション完了後にシーンを切り替える
        if (sceneName == "ConfigScene")
        {
            ConfigSceneController.OpenFromGame();
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // 設定画面から戻ったとき
    public void OnReturnFromConfig()
    {
        ResetTransition();

        // タイトル画面のボタンを初期化
        MenuButtonSelectEffect[] buttons =
            FindObjectsByType<MenuButtonSelectEffect>(
                FindObjectsSortMode.None
            );

        foreach (MenuButtonSelectEffect menuButton in buttons)
        {
            menuButton.ResetButtonState();
        }

        // EventSystemの選択状態を解除
        EventSystem.current?.SetSelectedGameObject(null);
    }

    // 遷移画像を初期状態に戻す
    public void ResetTransition()
    {
        isLoading = false;

        if (transitionImage == null)
            return;

        RectTransform canvasRect =
            transitionImage.parent as RectTransform;

        if (canvasRect == null)
        {
            Debug.LogError(
                "Transition Image の親にCanvasのRectTransformがありません。"
            );
            return;
        }

        float canvasWidth = canvasRect.rect.width;
        float imageWidth = transitionImage.rect.width;

        transitionImage.anchoredPosition = new Vector2(
            -canvasWidth / 2f - imageWidth / 2f,
            0f
        );

        transitionImage.gameObject.SetActive(false);
    }
}
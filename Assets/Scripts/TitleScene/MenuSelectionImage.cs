
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectionImage : MonoBehaviour
{
    [Header("表示する画像")]
    [SerializeField] private RectTransform selectImage;

    [Header("選択対象のボタン")]
    [SerializeField] private Button[] buttons;

    [Header("画像の位置調整")]
    [SerializeField] private Vector2 offset = new Vector2(-80f, 0f);



    private Button hoveredButton;

    private void Start()
    {
        if (selectImage != null)
        {
            selectImage.gameObject.SetActive(false);
        }

        // 各ボタンにマウスイベントを登録
        foreach (Button button in buttons)
        {
            if (button == null) continue;

            MenuButtonHoverHandler handler =
                button.GetComponent<MenuButtonHoverHandler>();

            if (handler == null)
            {
                handler = button.gameObject.AddComponent<MenuButtonHoverHandler>();
            }

            handler.onEnter = () => hoveredButton = button;
            handler.onExit = () =>
            {
                if (hoveredButton == button)
                    hoveredButton = null;
            };
        }
    }

    private void Update()
    {
        if (selectImage == null) return;

        Button targetButton = hoveredButton;

        // マウスが乗っていなければ、EventSystemの選択を使う
        if (targetButton == null && EventSystem.current != null)
        {
            GameObject selected =
                EventSystem.current.currentSelectedGameObject;

            foreach (Button button in buttons)
            {
                if (button != null && button.gameObject == selected)
                {
                    targetButton = button;
                    break;
                }
            }
        }

        if (targetButton == null || !targetButton.interactable)
        {
            selectImage.gameObject.SetActive(false);
            return;
        }

        selectImage.gameObject.SetActive(true);

        RectTransform buttonRect =
     targetButton.GetComponent<RectTransform>();

        // ボタンの左側に矢印を表示する
        Vector3 worldPos = buttonRect.position;

        selectImage.position = new Vector3(
            worldPos.x + offset.x,
            worldPos.y + offset.y,
            selectImage.position.z
        );
    }
}

public class MenuButtonHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public System.Action onEnter;
    public System.Action onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }
}

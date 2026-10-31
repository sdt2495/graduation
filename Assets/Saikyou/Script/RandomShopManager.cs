using UnityEngine;

public class RandomShopManager : MonoBehaviour
{
    public GameObject shopPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
        }
    }
}

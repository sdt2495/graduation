using UnityEngine;

public class HPUI : MonoBehaviour
{
    [SerializeField] private PlayerHPSlot[] hpSlot;

    public void UpdateHP(int currentHP)
    {
        for (int i = 0; i < hpSlot.Length; i++)
        {
            if(i < currentHP)
            {
                hpSlot[i].ResetHP();
            }
            else
            {
                hpSlot[i].BreakHaert();
            }
        }

    }
}

using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText;

    public void UpdateWave(int currentWave, int maxWave)
    {
        waveText.text = $"{currentWave:D2}/{maxWave:D2}";
    }
}

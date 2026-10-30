using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SystemSEVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string parameterName;

    private void Start()
    {
        if (slider == null)
        {
            Debug.LogError($"{name}: Slider‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            enabled = false;
            return;
        }

        if (mixer == null)
        {
            Debug.LogError($"{name}: AudioMixer‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            enabled = false;
            return;
        }

        if (string.IsNullOrEmpty(parameterName))
        {
            Debug.LogError($"{name}: ParameterName‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            enabled = false;
            return;
        }

        slider.onValueChanged.AddListener(SetVolume);

        SetVolume(slider.value);
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(SetVolume);
        }
    }

    private void SetVolume(float value)
    {
        value = Mathf.Max(value, 0.0001f);

        float db = Mathf.Log10(value) * 20f;

        if (!mixer.SetFloat(parameterName, db))
        {
            Debug.LogWarning(
                $"AudioMixer Parameter '{parameterName}' ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
        }
    }
}
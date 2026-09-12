using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashEffect : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.15f;

    public IEnumerator PlayFlash()
    {
        if (flashImage == null)
        {
            Debug.LogError("FlashImageÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            yield break;
        }

        Color color = flashImage.color;

        color.a = 1f;
        flashImage.color = color;

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(
                1f,
                0f,
                timer / flashDuration);

            flashImage.color = color;

            yield return null;
        }

        color.a = 0f;
        flashImage.color = color;
    }
}
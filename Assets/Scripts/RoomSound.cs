using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RoomSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip theme, exitTheme;
    [SerializeField]
    private Volume volume;

    private AudioSource audioSource;  
    private Coroutine vignetteCoroutine;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null)
            {
                audioSource.clip = theme;
                audioSource.Play();
            }

            // Start the vignette effect
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }
            vignetteCoroutine = StartCoroutine(GrowVignette());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null)
            {
                audioSource.clip = exitTheme;
                audioSource.Play();
            }

            // reset vignette
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }

            Vignette vignette;
            if (volume.profile.TryGet(out vignette))
            {
                vignette.intensity.value = 0f;  
            }
        }
    }

    private IEnumerator GrowVignette()
    {
        Vignette vignette;
        if (volume.profile.TryGet(out vignette))
        {
            float targetIntensity = 0.5f;
            float duration = 2.0f;  
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float newIntensity = Mathf.Lerp(0, targetIntensity, timeElapsed / duration);
                vignette.intensity.value = newIntensity;
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            float palpitationDuration = 0.5f;
            while (true)
            {
                float currentIntensity = 0.5f + 0.05f * Mathf.Sin(2 * Mathf.PI * Time.time / palpitationDuration);
                vignette.intensity.value = currentIntensity;
                yield return null;
            }
        }
    }
}

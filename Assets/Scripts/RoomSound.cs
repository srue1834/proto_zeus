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

    private AudioSource audioSource;  // Reference to the AudioSource component
    private Coroutine vignetteCoroutine;

    private void Start()
    {
        // Get the reference to the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("RoomSound script requires an AudioSource component!");
        }
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

            // Reset vignette
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }

            Vignette vignette;
            if (volume.profile.TryGet(out vignette))
            {
                vignette.intensity.value = 0f;  // Reset to the default or desired value
            }
        }
    }

    private IEnumerator GrowVignette()
    {
        Vignette vignette;
        if (volume.profile.TryGet(out vignette))
        {
            float targetIntensity = 0.5f;
            float duration = 2.0f;  // Time taken to reach the target intensity
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float newIntensity = Mathf.Lerp(0, targetIntensity, timeElapsed / duration);
                vignette.intensity.value = newIntensity;
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Begin "palpitation" effect
            float palpitationDuration = 0.5f;  // Duration for one full cycle (increase + decrease)
            while (true)
            {
                float currentIntensity = 0.5f + 0.05f * Mathf.Sin(2 * Mathf.PI * Time.time / palpitationDuration);
                vignette.intensity.value = currentIntensity;
                yield return null;
            }
        }
    }
}

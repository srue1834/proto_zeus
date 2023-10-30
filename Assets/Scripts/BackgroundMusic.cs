using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource source;
    static public BackgroundMusic instance;

    private AudioClip defaultTheme; // Default theme to play on scene change

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Add a listener for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Callback function that's triggered when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the scene is not the "Main" scene, revert to the default theme
        if (scene.name != "Main")
        {
            ChangeTrack(defaultTheme); // This will revert to the default theme
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the sceneLoaded event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0f;

        StartCoroutine(Fade(true, source, 2f, 1f));
        StartCoroutine(Fade(false, source, 2f, 0f));
    }

    private void Update()
    {
        if (!source.isPlaying)
        {
            source.Play();
            StartCoroutine(Fade(true, source, 2f, 1f));
            StartCoroutine(Fade(false, source, 2f, 0f));
        }

    }

    public IEnumerator Fade(bool fadeIn, AudioSource source, float duration, float targetVol)
    {
        if (!fadeIn)
        {
            double lengthSource = (double)source.clip.samples / source.clip.frequency;
            yield return new WaitForSecondsRealtime((float)(lengthSource - duration));
        }

        float time = 0f;
        float startVol = source.volume;

        while (time < duration)
        {
            string fadeSit = fadeIn ? "fadeIn" : "fadeOut";
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, time / duration);
            yield return null;
        }

        yield break;

    }

    public void ChangeTrack(AudioClip clip)
    {
        StopAllCoroutines();
        source.clip = clip;
        source.volume = 0f;

        StartCoroutine(Fade(true, source, 2f, 1f));
        StartCoroutine(Fade(false, source, 2f, 0f));
    }

    public AudioClip GetTrack()
    {
        return source.clip;
    }
}

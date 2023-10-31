using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class CutsceneController : MonoBehaviour
{
    public float cutsceneDuration = 39.79f;
    public string mainMenuSceneName = "MainMenuScene";

    private void Start()
    {
        StartCoroutine(EndCutsceneAfterTime());
    }

    private IEnumerator EndCutsceneAfterTime()
    {
        yield return new WaitForSeconds(cutsceneDuration);
        // After the wait time, transition to the main menu
        TransitionToMainMenu();
    }

    private void TransitionToMainMenu()
    {
        // Using your SceneManagerScript to load the main menu
        SceneManagerScript sceneManager = FindObjectOfType<SceneManagerScript>();
        if (sceneManager != null)
        {
            sceneManager.LoadScene(mainMenuSceneName);
        }
        
    }
}

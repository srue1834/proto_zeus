using UnityEngine;
using System.Collections;

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
        TransitionToMainMenu();
    }

    private void TransitionToMainMenu()
    {
        SceneManagerScript sceneManager = FindObjectOfType<SceneManagerScript>();
        if (sceneManager != null)
        {
            sceneManager.LoadScene(mainMenuSceneName);
        }
        
    }
}

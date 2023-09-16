using UnityEngine;
using UnityEngine.Playables;

public class CutSceneManager : MonoBehaviour
{
    public PlayerController playerController;
    private PlayableDirector playableDirector;

    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        playableDirector.played += OnPlayableDirectorStarted;
        playableDirector.stopped += OnPlayableDirectorStopped;
    }

    public void OnPlayableDirectorStarted(PlayableDirector aDirector)
    {
        playerController.isInCutscene = true;
        //Debug.Log("Cutscene started.");
    }

    public void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        playerController.isInCutscene = false;
        //Debug.Log("Cutscene ended.");
    }

    void OnDestroy()
    {
        playableDirector.played -= OnPlayableDirectorStarted;
        playableDirector.stopped -= OnPlayableDirectorStopped;
    }
}

using UnityEngine;

public class PersistAcrossScenes : MonoBehaviour
{
    public static PersistAcrossScenes instance;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}

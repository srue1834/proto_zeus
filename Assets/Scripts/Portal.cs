using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public GameObject zeus;  // Drag Zeus GameObject here from the editor

    private void OnTriggerEnter(Collider other)
    {
        ZeusPickup zeusPickup = zeus.GetComponent<ZeusPickup>();

        if (other.CompareTag("Player") && zeusPickup.IsCarryingZeus())
        {
            // Load the next level
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
    }
}

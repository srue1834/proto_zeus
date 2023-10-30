using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip theme, previousTheme, exitTheme;  // Add exitTheme

    private void OnTriggerEnter(Collider other)
    {
        previousTheme = BackgroundMusic.instance.GetTrack();
        if (other.CompareTag("Player"))
        {
            BackgroundMusic.instance.ChangeTrack(theme);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BackgroundMusic.instance.ChangeTrack(exitTheme); // Change to the exit theme here
        }
    }
}

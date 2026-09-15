using UnityEngine;

/// <summary>
/// Handles application-level input: pausing the simulation and quitting.
/// </summary>
public class ApplicationController : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.P;
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    /// <summary>Whether the simulation is currently paused.</summary>
    public bool IsPaused { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(quitKey))
        {
            Quit();
        }
    }

    private void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    private void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
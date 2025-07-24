using UnityEngine;
using UnityEngine.SceneManagement; // Ensure this namespace is included

public class SceneLoader : MonoBehaviour
{
    // Method to load a scene by name
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName); // Load scene by name
        }
        else
        {
            Debug.LogError("Scene name is invalid!");
        }
    }

    // Method to load a scene by index
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex); // Load scene by index
        }
        else
        {
            Debug.LogError("Scene index is out of range!");
        }
    }

    // Method to reload the current scene
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current active scene
    }

    // Method to quit the application
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quits the application
    }
}

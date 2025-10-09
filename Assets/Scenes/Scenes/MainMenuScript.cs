using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Call this from the Play button
    public void PlayGame()
    {
        // Load the next scene in Build Settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Optional: quit the game
    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}

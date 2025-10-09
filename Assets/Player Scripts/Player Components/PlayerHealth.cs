using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI References")]
    public GameObject deathScreen;

    private UIHearts uiHearts;

    void Start()
    {
        currentHealth = maxHealth;
        uiHearts = FindObjectOfType<UIHearts>();

        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);

        if (deathScreen != null)
            deathScreen.SetActive(false); 
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine()); 
        }
    }

 
    private IEnumerator DieRoutine()
    {
        Debug.Log("Player died!");

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            Debug.Log("Death screen activated");
        }
        else
        {
            Debug.LogWarning("⚠️ Death screen is not assigned!");
        }

        yield return null;

        Time.timeScale = 0f;
    }

   
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene"); // ✅ Make sure this name matches your menu scene!
    }
}

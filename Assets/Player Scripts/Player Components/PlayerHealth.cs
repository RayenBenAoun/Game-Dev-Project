using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI References")]
    [SerializeField] private UIHearts uiHearts;      // ← drag Hearts object (with UIHearts) here
    public GameObject deathScreen;                   // ← drag your DeathScreen panel here

    private bool isDead = false;

    void Awake()
    {
        // If not assigned in Inspector, fallback to safe find (works if UI starts inactive)
        if (uiHearts == null)
            uiHearts = FindFirstObjectByType<UIHearts>(FindObjectsInactive.Include);
    }

    void Start()
    {
        currentHealth = Mathf.Max(1, maxHealth);

        // Build + draw hearts
        if (uiHearts != null)
        {
            uiHearts.Build(maxHealth);
            uiHearts.UpdateHearts(currentHealth, maxHealth);
        }

        if (deathScreen != null) deathScreen.SetActive(false);

        // Ensure gameplay isn't frozen from a previous pause
        Time.timeScale = 1f;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0, maxHealth);

        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Abs(amount), 0, maxHealth);
        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax, bool keepRatio = false)
    {
        newMax = Mathf.Max(1, newMax);
        if (keepRatio)
        {
            float ratio = (maxHealth > 0) ? (float)currentHealth / maxHealth : 1f;
            maxHealth = newMax;
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(ratio * maxHealth), 0, maxHealth);
        }
        else
        {
            maxHealth = newMax;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        if (uiHearts != null)
        {
            uiHearts.Build(maxHealth);
            uiHearts.UpdateHearts(currentHealth, maxHealth);
        }
    }

    private IEnumerator DieRoutine()
    {
        if (isDead) yield break;
        isDead = true;

        if (deathScreen != null)
            deathScreen.SetActive(true);
        else
            Debug.LogWarning("Death screen is not assigned!");

        // Let UI draw one frame, then pause gameplay
        yield return null;
        Time.timeScale = 0f;
    }

    // UI Buttons
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    private UIHearts uiHearts;

    void Start()
    {
        currentHealth = maxHealth;
        uiHearts = FindObjectOfType<UIHearts>();

        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (uiHearts != null)
            uiHearts.UpdateHearts(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            // You can add death animation, restart, etc.
        }
    }
}

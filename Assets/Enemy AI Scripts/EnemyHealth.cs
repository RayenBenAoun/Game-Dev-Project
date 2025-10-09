using UnityEngine;
using System.Collections;

public enum EnemyColor { Red, Blue, Green, Yellow }
public enum EnemyState { Alive, DownedColor, Dead }

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public EnemyColor enemyColor;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public float flashInterval = 0.3f;

    [Header("Downed State")]
    public float downedDuration = 3f;

    [Header("Death Handling")]
    public bool destroyRoot = true;
    public float destroyDelay = 0f;

    private int currentHealth;
    private EnemyState state = EnemyState.Alive;
    private bool dead;
    private Color originalColor;

    private Coroutine downedRoutine;
    private Coroutine flashRoutine;

    // 🟢 Public property so AI can check if enemy is downed
    public bool IsDowned => state == EnemyState.DownedColor;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount, EnemyColor? projectileColor = null)
    {
        Debug.Log($"[{name}] TakeDamage | State={state} | ProjectileColor={projectileColor}");

        if (dead) return;

        switch (state)
        {
            case EnemyState.Alive:
                currentHealth -= amount;
                if (currentHealth <= 0)
                    EnterDownedColor();
                break;

            case EnemyState.DownedColor:
                if (projectileColor.HasValue && projectileColor.Value == enemyColor)
                {
                    Debug.Log($"[{name}] Correct color match! Dying...");
                    Die();
                }
                break;
        }
    }

    private void EnterDownedColor()
    {
        Debug.Log($"[{name}] Entered DownedColor state.");
        state = EnemyState.DownedColor;

        if (spriteRenderer != null)
            spriteRenderer.color = GetColorForEnemy() * 1.2f; // tint and brighten

        if (downedRoutine != null) StopCoroutine(downedRoutine);
        downedRoutine = StartCoroutine(DownedTimer());

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashEffect());
    }

    private IEnumerator DownedTimer()
    {
        yield return new WaitForSeconds(downedDuration);
        if (!dead)
            Revive();
    }

    private IEnumerator FlashEffect()
    {
        bool visible = true;
        while (state == EnemyState.DownedColor)
        {
            visible = !visible;
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
            yield return new WaitForSeconds(flashInterval);
        }
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    private void Revive()
    {
        Debug.Log($"[{name}] Revived to Alive state.");
        state = EnemyState.Alive;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
    }

    private void Die()
    {
        state = EnemyState.Dead;
        dead = true;

        if (downedRoutine != null) StopCoroutine(downedRoutine);
        if (flashRoutine != null) StopCoroutine(flashRoutine);

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        var rb = GetComponentInChildren<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GameObject toDestroy = destroyRoot ? transform.root.gameObject : gameObject;
        Destroy(toDestroy, destroyDelay);
    }

    private Color GetColorForEnemy()
    {
        switch (enemyColor)
        {
            case EnemyColor.Red: return Color.red;
            case EnemyColor.Blue: return Color.blue;
            case EnemyColor.Green: return Color.green;
            case EnemyColor.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }
}

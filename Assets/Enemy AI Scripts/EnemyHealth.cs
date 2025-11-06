using UnityEngine;
using System.Collections;

// Keep EnemyColor in a separate file (EnemyEnums.cs) to avoid duplicate-enum errors.
// public enum EnemyColor { Red, Blue, Green, Yellow }

public enum EnemyState { Alive, DownedColor, Dead }

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public EnemyColor enemyColor;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public float flashInterval = 0.3f;   // flash speed while downed

    [Header("Downed State")]
    public float downedDuration = 3f;    // how long the downed window lasts

    [Header("Death Handling")]
    public bool destroyRoot = true;
    public float destroyDelay = 0f;

    private int currentHealth;
    private EnemyState state = EnemyState.Alive;
    private bool dead;

    private Color baseColor;             // original sprite color (no default tint)
    private Coroutine downedRoutine;
    private Coroutine flashRoutine;

    public bool IsDowned => state == EnemyState.DownedColor;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color; // ✅ keep original color
        else
            baseColor = Color.white;
    }

    public void TakeDamage(int amount, EnemyColor? projectileColor = null)
    {
        // Safe logging for nullable enum
        string projStr = projectileColor.HasValue ? projectileColor.Value.ToString() : "None";
        Debug.Log($"[{name}] TakeDamage | State={state} | ProjectileColor={projStr}");

        if (dead) return;

        switch (state)
        {
            case EnemyState.Alive:
                currentHealth -= amount;
                if (currentHealth <= 0)
                    EnterDownedColor();
                break;

            case EnemyState.DownedColor:
                // Only the matching color kills while downed
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
        if (state == EnemyState.DownedColor) return;

        Debug.Log($"[{name}] Entered DownedColor state.");
        state = EnemyState.DownedColor;

        // timers
        if (downedRoutine != null) StopCoroutine(downedRoutine);
        downedRoutine = StartCoroutine(DownedTimer());

        // visual flash (brighten, not hide)
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashEffect());
    }

    private IEnumerator DownedTimer()
    {
        yield return new WaitForSeconds(downedDuration);
        if (!dead) Revive();
    }

    private IEnumerator FlashEffect()
    {
        // Brighten original color consistently for all sprites
        Color bright = baseColor * 2.2f;
        bright.r = Mathf.Clamp01(bright.r);
        bright.g = Mathf.Clamp01(bright.g);
        bright.b = Mathf.Clamp01(bright.b);
        bright.a = 1f;

        float t = 0f;
        while (state == EnemyState.DownedColor)
        {
            bool on = (Mathf.FloorToInt(t / flashInterval) % 2 == 0);
            if (spriteRenderer) spriteRenderer.color = on ? bright : baseColor;

            t += Time.deltaTime;
            yield return null;
        }

        // Reset color on exit
        if (spriteRenderer) spriteRenderer.color = baseColor;
    }

    private void Revive()
    {
        Debug.Log($"[{name}] Revived to Alive state.");
        state = EnemyState.Alive;
        currentHealth = maxHealth;

        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        if (spriteRenderer) spriteRenderer.color = baseColor;
    }

    private void Die()
    {
        state = EnemyState.Dead;
        dead = true;

        if (downedRoutine != null) { StopCoroutine(downedRoutine); downedRoutine = null; }
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }

        // Disable hit + physics
        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        var rb = GetComponentInChildren<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Optional: hide immediately (or keep visible until destroyDelay)
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;

        GameObject target = destroyRoot ? transform.root.gameObject : gameObject;
        Destroy(target, destroyDelay);
    }
}

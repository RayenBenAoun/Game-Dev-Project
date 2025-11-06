using UnityEngine;
using System.Collections;

public enum BossColorState { Red, Blue, Green, Yellow, None }

public class BossAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                // drag the Player here (or tag Player and I'll grab it)
    public SpriteRenderer spriteRenderer;   // boss sprite
    public GameObject bossProjectilePrefab; // projectile the boss fires at you
    public LayerMask coverMask;             // same "Cover" layer your drawn barriers use

    [Header("Stats")]
    public int maxHealth = 10;
    public float shootInterval = 2f;        // time between shots during barrage
    public float colorPhaseInterval = 6f;   // how often boss enters 'color flash' vulnerable state
    public float vulnerableDuration = 2f;   // how long you have to shoot correct color
    public int contactDamage = 1;           // damage per hit from projectile

    private int currentHealth;
    private bool isAlive = true;

    private BossColorState currentColorState = BossColorState.None;
    private bool isVulnerable = false;
    private Color baseColor;

    void Awake()
    {
        currentHealth = maxHealth;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
        else
            baseColor = Color.white;
    }

    void Start()
    {
        // auto-find player if not set
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        float colorTimer = 0f;

        while (isAlive)
        {
            // 1) either shoot or enter color phase based on a timer
            colorTimer += shootInterval;

            if (colorTimer >= colorPhaseInterval)
            {
                colorTimer = 0f;
                yield return StartCoroutine(ColorVulnerablePhase());
            }
            else
            {
                FireAtPlayer();
                yield return new WaitForSeconds(shootInterval);
            }
        }
    }

    void FireAtPlayer()
    {
        if (!player || bossProjectilePrefab == null) return;

        // check if player's line of sight is blocked by cover
        bool blocked = Physics2D.Linecast(transform.position, player.position, coverMask);
        // if blocked, still fire (you WANT player to build cover), but we could aim slightly offset later

        GameObject projObj = Instantiate(bossProjectilePrefab, transform.position, Quaternion.identity);
        BossProjectile proj = projObj.GetComponent<BossProjectile>();
        if (proj != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            proj.Init(dir, contactDamage);
        }
    }

    IEnumerator ColorVulnerablePhase()
    {
        // pick random color
        currentColorState = GetRandomBossColor();
        isVulnerable = true;

        // flash that color
        Color flashColor = ToUnityColor(currentColorState) * 2.2f;
        flashColor.r = Mathf.Clamp01(flashColor.r);
        flashColor.g = Mathf.Clamp01(flashColor.g);
        flashColor.b = Mathf.Clamp01(flashColor.b);
        flashColor.a = 1f;

        if (spriteRenderer)
            spriteRenderer.color = flashColor;

        // wait while vulnerable
        yield return new WaitForSeconds(vulnerableDuration);

        // end vulnerable
        isVulnerable = false;
        currentColorState = BossColorState.None;
        if (spriteRenderer)
            spriteRenderer.color = baseColor;
    }

    BossColorState GetRandomBossColor()
    {
        int r = Random.Range(0, 4); // 0-3
        switch (r)
        {
            case 0: return BossColorState.Red;
            case 1: return BossColorState.Blue;
            case 2: return BossColorState.Green;
            case 3: return BossColorState.Yellow;
        }
        return BossColorState.Red;
    }

    Color ToUnityColor(BossColorState c)
    {
        switch (c)
        {
            case BossColorState.Red: return Color.red;
            case BossColorState.Blue: return Color.blue;
            case BossColorState.Green: return Color.green;
            case BossColorState.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }

    // called by PLAYER PROJECTILE when it hits the boss
    public void TakePlayerHit(EnemyColor projectileColor)
    {
        // only take damage if we're vulnerable AND the colors match
        if (!isVulnerable) return;

        // convert BossColorState to EnemyColor to compare
        EnemyColor needed = EnemyColor.Red;
        switch (currentColorState)
        {
            case BossColorState.Red: needed = EnemyColor.Red; break;
            case BossColorState.Blue: needed = EnemyColor.Blue; break;
            case BossColorState.Green: needed = EnemyColor.Green; break;
            case BossColorState.Yellow: needed = EnemyColor.Yellow; break;
        }

        if (projectileColor != needed) return;

        currentHealth--;
        Debug.Log("[BOSS] Took damage. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false;
        StopAllCoroutines();
        Debug.Log("BOSS DEFEATED");

        // turn off sprite / collider / whatever you want:
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var c in cols) c.enabled = false;

        if (spriteRenderer) spriteRenderer.color = Color.white;
        Destroy(gameObject, 0.5f);
    }
}

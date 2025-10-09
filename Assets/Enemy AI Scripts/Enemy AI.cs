using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.5f;

    protected Transform player;
    protected Rigidbody2D rb;
    protected bool canAttack = true;
    protected bool isChasing = false;
    protected EnemyHealth health; // reference to health

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        health = GetComponent<EnemyHealth>();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // 🟢 If this enemy is downed, freeze
        if (health != null && health.IsDowned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectionRange)
        {
            isChasing = true;
            ChasePlayer();

            if (dist <= attackRange && canAttack)
                StartCoroutine(AttackRoutine());
        }
        else
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    protected virtual IEnumerator AttackRoutine()
    {
        canAttack = false;
        DoAttack();
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected virtual void DoAttack()
    {
        Debug.Log($"[{name}] performs basic melee attack.");
        // Add animation trigger or damage application here
    }
}

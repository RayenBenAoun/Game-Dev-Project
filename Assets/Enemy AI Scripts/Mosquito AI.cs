using UnityEngine;
using System.Collections;

public class MosquitoAI : EnemyAI
{
    public GameObject bloodSpitPrefab;
    public float spitSpeed = 5f;
    public float attackStopTime = 0.5f;

    private bool hasMeleeHit = false;

    protected override IEnumerator AttackRoutine()
    {
        canAttack = false;
        rb.linearVelocity = Vector2.zero; // stop while attacking

        DoAttack();

        yield return new WaitForSeconds(attackStopTime);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected override void DoAttack()
    {
        if (!hasMeleeHit)
        {
            Debug.Log($"[{name}] stabs player with proboscis!");
            DamagePlayer(1); // first melee hit
            hasMeleeHit = true;
        }
        else
        {
            Debug.Log($"[{name}] spits blood!");
            SpitBlood();
        }
    }

    private void SpitBlood()
    {
        if (bloodSpitPrefab == null || player == null) return;

        GameObject spit = Instantiate(bloodSpitPrefab, transform.position, Quaternion.identity);
        Rigidbody2D spitRb = spit.GetComponent<Rigidbody2D>();
        if (spitRb != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            spitRb.linearVelocity = dir * spitSpeed;
        }
    }

    private void DamagePlayer(int amount)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(amount);
    }
}

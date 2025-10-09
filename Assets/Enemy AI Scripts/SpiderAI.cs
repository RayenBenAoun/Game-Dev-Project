using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderAI : EnemyAI
{
    public bool canShootWeb = true;
    public GameObject webPrefab;
    public float webSpeed = 6f;
    public float attackStopTime = 0.5f;
    public float webChance = 0.3f; // 30% chance to shoot web

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
        if (canShootWeb && Random.value < webChance)
        {
            Debug.Log($"[{name}] shoots a sticky web!");
            ShootWeb();
        }
        else
        {
            Debug.Log($"[{name}] bites the player!");
            DamagePlayer(1);
        }
    }

    private void ShootWeb()
    {
        if (webPrefab == null || player == null) return;

        GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
        Rigidbody2D webRb = web.GetComponent<Rigidbody2D>();
        if (webRb != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            webRb.linearVelocity = dir * webSpeed;
        }
    }

    private void DamagePlayer(int amount)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(amount);
    }
}


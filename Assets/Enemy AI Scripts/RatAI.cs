using UnityEngine;
using System.Collections;

public class RatAI : EnemyAI
{
    public float attackStopTime = 0.5f; // how long the rat pauses after an attack

    protected override IEnumerator AttackRoutine()
    {
        canAttack = false;

        // Stop moving for the bite
        rb.linearVelocity = Vector2.zero;

        DoAttack();

        player.GetComponent<PlayerHealth>().TakeDamage(1);

        // Pause movement briefly
        yield return new WaitForSeconds(attackStopTime);

        // Attack cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected override void DoAttack()
    {
        Debug.Log($"[{name}] bites the player!");
        // Add melee damage to player here
    }
}

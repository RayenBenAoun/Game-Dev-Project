using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 5f;
    public int damage = 1;

    private Vector2 direction;

    public void Init(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // move straight every frame
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // hit player
        if (col.CompareTag("Player"))
        {
            var hp = col.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // hit cover (your drawn barrier should be tagged "Cover")
        if (col.CompareTag("Cover"))
        {
            Destroy(gameObject);
            return;
        }

        // OPTIONAL: if it hits walls/tiles/etc you can also Destroy here
    }
}

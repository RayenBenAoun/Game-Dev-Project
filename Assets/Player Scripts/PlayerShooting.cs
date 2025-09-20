using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public GameObject redProjectile;
    public GameObject blueProjectile;
    public GameObject greenProjectile;
    public GameObject yellowProjectile;
    public float projectileSpeed = 10f;
    public float cooldownTime = 5f;

    private bool redReady = true, blueReady = true, greenReady = true, yellowReady = true;

    void Update()
    {
        var kb = Keyboard.current;

        if (kb.digit1Key.wasPressedThisFrame && redReady)
        { Shoot(redProjectile, EnemyColor.Red); redReady = false; StartCoroutine(Cooldown(() => redReady = true)); }

        if (kb.digit2Key.wasPressedThisFrame && blueReady)
        { Shoot(blueProjectile, EnemyColor.Blue); blueReady = false; StartCoroutine(Cooldown(() => blueReady = true)); }

        if (kb.digit3Key.wasPressedThisFrame && greenReady)
        { Shoot(greenProjectile, EnemyColor.Green); greenReady = false; StartCoroutine(Cooldown(() => greenReady = true)); }

        if (kb.digit4Key.wasPressedThisFrame && yellowReady)
        { Shoot(yellowProjectile, EnemyColor.Yellow); yellowReady = false; StartCoroutine(Cooldown(() => yellowReady = true)); }
    }

    void Shoot(GameObject prefab, EnemyColor color)
    {
        if (prefab == null) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 origin = transform.position;
        Vector2 dir = (mouseWorld - origin).normalized;

        GameObject proj = Instantiate(prefab, origin, Quaternion.identity);
        proj.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);

        var projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.projectileColor = color;
            projScript.Launch(dir, projectileSpeed);
        }
    }

    IEnumerator Cooldown(System.Action resetFlag)
    {
        yield return new WaitForSeconds(cooldownTime);
        resetFlag.Invoke();
    }
}

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem; // using the New Input System

//[RequireComponent(typeof(LineRenderer))]
//public class TraceAbility : MonoBehaviour
//{
//    [Header("Input")]
//    public bool useMouse = false;          // true = Left Mouse to draw; false = Space key

//    [Header("Drawing")]
//    public float minPointDistance = 0.15f; // minimum distance between saved points
//    public float closeThreshold = 0.3f;    // distance between last & first to count as a closed loop
//    public float lineWidth = 0.06f;

//    [Header("Resource")]
//    public float maxResource = 100f;
//    public float drainPerSecond = 25f;     // while drawing
//    public float regenPerSecond = 20f;     // while not drawing
//    public float regenDelay = 1.0f;        // wait this long after drawing stops before regen starts

//    [Header("Damage")]
//    public int damageOnClose = 1;          // damage to each enemy inside the loop
//    public float zoneLifetime = 0.1f;      // how long the polygon collider exists

//    LineRenderer lr;
//    List<Vector2> points = new List<Vector2>();
//    bool tracing = false;

//    float resource;
//    float lastUseTime;

//    void Awake()
//    {
//        lr = GetComponent<LineRenderer>();
//        lr.positionCount = 0;
//        lr.startWidth = lineWidth;
//        lr.endWidth = lineWidth;
//        lr.useWorldSpace = true;
//        resource = maxResource;
//    }

//    void Update()
//    {
//        // --- INPUT ---
//        bool pressBegan = false;
//        bool pressed = false;
//        bool pressEnded = false;

//        if (useMouse)
//        {
//            pressBegan = Mouse.current.leftButton.wasPressedThisFrame;
//            pressed = Mouse.current.leftButton.isPressed;
//            pressEnded = Mouse.current.leftButton.wasReleasedThisFrame;
//        }
//        else
//        {
//            pressBegan = Keyboard.current.spaceKey.wasPressedThisFrame;
//            pressed = Keyboard.current.spaceKey.isPressed;
//            pressEnded = Keyboard.current.spaceKey.wasReleasedThisFrame;
//        }

//        // Start tracing
//        if (!tracing && pressBegan && resource > 5f)
//        {
//            StartTrace();
//        }

//        // Continue tracing
//        if (tracing && pressed)
//        {
//            // drain resource
//            float drain = drainPerSecond * Time.deltaTime;
//            resource = Mathf.Max(0f, resource - drain);
//            lastUseTime = Time.time;

//            // add points as you move
//            Vector2 pos = GetWorldCursorOrPlayerFeet();
//            if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], pos) >= minPointDistance)
//            {
//                AddPoint(pos);
//            }

//            // stop if empty
//            if (resource <= 0f)
//            {
//                EndTrace();
//            }
//        }

//        // End tracing if button released
//        if (tracing && pressEnded)
//        {
//            EndTrace();
//        }

//        // Regenerate resource if not tracing
//        if (!tracing && (Time.time - lastUseTime) >= regenDelay)
//        {
//            resource = Mathf.Min(maxResource, resource + regenPerSecond * Time.deltaTime);
//        }
//    }

//    Vector2 GetWorldCursorOrPlayerFeet()
//    {
//        if (useMouse)
//        {
//            var m = Mouse.current.position.ReadValue();
//            Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(m.x, m.y, -Camera.main.transform.position.z));
//            return new Vector2(world.x, world.y);
//        }
//        else
//        {
//            // draw from player�s position (feet)
//            return (Vector2)transform.position;
//        }
//    }

//    void StartTrace()
//    {
//        tracing = true;
//        points.Clear();
//        lr.positionCount = 0;

//        // seed first point
//        AddPoint(GetWorldCursorOrPlayerFeet());
//    }

//    void AddPoint(Vector2 p)
//    {
//        points.Add(p);
//        lr.positionCount = points.Count;
//        lr.SetPosition(points.Count - 1, new Vector3(p.x, p.y, 0f));
//    }

//    void EndTrace()
//    {
//        tracing = false;

//        // If we have a valid closed loop (at least 3 points and ends near start), apply damage
//        if (points.Count >= 3 && Vector2.Distance(points[0], points[points.Count - 1]) <= closeThreshold)
//        {
//            ApplyDamageInsidePolygon(points);
//        }

//        // Clear line after finishing (optional: keep it for a short time if you want)
//        lr.positionCount = 0;
//        points.Clear();
//    }

//    void ApplyDamageInsidePolygon(List<Vector2> worldPoly)
//    {
//        // Build a temporary GameObject with a PolygonCollider2D set as trigger
//        GameObject zone = new GameObject("DamageZone");
//        var poly = zone.AddComponent<PolygonCollider2D>();
//        poly.isTrigger = true;

//        // Put it at the origin so we can assign world positions as local points
//        zone.transform.position = Vector3.zero;

//        // Set collider points
//        poly.pathCount = 1;
//        poly.SetPath(0, worldPoly.ToArray());

//        // Collect overlapping colliders using OverlapCollider
//        var filter = new ContactFilter2D();
//        filter.NoFilter();
//        List<Collider2D> hits = new List<Collider2D>();
//        poly.Overlap(filter, hits);

//        foreach (var hit in hits)
//        {
//            // Damage anything with EnemyHealth on it
//            var eh = hit.GetComponentInParent<EnemyHealth>();
//            if (eh != null)
//            {
//                eh.TakeDamage(damageOnClose);
//            }
//        }

//        // Remove the zone after a brief moment
//        Destroy(zone, zoneLifetime);
//    }

//    // Expose resource value (for UI bars later)
//    public float Resource01 => Mathf.Clamp01(resource / maxResource);
//}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class PlayerOutline : MonoBehaviour
{
    [Header("Drawing")]
    public float minPointDistance = 0.05f;
    public float closeThreshold = 0.3f;
    public int minVerticesToClose = 6;
    public float minPerimeterToClose = 0.75f;
    public float lineWidth = 0.08f;

    [Header("Resource")]
    public float maxDrawTime = 5f;
    private float currentDrawTime;
    public float regenRate = 1f;
    public Slider resourceBar;

    [Header("Cover")]
    public float coverLifetime = 2f;

    [Header("Combat")]
    public int damageOnClose = 1; // ✅ damage applied when polygon closes

    private LineRenderer lr;
    private readonly List<Vector2> points = new List<Vector2>();
    private float pathLength = 0f;
    private bool drawing = false;
    private bool shapeActive = false;
    private float shapeTimer = 0f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        if (lr.material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            lr.material = new Material(shader);
        }
        lr.startColor = Color.white;
        lr.endColor = Color.white;

        currentDrawTime = maxDrawTime;

        if (resourceBar != null)
        {
            resourceBar.maxValue = maxDrawTime;
            resourceBar.value = currentDrawTime;
        }
    }

    void Update()
    {
        if (resourceBar != null)
            resourceBar.value = currentDrawTime;

        if (shapeActive)
        {
            shapeTimer -= Time.deltaTime;
            if (shapeTimer <= 0f)
            {
                ClearLine();
                shapeActive = false;
            }
            return;
        }

        bool start = Keyboard.current.spaceKey.wasPressedThisFrame;
        bool hold = Keyboard.current.spaceKey.isPressed;
        bool end = Keyboard.current.spaceKey.wasReleasedThisFrame;

        // Start drawing
        if (!drawing && start && currentDrawTime > 0f)
        {
            drawing = true;
            points.Clear();
            lr.positionCount = 0;
            pathLength = 0f;

            Vector2 p = transform.position;
            AddPoint(p);
            AddPoint(p);
        }

        // Continue drawing
        if (drawing && hold)
        {
            currentDrawTime -= Time.deltaTime;
            if (currentDrawTime <= 0f)
            {
                EndDrawingAsCover();
                return;
            }

            Vector2 p = transform.position;

            if (lr.positionCount >= 2)
                lr.SetPosition(lr.positionCount - 1, p);

            if (points.Count == 0 || Vector2.Distance(points[^1], p) >= minPointDistance)
                AddPoint(p);

            if (CanAttemptClose() && IsClosedByDistance())
                CloseAndResolve();
        }

        // Stop drawing
        if (drawing && end)
        {
            drawing = false;

            if (CanAttemptClose() && IsClosedByDistance())
            {
                CloseAndResolve();
            }
            else
            {
                EndDrawingAsCover();
            }
        }
        else if (!drawing && currentDrawTime < maxDrawTime)
        {
            currentDrawTime += Time.deltaTime * regenRate;
            if (currentDrawTime > maxDrawTime)
                currentDrawTime = maxDrawTime;
        }
    }

    void AddPoint(Vector2 p)
    {
        if (points.Count > 0)
            pathLength += Vector2.Distance(points[^1], p);

        points.Add(p);
        lr.positionCount = points.Count;
        lr.SetPosition(points.Count - 1, p);
    }

    bool CanAttemptClose() => points.Count >= minVerticesToClose && pathLength >= minPerimeterToClose;
    bool IsClosedByDistance() => Vector2.Distance(points[0], points[^1]) <= closeThreshold;

    void CloseAndResolve()
    {
        drawing = false;
        Vector2 first = points[0];
        points[^1] = first;
        lr.SetPosition(points.Count - 1, first);

        ApplyDamageInsidePolygon(points);

        shapeActive = true;
        shapeTimer = 1f; // polygon stays briefly
    }

    void ClearLine()
    {
        lr.positionCount = 0;
        points.Clear();
        pathLength = 0f;
    }

    void EndDrawingAsCover()
    {
        drawing = false;
        Debug.Log("Created cover object!");

        shapeActive = true;
        shapeTimer = coverLifetime;

        if (points.Count >= 2)
        {
            GameObject cover = new GameObject("Cover");
            var lrCopy = cover.AddComponent<LineRenderer>();

            lrCopy.useWorldSpace = true;
            lrCopy.widthMultiplier = lineWidth;
            lrCopy.positionCount = lr.positionCount;
            lrCopy.SetPositions(GetLinePositions());

            lrCopy.material = new Material(Shader.Find("Sprites/Default"));
            lrCopy.startColor = Color.gray;
            lrCopy.endColor = Color.gray;

            var edge = cover.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.05f;
            edge.SetPoints(points);

            Destroy(cover, coverLifetime);
        }

        ClearLine();
    }

    void ApplyDamageInsidePolygon(List<Vector2> worldPoly)
    {
        if (worldPoly.Count < 3) return;

        GameObject zone = new GameObject("DamageZone");
        var poly = zone.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.pathCount = 1;

        // ✅ convert world → local before feeding collider
        Vector2[] localPoints = new Vector2[worldPoly.Count];
        for (int i = 0; i < worldPoly.Count; i++)
            localPoints[i] = zone.transform.InverseTransformPoint(worldPoly[i]);

        poly.SetPath(0, localPoints);

        var hits = new List<Collider2D>();
        poly.Overlap(ContactFilter2D.noFilter, hits);

        foreach (var hit in hits)
        {
            var eh = hit.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                Debug.Log($"[PolygonHit] Damaged {eh.name}");
                eh.TakeDamage(damageOnClose); // ✅ now uses your field
            }
        }

        Destroy(zone);
    }

    private Vector3[] GetLinePositions()
    {
        Vector3[] positions = new Vector3[lr.positionCount];
        lr.GetPositions(positions);
        return positions;
    }
}

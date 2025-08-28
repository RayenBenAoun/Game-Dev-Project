using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{ 
    public float speed = 4f;
    private Rigidbody2D rb;
    private Controls controls;
    private Vector2 moveInput;


    private static readonly Vector2 isoRight = new Vector2(1f, -0.5f).normalized;
    private static readonly Vector2 isoUp = new Vector2(1f, 0.5f).normalized;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Keyboard.current.aKey.isPressed ? -1 :
                  Keyboard.current.dKey.isPressed ? 1 : 0;

        float v = Keyboard.current.sKey.isPressed ? -1 :
                  Keyboard.current.wKey.isPressed ? 1 : 0;

        moveInput = new Vector2(h, v).normalized;

        Vector2 dir = (isoRight * moveInput.x + isoUp * moveInput.y).normalized;
        rb.linearVelocity = dir * speed;


    }
}


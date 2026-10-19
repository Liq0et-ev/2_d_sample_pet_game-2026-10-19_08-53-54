using UnityEngine;
using UnityEngine.InputSystem;

// WASD / arrow key movement with simple collision against the map.
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public Vector2 Facing { get; private set; } = Vector2.right;
    public bool Moving { get; private set; }

    static readonly Vector2 Half = new Vector2(0.35f, 0.2f);
    float bob;

    void Update()
    {
        var gm = GameManager.I;
        var kb = Keyboard.current;
        Vector2 dir = Vector2.zero;
        if (kb != null && !gm.ui.Blocking)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1;
        }
        Moving = dir.sqrMagnitude > 0.01f;
        if (Moving)
        {
            dir.Normalize();
            Facing = dir;
            Vector2 p = World.Move(transform.position, dir * speed * Time.deltaTime, Half);
            transform.position = new Vector3(p.x, p.y, 0f);
            bob += Time.deltaTime * 14f;
            if (Mathf.Abs(dir.x) > 0.1f)
                transform.localScale = new Vector3(dir.x > 0 ? 1f : -1f, 1f, 1f);
        }
        else bob = 0f;
        var body = transform.Find("Body");
        if (body != null) body.localPosition = new Vector3(0, 0.7f + Mathf.Abs(Mathf.Sin(bob)) * 0.06f, 0);
    }
}

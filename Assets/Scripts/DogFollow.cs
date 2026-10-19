using UnityEngine;

// The active dog trails the player.
public class DogFollow : MonoBehaviour
{
    public Transform player;
    public bool Moving { get; private set; }

    static readonly Vector2 Half = new Vector2(0.4f, 0.15f);
    float t;

    public void Snap()
    {
        Vector2 p = World.Move(player.position, new Vector2(-1.4f, 0), Half);
        transform.position = new Vector3(p.x, p.y, 0);
    }

    void Update()
    {
        var gm = GameManager.I;
        if (!gm.hasDog || player == null) return;

        Vector2 pos = transform.position;
        Vector2 target = player.position;
        Vector2 to = target - pos;
        float dist = to.magnitude;
        float speed = 0f;
        if (dist > 2f) speed = Mathf.Min(8.5f, 3f + (dist - 2f) * 2.5f);
        if (gm.dog.energy < 15f || gm.dog.sick) speed *= 0.6f;

        Moving = speed > 0.1f;
        if (Moving)
        {
            Vector2 step = to.normalized * speed * Time.deltaTime;
            Vector2 p = World.Move(pos, step, Half);
            transform.position = new Vector3(p.x, p.y, 0);
            if (Mathf.Abs(to.x) > 0.3f)
                transform.localScale = new Vector3(to.x > 0 ? 1f : -1f, 1f, 1f);
        }

        t += Time.deltaTime * (Moving ? 16f : 6f);
        var vis = GetComponent<DogVisual>();
        if (vis != null && vis.status != null)
            vis.status.transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        if (vis != null && vis.tail != null)
        {
            float wag = gm.dog.mood > 50f ? 28f : 8f;
            vis.tail.localRotation = Quaternion.Euler(0, 0, 35f + Mathf.Sin(t) * wag);
        }
        var body = transform.Find("Body");
        if (body != null && Moving)
            body.localPosition = new Vector3(0, 0.45f + Mathf.Abs(Mathf.Sin(t)) * 0.05f, 0);
    }
}

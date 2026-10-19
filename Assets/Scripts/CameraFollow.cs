using UnityEngine;

// Smoothly follows the player, clamped to the map.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;
        var cam = GetComponent<Camera>();
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        Vector3 t = target.position + new Vector3(0, 0.8f, 0);
        t.x = Mathf.Clamp(t.x, World.Bounds.xMin + w, Mathf.Max(World.Bounds.xMin + w, World.Bounds.xMax - w));
        t.y = Mathf.Clamp(t.y, World.Bounds.yMin + h, Mathf.Max(World.Bounds.yMin + h, World.Bounds.yMax - h));
        t.z = -10f;
        float k = 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime);
        transform.position = Vector3.Lerp(transform.position, t, k);
    }

    public void Snap()
    {
        if (target == null) return;
        var p = target.position; p.z = -10f;
        transform.position = p;
    }
}

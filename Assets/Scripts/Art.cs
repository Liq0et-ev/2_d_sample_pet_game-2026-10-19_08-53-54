using System.Collections.Generic;
using UnityEngine;

// Procedural 2D art: no imported assets are needed, everything is built from two generated sprites.
public static class Art
{
    static Sprite _square, _circle;
    static Font _font;

    public static Sprite Square { get { if (_square == null) _square = MakeSprite(false); return _square; } }
    public static Sprite Circle { get { if (_circle == null) _circle = MakeSprite(true); return _circle; } }

    static Sprite _ring;
    public static Sprite Ring
    {
        get
        {
            if (_ring == null)
            {
                int n = 256;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Clamp01((1f - d) * n * 0.5f) * Mathf.Clamp01((d - 0.93f) * n * 0.5f);
                        px[y * n + x] = new Color(1, 1, 1, a);
                    }
                tex.SetPixels(px);
                tex.Apply();
                _ring = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
            }
            return _ring;
        }
    }

    public static Font Font
    {
        get
        {
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
            return _font;
        }
    }

    static Sprite MakeSprite(bool circle)
    {
        int n = circle ? 64 : 4;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.filterMode = circle ? FilterMode.Bilinear : FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float a = 1f;
                if (circle)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f;
                    float dy = (y + 0.5f) / n * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    a = Mathf.Clamp01((1f - d) * n * 0.5f);
                }
                px[y * n + x] = new Color(1, 1, 1, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
    }

    public static SpriteRenderer Make(string name, Transform parent, bool circle, Vector2 pos, Vector2 size, Color color, int order, float rotZ = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        go.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circle ? Circle : Square;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    public static TextMesh Label(string text, Transform parent, Vector2 pos, float height, Color color, int order)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = Font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = height / 6.4f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = Font.material;
        mr.sortingOrder = order;
        return tm;
    }

    public static void LabelWithShadow(string text, Transform parent, Vector2 pos, float height, Color color, int order)
    {
        Label(text, parent, pos + new Vector2(0.06f, -0.06f), height, new Color(0, 0, 0, 0.7f), order);
        Label(text, parent, pos, height, color, order + 1);
    }

    // ---- Characters (pivot = feet) ----
    public static GameObject BuildPlayer(Transform parent)
    {
        var root = new GameObject("Player");
        root.transform.SetParent(parent, false);
        Make("Shadow", root.transform, true, new Vector2(0, 0.05f), new Vector2(0.95f, 0.35f), new Color(0, 0, 0, 0.3f), 0);
        Make("LegL", root.transform, false, new Vector2(-0.14f, 0.2f), new Vector2(0.18f, 0.4f), new Color(0.2f, 0.22f, 0.4f), 1);
        Make("LegR", root.transform, false, new Vector2(0.14f, 0.2f), new Vector2(0.18f, 0.4f), new Color(0.2f, 0.22f, 0.4f), 1);
        Make("Body", root.transform, false, new Vector2(0, 0.7f), new Vector2(0.52f, 0.55f), new Color(0.25f, 0.55f, 0.85f), 2);
        Make("ArmL", root.transform, false, new Vector2(-0.34f, 0.7f), new Vector2(0.14f, 0.45f), new Color(0.22f, 0.5f, 0.8f), 2);
        Make("ArmR", root.transform, false, new Vector2(0.34f, 0.7f), new Vector2(0.14f, 0.45f), new Color(0.22f, 0.5f, 0.8f), 2);
        Make("Head", root.transform, true, new Vector2(0, 1.2f), new Vector2(0.5f, 0.5f), new Color(1f, 0.82f, 0.68f), 3);
        Make("Hair", root.transform, true, new Vector2(0, 1.33f), new Vector2(0.52f, 0.34f), new Color(0.35f, 0.2f, 0.1f), 4);
        Make("EyeL", root.transform, true, new Vector2(-0.1f, 1.2f), new Vector2(0.07f, 0.07f), Color.black, 5);
        Make("EyeR", root.transform, true, new Vector2(0.1f, 1.2f), new Vector2(0.07f, 0.07f), Color.black, 5);
        return root;
    }

    public static GameObject BuildDog(Transform parent)
    {
        var root = new GameObject("Dog");
        root.transform.SetParent(parent, false);
        var vis = root.AddComponent<DogVisual>();
        var t = root.transform;
        Make("Shadow", t, true, new Vector2(0, 0.04f), new Vector2(1.2f, 0.35f), new Color(0, 0, 0, 0.3f), 0);
        vis.dark.Add(Make("LegA", t, false, new Vector2(-0.3f, 0.15f), new Vector2(0.14f, 0.3f), Color.white, 1));
        vis.dark.Add(Make("LegB", t, false, new Vector2(0.28f, 0.15f), new Vector2(0.14f, 0.3f), Color.white, 1));
        var tail = Make("Tail", t, false, new Vector2(-0.55f, 0.55f), new Vector2(0.14f, 0.45f), Color.white, 1, 35f);
        vis.dark.Add(tail);
        vis.tail = tail.transform;
        vis.coat.Add(Make("Body", t, true, new Vector2(0, 0.45f), new Vector2(1.15f, 0.62f), Color.white, 2));
        vis.coat.Add(Make("Head", t, true, new Vector2(0.55f, 0.72f), new Vector2(0.52f, 0.5f), Color.white, 3));
        vis.dark.Add(Make("EarA", t, true, new Vector2(0.42f, 0.98f), new Vector2(0.17f, 0.3f), Color.white, 2, 15f));
        vis.dark.Add(Make("EarB", t, true, new Vector2(0.7f, 0.98f), new Vector2(0.17f, 0.3f), Color.white, 2, -15f));
        Make("Snout", t, true, new Vector2(0.78f, 0.66f), new Vector2(0.22f, 0.18f), new Color(0.95f, 0.88f, 0.8f), 4);
        Make("Nose", t, true, new Vector2(0.88f, 0.69f), new Vector2(0.09f, 0.08f), Color.black, 5);
        Make("Eye", t, true, new Vector2(0.64f, 0.8f), new Vector2(0.08f, 0.08f), Color.black, 5);
        vis.status = Label("", t, new Vector2(0.2f, 1.6f), 0.5f, Color.white, 5000);
        root.AddComponent<YSort>();
        return root;
    }
}

public class DogVisual : MonoBehaviour
{
    public List<SpriteRenderer> coat = new List<SpriteRenderer>();
    public List<SpriteRenderer> dark = new List<SpriteRenderer>();
    public Transform tail;
    public TextMesh status;

    public void Recolor(Color c)
    {
        foreach (var r in coat) r.color = c;
        var d = c * 0.65f; d.a = 1f;
        foreach (var r in dark) r.color = d;
    }
}

// Orders sprites by Y so characters pass in front of / behind buildings and trees.
public class YSort : MonoBehaviour
{
    SpriteRenderer[] rs;
    int[] rel;

    void Start()
    {
        rs = GetComponentsInChildren<SpriteRenderer>(true);
        rel = new int[rs.Length];
        for (int i = 0; i < rs.Length; i++) rel[i] = rs[i].sortingOrder;
    }

    void LateUpdate()
    {
        if (rs == null) return;
        int b = Mathf.RoundToInt(-transform.position.y * 20f);
        for (int i = 0; i < rs.Length; i++) rs[i].sortingOrder = b + rel[i];
    }
}

using System.Collections.Generic;
using UnityEngine;

public enum ZoneType { Welfare, Training, Park, Home, Shop, Clinic }

public class ZoneInfo
{
    public ZoneType type;
    public string name;
    public string letter;
    public Vector2 center;
    public Vector2 interact;
    public Color tint;
}

// Static map data: zones, bounds and collision rectangles.
public static class World
{
    public static readonly Rect Bounds = new Rect(-31f, -19f, 62f, 38f);
    public static readonly List<Rect> Blockers = new List<Rect>();
    public static readonly Vector2 ZoneHalf = new Vector2(9.5f, 7f);

    public static readonly ZoneInfo[] Zones =
    {
        new ZoneInfo { type = ZoneType.Welfare,  name = "Labklājības centrs",   letter = "L", center = new Vector2(-20, 11),  interact = new Vector2(-20, 10.2f),  tint = new Color(0.42f, 0.68f, 0.40f) },
        new ZoneInfo { type = ZoneType.Training, name = "Treniņu laukums",      letter = "T", center = new Vector2(0, 11),    interact = new Vector2(0, 10.2f),    tint = new Color(0.50f, 0.70f, 0.36f) },
        new ZoneInfo { type = ZoneType.Park,     name = "Parks",                letter = "P", center = new Vector2(20, 11),   interact = new Vector2(20, 10.2f),   tint = new Color(0.34f, 0.66f, 0.38f) },
        new ZoneInfo { type = ZoneType.Home,     name = "Māja",                 letter = "M", center = new Vector2(-20, -11), interact = new Vector2(-20, -10.2f), tint = new Color(0.44f, 0.66f, 0.42f) },
        new ZoneInfo { type = ZoneType.Shop,     name = "Veikals",              letter = "V", center = new Vector2(0, -11),   interact = new Vector2(0, -10.2f),   tint = new Color(0.46f, 0.64f, 0.40f) },
        new ZoneInfo { type = ZoneType.Clinic,   name = "Veterinārā klīnika",   letter = "K", center = new Vector2(20, -11),  interact = new Vector2(20, -10.2f),  tint = new Color(0.40f, 0.66f, 0.44f) },
    };

    public static ZoneInfo Get(ZoneType t)
    {
        foreach (var z in Zones) if (z.type == t) return z;
        return null;
    }

    static bool Hit(Vector2 p, Vector2 half)
    {
        var r = new Rect(p.x - half.x, p.y - half.y, half.x * 2f, half.y * 2f);
        for (int i = 0; i < Blockers.Count; i++) if (Blockers[i].Overlaps(r)) return true;
        return false;
    }

    // Axis-separated movement so characters slide along walls.
    public static Vector2 Move(Vector2 pos, Vector2 delta, Vector2 half)
    {
        var p = pos;
        var nx = new Vector2(Mathf.Clamp(p.x + delta.x, Bounds.xMin + half.x, Bounds.xMax - half.x), p.y);
        if (!Hit(nx, half)) p = nx;
        var ny = new Vector2(p.x, Mathf.Clamp(p.y + delta.y, Bounds.yMin + half.y, Bounds.yMax - half.y));
        if (!Hit(ny, half)) p = ny;
        return p;
    }

    public static ZoneInfo NearestInteract(Vector2 pos, float radius)
    {
        ZoneInfo best = null;
        float bd = radius;
        foreach (var z in Zones)
        {
            float d = Vector2.Distance(pos, z.interact);
            if (d <= bd) { bd = d; best = z; }
        }
        return best;
    }

    public static ZoneInfo ZoneAt(Vector2 pos)
    {
        foreach (var z in Zones)
            if (Mathf.Abs(pos.x - z.center.x) <= ZoneHalf.x && Mathf.Abs(pos.y - z.center.y) <= ZoneHalf.y) return z;
        return null;
    }
}

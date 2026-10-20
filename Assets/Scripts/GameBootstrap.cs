using UnityEngine;

// Builds the whole game at runtime, so pressing Play in any scene starts "Gudrais suns".
public static class GameBootstrap
{
    public const string SceneName = "GudraisSuns";

    static readonly Color Grass = new Color(0.38f, 0.64f, 0.34f);
    static readonly Color PathCol = new Color(0.84f, 0.78f, 0.62f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Start()
    {
        // Only run in the dedicated scene so the other sample scenes stay untouched.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneName) return;
        if (Object.FindFirstObjectByType<GameManager>() != null) return;

        var root = new GameObject("GudraisSuns");
        var gm = root.AddComponent<GameManager>();

        // ---- world ----
        var world = new GameObject("World").transform;
        world.SetParent(root.transform, false);
        BuildMap(world);

        // ---- camera ----
        var cam = Camera.main;
        if (cam == null)
        {
            var cgo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = cgo.AddComponent<Camera>();
            cgo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.25f, 0.45f, 0.25f);
        cam.transform.rotation = Quaternion.identity;
        cam.transform.position = new Vector3(0, 0, -10);

        // ---- player ----
        var player = Art.BuildPlayer(root.transform);
        player.transform.position = new Vector3(-4f, -2.5f, 0);
        player.AddComponent<YSort>();
        player.AddComponent<PlayerMovement>();
        gm.player = player.transform;

        var follow = cam.gameObject.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        follow.Snap();

        // ---- dog (hidden until received from the welfare centre) ----
        var dogObj = Art.BuildDog(root.transform);
        gm.dogObj = dogObj;
        gm.dog = dogObj.AddComponent<DogStatus>();
        gm.dogProg = dogObj.AddComponent<DogProgress>();
        gm.dogFollow = dogObj.AddComponent<DogFollow>();
        gm.dogFollow.player = player.transform;
        dogObj.SetActive(false);

        // ---- systems ----
        gm.playerProg = root.AddComponent<PlayerProgress>();
        gm.economy = root.AddComponent<EconomyManager>();
        gm.care = root.AddComponent<CareSystem>();
        gm.training = root.AddComponent<TrainingSystem>();
        gm.shop = root.AddComponent<ShopSystem>();
        gm.stick = root.AddComponent<StickGame>();
        gm.welfare = root.AddComponent<WelfareAndAdoption>();
        root.AddComponent<InteractionController>();
        gm.audioMgr = root.AddComponent<AudioManager>();
        gm.ui = root.AddComponent<UIManager>();
        gm.audioMgr.Build();
        gm.ui.Build();

        gm.ui.OpenMenu(IntroMenu);
    }

    static MenuDef IntroMenu()
    {
        var m = new MenuDef { title = "GUDRAIS SUNS" };
        m.body = "Mierīga 2D suņu aprūpes, apmācības un menedžmenta spēle.\n\n" +
                 "Saņem suņus no dzīvnieku labklājības centra, rūpējies par tiem, iemāci komandas un sagatavo adopcijai. " +
                 "Par panākumiem saņem monētas un pieredzi centra attīstīšanai. Mērķis – 10 veiksmīgas adopcijas.\n\n" +
                 "WASD – kustība   |   E – mijiedarbība   |   Pele – izvēlnes   |   ESC – pauze";
        m.closeLabel = "Sākt spēli";
        return m;
    }

    // ------------------------------------------------------------------ map
    static Color Shade(Color c, float k) { return new Color(c.r * k, c.g * k, c.b * k, 1f); }

    static void BuildMap(Transform root)
    {
        World.Blockers.Clear();

        Art.Make("Grass", root, false, Vector2.zero, new Vector2(80, 50), Grass, -10000);
        foreach (var z in World.Zones)
            Art.Make("Area_" + z.type, root, false, z.center, new Vector2(World.ZoneHalf.x * 2, World.ZoneHalf.y * 2), z.tint, -9900);

        // paths
        Art.Make("PathH", root, false, Vector2.zero, new Vector2(62, 2.4f), PathCol, -9800);
        foreach (var z in World.Zones)
        {
            float len = Mathf.Abs(z.interact.y);
            Art.Make("PathV_" + z.type, root, false, new Vector2(z.center.x, z.interact.y * 0.5f), new Vector2(2.4f, len), PathCol, -9800);
            Art.Make("Doormat_" + z.type, root, true, z.interact, new Vector2(2.6f, 1.2f), Shade(PathCol, 0.9f), -9790);
        }

        // plaza fountain
        Art.Make("PlazaRing", root, true, Vector2.zero, new Vector2(7f, 5f), Shade(PathCol, 0.95f), -9780);
        Art.Make("FountainBase", root, true, new Vector2(0, 0.1f), new Vector2(3.4f, 2.3f), new Color(0.65f, 0.65f, 0.7f), -9770);
        Art.Make("FountainWater", root, true, new Vector2(0, 0.1f), new Vector2(2.9f, 1.9f), new Color(0.35f, 0.65f, 0.9f), -9760);
        Art.Make("FountainJet", root, false, new Vector2(0, 0.7f), new Vector2(0.18f, 0.9f), new Color(0.8f, 0.92f, 1f), 100);
        World.Blockers.Add(new Rect(-1.4f, -0.7f, 2.8f, 1.5f));
        Art.LabelWithShadow("Centrs", root, new Vector2(0, -1.9f), 0.55f, new Color(1, 1, 1, 0.9f), -9000);

        // buildings
        MakeBuilding(root, World.Get(ZoneType.Welfare), new Vector2(7.5f, 4.2f), new Color(0.93f, 0.88f, 0.76f), new Color(0.35f, 0.55f, 0.75f));
        MakeBuilding(root, World.Get(ZoneType.Home), new Vector2(6.5f, 4.2f), new Color(0.96f, 0.85f, 0.7f), new Color(0.75f, 0.3f, 0.25f));
        MakeBuilding(root, World.Get(ZoneType.Shop), new Vector2(7f, 4.2f), new Color(0.95f, 0.9f, 0.6f), new Color(0.85f, 0.55f, 0.2f));
        MakeBuilding(root, World.Get(ZoneType.Clinic), new Vector2(7.5f, 4.2f), new Color(0.95f, 0.95f, 0.97f), new Color(0.8f, 0.2f, 0.25f));
        BuildTraining(root, World.Get(ZoneType.Training));
        BuildPark(root, World.Get(ZoneType.Park));

        // trees
        var rng = new System.Random(7);
        int placed = 0, tries = 0;
        while (placed < 70 && tries < 2000)
        {
            tries++;
            float x = (float)(rng.NextDouble() * 60 - 30);
            float y = (float)(rng.NextDouble() * 36 - 18);
            if (TreeBlocked(new Vector2(x, y))) continue;
            Tree(root, new Vector2(x, y));
            placed++;
        }
    }

    static bool TreeBlocked(Vector2 p)
    {
        if (Mathf.Abs(p.y) < 3.2f) return true;                      // central road / plaza
        foreach (var z in World.Zones)
        {
            if (Mathf.Abs(p.x - z.center.x) < 2.6f) return true;       // vertical paths
            if (Vector2.Distance(p, z.interact) < 4f) return true;
        }
        foreach (var b in World.Blockers)
        {
            var r = new Rect(b.xMin - 2f, b.yMin - 2f, b.width + 4f, b.height + 4f);
            if (r.Contains(p)) return true;
        }
        // keep training field and park pond free
        var tz = World.Get(ZoneType.Training);
        if (Mathf.Abs(p.x - tz.center.x) < 7.5f && Mathf.Abs(p.y - tz.center.y) < 5f) return true;
        return false;
    }

    static void Tree(Transform root, Vector2 p)
    {
        int b = Mathf.RoundToInt(-p.y * 20f);
        var tree = new GameObject("Tree").transform;
        tree.SetParent(root, false);
        tree.localPosition = p;
        Art.Make("Shadow", tree, true, new Vector2(0, 0), new Vector2(1.6f, 0.5f), new Color(0, 0, 0, 0.25f), -9500);
        Art.Make("Trunk", tree, false, new Vector2(0, 0.4f), new Vector2(0.35f, 0.8f), new Color(0.4f, 0.27f, 0.15f), b);
        Art.Make("Canopy", tree, true, new Vector2(0, 1.5f), new Vector2(2.1f, 2f), new Color(0.2f, 0.5f, 0.25f), b + 1);
        Art.Make("CanopyHi", tree, true, new Vector2(-0.3f, 1.8f), new Vector2(1.2f, 1.1f), new Color(0.28f, 0.6f, 0.3f), b + 2);
        World.Blockers.Add(new Rect(p.x - 0.3f, p.y - 0.1f, 0.6f, 0.5f));
    }

    static void MakeBuilding(Transform root, ZoneInfo z, Vector2 size, Color wall, Color roof)
    {
        bool top = z.center.y > 0;
        Vector2 c = new Vector2(z.center.x, top ? 14f : -14f);
        int b = Mathf.RoundToInt(-(c.y - size.y * 0.5f) * 20f);
        var g = new GameObject("Building_" + z.type).transform;
        g.SetParent(root, false);
        g.localPosition = c;

        Art.Make("Shadow", g, false, new Vector2(0.4f, -size.y * 0.5f - 0.1f), new Vector2(size.x + 0.5f, 0.5f), new Color(0, 0, 0, 0.2f), -9500);
        Art.Make("Wall", g, false, Vector2.zero, size, wall, b);
        Art.Make("Roof", g, false, new Vector2(0, size.y * 0.22f), new Vector2(size.x + 0.4f, size.y * 0.56f), roof, b + 1);
        Art.Make("RoofEdge", g, false, new Vector2(0, -size.y * 0.06f), new Vector2(size.x + 0.4f, 0.14f), Shade(roof, 0.7f), b + 2);
        Art.Make("Door", g, false, new Vector2(0, -size.y * 0.5f + 0.7f), new Vector2(1.0f, 1.4f), new Color(0.4f, 0.26f, 0.15f), b + 2);
        Art.Make("WinL", g, false, new Vector2(-size.x * 0.3f, -size.y * 0.5f + 1.1f), new Vector2(0.9f, 0.8f), new Color(0.7f, 0.88f, 1f), b + 2);
        Art.Make("WinR", g, false, new Vector2(size.x * 0.3f, -size.y * 0.5f + 1.1f), new Vector2(0.9f, 0.8f), new Color(0.7f, 0.88f, 1f), b + 2);

        // signs
        if (z.type == ZoneType.Clinic)
        {
            Art.Make("CrossV", g, false, new Vector2(0, size.y * 0.22f), new Vector2(0.4f, 1.3f), Color.white, b + 3);
            Art.Make("CrossH", g, false, new Vector2(0, size.y * 0.22f), new Vector2(1.3f, 0.4f), Color.white, b + 3);
        }
        if (z.type == ZoneType.Shop)
            Art.Make("Awning", g, false, new Vector2(0, -size.y * 0.5f + 1.8f), new Vector2(size.x * 0.8f, 0.35f), new Color(0.9f, 0.3f, 0.3f), b + 3);
        if (z.type == ZoneType.Welfare)
            Art.Make("Paw", g, true, new Vector2(0, size.y * 0.22f), new Vector2(0.9f, 0.8f), new Color(1f, 0.95f, 0.8f), b + 3);

        World.Blockers.Add(new Rect(c.x - size.x * 0.5f, c.y - size.y * 0.5f, size.x, size.y));
        float labelY = top ? c.y + size.y * 0.5f + 0.9f : c.y + size.y * 0.5f + 0.9f;
        Art.LabelWithShadow(z.name, root, new Vector2(c.x, labelY), 0.7f, Color.white, 8000);
    }

    static void BuildTraining(Transform root, ZoneInfo z)
    {
        var c = z.center;
        Art.Make("Field", root, false, new Vector2(c.x, c.y + 2f), new Vector2(15f, 8.4f), new Color(0.78f, 0.72f, 0.5f), -9700);
        Art.Make("FieldIn", root, false, new Vector2(c.x, c.y + 2f), new Vector2(14.2f, 7.6f), new Color(0.55f, 0.74f, 0.4f), -9690);
        // fence
        Color fence = new Color(0.95f, 0.95f, 0.9f);
        Art.Make("FenceT", root, false, new Vector2(c.x, c.y + 6.2f), new Vector2(15f, 0.18f), fence, -9680);
        Art.Make("FenceL", root, false, new Vector2(c.x - 7.5f, c.y + 2f), new Vector2(0.18f, 8.4f), fence, -9680);
        Art.Make("FenceR", root, false, new Vector2(c.x + 7.5f, c.y + 2f), new Vector2(0.18f, 8.4f), fence, -9680);
        // cones
        for (int i = 0; i < 5; i++)
        {
            var p = new Vector2(c.x - 5f + i * 2.5f, c.y + 3.6f);
            Art.Make("Cone", root, true, p + new Vector2(0, 0.25f), new Vector2(0.6f, 0.7f), new Color(1f, 0.5f, 0.1f), Mathf.RoundToInt(-p.y * 20f));
            Art.Make("ConeStripe", root, false, p + new Vector2(0, 0.3f), new Vector2(0.4f, 0.1f), Color.white, Mathf.RoundToInt(-p.y * 20f) + 1);
        }
        // hurdle and tunnel
        Art.Make("HurdleBar", root, false, new Vector2(c.x - 4f, c.y + 1.4f), new Vector2(2.4f, 0.14f), Color.white, -9000);
        Art.Make("HurdleL", root, false, new Vector2(c.x - 5.1f, c.y + 1.2f), new Vector2(0.14f, 0.7f), new Color(0.8f, 0.3f, 0.3f), -9000);
        Art.Make("HurdleR", root, false, new Vector2(c.x - 2.9f, c.y + 1.2f), new Vector2(0.14f, 0.7f), new Color(0.8f, 0.3f, 0.3f), -9000);
        Art.Make("Tunnel", root, true, new Vector2(c.x + 4f, c.y + 1.6f), new Vector2(3.2f, 1.7f), new Color(0.3f, 0.45f, 0.75f), -9000);
        Art.Make("TunnelHole", root, true, new Vector2(c.x + 4.9f, c.y + 1.6f), new Vector2(0.8f, 1.2f), new Color(0.08f, 0.1f, 0.2f), -8999);
        Art.Make("Sign", root, false, z.interact + new Vector2(2.2f, 0.4f), new Vector2(1.6f, 0.9f), new Color(0.5f, 0.35f, 0.2f), -8000);
        Art.LabelWithShadow(z.name, root, new Vector2(c.x, c.y + 6.9f), 0.7f, Color.white, 8000);
    }

    static void BuildPark(Transform root, ZoneInfo z)
    {
        var c = z.center;
        var pond = new Vector2(c.x + 4f, c.y + 3.2f);
        Art.Make("PondRim", root, true, pond, new Vector2(7.4f, 4.4f), new Color(0.75f, 0.7f, 0.5f), -9700);
        Art.Make("Pond", root, true, pond, new Vector2(6.8f, 3.9f), new Color(0.3f, 0.6f, 0.85f), -9690);
        Art.Make("Lily", root, true, pond + new Vector2(-1f, 0.3f), new Vector2(0.7f, 0.45f), new Color(0.3f, 0.7f, 0.35f), -9680);
        World.Blockers.Add(new Rect(pond.x - 2.8f, pond.y - 1.5f, 5.6f, 3f));
        // bench at the interaction point
        Art.Make("BenchSeat", root, false, z.interact + new Vector2(-2.6f, 0.2f), new Vector2(1.8f, 0.4f), new Color(0.55f, 0.37f, 0.2f), -8000);
        Art.Make("BenchBack", root, false, z.interact + new Vector2(-2.6f, 0.55f), new Vector2(1.8f, 0.2f), new Color(0.45f, 0.3f, 0.16f), -7999);
        // flowers
        var rng = new System.Random(5);
        Color[] fl = { new Color(1f, 0.5f, 0.6f), new Color(1f, 0.9f, 0.3f), new Color(0.8f, 0.6f, 1f), Color.white };
        for (int i = 0; i < 40; i++)
        {
            var p = new Vector2(c.x + (float)(rng.NextDouble() * 18 - 9), c.y + (float)(rng.NextDouble() * 12 - 4));
            if (Mathf.Abs(p.x - c.x) < 1.6f) continue;
            if (Vector2.Distance(p, pond) < 3.9f) continue;
            Art.Make("Flower", root, true, p, new Vector2(0.22f, 0.22f), fl[i % 4], -9600);
        }
        Art.LabelWithShadow(z.name, root, new Vector2(c.x, c.y + 6.4f), 0.7f, Color.white, 8000);
    }
}

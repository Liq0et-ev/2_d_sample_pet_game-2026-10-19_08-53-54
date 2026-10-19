using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MenuItem
{
    public string label;
    public Action onClick;
    public bool enabled;
    public MenuItem(string l, Action a, bool e = true) { label = l; onClick = a; enabled = e; }
}

public class MenuDef
{
    public string title = "";
    public string body = "";
    public List<MenuItem> items = new List<MenuItem>();
    public bool showClose = true;
    public string closeLabel = "Aizvērt";
}

// HUD, minimap, toasts and a generic modal menu built entirely from code.
public class UIManager : MonoBehaviour
{
    class Bar { public Image fill; public Text label; public bool goodHigh; public string name; }

    static readonly Vector2 TL = new Vector2(0, 1), TR = new Vector2(1, 1), BL = new Vector2(0, 0),
        BC = new Vector2(0.5f, 0), TC = new Vector2(0.5f, 1), C = new Vector2(0.5f, 0.5f);

    Font font;
    Canvas canvas;
    Bar bHealth, bHunger, bEnergy, bMood;
    Text dogLine, coinsText, levelText, adoptText, questText, promptText, toastText, noDogText;
    RectTransform dotPlayer, dotDog;
    GameObject dogPanel;
    Image fadeImg;

    GameObject menuRoot;
    Text menuTitle, menuBody;
    RectTransform menuPanel;
    readonly List<GameObject> menuButtons = new List<GameObject>();
    Func<MenuDef> builder;

    readonly List<KeyValuePair<string, float>> toasts = new List<KeyValuePair<string, float>>();

    public int modalCount;
    public Action onModalCancel;
    public bool Blocking { get { return (menuRoot != null && menuRoot.activeSelf) || modalCount > 0; } }
    public bool MenuOpen { get { return menuRoot != null && menuRoot.activeSelf; } }

    // ---------- construction helpers (public so minigames can reuse them) ----------
    public RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public Image NewImage(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color c)
    {
        var rt = NewRect(name, parent, anchor, pivot, pos, size);
        var im = rt.gameObject.AddComponent<Image>();
        im.color = c;
        im.raycastTarget = false;
        return im;
    }

    public Text NewText(Transform parent, string s, int size, TextAnchor al, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 sz)
    {
        var rt = NewRect("Text", parent, anchor, pivot, pos, sz);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.fontSize = size; t.alignment = al; t.color = c; t.text = s;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    public Button NewButton(Transform parent, string label, Action cb, Vector2 anchor, Vector2 pos, Vector2 size, bool enabled = true)
    {
        var rt = NewRect("Button", parent, anchor, new Vector2(0.5f, 0.5f), pos, size);
        StyleButton(rt.gameObject, label, cb, enabled);
        return rt.GetComponent<Button>();
    }

    void StyleButton(GameObject go, string label, Action cb, bool enabled)
    {
        var im = go.AddComponent<Image>();
        im.color = Color.white;
        var b = go.AddComponent<Button>();
        b.targetGraphic = im;
        var cols = b.colors;
        cols.normalColor = new Color(0.22f, 0.45f, 0.62f);
        cols.highlightedColor = new Color(0.3f, 0.58f, 0.78f);
        cols.pressedColor = new Color(0.15f, 0.32f, 0.46f);
        cols.disabledColor = new Color(0.25f, 0.25f, 0.28f, 0.8f);
        b.colors = cols;
        b.interactable = enabled;
        var t = NewText(go.transform, label, 26, TextAnchor.MiddleCenter, enabled ? Color.white : new Color(0.6f, 0.6f, 0.6f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var rt = t.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10, 4); rt.offsetMax = new Vector2(-10, -4);
        if (cb != null) b.onClick.AddListener(() => cb());
    }

    // Full-screen dimmed modal with a centred panel; caller destroys the returned root.
    public GameObject CreateModal(Vector2 size, out RectTransform panel)
    {
        var dim = NewImage("Modal", canvas.transform, C, C, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.5f));
        dim.rectTransform.anchorMin = Vector2.zero; dim.rectTransform.anchorMax = Vector2.one;
        dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
        dim.raycastTarget = true;
        panel = NewRect("Panel", dim.transform, C, C, Vector2.zero, size);
        var bg = panel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.14f, 0.2f, 0.96f);
        modalCount++;
        return dim.gameObject;
    }

    public void CloseModal(GameObject root)
    {
        if (root != null) Destroy(root);
        modalCount = Mathf.Max(0, modalCount - 1);
        onModalCancel = null;
    }

    // ---------- build ----------
    public void Build()
    {
        font = Art.Font;
        var cgo = new GameObject("Canvas");
        canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var sc = cgo.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        BuildHud();
        BuildMenu();

        fadeImg = NewImage("Fade", canvas.transform, C, C, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
        fadeImg.rectTransform.anchorMin = Vector2.zero; fadeImg.rectTransform.anchorMax = Vector2.one;
        fadeImg.rectTransform.offsetMin = fadeImg.rectTransform.offsetMax = Vector2.zero;
    }

    Bar MakeBar(Transform parent, string name, bool goodHigh, Vector2 pos)
    {
        var bg = NewImage(name + "BG", parent, TL, TL, pos, new Vector2(330, 34), new Color(0, 0, 0, 0.6f));
        var frt = NewRect("Fill", bg.transform, Vector2.zero, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(2, 2); frt.offsetMax = new Vector2(-2, -2);
        var fill = frt.gameObject.AddComponent<Image>();
        fill.raycastTarget = false;
        var lbl = NewText(bg.transform, name, 20, TextAnchor.MiddleCenter, Color.white, C, C, Vector2.zero, new Vector2(330, 34));
        return new Bar { fill = fill, label = lbl, goodHigh = goodHigh, name = name };
    }

    void SetBar(Bar b, float v)
    {
        v = Mathf.Clamp(v, 0f, 100f);
        var rt = b.fill.rectTransform;
        rt.anchorMax = new Vector2(v / 100f, 1f);
        float good = b.goodHigh ? v / 100f : 1f - v / 100f;
        b.fill.color = Color.Lerp(new Color(0.85f, 0.2f, 0.2f), new Color(0.3f, 0.8f, 0.35f), good);
        b.label.text = b.name + "  " + Mathf.RoundToInt(v);
    }

    void BuildHud()
    {
        var white = Color.white;
        dogPanel = NewImage("DogPanel", canvas.transform, TL, TL, new Vector2(20, -20), new Vector2(720, 170), new Color(0, 0, 0, 0.5f)).gameObject;
        dogLine = NewText(dogPanel.transform, "", 28, TextAnchor.MiddleLeft, white, TL, TL, new Vector2(16, -8), new Vector2(690, 40));
        bHealth = MakeBar(dogPanel.transform, "Veselība", true, new Vector2(16, -58));
        bHunger = MakeBar(dogPanel.transform, "Izsalkums", false, new Vector2(374, -58));
        bEnergy = MakeBar(dogPanel.transform, "Enerģija", true, new Vector2(16, -108));
        bMood = MakeBar(dogPanel.transform, "Garastāvoklis", true, new Vector2(374, -108));
        noDogText = NewText(canvas.transform, "Nav aktīvā suņa", 28, TextAnchor.MiddleLeft, white, TL, TL, new Vector2(36, -30), new Vector2(600, 40));

        var stats = NewImage("Stats", canvas.transform, TR, TR, new Vector2(-20, -20), new Vector2(380, 120), new Color(0, 0, 0, 0.5f));
        coinsText = NewText(stats.transform, "", 30, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.3f), TL, TL, new Vector2(16, -6), new Vector2(350, 40));
        levelText = NewText(stats.transform, "", 24, TextAnchor.MiddleLeft, white, TL, TL, new Vector2(16, -46), new Vector2(350, 34));
        adoptText = NewText(stats.transform, "", 24, TextAnchor.MiddleLeft, white, TL, TL, new Vector2(16, -80), new Vector2(350, 34));

        var mm = NewImage("Minimap", canvas.transform, TR, TR, new Vector2(-20, -150), new Vector2(380, 232), new Color(0.2f, 0.36f, 0.2f, 0.85f));
        float mw = 380, mh = 232;
        foreach (var z in World.Zones)
        {
            var r = NewImage("Z", mm.transform, BL, C, MapPos(z.center, mw, mh), new Vector2(64, 40), new Color(0.12f, 0.2f, 0.12f, 0.8f));
            NewText(r.transform, z.letter, 22, TextAnchor.MiddleCenter, white, C, C, Vector2.zero, new Vector2(64, 40));
        }
        NewImage("PathH", mm.transform, BL, C, MapPos(Vector2.zero, mw, mh), new Vector2(mw - 10, 8), new Color(0.84f, 0.78f, 0.62f, 0.6f)).transform.SetAsFirstSibling();
        dotDog = NewImage("DotDog", mm.transform, BL, C, Vector2.zero, new Vector2(12, 12), new Color(1f, 0.6f, 0.15f)).rectTransform;
        dotPlayer = NewImage("DotPlayer", mm.transform, BL, C, Vector2.zero, new Vector2(14, 14), Color.white).rectTransform;

        var qp = NewImage("Quest", canvas.transform, TC, TC, new Vector2(0, -20), new Vector2(760, 90), new Color(0, 0, 0, 0.5f));
        NewText(qp.transform, "UZDEVUMS", 18, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.3f), TL, TL, new Vector2(14, -6), new Vector2(300, 24));
        questText = NewText(qp.transform, "", 24, TextAnchor.UpperLeft, white, TL, TL, new Vector2(14, -28), new Vector2(732, 60));

        promptText = NewText(canvas.transform, "", 36, TextAnchor.MiddleCenter, white, BC, BC, new Vector2(0, 70), new Vector2(900, 50));
        toastText = NewText(canvas.transform, "", 28, TextAnchor.LowerCenter, new Color(1f, 0.95f, 0.7f), BC, BC, new Vector2(0, 130), new Vector2(1100, 160));
        NewText(canvas.transform, "WASD – kustība   |   E – mijiedarbība   |   ESC – pauze", 20, TextAnchor.LowerLeft,
            new Color(1, 1, 1, 0.8f), BL, BL, new Vector2(20, 14), new Vector2(900, 30));
    }

    Vector2 MapPos(Vector2 w, float mw, float mh)
    {
        float u = (w.x - World.Bounds.xMin) / World.Bounds.width;
        float v = (w.y - World.Bounds.yMin) / World.Bounds.height;
        return new Vector2(5 + u * (mw - 10), 5 + v * (mh - 10));
    }

    void BuildMenu()
    {
        var dim = NewImage("MenuDim", canvas.transform, C, C, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.55f));
        dim.rectTransform.anchorMin = Vector2.zero; dim.rectTransform.anchorMax = Vector2.one;
        dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
        dim.raycastTarget = true;
        menuRoot = dim.gameObject;

        menuPanel = NewRect("Panel", dim.transform, C, C, Vector2.zero, new Vector2(900, 100));
        var bg = menuPanel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.14f, 0.2f, 0.97f);
        var vl = menuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(32, 32, 28, 28);
        vl.spacing = 12;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childControlWidth = true; vl.childControlHeight = true;
        vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
        var fit = menuPanel.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        menuTitle = NewText(menuPanel, "", 42, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f), C, C, Vector2.zero, Vector2.zero);
        menuTitle.fontStyle = FontStyle.Bold;
        menuBody = NewText(menuPanel, "", 26, TextAnchor.UpperLeft, Color.white, C, C, Vector2.zero, Vector2.zero);
        menuRoot.SetActive(false);
    }

    // ---------- menu API ----------
    public void OpenMenu(Func<MenuDef> build)
    {
        builder = build;
        menuRoot.SetActive(true);
        Rebuild();
    }

    public void RefreshMenu() { if (MenuOpen && builder != null) Rebuild(); }

    public void CloseMenu()
    {
        menuRoot.SetActive(false);
        builder = null;
    }

    void Rebuild()
    {
        var def = builder();
        menuTitle.text = def.title;
        menuBody.text = def.body;
        menuBody.gameObject.SetActive(!string.IsNullOrEmpty(def.body));
        foreach (var b in menuButtons) Destroy(b);
        menuButtons.Clear();

        foreach (var it in def.items)
        {
            var item = it;
            AddMenuButton(item.label, () =>
            {
                GameManager.I.audioMgr.Play(Sfx.Click);
                if (item.onClick != null) item.onClick();
                if (MenuOpen && builder != null) Rebuild();
            }, item.enabled);
        }
        if (def.showClose)
            AddMenuButton(def.closeLabel, () => { GameManager.I.audioMgr.Play(Sfx.Click); CloseMenu(); }, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuPanel);
    }

    void AddMenuButton(string label, Action cb, bool enabled)
    {
        var go = new GameObject("Btn", typeof(RectTransform));
        go.transform.SetParent(menuPanel, false);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 56; le.preferredHeight = 56;
        StyleButton(go, label, cb, enabled);
        menuButtons.Add(go);
    }

    // ---------- toasts / prompt / fade ----------
    public void Toast(string msg)
    {
        toasts.Add(new KeyValuePair<string, float>(msg, Time.unscaledTime + 4.5f));
        if (toasts.Count > 4) toasts.RemoveAt(0);
    }

    public void SetPrompt(string s) { if (promptText != null) promptText.text = s; }

    public void Fade(float seconds = 0.9f) { StartCoroutine(FadeRoutine(seconds)); }

    IEnumerator FadeRoutine(float s)
    {
        float t = 0f;
        while (t < s)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Sin(Mathf.Clamp01(t / s) * Mathf.PI);
            fadeImg.color = new Color(0, 0, 0, a);
            yield return null;
        }
        fadeImg.color = new Color(0, 0, 0, 0);
    }

    // ---------- pause ----------
    MenuDef PauseMenu()
    {
        var m = new MenuDef { title = "Pauze" };
        m.body = "Vadība: WASD – kustība, E – mijiedarbība, pele – izvēlnes, ESC – pauze.\n" +
                 "Suns automātiski seko tev. Rūpējies par tā vajadzībām, trenē komandas un sagatavo adopcijai.";
        m.closeLabel = "Turpināt";
        m.items.Add(new MenuItem("Skaņa: " + (GameManager.I.audioMgr.muted ? "izslēgta" : "ieslēgta"),
            () => GameManager.I.audioMgr.muted = !GameManager.I.audioMgr.muted));
        m.items.Add(new MenuItem("Iziet no spēles", () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
        return m;
    }

    // ---------- per-frame ----------
    void Update()
    {
        var gm = GameManager.I;
        Time.timeScale = Blocking ? 0f : 1f;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (modalCount > 0) { var cb = onModalCancel; if (cb != null) cb(); }
            else if (MenuOpen) CloseMenu();
            else OpenMenu(PauseMenu);
        }

        // toasts
        toasts.RemoveAll(k => k.Value < Time.unscaledTime);
        var sb = new System.Text.StringBuilder();
        foreach (var k in toasts) { if (sb.Length > 0) sb.Append('\n'); sb.Append(k.Key); }
        toastText.text = sb.ToString();

        // HUD
        bool hd = gm.hasDog;
        dogPanel.SetActive(hd);
        noDogText.gameObject.SetActive(!hd);
        if (hd)
        {
            var d = gm.dog;
            dogLine.text = d.dogName + "  (" + DogStatus.TemperamentName(d.temperament) + ")  –  " + gm.dogProg.level + ". līmenis" +
                           (d.sick ? "   [SLIMS]" : "");
            SetBar(bHealth, d.health);
            SetBar(bHunger, d.Hunger);
            SetBar(bEnergy, d.energy);
            SetBar(bMood, d.mood);
        }
        var e = gm.economy;
        coinsText.text = "Monētas: " + e.coins + (e.lastPassive > 0 ? "  (+" + e.lastPassive + ")" : "");
        levelText.text = "Spēlētājs: " + gm.playerProg.level + ". līm.  (" + gm.playerProg.xp + "/" + gm.playerProg.XpToNext + ")";
        adoptText.text = "Adopcijas: " + gm.adoptions + "/" + GameManager.WinAdoptions;
        questText.text = gm.GetQuest();

        float mw = 380, mh = 232;
        dotPlayer.anchoredPosition = MapPos(gm.player.position, mw, mh);
        dotDog.gameObject.SetActive(hd);
        if (hd) dotDog.anchoredPosition = MapPos(gm.dogObj.transform.position, mw, mh);
    }
}

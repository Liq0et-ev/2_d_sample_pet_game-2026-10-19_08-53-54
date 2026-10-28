using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// ---------------------------------------------------------------------
// Izvelnes datu klases
// ---------------------------------------------------------------------

// Viena izvelnes poga. Ir divi veidi:
//   1) poga bez parametra:  new IzvelnesPoga("Teksts", MansMetode)
//   2) poga ar skaitli:     new IzvelnesPoga("Teksts", MansMetode, 3)   (metode saņem skaitli 3)
public class IzvelnesPoga
{
    public string uzraksts;
    public Action darbiba;                    // ko darit, nospiezot (bez parametra)
    public Action<int> darbibaArParametru;    // ko darit, nospiezot (ar skaitli)
    public int parametrs;
    public bool iespejota;                    // ja false, poga ir pelēka un to nevar nospiest

    public IzvelnesPoga(string jaunsUzraksts, Action jaunaDarbiba, bool jaunaIespejota = true)
    {
        uzraksts = jaunsUzraksts;
        darbiba = jaunaDarbiba;
        iespejota = jaunaIespejota;
    }

    public IzvelnesPoga(string jaunsUzraksts, Action<int> jaunaDarbiba, int jaunsParametrs, bool jaunaIespejota = true)
    {
        uzraksts = jaunsUzraksts;
        darbibaArParametru = jaunaDarbiba;
        parametrs = jaunsParametrs;
        iespejota = jaunaIespejota;
    }
}

// Visas izvelnes apraksts: virsraksts, teksts un pogu saraksts
public class IzvelnesApraksts
{
    public string virsraksts = "";
    public string teksts = "";
    public List<IzvelnesPoga> pogas = new List<IzvelnesPoga>();
    public bool rakstitAizvertPogu = true;
    public string aizvertPogasTeksts = "Aizvērt";
}

// Mazs skripts, kas pie katras izvelnes pogas pasaka UIManager, ka poga ir nospiesta
public class PogasKlikskis : MonoBehaviour
{
    public IzvelnesPoga poga;
    public UIManager saskarne;

    public void Nospiests()
    {
        saskarne.ApstradatPogasKlikski(poga);
    }
}

// Maza klase vienam pazinojumam ekrana
public class PazinojumaIeraksts
{
    public string teksts;
    public float beigasLaiks;
}

// Klase vienai statusa joslai (veseliba, izsalkums utt.)
public class RadaJosla
{
    public Image aizpildijums;
    public Text uzraksts;
    public bool labiAugsta;       // true: augsta vertiba ir laba (zala). false: augsta vertiba ir slikta (sarkana).
    public string nosaukums;
}

// ---------------------------------------------------------------------
// UIManager: spēles saskarne (HUD, minikarte, pazinojumi un izvelnes).
// Viss tiek uzbuvēts ar kodu, nevajag veidot neko Unity redaktora.
// ---------------------------------------------------------------------
public class UIManager : MonoBehaviour
{
    // Enkuru punkti (kur ekrana piestiprinam elementu)
    static Vector2 augsaKreisi = new Vector2(0, 1);
    static Vector2 augsaLabi = new Vector2(1, 1);
    static Vector2 augsaVidu = new Vector2(0.5f, 1);
    static Vector2 apaksaKreisi = new Vector2(0, 0);
    static Vector2 apaksaVidu = new Vector2(0.5f, 0);
    static Vector2 centrs = new Vector2(0.5f, 0.5f);

    Font burtuFonts;
    Canvas audekls;

    // Suna statusa joslas
    RadaJosla veselibasJosla;
    RadaJosla izsalkumaJosla;
    RadaJosla energijasJosla;
    RadaJosla garastavoklaJosla;

    // Teksti
    Text sunaRinda;
    Text monetuTeksts;
    Text limenaTeksts;
    Text adopcijuTeksts;
    Text uzdevumaTeksts;
    Text uzaicinajumaTeksts;
    Text pazinojumuTeksts;
    Text navSunaTeksts;

    // Minikarte
    RectTransform speletajaPunkts;
    RectTransform sunaPunkts;
    float kartesPlatums = 380f;
    float kartesAugstums = 232f;

    GameObject sunaPaneli;
    Image aptumsaAttels;

    // Trieciena pauze: ļoti īsa spēles apstāšanās spēcīgā brīdī
    float laikaPauzesTaimeris = 0f;

    // Aptumsuma (fade) animacija
    float aptumsaTaimeris = 0f;
    float aptumsaIlgums = 0f;

    // Izvelne
    GameObject izvelnesSakne;
    Text izvelnesVirsraksts;
    Text izvelnesTeksts;
    RectTransform izvelnesPanelis;
    List<GameObject> izvelnesPogas = new List<GameObject>();
    Func<IzvelnesApraksts> izvelnesBuvetajs;      // funkcija, kas uzbuvē izvelnes saturu

    List<PazinojumaIeraksts> pazinojumi = new List<PazinojumaIeraksts>();

    // Atverto "logu" skaits (piemeram, minispeles) un ko darit, nospiezot ESC
    public int logaSkaits = 0;
    public Action logaAtcelsana;

    // Uz brīdi (sekundēs) apstādina spēli, lai radītu trieciena sajūtu
    public void ApstadinatLaiku(float sekundes)
    {
        laikaPauzesTaimeris = Mathf.Max(laikaPauzesTaimeris, sekundes);
    }

    // Vai spele ir apstadinata (izvelne vai minispele ir atverta)?
    public bool IrBloketa()
    {
        if (izvelnesSakne != null && izvelnesSakne.activeSelf)
        {
            return true;
        }
        return logaSkaits > 0;
    }

    public bool IzvelneAtverta()
    {
        return izvelnesSakne != null && izvelnesSakne.activeSelf;
    }

    // =================================================================
    // 1. Palīgfunkcijas elementu veidošanai
    // =================================================================

    // Izveido tukšu taisnstūri (RectTransform) ar dotajiem iestatijumiem
    public RectTransform IzveidotTaisnstuli(string nosaukums, Transform vecaks, Vector2 enkurs,
        Vector2 asispunkts, Vector2 pozicija, Vector2 izmers)
    {
        GameObject objekts = new GameObject(nosaukums, typeof(RectTransform));
        RectTransform taisnstulis = objekts.GetComponent<RectTransform>();
        taisnstulis.SetParent(vecaks, false);
        taisnstulis.anchorMin = enkurs;
        taisnstulis.anchorMax = enkurs;
        taisnstulis.pivot = asispunkts;
        taisnstulis.anchoredPosition = pozicija;
        taisnstulis.sizeDelta = izmers;
        return taisnstulis;
    }

    // Izveido krasainu attelu (laukumu)
    public Image IzveidotAttelu(string nosaukums, Transform vecaks, Vector2 enkurs, Vector2 asispunkts,
        Vector2 pozicija, Vector2 izmers, Color krasa)
    {
        RectTransform taisnstulis = IzveidotTaisnstuli(nosaukums, vecaks, enkurs, asispunkts, pozicija, izmers);
        Image attels = taisnstulis.gameObject.AddComponent<Image>();
        attels.color = krasa;
        attels.raycastTarget = false;
        return attels;
    }

    // Izveido tekstu ekrana
    public Text IzveidotTekstu(Transform vecaks, string saturs, int burtuIzmers, TextAnchor izlidzinajums,
        Color krasa, Vector2 enkurs, Vector2 asispunkts, Vector2 pozicija, Vector2 izmers)
    {
        RectTransform taisnstulis = IzveidotTaisnstuli("Teksts", vecaks, enkurs, asispunkts, pozicija, izmers);
        Text teksts = taisnstulis.gameObject.AddComponent<Text>();
        teksts.font = burtuFonts;
        teksts.fontSize = burtuIzmers;
        teksts.alignment = izlidzinajums;
        teksts.color = krasa;
        teksts.text = saturs;
        teksts.raycastTarget = false;
        teksts.horizontalOverflow = HorizontalWrapMode.Wrap;
        teksts.verticalOverflow = VerticalWrapMode.Overflow;
        return teksts;
    }

    // Izveido pogu (to izmanto minispeles)
    public Button IzveidotPogu(Transform vecaks, string uzraksts, UnityAction darbiba, Vector2 enkurs,
        Vector2 pozicija, Vector2 izmers, bool iespejota = true)
    {
        RectTransform taisnstulis = IzveidotTaisnstuli("Poga", vecaks, enkurs, centrs, pozicija, izmers);
        NoformetPogu(taisnstulis.gameObject, uzraksts, darbiba, iespejota);
        return taisnstulis.GetComponent<Button>();
    }

    // Pievieno objektam pogas izskatu un uzrakstu
    void NoformetPogu(GameObject objekts, string uzraksts, UnityAction darbiba, bool iespejota)
    {
        Image fons = objekts.AddComponent<Image>();
        fons.color = Color.white;

        Button poga = objekts.AddComponent<Button>();
        poga.targetGraphic = fons;

        // Pogas krasas dažādos stāvokļos
        ColorBlock krasas = poga.colors;
        krasas.normalColor = new Color(0.22f, 0.45f, 0.62f);
        krasas.highlightedColor = new Color(0.3f, 0.58f, 0.78f);
        krasas.pressedColor = new Color(0.15f, 0.32f, 0.46f);
        krasas.disabledColor = new Color(0.25f, 0.25f, 0.28f, 0.8f);
        poga.colors = krasas;
        poga.interactable = iespejota;

        // Teksta krasa
        Color tekstaKrasa = Color.white;
        if (iespejota == false)
        {
            tekstaKrasa = new Color(0.6f, 0.6f, 0.6f);
        }
        Text teksts = IzveidotTekstu(objekts.transform, uzraksts, 26, TextAnchor.MiddleCenter, tekstaKrasa,
            centrs, centrs, Vector2.zero, Vector2.zero);

        // Teksts aizpilda visu pogu (ar nelielu atkapi no malam)
        RectTransform tekstaTaisnstulis = teksts.rectTransform;
        tekstaTaisnstulis.anchorMin = Vector2.zero;
        tekstaTaisnstulis.anchorMax = Vector2.one;
        tekstaTaisnstulis.offsetMin = new Vector2(10, 4);
        tekstaTaisnstulis.offsetMax = new Vector2(-10, -4);

        if (darbiba != null)
        {
            poga.onClick.AddListener(darbiba);
        }
    }

    // Izveido tumšu logu pa visu ekranu ar paneli vidu (minispelem).
    // Kad logs vairs nav vajadzigs, izsauc AizvertLogu.
    public GameObject IzveidotLogu(Vector2 izmers, out RectTransform panelis)
    {
        Image aptumsojums = IzveidotAttelu("Logs", audekls.transform, centrs, centrs, Vector2.zero, Vector2.zero,
            new Color(0, 0, 0, 0.5f));
        aptumsojums.rectTransform.anchorMin = Vector2.zero;
        aptumsojums.rectTransform.anchorMax = Vector2.one;
        aptumsojums.rectTransform.offsetMin = Vector2.zero;
        aptumsojums.rectTransform.offsetMax = Vector2.zero;
        aptumsojums.raycastTarget = true;       // bloke klikšķus uz spēli aiz loga

        panelis = IzveidotTaisnstuli("Panelis", aptumsojums.transform, centrs, centrs, Vector2.zero, izmers);
        Image fons = panelis.gameObject.AddComponent<Image>();
        fons.color = new Color(0.1f, 0.14f, 0.2f, 0.96f);

        logaSkaits = logaSkaits + 1;
        return aptumsojums.gameObject;
    }

    public void AizvertLogu(GameObject logaSakne)
    {
        if (logaSakne != null)
        {
            Destroy(logaSakne);
        }
        logaSkaits = Mathf.Max(0, logaSkaits - 1);
        logaAtcelsana = null;
    }

    // =================================================================
    // 2. Visas saskarnes uzbuve (izsauc GameBootstrap)
    // =================================================================
    public void Izveidot()
    {
        burtuFonts = Maksla.Fonts();

        // Audekls (Canvas) - uz tā zimejas visi UI elementi
        GameObject audeklaObjekts = new GameObject("Audekls");
        audekls = audeklaObjekts.AddComponent<Canvas>();
        audekls.renderMode = RenderMode.ScreenSpaceOverlay;
        audekls.sortingOrder = 10;

        CanvasScaler merogotajs = audeklaObjekts.AddComponent<CanvasScaler>();
        merogotajs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        merogotajs.referenceResolution = new Vector2(1920, 1080);
        merogotajs.matchWidthOrHeight = 0.5f;
        audeklaObjekts.AddComponent<GraphicRaycaster>();

        // Ir vajadzigs EventSystem, lai pogas reagetu uz peli
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        IzveidotSaskarni();
        IzveidotIzvelnesLogu();

        // Melns attels pa visu ekranu, ko izmantojam aptumsosanai
        aptumsaAttels = IzveidotAttelu("Aptumsojums", audekls.transform, centrs, centrs, Vector2.zero, Vector2.zero,
            new Color(0, 0, 0, 0));
        aptumsaAttels.rectTransform.anchorMin = Vector2.zero;
        aptumsaAttels.rectTransform.anchorMax = Vector2.one;
        aptumsaAttels.rectTransform.offsetMin = Vector2.zero;
        aptumsaAttels.rectTransform.offsetMax = Vector2.zero;
    }

    // ---------------- Statusa joslas ----------------
    RadaJosla IzveidotJoslu(Transform vecaks, string nosaukums, bool labiAugsta, Vector2 pozicija)
    {
        Image fons = IzveidotAttelu(nosaukums + "Fons", vecaks, augsaKreisi, augsaKreisi, pozicija,
            new Vector2(330, 34), new Color(0, 0, 0, 0.6f));

        // Krasainais aizpildijums (augstums un platums pielagojas ar enkuriem)
        RectTransform aizpildijumaTaisnstulis = IzveidotTaisnstuli("Aizpildijums", fons.transform, Vector2.zero,
            new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        aizpildijumaTaisnstulis.anchorMin = Vector2.zero;
        aizpildijumaTaisnstulis.anchorMax = Vector2.one;
        aizpildijumaTaisnstulis.offsetMin = new Vector2(2, 2);
        aizpildijumaTaisnstulis.offsetMax = new Vector2(-2, -2);
        Image aizpildijums = aizpildijumaTaisnstulis.gameObject.AddComponent<Image>();
        aizpildijums.raycastTarget = false;

        Text uzraksts = IzveidotTekstu(fons.transform, nosaukums, 20, TextAnchor.MiddleCenter, Color.white,
            centrs, centrs, Vector2.zero, new Vector2(330, 34));

        RadaJosla josla = new RadaJosla();
        josla.aizpildijums = aizpildijums;
        josla.uzraksts = uzraksts;
        josla.labiAugsta = labiAugsta;
        josla.nosaukums = nosaukums;
        return josla;
    }

    // Atjauno joslas garumu, krasu un tekstu atbilstosi vertibai (0-100)
    void IestatitJoslu(RadaJosla josla, float vertiba)
    {
        vertiba = Mathf.Clamp(vertiba, 0f, 100f);
        josla.aizpildijums.rectTransform.anchorMax = new Vector2(vertiba / 100f, 1f);

        // Cik "labs" ir stāvoklis (0 = slikts, 1 = labs)
        float labums = vertiba / 100f;
        if (josla.labiAugsta == false)
        {
            labums = 1f - labums;
        }
        Color slikta = new Color(0.85f, 0.2f, 0.2f);
        Color laba = new Color(0.3f, 0.8f, 0.35f);
        josla.aizpildijums.color = Color.Lerp(slikta, laba, labums);
        josla.uzraksts.text = josla.nosaukums + "  " + Mathf.RoundToInt(vertiba);
    }

    // ---------------- HUD ----------------
    void IzveidotSaskarni()
    {
        Color balta = Color.white;

        // Suna panelis (augša pa kreisi)
        Image sunaPanelisAttels = IzveidotAttelu("SunaPanelis", audekls.transform, augsaKreisi, augsaKreisi,
            new Vector2(20, -20), new Vector2(720, 170), new Color(0, 0, 0, 0.5f));
        sunaPaneli = sunaPanelisAttels.gameObject;
        sunaRinda = IzveidotTekstu(sunaPaneli.transform, "", 28, TextAnchor.MiddleLeft, balta,
            augsaKreisi, augsaKreisi, new Vector2(16, -8), new Vector2(690, 40));
        veselibasJosla = IzveidotJoslu(sunaPaneli.transform, "Veselība", true, new Vector2(16, -58));
        izsalkumaJosla = IzveidotJoslu(sunaPaneli.transform, "Izsalkums", false, new Vector2(374, -58));
        energijasJosla = IzveidotJoslu(sunaPaneli.transform, "Enerģija", true, new Vector2(16, -108));
        garastavoklaJosla = IzveidotJoslu(sunaPaneli.transform, "Garastāvoklis", true, new Vector2(374, -108));

        navSunaTeksts = IzveidotTekstu(audekls.transform, "Nav aktīvā suņa", 28, TextAnchor.MiddleLeft, balta,
            augsaKreisi, augsaKreisi, new Vector2(36, -30), new Vector2(600, 40));

        // Speletaja statistika (augša pa labi)
        Image statistika = IzveidotAttelu("Statistika", audekls.transform, augsaLabi, augsaLabi,
            new Vector2(-20, -20), new Vector2(380, 120), new Color(0, 0, 0, 0.5f));
        monetuTeksts = IzveidotTekstu(statistika.transform, "", 30, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.3f),
            augsaKreisi, augsaKreisi, new Vector2(16, -6), new Vector2(350, 40));
        limenaTeksts = IzveidotTekstu(statistika.transform, "", 24, TextAnchor.MiddleLeft, balta,
            augsaKreisi, augsaKreisi, new Vector2(16, -46), new Vector2(350, 34));
        adopcijuTeksts = IzveidotTekstu(statistika.transform, "", 24, TextAnchor.MiddleLeft, balta,
            augsaKreisi, augsaKreisi, new Vector2(16, -80), new Vector2(350, 34));

        IzveidotMinikarti();

        // Uzdevuma panelis (augša vidū)
        Image uzdevumaPanelis = IzveidotAttelu("Uzdevums", audekls.transform, augsaVidu, augsaVidu,
            new Vector2(0, -20), new Vector2(760, 90), new Color(0, 0, 0, 0.5f));
        IzveidotTekstu(uzdevumaPanelis.transform, "UZDEVUMS", 18, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.3f),
            augsaKreisi, augsaKreisi, new Vector2(14, -6), new Vector2(300, 24));
        uzdevumaTeksts = IzveidotTekstu(uzdevumaPanelis.transform, "", 24, TextAnchor.UpperLeft, balta,
            augsaKreisi, augsaKreisi, new Vector2(14, -28), new Vector2(732, 60));

        // Uzaicinajums "[E] ..." un pazinojumu teksts apaksa
        uzaicinajumaTeksts = IzveidotTekstu(audekls.transform, "", 36, TextAnchor.MiddleCenter, balta,
            apaksaVidu, apaksaVidu, new Vector2(0, 70), new Vector2(900, 50));
        pazinojumuTeksts = IzveidotTekstu(audekls.transform, "", 28, TextAnchor.LowerCenter, new Color(1f, 0.95f, 0.7f),
            apaksaVidu, apaksaVidu, new Vector2(0, 130), new Vector2(1100, 160));

        IzveidotTekstu(audekls.transform, "WASD – kustība   |   E – mijiedarbība   |   ESC – pauze", 20, TextAnchor.LowerLeft,
            new Color(1, 1, 1, 0.8f), apaksaKreisi, apaksaKreisi, new Vector2(20, 14), new Vector2(900, 30));
    }

    void IzveidotMinikarti()
    {
        Image karte = IzveidotAttelu("Minikarte", audekls.transform, augsaLabi, augsaLabi,
            new Vector2(-20, -150), new Vector2(kartesPlatums, kartesAugstums), new Color(0.2f, 0.36f, 0.2f, 0.85f));

        // Katra zona ir mazs tumšs taisnstūris ar burtu
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            Vector2 vieta = KartesPozicija(zona.centrs);
            Image zonasAttels = IzveidotAttelu("Zona", karte.transform, apaksaKreisi, centrs, vieta,
                new Vector2(64, 40), new Color(0.12f, 0.2f, 0.12f, 0.8f));
            IzveidotTekstu(zonasAttels.transform, zona.burts, 22, TextAnchor.MiddleCenter, Color.white,
                centrs, centrs, Vector2.zero, new Vector2(64, 40));
        }

        // Galvenais celš (horizontāla svītra)
        Image cels = IzveidotAttelu("Cels", karte.transform, apaksaKreisi, centrs, KartesPozicija(Vector2.zero),
            new Vector2(kartesPlatums - 10, 8), new Color(0.84f, 0.78f, 0.62f, 0.6f));
        cels.transform.SetAsFirstSibling();

        // Punkti: oranžs = suns, balts = speletajs
        sunaPunkts = IzveidotAttelu("SunaPunkts", karte.transform, apaksaKreisi, centrs, Vector2.zero,
            new Vector2(12, 12), new Color(1f, 0.6f, 0.15f)).rectTransform;
        speletajaPunkts = IzveidotAttelu("SpeletajaPunkts", karte.transform, apaksaKreisi, centrs, Vector2.zero,
            new Vector2(14, 14), Color.white).rectTransform;
    }

    // Parversts pasaules koordinatas par vietu minikarte
    Vector2 KartesPozicija(Vector2 pasaulesVieta)
    {
        float u = (pasaulesVieta.x - Pasaule.Robezas.xMin) / Pasaule.Robezas.width;
        float v = (pasaulesVieta.y - Pasaule.Robezas.yMin) / Pasaule.Robezas.height;
        float x = 5f + u * (kartesPlatums - 10f);
        float y = 5f + v * (kartesAugstums - 10f);
        return new Vector2(x, y);
    }

    // =================================================================
    // 3. Izvelne
    // =================================================================
    void IzveidotIzvelnesLogu()
    {
        // Tumšs fons pa visu ekranu
        Image aptumsojums = IzveidotAttelu("IzvelnesFons", audekls.transform, centrs, centrs, Vector2.zero,
            Vector2.zero, new Color(0, 0, 0, 0.55f));
        aptumsojums.rectTransform.anchorMin = Vector2.zero;
        aptumsojums.rectTransform.anchorMax = Vector2.one;
        aptumsojums.rectTransform.offsetMin = Vector2.zero;
        aptumsojums.rectTransform.offsetMax = Vector2.zero;
        aptumsojums.raycastTarget = true;
        izvelnesSakne = aptumsojums.gameObject;

        // Paneli, kura saturs sakārtojas automātiski (virsraksts, teksts, pogas viena zem otras)
        izvelnesPanelis = IzveidotTaisnstuli("Panelis", aptumsojums.transform, centrs, centrs, Vector2.zero, new Vector2(900, 100));
        Image fons = izvelnesPanelis.gameObject.AddComponent<Image>();
        fons.color = new Color(0.1f, 0.14f, 0.2f, 0.97f);

        VerticalLayoutGroup izkartojums = izvelnesPanelis.gameObject.AddComponent<VerticalLayoutGroup>();
        izkartojums.padding = new RectOffset(32, 32, 28, 28);
        izkartojums.spacing = 12;
        izkartojums.childAlignment = TextAnchor.UpperCenter;
        izkartojums.childControlWidth = true;
        izkartojums.childControlHeight = true;
        izkartojums.childForceExpandWidth = true;
        izkartojums.childForceExpandHeight = false;

        ContentSizeFitter pielagotajs = izvelnesPanelis.gameObject.AddComponent<ContentSizeFitter>();
        pielagotajs.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        izvelnesVirsraksts = IzveidotTekstu(izvelnesPanelis, "", 42, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f),
            centrs, centrs, Vector2.zero, Vector2.zero);
        izvelnesVirsraksts.fontStyle = FontStyle.Bold;
        izvelnesTeksts = IzveidotTekstu(izvelnesPanelis, "", 26, TextAnchor.UpperLeft, Color.white,
            centrs, centrs, Vector2.zero, Vector2.zero);

        izvelnesSakne.SetActive(false);
    }

    // Atver izvelni. "buvetajs" ir funkcija, kas atgriež izvelnes saturu.
    // Katru reizi, kad izvelne tiek atjaunota, funkcija tiek izsaukta atkal.
    public void AtvertIzvelni(Func<IzvelnesApraksts> buvetajs)
    {
        izvelnesBuvetajs = buvetajs;
        izvelnesSakne.SetActive(true);
        ParbuvetIzvelni();
    }

    public void AtjaunotIzvelni()
    {
        if (IzvelneAtverta() && izvelnesBuvetajs != null)
        {
            ParbuvetIzvelni();
        }
    }

    public void AizvertIzvelni()
    {
        izvelnesSakne.SetActive(false);
        izvelnesBuvetajs = null;
    }

    // Uzbuvē izvelnes saturu no jauna
    void ParbuvetIzvelni()
    {
        IzvelnesApraksts apraksts = izvelnesBuvetajs();
        izvelnesVirsraksts.text = apraksts.virsraksts;
        izvelnesTeksts.text = apraksts.teksts;
        izvelnesTeksts.gameObject.SetActive(apraksts.teksts != "");

        // Izdzēšam vecās pogas
        for (int i = 0; i < izvelnesPogas.Count; i++)
        {
            Destroy(izvelnesPogas[i]);
        }
        izvelnesPogas.Clear();

        // Izveidojam jaunas pogas
        for (int i = 0; i < apraksts.pogas.Count; i++)
        {
            PievienotIzvelnesPogu(apraksts.pogas[i]);
        }

        // Aizvert poga apaksa
        if (apraksts.rakstitAizvertPogu)
        {
            IzvelnesPoga aizvert = new IzvelnesPoga(apraksts.aizvertPogasTeksts, AizvertIzvelni);
            PievienotIzvelnesPogu(aizvert);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(izvelnesPanelis);
    }

    void PievienotIzvelnesPogu(IzvelnesPoga poga)
    {
        GameObject objekts = new GameObject("Poga", typeof(RectTransform));
        objekts.transform.SetParent(izvelnesPanelis, false);

        LayoutElement izmers = objekts.AddComponent<LayoutElement>();
        izmers.minHeight = 56;
        izmers.preferredHeight = 56;

        // Klikšķa apstrādei pieliekam mazo skriptu, kas zina, kura poga tas ir
        PogasKlikskis klikskis = objekts.AddComponent<PogasKlikskis>();
        klikskis.poga = poga;
        klikskis.saskarne = this;

        NoformetPogu(objekts, poga.uzraksts, klikskis.Nospiests, poga.iespejota);
        izvelnesPogas.Add(objekts);
    }

    // Izsauc PogasKlikskis, kad kāda izvelnes poga ir nospiesta
    public void ApstradatPogasKlikski(IzvelnesPoga poga)
    {
        GameManager.Galvenais.skana.Atskanot(Skana.Klikskis);

        if (poga.darbiba != null)
        {
            poga.darbiba();
        }
        if (poga.darbibaArParametru != null)
        {
            poga.darbibaArParametru(poga.parametrs);
        }

        // Ja izvelne vēl ir atvērta, atjaunojam to (lai redzētu jaunās vērtības)
        if (IzvelneAtverta() && izvelnesBuvetajs != null)
        {
            ParbuvetIzvelni();
        }
    }

    // =================================================================
    // 4. Pazinojumi, uzaicinajums, aptumsosana
    // =================================================================

    // Paradam zinu ekrana uz 4.5 sekundem
    public void Pazinojums(string zina)
    {
        PazinojumaIeraksts ieraksts = new PazinojumaIeraksts();
        ieraksts.teksts = zina;
        ieraksts.beigasLaiks = Time.unscaledTime + 4.5f;
        pazinojumi.Add(ieraksts);

        // Vienlaikus rāda ne vairak ka 4 zinas
        if (pazinojumi.Count > 4)
        {
            pazinojumi.RemoveAt(0);
        }
    }

    // Uzraksts apaksa, piemēram "[E] Veikals"
    public void IestatitUzaicinajumu(string teksts)
    {
        if (uzaicinajumaTeksts != null)
        {
            uzaicinajumaTeksts.text = teksts;
        }
    }

    // Uz brīdi aptumso ekranu un tad atkal atgriež (piemēram, atpūšoties)
    public void Aptumsot(float sekundes = 0.9f)
    {
        aptumsaIlgums = sekundes;
        aptumsaTaimeris = 0f;
    }

    void AtjaunotAptumsojumu()
    {
        if (aptumsaIlgums <= 0f)
        {
            return;
        }

        aptumsaTaimeris = aptumsaTaimeris + Time.unscaledDeltaTime;
        if (aptumsaTaimeris >= aptumsaIlgums)
        {
            aptumsaIlgums = 0f;
            aptumsaAttels.color = new Color(0, 0, 0, 0);
            return;
        }

        // Sinusa forma: sākumā tumšs vairāk un vairāk, tad atkal gaišs
        float progress = Mathf.Clamp01(aptumsaTaimeris / aptumsaIlgums);
        float caurspidiba = Mathf.Sin(progress * Mathf.PI);
        aptumsaAttels.color = new Color(0, 0, 0, caurspidiba);
    }

    // =================================================================
    // 5. Pauzes izvelne
    // =================================================================
    IzvelnesApraksts PauzesIzvelne()
    {
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Pauze";
        izvelne.teksts = "Vadība: WASD – kustība, E – mijiedarbība, pele – izvēlnes, ESC – pauze.\n" +
                         "Suns automātiski seko tev. Rūpējies par tā vajadzībām, trenē komandas un sagatavo adopcijai.";
        izvelne.aizvertPogasTeksts = "Turpināt";

        string skanasTeksts = "Skaņa: ieslēgta";
        if (GameManager.Galvenais.skana.izslegts)
        {
            skanasTeksts = "Skaņa: izslēgta";
        }
        izvelne.pogas.Add(new IzvelnesPoga(skanasTeksts, ParslegtSkanu));
        izvelne.pogas.Add(new IzvelnesPoga("Iziet no spēles", IzietNoSpeles));
        return izvelne;
    }

    void ParslegtSkanu()
    {
        AudioManager skana = GameManager.Galvenais.skana;
        skana.izslegts = !skana.izslegts;
    }

    void IzietNoSpeles()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =================================================================
    // 6. Katru kadru
    // =================================================================
    void Update()
    {
        GameManager spele = GameManager.Galvenais;

        // Kamēr ir atverta izvelne vai minispēle, spēles laiks stāv
        if (IrBloketa() || laikaPauzesTaimeris > 0f)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        if (laikaPauzesTaimeris > 0f)
        {
            laikaPauzesTaimeris = laikaPauzesTaimeris - Time.unscaledDeltaTime;
        }

        // ESC poga
        Keyboard tastatura = Keyboard.current;
        if (tastatura != null && tastatura.escapeKey.wasPressedThisFrame)
        {
            if (logaSkaits > 0)
            {
                if (logaAtcelsana != null)
                {
                    logaAtcelsana();
                }
            }
            else if (IzvelneAtverta())
            {
                AizvertIzvelni();
            }
            else
            {
                AtvertIzvelni(PauzesIzvelne);
            }
        }

        AtjaunotPazinojumus();
        AtjaunotAptumsojumu();
        AtjaunotSaskarni(spele);
    }

    // Izdzēš vecos pazinojumus un parāda atlikušos
    void AtjaunotPazinojumus()
    {
        for (int i = pazinojumi.Count - 1; i >= 0; i--)
        {
            if (pazinojumi[i].beigasLaiks < Time.unscaledTime)
            {
                pazinojumi.RemoveAt(i);
            }
        }

        string kopaTeksts = "";
        for (int i = 0; i < pazinojumi.Count; i++)
        {
            if (kopaTeksts != "")
            {
                kopaTeksts = kopaTeksts + "\n";
            }
            kopaTeksts = kopaTeksts + pazinojumi[i].teksts;
        }
        pazinojumuTeksts.text = kopaTeksts;
    }

    // Atjauno HUD tekstus un joslas
    void AtjaunotSaskarni(GameManager spele)
    {
        bool irSuns = spele.irSuns;
        sunaPaneli.SetActive(irSuns);
        navSunaTeksts.gameObject.SetActive(!irSuns);

        if (irSuns)
        {
            DogStatus suns = spele.suns;
            string slimibasTeksts = "";
            if (suns.slims)
            {
                slimibasTeksts = "   [SLIMS]";
            }
            sunaRinda.text = suns.vards + "  (" + DogStatus.RaksturaNosaukums(suns.raksturs) + ")  –  " +
                             spele.sunaProgress.limenis + ". līmenis" + slimibasTeksts;

            IestatitJoslu(veselibasJosla, suns.veseliba);
            IestatitJoslu(izsalkumaJosla, suns.Izsalkums());
            IestatitJoslu(energijasJosla, suns.energija);
            IestatitJoslu(garastavoklaJosla, suns.garastavoklis);
        }

        EconomyManager ekonomika = spele.ekonomika;
        string pasivasTeksts = "";
        if (ekonomika.pedejaPasivaPelna > 0)
        {
            pasivasTeksts = "  (+" + ekonomika.pedejaPasivaPelna + ")";
        }
        monetuTeksts.text = "Monētas: " + ekonomika.monetas + pasivasTeksts;

        PlayerProgress speletajaProgress = spele.speletajaProgress;
        limenaTeksts.text = "Spēlētājs: " + speletajaProgress.limenis + ". līm.  (" +
                            speletajaProgress.pieredze + "/" + speletajaProgress.PieredzesVajag() + ")";
        adopcijuTeksts.text = "Adopcijas: " + spele.adopcijas + "/" + GameManager.UzvarasAdopcijas;
        uzdevumaTeksts.text = spele.DabutUzdevumu();

        // Minikartes punkti
        speletajaPunkts.anchoredPosition = KartesPozicija(spele.KartesVieta(spele.speletajs));
        sunaPunkts.gameObject.SetActive(irSuns);
        if (irSuns)
        {
            sunaPunkts.anchoredPosition = KartesPozicija(spele.KartesVieta(spele.sunaObjekts.transform));
        }
    }
}

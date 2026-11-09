using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// GalvenaIzvelne: spēles galvenā izvēlne (aina "GalvenaIzvelne").
// Satur trīs paneļus, starp kuriem var pārslēgties:
//   1. Galvenais panelis  - Sākt spēli (2D), Sākt spēli (3D), Kā spēlēt, Iestatījumi, Iziet
//   2. Iestatījumu panelis - skaņa, skaļums, pilnekrāns
//   3. Palīdzības panelis  - vadība un spēles mērķis
// Poga "Sākt spēli" ielādē spēles ainu (SceneManager.LoadScene).
// Atpakaļ uz šo izvēlni var atgriezties no spēles ar ESC -> "Atpakaļ uz galveno izvēlni".
public class GalvenaIzvelne : MonoBehaviour
{
    public const string AinasNosaukums = "GalvenaIzvelne";
    public const string Aina2D = "GudraisSuns";
    public const string Aina3D = "GudraisSuns3D";

    // UI krāsas (kontrasts: gaiši teksti uz tumša fona)
    static Color panelaKrasa = new Color(0.08f, 0.12f, 0.2f, 0.93f);
    static Color virsrakstaKrasa = new Color(1f, 0.88f, 0.4f);
    static Color pogasKrasa = new Color(0.22f, 0.45f, 0.62f);
    static Color pogasUzVedesKrasa = new Color(0.3f, 0.58f, 0.78f);
    static Color pogasNospiestaKrasa = new Color(0.15f, 0.32f, 0.46f);
    static Color iziesanasPogasKrasa = new Color(0.6f, 0.25f, 0.25f);

    Font burtuFonts;
    Canvas audekls;

    GameObject galvenaisPanelis;
    GameObject iestatijumuPanelis;
    GameObject palidzibasPanelis;

    Text zinaTeksts;               // kļūdas ziņa (piemēram, ja aina nav atrasta)
    Text skanasPogasTeksts;
    Text skalumaTeksts;
    Text pilnekranaPogasTeksts;

    AudioSource klikskaAvots;
    AudioClip klikskaKlips;

    // =================================================================
    // Sākums: Unity pats izsauc šo funkciju pēc ainas ielādes
    // =================================================================
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Registreties()
    {
        // Šī funkcija tiek izsaukta TIKAI VIENU REIZI (kad sākas spēle). Tāpēc, lai spēle
        // uzbūvētos arī pēc ainas maiņas (SceneManager.LoadScene), pieslēdzamies notikumam
        // "aina ielādēta".
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= PecAinasIeladesanas;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += PecAinasIeladesanas;
        Sakums();
    }

    static void PecAinasIeladesanas(UnityEngine.SceneManagement.Scene aina, UnityEngine.SceneManagement.LoadSceneMode rezims)
    {
        Sakums();
    }

    static void Sakums()
    {
        string pasreizejaAina = SceneManager.GetActiveScene().name;
        if (pasreizejaAina != AinasNosaukums)
        {
            return;
        }
        if (Object.FindAnyObjectByType<GalvenaIzvelne>() != null)
        {
            return;
        }

        GameObject objekts = new GameObject("GalvenaIzvelne");
        GalvenaIzvelne izvelne = objekts.AddComponent<GalvenaIzvelne>();
        izvelne.Izveidot();
    }

    void Izveidot()
    {
        Time.timeScale = 1f;                // ja uz izvēlni atgriežamies no pauzes, laiks atkal iet
        Iestatijumi.Pielietot();
        burtuFonts = Maksla.Fonts();

        IestatitKameru();
        IzveidotKlikskaSkanu();
        IzveidotFonu();
        IzveidotSaskarni();
        ParadiitPaneli(galvenaisPanelis);
    }

    // =================================================================
    // Kamera un fons
    // =================================================================
    void IestatitKameru()
    {
        Camera kamera = Camera.main;
        if (kamera == null)
        {
            GameObject kameraObjekts = new GameObject("Main Camera");
            kameraObjekts.tag = "MainCamera";
            kamera = kameraObjekts.AddComponent<Camera>();
            kameraObjekts.AddComponent<AudioListener>();
        }
        kamera.orthographic = true;
        kamera.orthographicSize = 7f;
        kamera.clearFlags = CameraClearFlags.SolidColor;
        kamera.backgroundColor = new Color(0.55f, 0.78f, 0.95f);
        kamera.transform.position = new Vector3(0f, 0f, -10f);
        kamera.transform.rotation = Quaternion.identity;
    }

    // Zīmējam izvēlnes fonu (debesis, saule, mākoņi, pakalni, koki un suns) no formām
    void IzveidotFonu()
    {
        // Ja ir 2D gaisma, vajag vienu globālo gaismu, citādi spraiti var būt tumši
        if (Apgaismojums2D.IrPieejams())
        {
            GameObject gaismasObjekts = new GameObject("GlobalaGaisma");
            Light2D gaisma = gaismasObjekts.AddComponent<Light2D>();
            gaisma.lightType = Light2D.LightType.Global;
            gaisma.intensity = 1f;
        }

        Transform fons = new GameObject("Fons").transform;

        // Debesis (divas joslas, apakšā gaišāka)
        Maksla.IzveidotFormu("Debesis", fons, false, new Vector2(0f, 3f), new Vector2(50f, 24f), new Color(0.5f, 0.74f, 0.94f), -100);
        Maksla.IzveidotFormu("DebesuApaksa", fons, false, new Vector2(0f, 0.2f), new Vector2(50f, 7f), new Color(0.72f, 0.88f, 0.98f), -99);

        // Saule ar apspīdējumu
        Maksla.IzveidotFormu("SauleApspidejums", fons, true, new Vector2(7.5f, 4.6f), new Vector2(4.2f, 4.2f), new Color(1f, 0.95f, 0.7f, 0.35f), -98);
        Maksla.IzveidotFormu("Saule", fons, true, new Vector2(7.5f, 4.6f), new Vector2(2.4f, 2.4f), new Color(1f, 0.92f, 0.5f), -97);

        // Mākoņi (katrs sastāv no trim apļiem)
        IzveidotMakoni(fons, new Vector2(-6f, 4.8f), 1.0f);
        IzveidotMakoni(fons, new Vector2(2f, 5.6f), 0.8f);
        IzveidotMakoni(fons, new Vector2(10f, 2.2f), 0.7f);

        // Pakalni un zeme
        Maksla.IzveidotFormu("Pakalns1", fons, true, new Vector2(-6f, -8.4f), new Vector2(26f, 12f), new Color(0.36f, 0.62f, 0.34f), -90);
        Maksla.IzveidotFormu("Pakalns2", fons, true, new Vector2(9f, -9f), new Vector2(26f, 12f), new Color(0.42f, 0.68f, 0.38f), -89);
        Maksla.IzveidotFormu("Zeme", fons, false, new Vector2(0f, -7.2f), new Vector2(50f, 5f), new Color(0.38f, 0.64f, 0.34f), -88);

        // Koki
        IzveidotKoku(fons, new Vector2(3.2f, -2.4f), 1.3f);
        IzveidotKoku(fons, new Vector2(11f, -1.6f), 1.0f);
        IzveidotKoku(fons, new Vector2(-12f, -2.0f), 1.1f);

        // Suns, kas sēž un luncina asti (skatās uz izvēlni, tāpēc pagriezts pa kreisi)
        GameObject suns = Maksla.IzveidotSunu(fons);
        suns.transform.position = new Vector3(7.2f, -4.6f, 0f);
        suns.transform.localScale = new Vector3(-2.6f, 2.6f, 1f);
        SunaIzskats izskats = suns.GetComponent<SunaIzskats>();
        izskats.Nokrasot(new Color(0.85f, 0.58f, 0.3f));
        izskats.statusaTeksts.text = "<3";
        izskats.statusaTeksts.transform.localScale = new Vector3(-1f, 1f, 1f);       // lai teksts nebūtu spoguļots
        IzvelnesSuns sunaSkripts = suns.AddComponent<IzvelnesSuns>();
        sunaSkripts.aste = izskats.aste;
    }

    void IzveidotMakoni(Transform vecaks, Vector2 vieta, float merogs)
    {
        Color baltsMakonis = new Color(1f, 1f, 1f, 0.92f);
        Maksla.IzveidotFormu("Makonis", vecaks, true, vieta, new Vector2(3.2f, 1.6f) * merogs, baltsMakonis, -96);
        Maksla.IzveidotFormu("Makonis", vecaks, true, vieta + new Vector2(-1.3f, -0.2f) * merogs, new Vector2(2.2f, 1.2f) * merogs, baltsMakonis, -96);
        Maksla.IzveidotFormu("Makonis", vecaks, true, vieta + new Vector2(1.4f, -0.25f) * merogs, new Vector2(2.4f, 1.2f) * merogs, baltsMakonis, -96);
    }

    void IzveidotKoku(Transform vecaks, Vector2 vieta, float merogs)
    {
        Maksla.IzveidotFormu("Stumbrs", vecaks, false, vieta + new Vector2(0f, 0.6f) * merogs, new Vector2(0.5f, 1.6f) * merogs, new Color(0.4f, 0.27f, 0.15f), -60);
        Maksla.IzveidotFormu("Lapotne", vecaks, true, vieta + new Vector2(0f, 2.2f) * merogs, new Vector2(3.2f, 3f) * merogs, new Color(0.2f, 0.5f, 0.25f), -59);
        Maksla.IzveidotFormu("LapotnesGaisma", vecaks, true, vieta + new Vector2(-0.4f, 2.7f) * merogs, new Vector2(1.8f, 1.6f) * merogs, new Color(0.28f, 0.6f, 0.3f), -58);
    }

    // =================================================================
    // Saskarne (Canvas, EventSystem un paneļi)
    // =================================================================
    void IzveidotSaskarni()
    {
        GameObject audeklaObjekts = new GameObject("Audekls");
        audekls = audeklaObjekts.AddComponent<Canvas>();
        audekls.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler merogotajs = audeklaObjekts.AddComponent<CanvasScaler>();
        merogotajs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        merogotajs.referenceResolution = new Vector2(1920, 1080);
        merogotajs.matchWidthOrHeight = 0.5f;
        audeklaObjekts.AddComponent<GraphicRaycaster>();

        // Bez EventSystem pogas nereaģētu uz peli
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        IzveidotVirsrakstu();
        IzveidotGalvenoPaneli();
        IzveidotIestatijumuPaneli();
        IzveidotPalidzibasPaneli();

        // Ziņa apakšā (parādās tikai problēmu gadījumā)
        zinaTeksts = JaunsTekstsPaLaikam(audekls.transform, "", 30, new Color(1f, 0.6f, 0.5f),
            new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(1400f, 50f));

        // Maza informācija stūrī
        JaunsTekstsPaLaikam(audekls.transform, "Grupa 9  |  2026./2027.", 24, new Color(1f, 1f, 1f, 0.9f),
            new Vector2(1f, 0f), new Vector2(-30f, 24f), new Vector2(500f, 36f)).alignment = TextAnchor.LowerRight;
    }

    // Liels virsraksts ekrāna augšā
    void IzveidotVirsrakstu()
    {
        Text virsraksts = JaunsTekstsPaLaikam(audekls.transform, "GUDRAIS SUNS", 140, virsrakstaKrasa,
            new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1500f, 180f));
        virsraksts.fontStyle = FontStyle.Bold;
        PievienotKontura(virsraksts.gameObject, new Color(0.1f, 0.15f, 0.3f), 6f);

        Text apaksvirsraksts = JaunsTekstsPaLaikam(audekls.transform, "Suņu aprūpes, apmācības un adopcijas spēle", 44, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0f, -215f), new Vector2(1500f, 60f));
        PievienotKontura(apaksvirsraksts.gameObject, new Color(0.1f, 0.15f, 0.3f), 3f);
    }

    // ----------------- Galvenais panelis -----------------
    void IzveidotGalvenoPaneli()
    {
        galvenaisPanelis = JaunsPanelis("GalvenaisPanelis");
        Transform p = galvenaisPanelis.transform;

        Text virsraksts = JaunsTekstsPanelim(p, "GALVENĀ IZVĒLNE", 40, virsrakstaKrasa, true);
        virsraksts.alignment = TextAnchor.MiddleCenter;

        Text neizmantots;
        JaunaPoga(p, "Sākt spēli (2D)", Sakt2D, pogasKrasa, out neizmantots);
        JaunaPoga(p, "Sākt spēli (3D)", Sakt3D, pogasKrasa, out neizmantots);
        JaunaPoga(p, "Kā spēlēt", ParadiitPalidzibu, pogasKrasa, out neizmantots);
        JaunaPoga(p, "Iestatījumi", ParadiitIestatijumus, pogasKrasa, out neizmantots);
        JaunaPoga(p, "Iziet", IzietNoSpeles, iziesanasPogasKrasa, out neizmantots);
    }

    // ----------------- Iestatījumu panelis -----------------
    void IzveidotIestatijumuPaneli()
    {
        iestatijumuPanelis = JaunsPanelis("IestatijumuPanelis");
        Transform p = iestatijumuPanelis.transform;

        Text virsraksts = JaunsTekstsPanelim(p, "IESTATĪJUMI", 40, virsrakstaKrasa, true);
        virsraksts.alignment = TextAnchor.MiddleCenter;

        JaunaPoga(p, "", ParslegtSkanu, pogasKrasa, out skanasPogasTeksts);

        // Skaļuma rinda: [ − ]   Skaļums: 80 %   [ + ]
        GameObject rinda = new GameObject("SkalumaRinda", typeof(RectTransform));
        rinda.transform.SetParent(p, false);
        HorizontalLayoutGroup izkartojums = rinda.AddComponent<HorizontalLayoutGroup>();
        izkartojums.spacing = 12;
        izkartojums.childAlignment = TextAnchor.MiddleCenter;
        izkartojums.childControlWidth = true;
        izkartojums.childControlHeight = true;
        izkartojums.childForceExpandWidth = false;
        izkartojums.childForceExpandHeight = true;
        LayoutElement rindasIzmers = rinda.AddComponent<LayoutElement>();
        rindasIzmers.minHeight = 76;
        rindasIzmers.preferredHeight = 76;

        Text neizmantots;
        Button minusPoga = JaunaPoga(rinda.transform, "−", SamazinatSkalumu, pogasKrasa, out neizmantots);
        minusPoga.GetComponent<LayoutElement>().preferredWidth = 90;
        skalumaTeksts = JaunsTekstsPanelim(rinda.transform, "", 34, Color.white, false);
        skalumaTeksts.alignment = TextAnchor.MiddleCenter;
        skalumaTeksts.gameObject.AddComponent<LayoutElement>().preferredWidth = 300;
        Button plusPoga = JaunaPoga(rinda.transform, "+", PalielinatSkalumu, pogasKrasa, out neizmantots);
        plusPoga.GetComponent<LayoutElement>().preferredWidth = 90;

        JaunaPoga(p, "", ParslegtPilnekranu, pogasKrasa, out pilnekranaPogasTeksts);
        JaunaPoga(p, "Atpakaļ", ParadiitGalvenoPaneli, pogasKrasa, out neizmantots);
    }

    // ----------------- Palīdzības panelis -----------------
    void IzveidotPalidzibasPaneli()
    {
        palidzibasPanelis = JaunsPanelis("PalidzibasPanelis");
        Transform p = palidzibasPanelis.transform;

        Text virsraksts = JaunsTekstsPanelim(p, "KĀ SPĒLĒT", 40, virsrakstaKrasa, true);
        virsraksts.alignment = TextAnchor.MiddleCenter;

        string saturs = "Mērķis: saņem suņus no labklājības centra, rūpējies par tiem, iemāci komandas " +
                        "un sagatavo adopcijai. Vajag 10 veiksmīgas adopcijas.\n\n" +
                        "WASD – kustība\n" +
                        "E – mijiedarbība ar ēkām\n" +
                        "Pele – izvēlnes un pogas\n" +
                        "Space – minispēles\n" +
                        "ESC – pauze (tur var atgriezties šeit vai iziet)";
        Text teksts = JaunsTekstsPanelim(p, saturs, 28, Color.white, false);
        teksts.alignment = TextAnchor.UpperLeft;

        Text neizmantots;
        JaunaPoga(p, "Atpakaļ", ParadiitGalvenoPaneli, pogasKrasa, out neizmantots);
    }

    // =================================================================
    // Paneļu pārslēgšana
    // =================================================================
    void ParadiitPaneli(GameObject panelis)
    {
        galvenaisPanelis.SetActive(panelis == galvenaisPanelis);
        iestatijumuPanelis.SetActive(panelis == iestatijumuPanelis);
        palidzibasPanelis.SetActive(panelis == palidzibasPanelis);
        zinaTeksts.text = "";
    }

    void ParadiitGalvenoPaneli()
    {
        AtskanotKlikski();
        ParadiitPaneli(galvenaisPanelis);
    }

    void ParadiitIestatijumus()
    {
        AtskanotKlikski();
        AtjaunotIestatijumuTekstus();
        ParadiitPaneli(iestatijumuPanelis);
    }

    void ParadiitPalidzibu()
    {
        AtskanotKlikski();
        ParadiitPaneli(palidzibasPanelis);
    }

    // =================================================================
    // Pogu darbības
    // =================================================================
    void Sakt2D()
    {
        AtskanotKlikski();
        IeladetAinu(Aina2D);
    }

    void Sakt3D()
    {
        AtskanotKlikski();
        IeladetAinu(Aina3D);
    }

    // Pārslēdzas uz citu ainu (ja tā ir pievienota Build Settings sarakstam)
    void IeladetAinu(string ainasNosaukums)
    {
        if (Application.CanStreamedLevelBeLoaded(ainasNosaukums) == false)
        {
            zinaTeksts.text = "Aina \"" + ainasNosaukums + "\" nav atrasta. Pievieno to: File > Build Profiles > Scene List.";
            return;
        }
        SceneManager.LoadScene(ainasNosaukums);
    }

    void IzietNoSpeles()
    {
        AtskanotKlikski();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ParslegtSkanu()
    {
        Iestatijumi.IestatitSkanu(!Iestatijumi.SkanaIeslegta());
        AtskanotKlikski();
        AtjaunotIestatijumuTekstus();
    }

    void PalielinatSkalumu()
    {
        Iestatijumi.IestatitSkalumu(Iestatijumi.DabutSkalumu() + 0.1f);
        AtskanotKlikski();
        AtjaunotIestatijumuTekstus();
    }

    void SamazinatSkalumu()
    {
        Iestatijumi.IestatitSkalumu(Iestatijumi.DabutSkalumu() - 0.1f);
        AtskanotKlikski();
        AtjaunotIestatijumuTekstus();
    }

    void ParslegtPilnekranu()
    {
        Iestatijumi.IestatitPilnekranu(!Iestatijumi.DabutPilnekranu());
        AtskanotKlikski();
        AtjaunotIestatijumuTekstus();
    }

    // Atjauno uzrakstus iestatījumu panelī
    void AtjaunotIestatijumuTekstus()
    {
        if (Iestatijumi.SkanaIeslegta())
        {
            skanasPogasTeksts.text = "Skaņa: ieslēgta";
        }
        else
        {
            skanasPogasTeksts.text = "Skaņa: izslēgta";
        }
        skalumaTeksts.text = "Skaļums: " + Mathf.RoundToInt(Iestatijumi.DabutSkalumu() * 100f) + " %";
        if (Iestatijumi.DabutPilnekranu())
        {
            pilnekranaPogasTeksts.text = "Pilnekrāns: ieslēgts";
        }
        else
        {
            pilnekranaPogasTeksts.text = "Pilnekrāns: izslēgts";
        }
    }

    // ESC taustiņš: no apakšpaneļa atgriežamies galvenajā panelī
    void Update()
    {
        Keyboard tastatura = Keyboard.current;
        if (tastatura == null || tastatura.escapeKey.wasPressedThisFrame == false)
        {
            return;
        }
        if (galvenaisPanelis.activeSelf == false)
        {
            ParadiitGalvenoPaneli();
        }
    }

    // =================================================================
    // Pogas klikšķa skaņa (īsa skaņa, ko izveidojam ar kodu)
    // =================================================================
    void IzveidotKlikskaSkanu()
    {
        int paraugi = 1300;
        float[] dati = new float[paraugi];
        for (int i = 0; i < paraugi; i++)
        {
            float t = i / 22050f;
            float izdzisana = Mathf.Clamp01(1f - t / 0.06f);
            dati[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * izdzisana * 0.4f;
        }
        klikskaKlips = AudioClip.Create("klikskis", paraugi, 1, 22050, false);
        klikskaKlips.SetData(dati, 0);

        klikskaAvots = gameObject.AddComponent<AudioSource>();
        klikskaAvots.playOnAwake = false;
    }

    void AtskanotKlikski()
    {
        if (Iestatijumi.SkanaIeslegta())
        {
            klikskaAvots.PlayOneShot(klikskaKlips, 0.8f);
        }
    }

    // =================================================================
    // Palīgfunkcijas UI elementu veidošanai
    // =================================================================

    // Panelis kreisajā pusē; saturs (teksti un pogas) sakārtojas viens zem otra pats
    GameObject JaunsPanelis(string nosaukums)
    {
        GameObject panelis = new GameObject(nosaukums, typeof(RectTransform));
        RectTransform taisnstulis = panelis.GetComponent<RectTransform>();
        taisnstulis.SetParent(audekls.transform, false);
        taisnstulis.anchorMin = new Vector2(0f, 0.5f);
        taisnstulis.anchorMax = new Vector2(0f, 0.5f);
        taisnstulis.pivot = new Vector2(0f, 0.5f);
        taisnstulis.anchoredPosition = new Vector2(150f, -90f);
        taisnstulis.sizeDelta = new Vector2(640f, 100f);

        Image fons = panelis.AddComponent<Image>();
        fons.color = panelaKrasa;

        VerticalLayoutGroup izkartojums = panelis.AddComponent<VerticalLayoutGroup>();
        izkartojums.padding = new RectOffset(36, 36, 30, 30);
        izkartojums.spacing = 16;
        izkartojums.childAlignment = TextAnchor.UpperCenter;
        izkartojums.childControlWidth = true;
        izkartojums.childControlHeight = true;
        izkartojums.childForceExpandWidth = true;
        izkartojums.childForceExpandHeight = false;

        ContentSizeFitter pielagotajs = panelis.AddComponent<ContentSizeFitter>();
        pielagotajs.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Rāmis ap paneli (Outline komponente zīmē kontūru)
        Outline rami = panelis.AddComponent<Outline>();
        rami.effectColor = new Color(1f, 0.88f, 0.4f, 0.8f);
        rami.effectDistance = new Vector2(3f, -3f);
        return panelis;
    }

    // Teksts panelī (izmēru nosaka izkārtojums)
    Text JaunsTekstsPanelim(Transform panelis, string saturs, int burtuIzmers, Color krasa, bool treknraksts)
    {
        GameObject objekts = new GameObject("Teksts", typeof(RectTransform));
        objekts.transform.SetParent(panelis, false);
        Text teksts = objekts.AddComponent<Text>();
        teksts.font = burtuFonts;
        teksts.fontSize = burtuIzmers;
        teksts.color = krasa;
        teksts.text = saturs;
        teksts.raycastTarget = false;
        if (treknraksts)
        {
            teksts.fontStyle = FontStyle.Bold;
        }
        return teksts;
    }

    // Teksts uz ekrāna ar fiksētu vietu (virsraksts, ziņa)
    Text JaunsTekstsPaLaikam(Transform vecaks, string saturs, int burtuIzmers, Color krasa,
        Vector2 enkurs, Vector2 pozicija, Vector2 izmers)
    {
        GameObject objekts = new GameObject("Teksts", typeof(RectTransform));
        RectTransform taisnstulis = objekts.GetComponent<RectTransform>();
        taisnstulis.SetParent(vecaks, false);
        taisnstulis.anchorMin = enkurs;
        taisnstulis.anchorMax = enkurs;
        taisnstulis.pivot = enkurs;
        taisnstulis.anchoredPosition = pozicija;
        taisnstulis.sizeDelta = izmers;

        Text teksts = objekts.AddComponent<Text>();
        teksts.font = burtuFonts;
        teksts.fontSize = burtuIzmers;
        teksts.color = krasa;
        teksts.text = saturs;
        teksts.alignment = TextAnchor.MiddleCenter;
        teksts.raycastTarget = false;
        return teksts;
    }

    void PievienotKontura(GameObject objekts, Color krasa, float biezums)
    {
        Outline kontura = objekts.AddComponent<Outline>();
        kontura.effectColor = krasa;
        kontura.effectDistance = new Vector2(biezums, -biezums);
    }

    // Poga ar tekstu; "tekstaKomponente" ļauj vēlāk nomainīt pogas uzrakstu
    Button JaunaPoga(Transform vecaks, string uzraksts, UnityAction darbiba, Color normalaKrasa, out Text tekstaKomponente)
    {
        GameObject objekts = new GameObject("Poga", typeof(RectTransform));
        objekts.transform.SetParent(vecaks, false);

        LayoutElement izmers = objekts.AddComponent<LayoutElement>();
        izmers.minHeight = 76;
        izmers.preferredHeight = 76;

        Image fons = objekts.AddComponent<Image>();
        fons.color = Color.white;

        Button poga = objekts.AddComponent<Button>();
        poga.targetGraphic = fons;
        ColorBlock krasas = poga.colors;
        krasas.normalColor = normalaKrasa;
        krasas.highlightedColor = Color.Lerp(normalaKrasa, Color.white, 0.25f);
        krasas.pressedColor = Color.Lerp(normalaKrasa, Color.black, 0.3f);
        krasas.selectedColor = normalaKrasa;
        poga.colors = krasas;
        poga.onClick.AddListener(darbiba);

        // Pogas uzraksts aizpilda visu pogu
        GameObject tekstaObjekts = new GameObject("Uzraksts", typeof(RectTransform));
        RectTransform tekstaTaisnstulis = tekstaObjekts.GetComponent<RectTransform>();
        tekstaTaisnstulis.SetParent(objekts.transform, false);
        tekstaTaisnstulis.anchorMin = Vector2.zero;
        tekstaTaisnstulis.anchorMax = Vector2.one;
        tekstaTaisnstulis.offsetMin = new Vector2(10f, 4f);
        tekstaTaisnstulis.offsetMax = new Vector2(-10f, -4f);

        tekstaKomponente = tekstaObjekts.AddComponent<Text>();
        tekstaKomponente.font = burtuFonts;
        tekstaKomponente.fontSize = 36;
        tekstaKomponente.alignment = TextAnchor.MiddleCenter;
        tekstaKomponente.color = Color.white;
        tekstaKomponente.text = uzraksts;
        tekstaKomponente.raycastTarget = false;
        return poga;
    }
}

// IzvelnesSuns: izvēlnes dekoratīvais suns luncina asti.
public class IzvelnesSuns : MonoBehaviour
{
    public Transform aste;

    void Update()
    {
        if (aste == null)
        {
            return;
        }
        float luncisana = Mathf.Sin(Time.time * 7f) * 28f;
        aste.localRotation = Quaternion.Euler(0f, 0f, 35f + luncisana);
    }
}

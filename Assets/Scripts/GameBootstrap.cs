using UnityEngine;

// GameBootstrap: uzbūvē visu spēli, kad nospiež Play.
// Tas darbojas tikai ainā ar nosaukumu "GudraisSuns", tāpēc citas ainas netiek ietekmētas.
public static class GameBootstrap
{
    public const string AinasNosaukums = "GudraisSuns";

    static Color zalesKrasa = new Color(0.38f, 0.64f, 0.34f);
    static Color celaKrasa = new Color(0.84f, 0.78f, 0.62f);

    // Unity pats izsauc šo funkciju pēc ainas ielādes
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Sakums()
    {
        // Strādājam tikai savā ainā
        string pasreizejaAina = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (pasreizejaAina != AinasNosaukums)
        {
            return;
        }
        if (Object.FindAnyObjectByType<GameManager>() != null)
        {
            return;
        }

        // Galvenais objekts, kuram pieliekam visas sistēmas
        GameObject sakne = new GameObject("GudraisSuns");
        GameManager spele = sakne.AddComponent<GameManager>();

        // 1. Pasaule (karte)
        GameObject pasauleObjekts = new GameObject("Pasaule");
        pasauleObjekts.transform.SetParent(sakne.transform, false);
        IzveidotKarti(pasauleObjekts.transform);

        // 2. Kamera
        Camera kamera = IestatitKameru();

        // 3. Spēlētājs
        GameObject speletajs = Maksla.IzveidotSpeletaju(sakne.transform);
        speletajs.transform.position = new Vector3(-4f, -2.5f, 0);
        speletajs.AddComponent<YKartosana>();
        speletajs.AddComponent<PlayerMovement>();
        spele.speletajs = speletajs.transform;

        CameraFollow kamerasSekosana = kamera.gameObject.GetComponent<CameraFollow>();
        if (kamerasSekosana == null)
        {
            kamerasSekosana = kamera.gameObject.AddComponent<CameraFollow>();
        }
        kamerasSekosana.merkis = speletajs.transform;
        spele.kamera2D = kamerasSekosana;
        kamerasSekosana.Pielekt();

        // 4. Suns (paslēpts, kamēr spēlētājs to nesaņem labklājības centrā)
        GameObject sunaObjekts = Maksla.IzveidotSunu(sakne.transform);
        spele.sunaObjekts = sunaObjekts;
        spele.suns = sunaObjekts.AddComponent<DogStatus>();
        spele.sunaProgress = sunaObjekts.AddComponent<DogProgress>();
        spele.sunaSekosana = sunaObjekts.AddComponent<DogFollow>();
        spele.sunaSekosana.speletajs = speletajs.transform;
        spele.sunaAnimacija = sunaObjekts.AddComponent<SunaAnimacija>();
        sunaObjekts.SetActive(false);

        // 5. Spēles sistēmas
        spele.speletajaProgress = sakne.AddComponent<PlayerProgress>();
        spele.ekonomika = sakne.AddComponent<EconomyManager>();
        spele.aprupe = sakne.AddComponent<CareSystem>();
        spele.trenins = sakne.AddComponent<TrainingSystem>();
        spele.veikals = sakne.AddComponent<ShopSystem>();
        spele.nujasSpele = sakne.AddComponent<NujasSpele>();
        spele.labklajiba = sakne.AddComponent<WelfareAndAdoption>();
        sakne.AddComponent<InteractionController>();
        spele.skana = sakne.AddComponent<AudioManager>();
        spele.saskarne = sakne.AddComponent<UIManager>();
        spele.skana.Izveidot();
        spele.saskarne.Izveidot();

        // 6. 2D gaisma (diena un nakts, laterna, lampas)
        sakne.AddComponent<Apgaismojums2D>();

        // 7. Ievada izvēlne
        spele.saskarne.AtvertIzvelni(IevadaIzvelne);
    }

    // Atrod (vai izveido) kameru un iestata to 2D spēlei
    static Camera IestatitKameru()
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
        kamera.backgroundColor = new Color(0.25f, 0.45f, 0.25f);
        kamera.transform.rotation = Quaternion.identity;
        kamera.transform.position = new Vector3(0, 0, -10);
        return kamera;
    }

    static IzvelnesApraksts IevadaIzvelne()
    {
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "GUDRAIS SUNS";
        izvelne.teksts = "Mierīga 2D suņu aprūpes, apmācības un menedžmenta spēle.\n\n" +
                         "Saņem suņus no dzīvnieku labklājības centra, rūpējies par tiem, iemāci komandas un sagatavo adopcijai. " +
                         "Par panākumiem saņem monētas un pieredzi centra attīstīšanai. Mērķis – 10 veiksmīgas adopcijas.\n\n" +
                         "WASD – kustība   |   E – mijiedarbība   |   Pele – izvēlnes   |   ESC – pauze";
        izvelne.aizvertPogasTeksts = "Sākt spēli";
        return izvelne;
    }

    // Palīgfunkcija: padara krāsu tumšāku (reizina ar skaitli mazāku par 1)
    static Color Tumsinat(Color krasa, float koeficients)
    {
        return new Color(krasa.r * koeficients, krasa.g * koeficients, krasa.b * koeficients, 1f);
    }

    // =================================================================
    // KARTES UZBŪVE
    // =================================================================
    static void IzveidotKarti(Transform sakne)
    {
        Pasaule.Barjeras.Clear();

        IzveidotZaleUnCelus(sakne);
        IzveidotStruklaku(sakne);

        // Ēkas
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Labklajiba), new Vector2(7.5f, 4.2f), new Color(0.93f, 0.88f, 0.76f), new Color(0.35f, 0.55f, 0.75f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Maja), new Vector2(6.5f, 4.2f), new Color(0.96f, 0.85f, 0.7f), new Color(0.75f, 0.3f, 0.25f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Veikals), new Vector2(7f, 4.2f), new Color(0.95f, 0.9f, 0.6f), new Color(0.85f, 0.55f, 0.2f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Klinika), new Vector2(7.5f, 4.2f), new Color(0.95f, 0.95f, 0.97f), new Color(0.8f, 0.2f, 0.25f));

        // Treniņu laukums un parks (nav ēkas)
        IzveidotTreninuLaukumu(sakne, Pasaule.AtrastZonu(ZonasVeids.Trenins));
        IzveidotParku(sakne, Pasaule.AtrastZonu(ZonasVeids.Parks));

        IzveidotKokus(sakne);
    }

    // Zāle, zonu laukumi un ceļi
    static void IzveidotZaleUnCelus(Transform sakne)
    {
        Maksla.IzveidotFormu("Zale", sakne, false, Vector2.zero, new Vector2(80, 50), zalesKrasa, -10000);

        // Katrai zonai nedaudz atšķirīgs zāles tonis
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            Vector2 izmers = new Vector2(Pasaule.ZonasPusIzmers.x * 2, Pasaule.ZonasPusIzmers.y * 2);
            Maksla.IzveidotFormu("Laukums_" + zona.veids, sakne, false, zona.centrs, izmers, zona.krasa, -9900);
        }

        // Galvenais horizontālais ceļš
        Maksla.IzveidotFormu("CelsH", sakne, false, Vector2.zero, new Vector2(62, 2.4f), celaKrasa, -9800);

        // Vertikāls ceļš uz katru zonu
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            float garums = Mathf.Abs(zona.mijiedarbibasPunkts.y);
            Vector2 vidus = new Vector2(zona.centrs.x, zona.mijiedarbibasPunkts.y * 0.5f);
            Maksla.IzveidotFormu("CelsV_" + zona.veids, sakne, false, vidus, new Vector2(2.4f, garums), celaKrasa, -9800);
            Maksla.IzveidotFormu("Paklajs_" + zona.veids, sakne, true, zona.mijiedarbibasPunkts, new Vector2(2.6f, 1.2f), Tumsinat(celaKrasa, 0.9f), -9790);
        }
    }

    // Strūklaka kartes vidū
    static void IzveidotStruklaku(Transform sakne)
    {
        Maksla.IzveidotFormu("LaukumaGredzens", sakne, true, Vector2.zero, new Vector2(7f, 5f), Tumsinat(celaKrasa, 0.95f), -9780);
        Maksla.IzveidotFormu("StruklakasPamats", sakne, true, new Vector2(0, 0.1f), new Vector2(3.4f, 2.3f), new Color(0.65f, 0.65f, 0.7f), -9770);
        Maksla.IzveidotFormu("StruklakasUdens", sakne, true, new Vector2(0, 0.1f), new Vector2(2.9f, 1.9f), new Color(0.35f, 0.65f, 0.9f), -9760);
        Maksla.IzveidotFormu("StruklakasStrukla", sakne, false, new Vector2(0, 0.7f), new Vector2(0.18f, 0.9f), new Color(0.8f, 0.92f, 1f), 100);
        Pasaule.Barjeras.Add(new Rect(-1.4f, -0.7f, 2.8f, 1.5f));
        Maksla.IzveidotTekstuArVaru("Centrs", sakne, new Vector2(0, -1.9f), 0.55f, new Color(1, 1, 1, 0.9f), -9000);
    }

    // Uzbūvē vienu ēku (labklājības centrs, māja, veikals vai klīnika)
    static void IzveidotEku(Transform sakne, ZonasInfo zona, Vector2 izmers, Color sienuKrasa, Color jumtaKrasa)
    {
        // Augšējās zonas ēka ir augstāk (y = 14), apakšējās zemāk (y = -14)
        float ekasY = -14f;
        if (zona.centrs.y > 0)
        {
            ekasY = 14f;
        }
        Vector2 centrs = new Vector2(zona.centrs.x, ekasY);
        int kartiba = Mathf.RoundToInt(-(centrs.y - izmers.y * 0.5f) * 20f);

        GameObject eka = new GameObject("Eka_" + zona.veids);
        eka.transform.SetParent(sakne, false);
        eka.transform.localPosition = centrs;
        Transform t = eka.transform;

        Maksla.IzveidotFormu("Vara", t, false, new Vector2(0.4f, -izmers.y * 0.5f - 0.1f), new Vector2(izmers.x + 0.5f, 0.5f), new Color(0, 0, 0, 0.2f), -9500);
        Maksla.IzveidotFormu("Siena", t, false, Vector2.zero, izmers, sienuKrasa, kartiba);
        Maksla.IzveidotFormu("Jumts", t, false, new Vector2(0, izmers.y * 0.22f), new Vector2(izmers.x + 0.4f, izmers.y * 0.56f), jumtaKrasa, kartiba + 1);
        Maksla.IzveidotFormu("JumtaMala", t, false, new Vector2(0, -izmers.y * 0.06f), new Vector2(izmers.x + 0.4f, 0.14f), Tumsinat(jumtaKrasa, 0.7f), kartiba + 2);
        Maksla.IzveidotFormu("Durvis", t, false, new Vector2(0, -izmers.y * 0.5f + 0.7f), new Vector2(1.0f, 1.4f), new Color(0.4f, 0.26f, 0.15f), kartiba + 2);
        Maksla.IzveidotFormu("LogsKreisais", t, false, new Vector2(-izmers.x * 0.3f, -izmers.y * 0.5f + 1.1f), new Vector2(0.9f, 0.8f), new Color(0.7f, 0.88f, 1f), kartiba + 2);
        Maksla.IzveidotFormu("LogsLabais", t, false, new Vector2(izmers.x * 0.3f, -izmers.y * 0.5f + 1.1f), new Vector2(0.9f, 0.8f), new Color(0.7f, 0.88f, 1f), kartiba + 2);

        // Katrai ēkai savs atpazīšanas zīmējums
        if (zona.veids == ZonasVeids.Klinika)
        {
            // Balts krusts uz jumta
            Maksla.IzveidotFormu("KrustsVertikali", t, false, new Vector2(0, izmers.y * 0.22f), new Vector2(0.4f, 1.3f), Color.white, kartiba + 3);
            Maksla.IzveidotFormu("KrustsHorizontali", t, false, new Vector2(0, izmers.y * 0.22f), new Vector2(1.3f, 0.4f), Color.white, kartiba + 3);
        }
        if (zona.veids == ZonasVeids.Veikals)
        {
            Maksla.IzveidotFormu("Markize", t, false, new Vector2(0, -izmers.y * 0.5f + 1.8f), new Vector2(izmers.x * 0.8f, 0.35f), new Color(0.9f, 0.3f, 0.3f), kartiba + 3);
        }
        if (zona.veids == ZonasVeids.Labklajiba)
        {
            Maksla.IzveidotFormu("Kepa", t, true, new Vector2(0, izmers.y * 0.22f), new Vector2(0.9f, 0.8f), new Color(1f, 0.95f, 0.8f), kartiba + 3);
        }

        // Ēka ir siena, kurai cauri nevar iet
        Rect barjera = new Rect(centrs.x - izmers.x * 0.5f, centrs.y - izmers.y * 0.5f, izmers.x, izmers.y);
        Pasaule.Barjeras.Add(barjera);

        // Nosaukums virs ēkas
        float tekstaY = centrs.y + izmers.y * 0.5f + 0.9f;
        Maksla.IzveidotTekstuArVaru(zona.nosaukums, sakne, new Vector2(centrs.x, tekstaY), 0.7f, Color.white, 8000);
    }

    static void IzveidotTreninuLaukumu(Transform sakne, ZonasInfo zona)
    {
        Vector2 c = zona.centrs;
        Maksla.IzveidotFormu("Laukums", sakne, false, new Vector2(c.x, c.y + 2f), new Vector2(15f, 8.4f), new Color(0.78f, 0.72f, 0.5f), -9700);
        Maksla.IzveidotFormu("LaukumaIekspuse", sakne, false, new Vector2(c.x, c.y + 2f), new Vector2(14.2f, 7.6f), new Color(0.55f, 0.74f, 0.4f), -9690);

        // Žogs ap laukumu
        Color zogaKrasa = new Color(0.95f, 0.95f, 0.9f);
        Maksla.IzveidotFormu("ZogsAugsa", sakne, false, new Vector2(c.x, c.y + 6.2f), new Vector2(15f, 0.18f), zogaKrasa, -9680);
        Maksla.IzveidotFormu("ZogsKreisais", sakne, false, new Vector2(c.x - 7.5f, c.y + 2f), new Vector2(0.18f, 8.4f), zogaKrasa, -9680);
        Maksla.IzveidotFormu("ZogsLabais", sakne, false, new Vector2(c.x + 7.5f, c.y + 2f), new Vector2(0.18f, 8.4f), zogaKrasa, -9680);

        // Pieci konusi rindā
        for (int i = 0; i < 5; i++)
        {
            Vector2 vieta = new Vector2(c.x - 5f + i * 2.5f, c.y + 3.6f);
            int kartiba = Mathf.RoundToInt(-vieta.y * 20f);
            Maksla.IzveidotFormu("Konuss", sakne, true, vieta + new Vector2(0, 0.25f), new Vector2(0.6f, 0.7f), new Color(1f, 0.5f, 0.1f), kartiba);
            Maksla.IzveidotFormu("KonusaSvitra", sakne, false, vieta + new Vector2(0, 0.3f), new Vector2(0.4f, 0.1f), Color.white, kartiba + 1);
        }

        // Barjera (lēkšanai) un tunelis
        Color barjerasKrasa = new Color(0.8f, 0.3f, 0.3f);
        Maksla.IzveidotFormu("BarjerasStienis", sakne, false, new Vector2(c.x - 4f, c.y + 1.4f), new Vector2(2.4f, 0.14f), Color.white, -9000);
        Maksla.IzveidotFormu("BarjerasKreisais", sakne, false, new Vector2(c.x - 5.1f, c.y + 1.2f), new Vector2(0.14f, 0.7f), barjerasKrasa, -9000);
        Maksla.IzveidotFormu("BarjerasLabais", sakne, false, new Vector2(c.x - 2.9f, c.y + 1.2f), new Vector2(0.14f, 0.7f), barjerasKrasa, -9000);
        Maksla.IzveidotFormu("Tunelis", sakne, true, new Vector2(c.x + 4f, c.y + 1.6f), new Vector2(3.2f, 1.7f), new Color(0.3f, 0.45f, 0.75f), -9000);
        Maksla.IzveidotFormu("TunelaCaurums", sakne, true, new Vector2(c.x + 4.9f, c.y + 1.6f), new Vector2(0.8f, 1.2f), new Color(0.08f, 0.1f, 0.2f), -8999);
        Maksla.IzveidotFormu("Zime", sakne, false, zona.mijiedarbibasPunkts + new Vector2(2.2f, 0.4f), new Vector2(1.6f, 0.9f), new Color(0.5f, 0.35f, 0.2f), -8000);

        Maksla.IzveidotTekstuArVaru(zona.nosaukums, sakne, new Vector2(c.x, c.y + 6.9f), 0.7f, Color.white, 8000);
    }

    static void IzveidotParku(Transform sakne, ZonasInfo zona)
    {
        Vector2 c = zona.centrs;
        Vector2 diki = new Vector2(c.x + 4f, c.y + 3.2f);

        // Dīķis (ūdens, kuram cauri nevar iet)
        Maksla.IzveidotFormu("DikaMala", sakne, true, diki, new Vector2(7.4f, 4.4f), new Color(0.75f, 0.7f, 0.5f), -9700);
        Maksla.IzveidotFormu("Diki", sakne, true, diki, new Vector2(6.8f, 3.9f), new Color(0.3f, 0.6f, 0.85f), -9690);
        Maksla.IzveidotFormu("Lapa", sakne, true, diki + new Vector2(-1f, 0.3f), new Vector2(0.7f, 0.45f), new Color(0.3f, 0.7f, 0.35f), -9680);
        Pasaule.Barjeras.Add(new Rect(diki.x - 2.8f, diki.y - 1.5f, 5.6f, 3f));

        // Sols pie mijiedarbības vietas
        Maksla.IzveidotFormu("SolaSedeklis", sakne, false, zona.mijiedarbibasPunkts + new Vector2(-2.6f, 0.2f), new Vector2(1.8f, 0.4f), new Color(0.55f, 0.37f, 0.2f), -8000);
        Maksla.IzveidotFormu("SolaMugura", sakne, false, zona.mijiedarbibasPunkts + new Vector2(-2.6f, 0.55f), new Vector2(1.8f, 0.2f), new Color(0.45f, 0.3f, 0.16f), -7999);

        // Ziedi (nejauši izkaisīti, bet vienmēr vienādi, jo "sēkla" ir 5)
        System.Random nejausais = new System.Random(5);
        Color[] ziedaKrasas =
        {
            new Color(1f, 0.5f, 0.6f),
            new Color(1f, 0.9f, 0.3f),
            new Color(0.8f, 0.6f, 1f),
            Color.white
        };
        for (int i = 0; i < 40; i++)
        {
            float x = c.x + (float)(nejausais.NextDouble() * 18 - 9);
            float y = c.y + (float)(nejausais.NextDouble() * 12 - 4);
            Vector2 vieta = new Vector2(x, y);

            // Neliekam ziedus uz ceļa un uz dīķa
            if (Mathf.Abs(vieta.x - c.x) < 1.6f) continue;
            if (Vector2.Distance(vieta, diki) < 3.9f) continue;

            Maksla.IzveidotFormu("Zieds", sakne, true, vieta, new Vector2(0.22f, 0.22f), ziedaKrasas[i % 4], -9600);
        }

        Maksla.IzveidotTekstuArVaru(zona.nosaukums, sakne, new Vector2(c.x, c.y + 6.4f), 0.7f, Color.white, 8000);
    }

    // Izkaisa kokus pa karti (tikai tur, kur tie netraucē)
    static void IzveidotKokus(Transform sakne)
    {
        System.Random nejausais = new System.Random(7);
        int ievietoti = 0;
        int meginajumi = 0;

        while (ievietoti < 70 && meginajumi < 2000)
        {
            meginajumi = meginajumi + 1;
            float x = (float)(nejausais.NextDouble() * 60 - 30);
            float y = (float)(nejausais.NextDouble() * 36 - 18);
            Vector2 vieta = new Vector2(x, y);

            if (VaiKokaVietaAiznemta(vieta))
            {
                continue;
            }
            IzveidotKoku(sakne, vieta);
            ievietoti = ievietoti + 1;
        }
    }

    // Vai šajā vietā nedrīkst likt koku?
    public static bool VaiKokaVietaAiznemta(Vector2 vieta)
    {
        // Galvenais ceļš un centrs
        if (Mathf.Abs(vieta.y) < 3.2f) return true;

        // Ceļi uz zonām un mijiedarbības vietas
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            if (Mathf.Abs(vieta.x - zona.centrs.x) < 2.6f) return true;
            if (Vector2.Distance(vieta, zona.mijiedarbibasPunkts) < 4f) return true;
        }

        // Pie ēkām un citām barjerām
        for (int i = 0; i < Pasaule.Barjeras.Count; i++)
        {
            Rect b = Pasaule.Barjeras[i];
            Rect paplasinata = new Rect(b.xMin - 2f, b.yMin - 2f, b.width + 4f, b.height + 4f);
            if (paplasinata.Contains(vieta)) return true;
        }

        // Treniņu laukums jāpatur tīrs
        ZonasInfo trenins = Pasaule.AtrastZonu(ZonasVeids.Trenins);
        if (Mathf.Abs(vieta.x - trenins.centrs.x) < 7.5f && Mathf.Abs(vieta.y - trenins.centrs.y) < 5f) return true;

        return false;
    }

    static void IzveidotKoku(Transform sakne, Vector2 vieta)
    {
        int kartiba = Mathf.RoundToInt(-vieta.y * 20f);

        GameObject koks = new GameObject("Koks");
        koks.transform.SetParent(sakne, false);
        koks.transform.localPosition = vieta;
        Transform t = koks.transform;

        Maksla.IzveidotFormu("Vara", t, true, Vector2.zero, new Vector2(1.6f, 0.5f), new Color(0, 0, 0, 0.25f), -9500);
        Maksla.IzveidotFormu("Stumbrs", t, false, new Vector2(0, 0.4f), new Vector2(0.35f, 0.8f), new Color(0.4f, 0.27f, 0.15f), kartiba);
        Maksla.IzveidotFormu("Lapotne", t, true, new Vector2(0, 1.5f), new Vector2(2.1f, 2f), new Color(0.2f, 0.5f, 0.25f), kartiba + 1);
        Maksla.IzveidotFormu("LapotnesGaisma", t, true, new Vector2(-0.3f, 1.8f), new Vector2(1.2f, 1.1f), new Color(0.28f, 0.6f, 0.3f), kartiba + 2);

        // Barjera tikai pie stumbra
        Pasaule.Barjeras.Add(new Rect(vieta.x - 0.3f, vieta.y - 0.1f, 0.6f, 0.5f));
    }
}

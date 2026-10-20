using UnityEngine;

// GameBootstrap3D: uzbūvē 3D versiju spēlei, kad nospiež Play ainā "GudraisSuns3D".
// Spēles loģika (suns, veikals, treniņš, adopcija, UI, skaņa) ir tā pati, kas 2D versijā.
// Atšķiras tikai tas, kā viss izskatās un kustas.
// Kartes koordinātes: kartes X = pasaules X, kartes Y = pasaules Z.
public static class GameBootstrap3D
{
    public const string AinasNosaukums = "GudraisSuns3D";

    static Color zalesKrasa = new Color(0.38f, 0.64f, 0.34f);
    static Color celaKrasa = new Color(0.84f, 0.78f, 0.62f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Sakums()
    {
        string pasreizejaAina = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (pasreizejaAina != AinasNosaukums)
        {
            return;
        }
        if (Object.FindAnyObjectByType<GameManager>() != null)
        {
            return;
        }

        GameObject sakne = new GameObject("GudraisSuns3D");
        GameManager spele = sakne.AddComponent<GameManager>();
        spele.irTrisDimensiju = true;

        // 1. Gaisma un kamera
        IestatitGaismu();
        Camera kamera = IestatitKameru();

        // 2. Pasaule (karte)
        GameObject pasauleObjekts = new GameObject("Pasaule");
        pasauleObjekts.transform.SetParent(sakne.transform, false);
        IzveidotKarti(pasauleObjekts.transform);

        // 3. Spēlētājs
        GameObject speletajs = Maksla3D.IzveidotSpeletaju(sakne.transform);
        speletajs.transform.position = new Vector3(-4f, 0f, -2.5f);
        speletajs.AddComponent<PlayerMovement3D>();
        spele.speletajs = speletajs.transform;

        CameraFollow3D kamerasSekosana = kamera.gameObject.GetComponent<CameraFollow3D>();
        if (kamerasSekosana == null)
        {
            kamerasSekosana = kamera.gameObject.AddComponent<CameraFollow3D>();
        }
        kamerasSekosana.merkis = speletajs.transform;
        kamerasSekosana.Pielekt();

        // 4. Suns (paslēpts, kamēr spēlētājs to nesaņem labklājības centrā)
        GameObject sunaObjekts = Maksla3D.IzveidotSunu(sakne.transform);
        spele.sunaObjekts = sunaObjekts;
        spele.suns = sunaObjekts.AddComponent<DogStatus>();
        spele.sunaProgress = sunaObjekts.AddComponent<DogProgress>();
        spele.sunaSekosana3D = sunaObjekts.AddComponent<DogFollow3D>();
        spele.sunaSekosana3D.speletajs = speletajs.transform;
        spele.sunaAnimacija3D = sunaObjekts.AddComponent<SunaAnimacija3D>();
        sunaObjekts.GetComponent<SunaIzskats>().Nokrasot(spele.suns.kruasa);
        sunaObjekts.SetActive(false);

        // 5. Spēles sistēmas (tās pašas, kas 2D versijā)
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

        // 6. Ievada izvēlne
        spele.saskarne.AtvertIzvelni(IevadaIzvelne);
    }

    // Saule: saskaņota gaisma ar ēnām, un mīksts apkārtējais apgaismojums
    static void IestatitGaismu()
    {
        Light gaisma = Object.FindAnyObjectByType<Light>();
        if (gaisma == null)
        {
            GameObject gaismasObjekts = new GameObject("Saule");
            gaisma = gaismasObjekts.AddComponent<Light>();
        }
        gaisma.type = LightType.Directional;
        gaisma.color = new Color(1f, 0.96f, 0.88f);
        gaisma.intensity = 1.1f;
        gaisma.shadows = LightShadows.Soft;
        gaisma.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.6f, 0.7f);
    }

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
        kamera.orthographic = false;
        kamera.fieldOfView = 50f;
        kamera.nearClipPlane = 0.3f;
        kamera.farClipPlane = 200f;
        kamera.clearFlags = CameraClearFlags.SolidColor;
        kamera.backgroundColor = new Color(0.55f, 0.75f, 0.95f);
        return kamera;
    }

    static IzvelnesApraksts IevadaIzvelne()
    {
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "GUDRAIS SUNS 3D";
        izvelne.teksts = "Mierīga 3D suņu aprūpes, apmācības un menedžmenta spēle.\n\n" +
                         "Saņem suņus no dzīvnieku labklājības centra, rūpējies par tiem, iemāci komandas un sagatavo adopcijai. " +
                         "Par panākumiem saņem monētas un pieredzi centra attīstīšanai. Mērķis – 10 veiksmīgas adopcijas.\n\n" +
                         "WASD – kustība   |   E – mijiedarbība   |   Pele – izvēlnes   |   ESC – pauze";
        izvelne.aizvertPogasTeksts = "Sākt spēli";
        return izvelne;
    }

    // Padara krāsu tumšāku (reizina ar skaitli mazāku par 1)
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

        IzveidotZemiUnCelus(sakne);
        IzveidotStruklaku(sakne);

        // Ēkas
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Labklajiba), 7.5f, new Color(0.93f, 0.88f, 0.76f), new Color(0.35f, 0.55f, 0.75f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Maja), 6.5f, new Color(0.96f, 0.85f, 0.7f), new Color(0.75f, 0.3f, 0.25f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Veikals), 7f, new Color(0.95f, 0.9f, 0.6f), new Color(0.85f, 0.55f, 0.2f));
        IzveidotEku(sakne, Pasaule.AtrastZonu(ZonasVeids.Klinika), 7.5f, new Color(0.95f, 0.95f, 0.97f), new Color(0.8f, 0.2f, 0.25f));

        IzveidotTreninuLaukumu(sakne, Pasaule.AtrastZonu(ZonasVeids.Trenins));
        IzveidotParku(sakne, Pasaule.AtrastZonu(ZonasVeids.Parks));
        IzveidotKokus(sakne);
    }

    // Zeme, zonu laukumi un celi (plakani, ļoti plāni kubi dažādos augstumos, lai nemirgo)
    static void IzveidotZemiUnCelus(Transform sakne)
    {
        Maksla3D.Kubs("Zale", sakne, new Vector3(0f, -0.5f, 0f), new Vector3(140f, 1f, 120f), zalesKrasa);

        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            Vector3 vieta = new Vector3(zona.centrs.x, 0.005f, zona.centrs.y);
            Vector3 izmers = new Vector3(Pasaule.ZonasPusIzmers.x * 2f, 0.01f, Pasaule.ZonasPusIzmers.y * 2f);
            Maksla3D.Kubs("Laukums_" + zona.veids, sakne, vieta, izmers, zona.krasa);
        }

        // Galvenais celš un celi uz katru zonu
        Maksla3D.Kubs("CelsH", sakne, new Vector3(0f, 0.015f, 0f), new Vector3(62f, 0.01f, 2.4f), celaKrasa);
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            float garums = Mathf.Abs(zona.mijiedarbibasPunkts.y);
            Vector3 vidus = new Vector3(zona.centrs.x, 0.015f, zona.mijiedarbibasPunkts.y * 0.5f);
            Maksla3D.Kubs("CelsV_" + zona.veids, sakne, vidus, new Vector3(2.4f, 0.01f, garums), celaKrasa);

            Vector3 paklajaVieta = new Vector3(zona.mijiedarbibasPunkts.x, 0.02f, zona.mijiedarbibasPunkts.y);
            Maksla3D.Cilindrs("Paklajs_" + zona.veids, sakne, paklajaVieta, new Vector3(2.6f, 0.005f, 1.6f), Tumsinat(celaKrasa, 0.9f));
        }
    }

    // Strūklaka kartes vidū
    static void IzveidotStruklaku(Transform sakne)
    {
        Maksla3D.Cilindrs("LaukumaGredzens", sakne, new Vector3(0f, 0.022f, 0f), new Vector3(7f, 0.005f, 5f), Tumsinat(celaKrasa, 0.95f));
        Maksla3D.Cilindrs("StruklakasPamats", sakne, new Vector3(0f, 0.15f, 0.1f), new Vector3(3.4f, 0.15f, 2.3f), new Color(0.65f, 0.65f, 0.7f));
        Maksla3D.Cilindrs("StruklakasUdens", sakne, new Vector3(0f, 0.16f, 0.1f), new Vector3(2.9f, 0.16f, 1.9f), new Color(0.35f, 0.65f, 0.9f));
        Maksla3D.Cilindrs("StruklakasStrukla", sakne, new Vector3(0f, 0.9f, 0.1f), new Vector3(0.15f, 0.7f, 0.15f), new Color(0.8f, 0.92f, 1f));
        Pasaule.Barjeras.Add(new Rect(-1.4f, -0.7f, 2.8f, 1.5f));
        Maksla3D.TekstsArVaru("Centrs", sakne, new Vector3(0f, 1.9f, -2.2f), 0.6f, new Color(1f, 1f, 1f, 0.9f));
    }

    // Uzbūvē vienu ēku. "platums" ir ēkas izmērs pa X, dziļums vienmēr ir 4.2
    static void IzveidotEku(Transform sakne, ZonasInfo zona, float platums, Color sienuKrasa, Color jumtaKrasa)
    {
        float dzilums = 4.2f;
        float augstums = 3.5f;

        // Augšējo zonu ēkas ir pie Z = 14, apakšējo pie Z = -14
        float ekasZ = -14f;
        float priekssPuse = 1f;         // uz kuru pusi ir durvis (uz galveno celu)
        if (zona.centrs.y > 0)
        {
            ekasZ = 14f;
            priekssPuse = -1f;
        }
        float priekssaZ = priekssPuse * (dzilums * 0.5f + 0.03f);

        GameObject eka = new GameObject("Eka_" + zona.veids);
        eka.transform.SetParent(sakne, false);
        eka.transform.localPosition = new Vector3(zona.centrs.x, 0f, ekasZ);
        Transform t = eka.transform;

        Maksla3D.Kubs("Siena", t, new Vector3(0f, augstums * 0.5f, 0f), new Vector3(platums, augstums, dzilums), sienuKrasa);
        Maksla3D.Kubs("Jumts", t, new Vector3(0f, augstums + 0.25f, 0f), new Vector3(platums + 0.8f, 0.5f, dzilums + 0.8f), jumtaKrasa);
        Maksla3D.Kubs("JumtaGrieda", t, new Vector3(0f, augstums + 0.65f, 0f), new Vector3(platums * 0.6f, 0.4f, dzilums * 0.6f), Tumsinat(jumtaKrasa, 0.75f));
        Maksla3D.Kubs("Skurstenis", t, new Vector3(platums * 0.3f, augstums + 1.0f, 0.5f), new Vector3(0.6f, 1.2f, 0.6f), new Color(0.55f, 0.35f, 0.3f));
        Maksla3D.Kubs("Durvis", t, new Vector3(0f, 0.8f, priekssaZ), new Vector3(1.1f, 1.6f, 0.1f), new Color(0.4f, 0.26f, 0.15f));
        Maksla3D.Kubs("LogsKreisais", t, new Vector3(-platums * 0.3f, 1.9f, priekssaZ), new Vector3(1.0f, 0.9f, 0.1f), new Color(0.7f, 0.88f, 1f));
        Maksla3D.Kubs("LogsLabais", t, new Vector3(platums * 0.3f, 1.9f, priekssaZ), new Vector3(1.0f, 0.9f, 0.1f), new Color(0.7f, 0.88f, 1f));

        // Katrai ēkai savs atpazīšanas zīmējums
        if (zona.veids == ZonasVeids.Klinika)
        {
            Maksla3D.Kubs("KrustsVertikali", t, new Vector3(0f, 2.9f, priekssaZ + priekssPuse * 0.04f), new Vector3(0.35f, 1.1f, 0.1f), Color.white);
            Maksla3D.Kubs("KrustsHorizontali", t, new Vector3(0f, 2.9f, priekssaZ + priekssPuse * 0.04f), new Vector3(1.1f, 0.35f, 0.1f), Color.white);
        }
        if (zona.veids == ZonasVeids.Veikals)
        {
            Maksla3D.Kubs("Markize", t, new Vector3(0f, 2.4f, priekssaZ + priekssPuse * 0.5f), new Vector3(platums * 0.8f, 0.15f, 1.0f), new Color(0.9f, 0.3f, 0.3f));
        }
        if (zona.veids == ZonasVeids.Labklajiba)
        {
            // Ķepas nospiedums (liels spilventiņš un trīs pirkstiņi)
            Color kepasKrasa = new Color(1f, 0.95f, 0.8f);
            float kepasZ = priekssaZ + priekssPuse * 0.04f;
            Maksla3D.Sfera("KepaVidus", t, new Vector3(0f, 2.75f, kepasZ), new Vector3(0.7f, 0.55f, 0.1f), kepasKrasa);
            Maksla3D.Sfera("KepaPirksts1", t, new Vector3(-0.4f, 3.2f, kepasZ), new Vector3(0.24f, 0.28f, 0.1f), kepasKrasa);
            Maksla3D.Sfera("KepaPirksts2", t, new Vector3(0f, 3.4f, kepasZ), new Vector3(0.24f, 0.28f, 0.1f), kepasKrasa);
            Maksla3D.Sfera("KepaPirksts3", t, new Vector3(0.4f, 3.2f, kepasZ), new Vector3(0.24f, 0.28f, 0.1f), kepasKrasa);
        }

        // Ēka ir siena, kurai cauri nevar iet (tāda pati kā 2D versijā)
        Rect barjera = new Rect(zona.centrs.x - platums * 0.5f, ekasZ - dzilums * 0.5f, platums, dzilums);
        Pasaule.Barjeras.Add(barjera);

        Maksla3D.TekstsArVaru(zona.nosaukums, sakne, new Vector3(zona.centrs.x, augstums + 2.4f, ekasZ), 0.9f, Color.white);
    }

    // Parocīga žoga līnija no viena punkta uz otru (kartes koordinātēs)
    static void IzveidotZogu(Transform sakne, Vector2 no, Vector2 lidz)
    {
        Color zogaKrasa = new Color(0.95f, 0.95f, 0.9f);
        Vector2 starpiba = lidz - no;
        float garums = starpiba.magnitude;
        int stabuSkaits = Mathf.Max(2, Mathf.RoundToInt(garums / 2.5f) + 1);

        // Stabi
        for (int i = 0; i < stabuSkaits; i++)
        {
            float dala = i / (float)(stabuSkaits - 1);
            Vector2 vieta = no + starpiba * dala;
            Maksla3D.Kubs("ZogaStabs", sakne, new Vector3(vieta.x, 0.5f, vieta.y), new Vector3(0.14f, 1f, 0.14f), zogaKrasa);
        }

        // Divas horizontālas lattas starp stabiem
        Vector2 vidus = (no + lidz) * 0.5f;
        float pagrieziens = Mathf.Atan2(starpiba.x, starpiba.y) * Mathf.Rad2Deg;
        for (int i = 0; i < 2; i++)
        {
            float augstums = 0.35f + i * 0.4f;
            GameObject latta = Maksla3D.Kubs("ZogaLatta", sakne, new Vector3(vidus.x, augstums, vidus.y), new Vector3(0.07f, 0.07f, garums), zogaKrasa);
            latta.transform.localRotation = Quaternion.Euler(0f, pagrieziens, 0f);
        }
    }

    static void IzveidotTreninuLaukumu(Transform sakne, ZonasInfo zona)
    {
        Vector2 c = zona.centrs;

        // Smilšu laukums un zālāja iekšpuse
        Maksla3D.Kubs("Laukums", sakne, new Vector3(c.x, 0.025f, c.y + 2f), new Vector3(15f, 0.01f, 8.4f), new Color(0.78f, 0.72f, 0.5f));
        Maksla3D.Kubs("LaukumaIekspuse", sakne, new Vector3(c.x, 0.035f, c.y + 2f), new Vector3(14.2f, 0.01f, 7.6f), new Color(0.55f, 0.74f, 0.4f));

        // Žogs ap laukumu (trīs malas, priekšpuse atvērta)
        IzveidotZogu(sakne, new Vector2(c.x - 7.5f, c.y + 6.2f), new Vector2(c.x + 7.5f, c.y + 6.2f));
        IzveidotZogu(sakne, new Vector2(c.x - 7.5f, c.y - 2.2f), new Vector2(c.x - 7.5f, c.y + 6.2f));
        IzveidotZogu(sakne, new Vector2(c.x + 7.5f, c.y - 2.2f), new Vector2(c.x + 7.5f, c.y + 6.2f));

        // Pieci konusi rindā
        for (int i = 0; i < 5; i++)
        {
            IzveidotKonusu(sakne, new Vector3(c.x - 5f + i * 2.5f, 0f, c.y + 3.6f));
        }

        // Barjera (lēkšanai)
        Color barjerasKrasa = new Color(0.8f, 0.3f, 0.3f);
        Maksla3D.Kubs("BarjerasKreisais", sakne, new Vector3(c.x - 5.1f, 0.45f, c.y + 1.4f), new Vector3(0.14f, 0.9f, 0.14f), barjerasKrasa);
        Maksla3D.Kubs("BarjerasLabais", sakne, new Vector3(c.x - 2.9f, 0.45f, c.y + 1.4f), new Vector3(0.14f, 0.9f, 0.14f), barjerasKrasa);
        Maksla3D.Kubs("BarjerasStienis", sakne, new Vector3(c.x - 4f, 0.8f, c.y + 1.4f), new Vector3(2.3f, 0.12f, 0.12f), Color.white);

        // Tunelis (cilindrs, kas guļ uz sāniem)
        GameObject tunelis = Maksla3D.Cilindrs("Tunelis", sakne, new Vector3(c.x + 4f, 0.75f, c.y + 1.6f), new Vector3(1.5f, 1.6f, 1.5f), new Color(0.3f, 0.45f, 0.75f));
        tunelis.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        GameObject caurums = Maksla3D.Cilindrs("TunelaCaurums", sakne, new Vector3(c.x + 5.62f, 0.75f, c.y + 1.6f), new Vector3(1.1f, 0.02f, 1.1f), new Color(0.08f, 0.1f, 0.2f));
        caurums.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Zīme pie ieejas
        Maksla3D.Kubs("ZimesStabs", sakne, new Vector3(zona.mijiedarbibasPunkts.x + 2.2f, 0.7f, zona.mijiedarbibasPunkts.y + 0.5f), new Vector3(0.1f, 1.4f, 0.1f), new Color(0.4f, 0.28f, 0.16f));
        Maksla3D.Kubs("Zime", sakne, new Vector3(zona.mijiedarbibasPunkts.x + 2.2f, 1.4f, zona.mijiedarbibasPunkts.y + 0.5f), new Vector3(1.6f, 0.8f, 0.1f), new Color(0.5f, 0.35f, 0.2f));

        Maksla3D.TekstsArVaru(zona.nosaukums, sakne, new Vector3(c.x, 2.2f, c.y + 6.4f), 0.9f, Color.white);
    }

    // Konuss no sakrautiem cilindriem (Unity nav gatavas konusa formas)
    static void IzveidotKonusu(Transform sakne, Vector3 vieta)
    {
        Color oranza = new Color(1f, 0.5f, 0.1f);
        GameObject konuss = new GameObject("Konuss");
        konuss.transform.SetParent(sakne, false);
        konuss.transform.localPosition = vieta;
        Transform t = konuss.transform;

        Maksla3D.Cilindrs("Pamats", t, new Vector3(0f, 0.03f, 0f), new Vector3(0.8f, 0.03f, 0.8f), oranza);
        Maksla3D.Cilindrs("Apaksa", t, new Vector3(0f, 0.17f, 0f), new Vector3(0.55f, 0.11f, 0.55f), oranza);
        Maksla3D.Cilindrs("Svitra", t, new Vector3(0f, 0.3f, 0f), new Vector3(0.42f, 0.03f, 0.42f), Color.white);
        Maksla3D.Cilindrs("Vidus", t, new Vector3(0f, 0.43f, 0f), new Vector3(0.34f, 0.11f, 0.34f), oranza);
        Maksla3D.Cilindrs("Augsa", t, new Vector3(0f, 0.62f, 0f), new Vector3(0.18f, 0.1f, 0.18f), oranza);
    }

    static void IzveidotParku(Transform sakne, ZonasInfo zona)
    {
        Vector2 c = zona.centrs;
        Vector2 diki = new Vector2(c.x + 4f, c.y + 3.2f);

        // Dīķis (ūdens, kuram cauri nevar iet)
        Maksla3D.Cilindrs("DikaMala", sakne, new Vector3(diki.x, 0.03f, diki.y), new Vector3(7.4f, 0.03f, 4.4f), new Color(0.75f, 0.7f, 0.5f));
        Maksla3D.Cilindrs("Diki", sakne, new Vector3(diki.x, 0.04f, diki.y), new Vector3(6.8f, 0.04f, 3.9f), new Color(0.3f, 0.6f, 0.85f));
        Maksla3D.Cilindrs("Lapa", sakne, new Vector3(diki.x - 1f, 0.09f, diki.y + 0.3f), new Vector3(0.8f, 0.01f, 0.5f), new Color(0.3f, 0.7f, 0.35f));
        Pasaule.Barjeras.Add(new Rect(diki.x - 2.8f, diki.y - 1.5f, 5.6f, 3f));

        // Sols pie mijiedarbības vietas
        Color kokaKrasa = new Color(0.55f, 0.37f, 0.2f);
        Vector3 solaVieta = new Vector3(zona.mijiedarbibasPunkts.x - 2.6f, 0f, zona.mijiedarbibasPunkts.y + 0.2f);
        Maksla3D.Kubs("SolaSedeklis", sakne, solaVieta + new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.1f, 0.5f), kokaKrasa);
        Maksla3D.Kubs("SolaMugura", sakne, solaVieta + new Vector3(0f, 0.8f, 0.25f), new Vector3(1.8f, 0.5f, 0.1f), Tumsinat(kokaKrasa, 0.85f));
        Maksla3D.Kubs("SolaKajaK", sakne, solaVieta + new Vector3(-0.75f, 0.22f, 0f), new Vector3(0.1f, 0.44f, 0.4f), Tumsinat(kokaKrasa, 0.7f));
        Maksla3D.Kubs("SolaKajaL", sakne, solaVieta + new Vector3(0.75f, 0.22f, 0f), new Vector3(0.1f, 0.44f, 0.4f), Tumsinat(kokaKrasa, 0.7f));

        // Ziedi (tāda pati izkārtojuma "sēkla" kā 2D versijā)
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
            float z = c.y + (float)(nejausais.NextDouble() * 12 - 4);
            Vector2 vieta = new Vector2(x, z);

            if (Mathf.Abs(vieta.x - c.x) < 1.6f) continue;
            if (Vector2.Distance(vieta, diki) < 3.9f) continue;

            Maksla3D.Cilindrs("ZiedaKats", sakne, new Vector3(x, 0.12f, z), new Vector3(0.03f, 0.12f, 0.03f), new Color(0.25f, 0.55f, 0.25f));
            Maksla3D.Sfera("Zieds", sakne, new Vector3(x, 0.28f, z), new Vector3(0.25f, 0.2f, 0.25f), ziedaKrasas[i % 4]);
        }

        Maksla3D.TekstsArVaru(zona.nosaukums, sakne, new Vector3(c.x, 2.2f, c.y + 6f), 0.9f, Color.white);
    }

    // Izkaisa kokus pa karti (tāpat kā 2D versijā: tās pašas vietas)
    static void IzveidotKokus(Transform sakne)
    {
        System.Random nejausais = new System.Random(7);
        int ievietoti = 0;
        int meginajumi = 0;

        while (ievietoti < 70 && meginajumi < 2000)
        {
            meginajumi = meginajumi + 1;
            float x = (float)(nejausais.NextDouble() * 60 - 30);
            float z = (float)(nejausais.NextDouble() * 36 - 18);
            Vector2 vieta = new Vector2(x, z);

            if (GameBootstrap.VaiKokaVietaAiznemta(vieta))
            {
                continue;
            }
            IzveidotKoku(sakne, vieta);
            ievietoti = ievietoti + 1;
        }
    }

    static void IzveidotKoku(Transform sakne, Vector2 vieta)
    {
        GameObject koks = new GameObject("Koks");
        koks.transform.SetParent(sakne, false);
        koks.transform.localPosition = new Vector3(vieta.x, 0f, vieta.y);
        Transform t = koks.transform;

        Maksla3D.Cilindrs("Stumbrs", t, new Vector3(0f, 0.9f, 0f), new Vector3(0.4f, 0.9f, 0.4f), new Color(0.4f, 0.27f, 0.15f));
        Maksla3D.Sfera("Lapotne", t, new Vector3(0f, 2.7f, 0f), new Vector3(2.4f, 2.2f, 2.4f), new Color(0.2f, 0.5f, 0.25f));
        Maksla3D.Sfera("LapotnesAugsa", t, new Vector3(0f, 3.6f, 0f), new Vector3(1.6f, 1.5f, 1.6f), new Color(0.28f, 0.6f, 0.3f));

        // Barjera tikai pie stumbra
        Pasaule.Barjeras.Add(new Rect(vieta.x - 0.3f, vieta.y - 0.1f, 0.6f, 0.5f));
    }
}

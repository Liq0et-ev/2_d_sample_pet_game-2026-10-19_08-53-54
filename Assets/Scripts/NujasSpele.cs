using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// NujasSpele: minispele parka.
// Ap gredzenu griežas sarkana adata. Jānospiež Space (vai jānoklikšķina), kad adata ir zaļajā zonā.
//   zaļa zona        = labs metiens, suns kļūst laimīgāks
//   dzeltens centrs  = perfekts metiens, suns kļūst ļoti laimīgs
//   garām            = mēģinājums zaudēts
// Vienmēr ir 5 mēģinājumi un visi ir jāizmanto.
public class NujasSpele : MonoBehaviour
{
    const int Meginajumi = 5;
    const float GredzenaRadiuss = 170f;
    static Vector2 vidus = new Vector2(0.5f, 0.5f);

    GameObject logs;
    RectTransform adata;
    RectTransform zalaisLoks;
    RectTransform dzeltenaisLoks;
    Image zalaisAttels;
    Image dzeltenaisAttels;
    Text infoTeksts;
    Text rezultataTeksts;

    bool spelesNotiek = false;
    bool navKustibas = false;        // true, kamēr rādām rezultātu pēc metiena
    bool pedejaisMetiens = false;

    int meginajumaNumurs = 0;
    int labiMetieni = 0;
    int perfektiMetieni = 0;
    float garastavoklaPieaugums = 0f;

    float adatasLeknis = 0f;         // adatas leņķis grādos (0 = uz augšu, pulksteņrādītāja virzienā)
    float adatasAtrums = 0f;         // grādi sekundē
    float zonasSakums = 0f;          // zaļās zonas sākuma leņķis
    float zonasPlatums = 0f;         // zaļās zonas platums grādos
    float perfektaPlatums = 0f;      // dzeltenās zonas platums grādos
    float taimeris = 0f;
    float gaidisanasLaiks = 0f;      // īss brīdis spēles sākumā, lai iepriekšējais klikšķis neskaitītos

    // Sāk spēli (izsauc parka izvelne)
    public void Sakt()
    {
        GameManager spele = GameManager.Galvenais;
        spele.saskarne.AizvertIzvelni();

        RectTransform panelis;
        logs = spele.saskarne.IzveidotLogu(new Vector2(700, 760), out panelis);
        spele.saskarne.logaAtcelsana = null;      // ESC nedara neko - jāizmanto visi mēģinājumi

        Vector2 augsaVidu = new Vector2(0.5f, 1f);
        Vector2 apaksaVidu = new Vector2(0.5f, 0f);

        Text virsraksts = spele.saskarne.IzveidotTekstu(panelis, "Nūjas mešana", 40, TextAnchor.MiddleCenter,
            new Color(1f, 0.88f, 0.4f), augsaVidu, augsaVidu, new Vector2(0, -16), new Vector2(660, 50));
        virsraksts.fontStyle = FontStyle.Bold;
        infoTeksts = spele.saskarne.IzveidotTekstu(panelis, "", 26, TextAnchor.MiddleCenter, Color.white,
            augsaVidu, augsaVidu, new Vector2(0, -72), new Vector2(660, 40));

        Vector2 centraVieta = new Vector2(0, -10);

        // Baltais gredzens
        Image gredzens = spele.saskarne.IzveidotAttelu("Gredzens", panelis, vidus, vidus, centraVieta,
            new Vector2(GredzenaRadiuss * 2, GredzenaRadiuss * 2), new Color(1, 1, 1, 0.85f));
        gredzens.sprite = Maksla.Gredzens();

        // Zaļā un dzeltenā zona (loki uz gredzena)
        zalaisAttels = IzveidotLoku(panelis, centraVieta, GredzenaRadiuss * 2 + 18, new Color(0.3f, 0.85f, 0.4f, 0.95f));
        dzeltenaisAttels = IzveidotLoku(panelis, centraVieta, GredzenaRadiuss * 2 + 34, new Color(1f, 0.85f, 0.2f, 1f));
        zalaisLoks = zalaisAttels.rectTransform;
        dzeltenaisLoks = dzeltenaisAttels.rectTransform;

        // "Space" taustiņš gredzena vidū
        Image taustinaRamis = spele.saskarne.IzveidotAttelu("TaustinaRamis", panelis, vidus, vidus, centraVieta,
            new Vector2(250, 76), Color.white);
        Image taustinaFons = spele.saskarne.IzveidotAttelu("TaustinaFons", taustinaRamis.transform, vidus, vidus,
            Vector2.zero, new Vector2(240, 66), Color.black);
        spele.saskarne.IzveidotTekstu(taustinaFons.transform, "Space", 34, TextAnchor.MiddleCenter,
            new Color(0.65f, 0.65f, 0.65f), vidus, vidus, Vector2.zero, new Vector2(240, 66));

        // Sarkanā adata (tās apakša ir gredzena centrā, tāpēc pivot ir apakšā)
        adata = spele.saskarne.IzveidotAttelu("Adata", panelis, vidus, new Vector2(0.5f, 0f), centraVieta,
            new Vector2(7, GredzenaRadiuss + 14), new Color(0.9f, 0.15f, 0.15f)).rectTransform;

        rezultataTeksts = spele.saskarne.IzveidotTekstu(panelis, "", 34, TextAnchor.MiddleCenter, Color.white,
            apaksaVidu, apaksaVidu, new Vector2(0, 62), new Vector2(660, 50));
        spele.saskarne.IzveidotTekstu(panelis,
            "Space vai klikšķis – mest nūju, kad adata ir zaļajā zonā. Dzeltenais centrs = perfekts metiens!", 20,
            TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.75f), apaksaVidu, apaksaVidu, new Vector2(0, 14), new Vector2(660, 40));

        meginajumaNumurs = 0;
        labiMetieni = 0;
        perfektiMetieni = 0;
        garastavoklaPieaugums = 0f;
        pedejaisMetiens = false;
        navKustibas = false;
        spelesNotiek = true;
        gaidisanasLaiks = 0.5f;
        NakamaisMeginajums();
    }

    // Izveido loku (daļu no gredzena), ko pēc tam pagriežam un saīsinām
    Image IzveidotLoku(RectTransform panelis, Vector2 vieta, float izmers, Color krasa)
    {
        Image attels = GameManager.Galvenais.saskarne.IzveidotAttelu("Loks", panelis, vidus, vidus, vieta,
            new Vector2(izmers, izmers), krasa);
        attels.sprite = Maksla.Gredzens();
        attels.type = Image.Type.Filled;
        attels.fillMethod = Image.FillMethod.Radial360;
        attels.fillOrigin = (int)Image.Origin360.Top;
        attels.fillClockwise = true;
        return attels;
    }

    // Sagatavo nākamo mēģinājumu: nejauša zona, ātrāka adata, šaurāka zona
    void NakamaisMeginajums()
    {
        meginajumaNumurs = meginajumaNumurs + 1;

        zonasSakums = Random.Range(0f, 360f);
        zonasPlatums = Mathf.Lerp(72f, 42f, (meginajumaNumurs - 1) / (float)(Meginajumi - 1));
        perfektaPlatums = zonasPlatums * 0.26f;
        adatasAtrums = 220f + 28f * (meginajumaNumurs - 1);
        adatasLeknis = Mathf.Repeat(zonasSakums + 180f, 360f);     // adata sāk zonai pretējā pusē

        // Uzstādām lokus: fillAmount = cik daļa no apļa, rotation = kur tas sākas
        zalaisAttels.fillAmount = zonasPlatums / 360f;
        zalaisLoks.localRotation = Quaternion.Euler(0, 0, -zonasSakums);
        dzeltenaisAttels.fillAmount = perfektaPlatums / 360f;
        float dzeltenaSakums = zonasSakums + (zonasPlatums - perfektaPlatums) * 0.5f;
        dzeltenaisLoks.localRotation = Quaternion.Euler(0, 0, -dzeltenaSakums);
        adata.localRotation = Quaternion.Euler(0, 0, -adatasLeknis);

        rezultataTeksts.text = "";
        navKustibas = false;
        AtjaunotInfo();
    }

    void AtjaunotInfo()
    {
        infoTeksts.text = "Mēģinājums " + meginajumaNumurs + "/" + Meginajumi +
                          "   |   Labi: " + labiMetieni + "   Perfekti: " + perfektiMetieni;
    }

    // Spēlētājs nospieda Space vai peli - mēģinām mest nūju
    void MestNuju()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        // Cik grādu ir adata pēc zonas sākuma? (0 līdz 360)
        float starpiba = Mathf.Repeat(adatasLeknis - zonasSakums, 360f);

        // Katrs metiens nogurdina suni un padara mazliet izsalkušāku
        suns.Mainit(0, -1, -3, 0);

        if (starpiba <= zonasPlatums)
        {
            // Trāpījām zaļajā zonā. Vai arī dzeltenajā centrā?
            float attalumsNoCentra = Mathf.Abs(starpiba - zonasPlatums * 0.5f);
            bool perfekti = attalumsNoCentra <= perfektaPlatums * 0.5f;

            if (perfekti)
            {
                perfektiMetieni = perfektiMetieni + 1;
                suns.Mainit(0, 0, 0, 16);
                garastavoklaPieaugums = garastavoklaPieaugums + 16f;
                spele.ekonomika.Nopelnit(3, true);
                spele.sunaProgress.PieliktPieredzi(10);
                rezultataTeksts.text = "Perfekti! " + suns.vards + " ir ļoti laimīgs!";
                rezultataTeksts.color = new Color(1f, 0.9f, 0.3f);
                spele.skana.Atskanot(Skana.Limenis);
                spele.SunaUzplaiksne(new Color(1f, 0.9f, 0.3f));
                spele.TricetKameru(0.15f, 0.25f);
                spele.Triecienapauze(0.07f);
            }
            else
            {
                labiMetieni = labiMetieni + 1;
                suns.Mainit(0, 0, 0, 8);
                garastavoklaPieaugums = garastavoklaPieaugums + 8f;
                spele.ekonomika.Nopelnit(1, true);
                spele.sunaProgress.PieliktPieredzi(4);
                rezultataTeksts.text = "Labi! " + suns.vards + " priecājas.";
                rezultataTeksts.color = new Color(0.5f, 1f, 0.55f);
                spele.skana.Atskanot(Skana.Labi);
                spele.SunaUzplaiksne(new Color(0.5f, 1f, 0.5f));
            }
            spele.skana.Atskanot(Skana.Riet);
            spele.SunaPriecajas();
        }
        else
        {
            // Garām - mēģinājums zaudēts
            rezultataTeksts.text = "Garām! Mēģinājums zaudēts.";
            rezultataTeksts.color = new Color(1f, 0.5f, 0.5f);
            spele.skana.Atskanot(Skana.Slikti);
            spele.SunaUzplaiksne(new Color(1f, 0.4f, 0.4f));
        }

        AtjaunotInfo();
        navKustibas = true;
        taimeris = 0.9f;

        if (meginajumaNumurs >= Meginajumi)
        {
            pedejaisMetiens = true;
            taimeris = 1.8f;
            int garam = Meginajumi - labiMetieni - perfektiMetieni;
            rezultataTeksts.text = rezultataTeksts.text + "\nKopā: Labi " + labiMetieni + ", Perfekti " + perfektiMetieni + ", garām " + garam;
        }
    }

    // Aizver spēli un parāda kopsavilkumu
    void BeigtSpeli()
    {
        GameManager spele = GameManager.Galvenais;
        spelesNotiek = false;
        spele.saskarne.AizvertLogu(logs);
        logs = null;

        spele.speletajaProgress.PieliktPieredzi(2 * (labiMetieni + perfektiMetieni));
        spele.saskarne.Pazinojums("Nūjas mešana pabeigta: " + perfektiMetieni + " perfekti, " + labiMetieni +
                                  " labi. Garastāvoklis +" + Mathf.RoundToInt(garastavoklaPieaugums) + ".");
    }

    void Update()
    {
        if (spelesNotiek == false)
        {
            return;
        }

        float dt = Time.unscaledDeltaTime;
        if (gaidisanasLaiks > 0f)
        {
            gaidisanasLaiks = gaidisanasLaiks - dt;
        }

        // Pēc metiena īsi gaidām un tad ejam uz nākamo mēģinājumu (vai beidzam)
        if (navKustibas)
        {
            taimeris = taimeris - dt;
            if (taimeris <= 0f)
            {
                if (pedejaisMetiens)
                {
                    BeigtSpeli();
                }
                else
                {
                    NakamaisMeginajums();
                }
            }
            return;
        }

        // Adata griežas pulksteņrādītāja virzienā
        adatasLeknis = Mathf.Repeat(adatasLeknis + adatasAtrums * dt, 360f);
        adata.localRotation = Quaternion.Euler(0, 0, -adatasLeknis);

        if (gaidisanasLaiks > 0f)
        {
            return;
        }

        Keyboard tastatura = Keyboard.current;
        Mouse pele = Mouse.current;
        bool nospiestsSpace = tastatura != null && tastatura.spaceKey.wasPressedThisFrame;
        bool nospiestaPele = pele != null && pele.leftButton.wasPressedThisFrame;
        if (nospiestsSpace || nospiestaPele)
        {
            MestNuju();
        }
    }
}

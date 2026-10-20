using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TrainingSystem: komandu trenēšana un minispele "Aportēšana".
public class TrainingSystem : MonoBehaviour
{
    // Vai treniņā izmantot gardumu (palielina veiksmes iespēju)
    bool izmantotGardumu = false;

    public void AtvertIzvelni()
    {
        GameManager.Galvenais.saskarne.AtvertIzvelni(TreninaIzvelne);
    }

    // Atgriež iemeslu, kāpēc nevar trenēt, vai tukšu tekstu, ja var
    string KapecNevarTrenet()
    {
        DogStatus suns = GameManager.Galvenais.suns;
        if (suns.slims) return suns.vards + " ir slims – vispirms uz klīniku.";
        if (suns.energija < 15f) return suns.vards + " ir pārāk noguris – ļauj atpūsties.";
        if (suns.bariba < 15f) return suns.vards + " ir izsalcis – pabaro to.";
        return "";
    }

    IzvelnesApraksts TreninaIzvelne()
    {
        GameManager spele = GameManager.Galvenais;
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Treniņu laukums";

        if (spele.irSuns == false)
        {
            izvelne.teksts = "Tev nav suņa. Saņem to Labklājības centrā.";
            return izvelne;
        }

        DogStatus suns = spele.suns;
        DogProgress progress = spele.sunaProgress;
        EconomyManager ekonomika = spele.ekonomika;

        izvelne.teksts = suns.vards + " – " + progress.limenis + ". līmenis (" + progress.pieredze + "/" + progress.PieredzesVajag() + " pieredze)\n" +
                         "Enerģija " + Mathf.RoundToInt(suns.energija) + "  |  Garastāvoklis " + Mathf.RoundToInt(suns.garastavoklis) +
                         "  |  Gardumi: " + ekonomika.gardumi +
                         "\nLaukuma līmenis: " + ekonomika.laukumaLimenis + "/" + EconomyManager.MaksimalaisUzlabojums;

        string aizliegums = KapecNevarTrenet();
        bool varTrenet = aizliegums == "";
        if (varTrenet == false)
        {
            izvelne.teksts = izvelne.teksts + "\n" + aizliegums;
        }

        // Pa vienai pogai katrai no četrām komandām
        for (int i = 0; i < 4; i++)
        {
            Komanda komanda = (Komanda)i;
            string statuss;
            if (progress.VaiApgusta(komanda))
            {
                statuss = "apgūta";
            }
            else
            {
                statuss = Mathf.RoundToInt(progress.prasmes[i]) + "%";
            }
            string teksts = "Trenēt: " + KomandasNosaukumi.Dabut(komanda) + "  (" + statuss + ")";
            izvelne.pogas.Add(new IzvelnesPoga(teksts, Trenet, i, varTrenet));
        }

        // Gardumu poga (ieslēdz / izslēdz)
        string gardumaTeksts = "Izmantot gardumu treniņā: NĒ";
        if (izmantotGardumu)
        {
            gardumaTeksts = "Izmantot gardumu treniņā: JĀ";
        }
        izvelne.pogas.Add(new IzvelnesPoga(gardumaTeksts, ParslegtGardumu));

        // Minispēle "Aportēšana"
        bool varAportet = varTrenet && ekonomika.irRotallieta && suns.energija >= 20f;
        string aportesTeksts = "Minispēle: Aportēšana";
        if (ekonomika.irRotallieta == false)
        {
            aportesTeksts = "Minispēle: Aportēšana (vajag rotaļlietu)";
        }
        izvelne.pogas.Add(new IzvelnesPoga(aportesTeksts, SaktAportesanu, varAportet));

        // Laukuma uzlabojums
        if (ekonomika.laukumaLimenis < EconomyManager.MaksimalaisUzlabojums)
        {
            int jaunaisLimenis = ekonomika.laukumaLimenis + 1;
            int cena = 100 * jaunaisLimenis;
            int vajadzigsLimenis = jaunaisLimenis + 2;
            bool varUzlabot = spele.speletajaProgress.limenis >= vajadzigsLimenis;
            string uzlabojumaTeksts = "Uzlabot laukumu → " + jaunaisLimenis + ". līm. (" + cena + " monētas, spēlētāja līm. " + vajadzigsLimenis + ")";
            izvelne.pogas.Add(new IzvelnesPoga(uzlabojumaTeksts, UzlabotLaukumu, varUzlabot));
        }
        return izvelne;
    }

    void ParslegtGardumu()
    {
        izmantotGardumu = !izmantotGardumu;
    }

    void UzlabotLaukumu()
    {
        GameManager spele = GameManager.Galvenais;
        int jaunaisLimenis = spele.ekonomika.laukumaLimenis + 1;
        int cena = 100 * jaunaisLimenis;

        if (spele.ekonomika.IztereMonetas(cena))
        {
            spele.ekonomika.laukumaLimenis = jaunaisLimenis;
            spele.saskarne.Pazinojums("Treniņu laukums uzlabots!");
            spele.skana.Atskanot(Skana.Labi);
        }
    }

    // Aprēķina, cik liela ir iespēja, ka suns apgūs komandu (no 0.1 līdz 0.95)
    float VeiksmesIespeja(Komanda komanda)
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;
        EconomyManager ekonomika = spele.ekonomika;

        // Pamats: labāks garastāvoklis, vairāk enerģijas un augstāks limenis palīdz
        float iespeja = 0.45f + 0.20f * (suns.garastavoklis / 100f) + 0.15f * (suns.energija / 100f) + 0.03f * spele.sunaProgress.limenis;

        // Vēlākas komandas ir grūtākas
        iespeja = iespeja - 0.05f * (int)komanda;

        // Uzlabojumi un konusi
        iespeja = iespeja + 0.04f * ekonomika.laukumaLimenis;
        if (ekonomika.irKonusi)
        {
            iespeja = iespeja + 0.08f;
        }

        // Gardums palīdz
        if (izmantotGardumu && ekonomika.gardumi > 0)
        {
            iespeja = iespeja + 0.20f;
        }

        // Rakstura ietekme
        if (suns.raksturs == Raksturs.Aktivs && komanda == Komanda.PieManis)
        {
            iespeja = iespeja + 0.10f;
        }
        if (suns.raksturs == Raksturs.Mierigs && komanda != Komanda.PieManis)
        {
            iespeja = iespeja + 0.08f;
        }
        if (suns.raksturs == Raksturs.Bailigs && suns.garastavoklis < 55f)
        {
            iespeja = iespeja - 0.15f;
        }

        return Mathf.Clamp(iespeja, 0.1f, 0.95f);
    }

    // Trenē vienu komandu. Skaitlis "komandasNumurs" nāk no pogas (0 = Sēdi, 1 = Guli, ...)
    void Trenet(int komandasNumurs)
    {
        GameManager spele = GameManager.Galvenais;
        Komanda komanda = (Komanda)komandasNumurs;

        string aizliegums = KapecNevarTrenet();
        if (aizliegums != "")
        {
            spele.saskarne.Pazinojums(aizliegums);
            return;
        }

        DogStatus suns = spele.suns;
        DogProgress progress = spele.sunaProgress;
        EconomyManager ekonomika = spele.ekonomika;

        // Ja izmantojam gardumu, tas tiek patērēts
        bool gardumsIzmantots = false;
        if (izmantotGardumu && ekonomika.gardumi > 0)
        {
            ekonomika.gardumi = ekonomika.gardumi - 1;
            gardumsIzmantots = true;
        }

        // Vai šoreiz izdevās?
        bool izdevas = Random.value < VeiksmesIespeja(komanda);

        // Treniņš vienmēr nogurdina: aktīvs suns nogurst ātrāk, mierīgs lēnāk
        float energijasCena = 8f;
        if (suns.raksturs == Raksturs.Aktivs) energijasCena = energijasCena * 1.4f;
        if (suns.raksturs == Raksturs.Mierigs) energijasCena = energijasCena * 0.8f;
        float garastavoklaPieaugums = 0f;
        if (gardumsIzmantots) garastavoklaPieaugums = 4f;
        suns.Mainit(0, -4, -energijasCena, garastavoklaPieaugums);

        if (izdevas)
        {
            TreninsIzdevas(spele, suns, progress, komanda);
        }
        else
        {
            progress.prasmes[komandasNumurs] = Mathf.Min(100f, progress.prasmes[komandasNumurs] + 4f);
            suns.Mainit(0, 0, 0, -3);
            spele.skana.Atskanot(Skana.Slikti);
            spele.saskarne.Pazinojums(suns.vards + " nesaprata. Mēģini vēlreiz (gardums un laba noskaņa palīdz).");
        }
    }

    // Tiek izsaukta, kad treniņš izdevās
    void TreninsIzdevas(GameManager spele, DogStatus suns, DogProgress progress, Komanda komanda)
    {
        int numurs = (int)komanda;
        bool jauBijaApgusta = progress.VaiApgusta(komanda);

        // Prasme pieaug par 20 līdz 30 punktiem
        progress.prasmes[numurs] = Mathf.Min(100f, progress.prasmes[numurs] + Random.Range(20f, 30f));
        progress.PieliktPieredzi(15);
        spele.speletajaProgress.PieliktPieredzi(5);
        spele.ekonomika.Nopelnit(3);
        spele.skana.Atskanot(Skana.Labi);

        if (jauBijaApgusta == false && progress.VaiApgusta(komanda))
        {
            // Komanda tikko tika apgūta - bonuss!
            spele.ekonomika.Nopelnit(10, true);
            spele.saskarne.Pazinojums(suns.vards + " apguva komandu \"" + KomandasNosaukumi.Dabut(komanda) + "\"! (+10 monētas)");
            spele.skana.Atskanot(Skana.Riet);
        }
        else
        {
            spele.saskarne.Pazinojums("Veiksmīgi! \"" + KomandasNosaukumi.Dabut(komanda) + "\" – " + Mathf.RoundToInt(progress.prasmes[numurs]) + "%");
        }
    }

    // =================================================================
    // MINISPĒLE "APORTĒŠANA"
    // Marķieris kustas pa joslu. Jānospiež pogu, kad tas ir zaļajā zonā. Trīs raundi.
    // =================================================================
    GameObject aportesLogs;
    RectTransform markieris;
    RectTransform zalaZona;
    Text aportesInfo;
    bool aporteNotiek = false;
    bool aporteBeigusies = false;
    int raunds = 0;
    int trapijumi = 0;
    float zonasCentrs;       // zonas vieta joslā (0 līdz 1)
    float zonasPlatums;      // zonas platums (daļa no joslas)
    float aportesLaiks = 0f;
    float markieraAtrums = 1f;
    float beiguTaimeris = 0f;
    const float JoslasPlatums = 640f;

    void SaktAportesanu()
    {
        GameManager spele = GameManager.Galvenais;
        spele.saskarne.AizvertIzvelni();

        RectTransform panelis;
        aportesLogs = spele.saskarne.IzveidotLogu(new Vector2(760, 360), out panelis);
        spele.saskarne.logaAtcelsana = BeigtAportesanu;      // ESC pārtrauc spēli

        Vector2 augsaVidu = new Vector2(0.5f, 1f);
        Vector2 vidus = new Vector2(0.5f, 0.5f);

        Text virsraksts = spele.saskarne.IzveidotTekstu(panelis, "Aportēšana", 40, TextAnchor.MiddleCenter,
            new Color(1f, 0.88f, 0.4f), augsaVidu, augsaVidu, new Vector2(0, -16), new Vector2(700, 50));
        virsraksts.fontStyle = FontStyle.Bold;
        aportesInfo = spele.saskarne.IzveidotTekstu(panelis, "", 26, TextAnchor.MiddleCenter, Color.white,
            augsaVidu, augsaVidu, new Vector2(0, -76), new Vector2(700, 70));

        // Josla, zaļā zona un marķieris
        RectTransform josla = spele.saskarne.IzveidotAttelu("Josla", panelis, vidus, vidus, new Vector2(0, -10),
            new Vector2(JoslasPlatums, 44), new Color(0, 0, 0, 0.6f)).rectTransform;
        zalaZona = spele.saskarne.IzveidotAttelu("Zona", josla, new Vector2(0, 0.5f), vidus, Vector2.zero,
            new Vector2(100, 44), new Color(0.3f, 0.8f, 0.35f, 0.9f)).rectTransform;
        markieris = spele.saskarne.IzveidotAttelu("Markieris", josla, new Vector2(0, 0.5f), vidus, Vector2.zero,
            new Vector2(10, 64), Color.white).rectTransform;

        spele.saskarne.IzveidotPogu(panelis, "MEST!  (atstarpe)", MestAporti, new Vector2(0.5f, 0),
            new Vector2(0, 70), new Vector2(360, 64));

        raunds = 0;
        trapijumi = 0;
        aporteBeigusies = false;
        beiguTaimeris = 0f;
        aportesLaiks = 0f;
        aporteNotiek = true;
        NakamaisRaunds();
    }

    void NakamaisRaunds()
    {
        GameManager spele = GameManager.Galvenais;
        raunds = raunds + 1;

        // Katrā raundā marķieris iet ātrāk un zona kļūst šaurāka
        markieraAtrums = 0.7f + 0.25f * raunds;
        if (spele.suns.energija < 40f)
        {
            markieraAtrums = markieraAtrums - 0.1f;
        }

        zonasPlatums = 0.22f - 0.03f * raunds + 0.03f * spele.sunaProgress.limenis;
        if (spele.suns.raksturs == Raksturs.Aktivs)
        {
            zonasPlatums = zonasPlatums + 0.02f;
        }
        zonasPlatums = Mathf.Clamp(zonasPlatums, 0.1f, 0.3f);

        zonasCentrs = Random.Range(0.2f, 0.8f);
        zalaZona.sizeDelta = new Vector2(zonasPlatums * JoslasPlatums, 44);
        zalaZona.anchoredPosition = new Vector2(zonasCentrs * JoslasPlatums, 0);
        aportesLaiks = 0f;
        aportesInfo.text = "Raunds " + raunds + "/3  –  nospied MEST!, kad atzīme ir zaļajā zonā.   Trāpījumi: " + trapijumi;
    }

    // Spēlētājs nospiež "MEST!" vai atstarpi
    void MestAporti()
    {
        if (aporteNotiek == false || aporteBeigusies)
        {
            return;
        }
        GameManager spele = GameManager.Galvenais;

        // Kur šobrīd ir marķieris (0 līdz 1)?
        float markieraVieta = Mathf.PingPong(aportesLaiks * markieraAtrums, 1f);
        bool trapija = Mathf.Abs(markieraVieta - zonasCentrs) <= zonasPlatums * 0.5f;

        if (trapija)
        {
            trapijumi = trapijumi + 1;
            spele.skana.Atskanot(Skana.Labi);
        }
        else
        {
            spele.skana.Atskanot(Skana.Slikti);
        }

        if (raunds >= 3)
        {
            aporteBeigusies = true;
            beiguTaimeris = 1.4f;
            string beiguTeksts = "lieliski!";
            if (trapijumi < 2)
            {
                beiguTeksts = "nākamreiz labāk!";
            }
            aportesInfo.text = "Trāpījumi: " + trapijumi + "/3  –  " + beiguTeksts;
        }
        else
        {
            NakamaisRaunds();
        }
    }

    // Aizver minispēli un izsniedz balvas
    void BeigtAportesanu()
    {
        if (aporteNotiek == false)
        {
            return;
        }
        aporteNotiek = false;

        GameManager spele = GameManager.Galvenais;
        spele.saskarne.AizvertLogu(aportesLogs);
        aportesLogs = null;

        if (raunds == 0)
        {
            return;
        }

        spele.suns.Mainit(0, -4, -15, 4 * trapijumi - 2);
        if (trapijumi > 0)
        {
            spele.ekonomika.Nopelnit(4 * trapijumi);
            spele.sunaProgress.PieliktPieredzi(12 * trapijumi);
            spele.speletajaProgress.PieliktPieredzi(5 * trapijumi);
            spele.skana.Atskanot(Skana.Riet);
        }

        string balva = ".";
        if (trapijumi > 0)
        {
            balva = "  (+" + (4 * trapijumi) + " monētas)";
        }
        spele.saskarne.Pazinojums("Aportēšana: " + trapijumi + "/3 trāpījumi" + balva);
    }

    void Update()
    {
        if (aporteNotiek == false)
        {
            return;
        }

        aportesLaiks = aportesLaiks + Time.unscaledDeltaTime;

        // Pēc pēdējā raunda pagaidām mazliet un aizveram
        if (aporteBeigusies)
        {
            beiguTaimeris = beiguTaimeris - Time.unscaledDeltaTime;
            if (beiguTaimeris <= 0f)
            {
                BeigtAportesanu();
            }
            return;
        }

        // Pārvietojam marķieri
        float vieta = Mathf.PingPong(aportesLaiks * markieraAtrums, 1f);
        markieris.anchoredPosition = new Vector2(vieta * JoslasPlatums, 0);

        Keyboard tastatura = Keyboard.current;
        if (tastatura != null && tastatura.spaceKey.wasPressedThisFrame)
        {
            MestAporti();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

// WelfareAndAdoption: labklajibas centrs.
// Šeit speletajs sanem suni, nokārto adopcijas parbaudijumu un tiek veiktas aprupes parbaudes.
public class WelfareAndAdoption : MonoBehaviour
{
    // Iespejamie suņu vardi un kruasas
    static string[] sunuVardi = { "Reksis", "Bella", "Maksis", "Luna", "Čaks", "Mila", "Brūnis", "Ziba", "Toms", "Pūka", "Rūfus", "Dūka" };
    static Color[] sunuKrasas =
    {
        new Color(0.85f, 0.58f, 0.30f),
        new Color(0.55f, 0.38f, 0.25f),
        new Color(0.92f, 0.88f, 0.78f),
        new Color(0.35f, 0.33f, 0.35f),
        new Color(0.80f, 0.70f, 0.40f),
        new Color(0.75f, 0.45f, 0.30f)
    };

    // Adopcijas parbaudijuma posmi
    enum TestaPosms
    {
        Nav,            // parbaudijums nenotiek
        Raditajs,       // speletajs redz komandu secibu un to iegaumē
        Ievade,         // speletajs atkārto secibu
        Rezultats       // paradam rezultatu
    }

    TestaPosms posms = TestaPosms.Nav;
    List<Komanda> seciba = new List<Komanda>();       // komandas, kas jāatkārto
    int ievadesIndekss = 0;                             // cik komandas jau atkārtotas
    float raditajaTaimeris = 0f;                        // cik ilgi vēl rādām secibu
    string rezultataTeksts = "";
    bool rezultatsVeiksmigs = false;

    // Aprupes parbaudes
    float parbaudesTaimeris = 0f;
    int bridinajumi = 0;
    const float ParbaudesIntervals = 90f;

    public void AtvertIzvelni()
    {
        posms = TestaPosms.Nav;
        GameManager.Galvenais.saskarne.AtvertIzvelni(IzvelnesSaturs);
    }

    // =================================================================
    // Gatavibas parbaude
    // =================================================================

    // Atgriež sarakstu ar to, kas vēl nav izpildīts adopcijai
    public List<string> KasTrukst()
    {
        List<string> saraksts = new List<string>();
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        int apgustas = spele.sunaProgress.ApgustoKomanduSkaits();
        if (apgustas < 4) saraksts.Add("Apgūtas komandas: " + apgustas + "/4");
        if (suns.veseliba < 70f) saraksts.Add("Veselība ≥ 70 (tagad " + Mathf.RoundToInt(suns.veseliba) + ")");
        if (suns.slims) saraksts.Add("Suns ir slims");
        if (suns.garastavoklis < 50f) saraksts.Add("Garastāvoklis ≥ 50 (tagad " + Mathf.RoundToInt(suns.garastavoklis) + ")");
        if (suns.bariba < 40f) saraksts.Add("Suns nedrīkst būt izsalcis (paēdis ≥ 40)");
        if (suns.energija < 20f) saraksts.Add("Enerģija ≥ 20 (tagad " + Mathf.RoundToInt(suns.energija) + ")");
        return saraksts;
    }

    public bool VaiGatavsAdopcijai()
    {
        if (GameManager.Galvenais.irSuns == false)
        {
            return false;
        }
        return KasTrukst().Count == 0;
    }

    // =================================================================
    // Izvelne
    // =================================================================
    IzvelnesApraksts IzvelnesSaturs()
    {
        GameManager spele = GameManager.Galvenais;
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Labklājības centrs";

        // Ja notiek parbaudijums, rādam to
        if (posms == TestaPosms.Raditajs || posms == TestaPosms.Ievade)
        {
            return TestaIzvelne();
        }

        // Parbaudijuma rezultats
        if (posms == TestaPosms.Rezultats)
        {
            if (rezultatsVeiksmigs)
            {
                izvelne.virsraksts = "Pārbaudījums nokārtots!";
            }
            else
            {
                izvelne.virsraksts = "Pārbaudījums neizdevās";
            }
            izvelne.teksts = rezultataTeksts;
            izvelne.aizvertPogasTeksts = "Labi";
            return izvelne;
        }

        // Ja nav suņa, var saņemt jaunu
        if (spele.irSuns == false)
        {
            if (spele.adopcijas >= GameManager.UzvarasAdopcijas)
            {
                izvelne.teksts = "Darbinieks: Apsveicam! Tu esi sasniedzis lielāko atzinību – " + spele.adopcijas + " veiksmīgas adopcijas. Vari turpināt brīvajā režīmā.";
            }
            else if (spele.adopcijas == 0)
            {
                izvelne.teksts = "Darbinieks: Sveiks! Mēs palīdzam suņiem atrast jaunas ģimenes. Uzticēsim tev pirmo suni – parūpējies par to, iemāci 4 komandas un sagatavo adopcijai.";
            }
            else
            {
                izvelne.teksts = "Darbinieks: Paldies par darbu! Adopcijas: " + spele.adopcijas + "/" + GameManager.UzvarasAdopcijas + ". Esam gatavi uzticēt nākamo suni.";
            }
            izvelne.pogas.Add(new IzvelnesPoga("Saņemt suni", SanemtSuni));
            return izvelne;
        }

        // Ja ir suns, rādam, kas vēl jāizdara pirms adopcijas
        DogStatus suns = spele.suns;
        List<string> truksts = KasTrukst();
        izvelne.teksts = "Darbinieks: " + suns.vards + " – " + DogStatus.RaksturaNosaukums(suns.raksturs) + ", " + spele.sunaProgress.limenis + ". līmenis.\n";
        if (truksts.Count == 0)
        {
            izvelne.teksts = izvelne.teksts + "Visi nosacījumi izpildīti – var sākt adopcijas pārbaudījumu!";
        }
        else
        {
            izvelne.teksts = izvelne.teksts + "Vēl jāizpilda:";
            for (int i = 0; i < truksts.Count; i++)
            {
                izvelne.teksts = izvelne.teksts + "\n  • " + truksts[i];
            }
        }
        izvelne.pogas.Add(new IzvelnesPoga("Sākt adopcijas pārbaudījumu", SaktParbaudijumu, truksts.Count == 0));
        return izvelne;
    }

    // Piešķir jaunu nejaušu suni
    void SanemtSuni()
    {
        GameManager spele = GameManager.Galvenais;

        string vards = sunuVardi[Random.Range(0, sunuVardi.Length)];
        Color krasa = sunuKrasas[Random.Range(0, sunuKrasas.Length)];
        Raksturs raksturs = (Raksturs)Random.Range(0, 3);

        spele.suns.Inicializet(vards, raksturs, krasa);
        spele.sunaProgress.Atiestatit();
        spele.irSuns = true;

        // Pirmajam sunim ir pamācība (vajag pabarot), nākamajiem vairs ne
        spele.irPabarots = spele.adopcijas > 0;

        spele.sunaObjekts.SetActive(true);
        spele.sunaSekosana.Pielekt();
        spele.skana.Atskanot(Skana.Riet);
        spele.sunaAnimacija.Priecasanas();
        spele.saskarne.Pazinojums("Saņēmi suni " + vards + "! " + spele.suns.RaksturaApraksts());
        spele.saskarne.AizvertIzvelni();
        bridinajumi = 0;
    }

    // =================================================================
    // Adopcijas parbaudijums: jāiegaumē un jāatkārto komandu seciba
    // =================================================================
    void SaktParbaudijumu()
    {
        GameManager spele = GameManager.Galvenais;

        // Jo vairāk adopciju, jo garāka seciba (no 4 līdz 7 komandām)
        int garums = Mathf.Min(7, 4 + spele.adopcijas / 2);
        seciba.Clear();
        for (int i = 0; i < garums; i++)
        {
            seciba.Add((Komanda)Random.Range(0, 4));
        }

        ievadesIndekss = 0;
        posms = TestaPosms.Raditajs;
        raditajaTaimeris = 1.2f * garums + 1.5f;       // cik sekundes rādīt secību
    }

    IzvelnesApraksts TestaIzvelne()
    {
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Adopcijas pārbaudījums";
        izvelne.rakstitAizvertPogu = false;

        if (posms == TestaPosms.Raditajs)
        {
            string komandasTeksts = "";
            for (int i = 0; i < seciba.Count; i++)
            {
                if (i > 0)
                {
                    komandasTeksts = komandasTeksts + "  →  ";
                }
                komandasTeksts = komandasTeksts + KomandasNosaukumi.Dabut(seciba[i]);
            }
            izvelne.teksts = "Iegaumē komandu secību, kuru suns jāizpilda:\n\n" + komandasTeksts + "\n\nPēc brīža tā pazudīs...";
            return izvelne;
        }

        // Ievades posms: četras pogas, pa vienai katrai komandai
        izvelne.teksts = "Atkārto komandu secību! Izpildītas komandas: " + ievadesIndekss + "/" + seciba.Count;
        for (int i = 0; i < 4; i++)
        {
            Komanda komanda = (Komanda)i;
            izvelne.pogas.Add(new IzvelnesPoga(KomandasNosaukumi.Dabut(komanda), AtbildetUzKomandu, i));
        }
        return izvelne;
    }

    // Speletajs nospieda vienu no komandu pogam
    void AtbildetUzKomandu(int komandasNumurs)
    {
        GameManager spele = GameManager.Galvenais;
        Komanda izveleta = (Komanda)komandasNumurs;

        if (izveleta != seciba[ievadesIndekss])
        {
            BeigtParbaudijumu(false, "Darbinieks: Nepareiza komanda secībā. " + spele.suns.vards + " vēl nav gatavs. Pamēģini vēlreiz, kad būsi gatavs.");
            return;
        }

        spele.skana.Atskanot(Skana.Labi);
        ievadesIndekss = ievadesIndekss + 1;
        if (ievadesIndekss >= seciba.Count)
        {
            BeigtParbaudijumu(true, "");
        }
    }

    void BeigtParbaudijumu(bool veiksmigi, string teksts)
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;
        rezultatsVeiksmigs = veiksmigi;
        posms = TestaPosms.Rezultats;

        // Ja neizdevās, suns mazliet nogurst un nobažās
        if (veiksmigi == false)
        {
            suns.Mainit(0, 0, -5, -3);
            spele.skana.Atskanot(Skana.Slikti);
            rezultataTeksts = teksts;
            return;
        }

        // Ja izdevās, suns tiek adoptēts
        int sunaLimenis = spele.sunaProgress.limenis;
        int balva = 60 + 15 * sunaLimenis + 5 * spele.adopcijas;
        spele.adopcijas = spele.adopcijas + 1;
        spele.ekonomika.Nopelnit(balva);
        spele.speletajaProgress.PieliktPieredzi(40 + 10 * sunaLimenis);

        string vards = suns.vards;
        spele.irSuns = false;
        spele.sunaObjekts.SetActive(false);
        spele.skana.Atskanot(Skana.Limenis);

        rezultataTeksts = vards + " tika veiksmīgi adoptēts un ieguva jaunu ģimeni!\nAtlīdzība: " + balva + " monētas.\nAdopcijas: " + spele.adopcijas + "/" + GameManager.UzvarasAdopcijas;
        if (spele.adopcijas == GameManager.UzvarasAdopcijas)
        {
            rezultataTeksts = rezultataTeksts + "\n\nTu sasniedzi galveno mērķi – labklājības centrs piešķir tev īpašu atzinību! Vari turpināt brīvajā režīmā.";
        }
    }

    // =================================================================
    // Katru kadru: parbaudijuma taimeris un periodiskas aprupes parbaudes
    // =================================================================
    void Update()
    {
        GameManager spele = GameManager.Galvenais;

        // 1. Kamēr rāda komandu secību, skaitām laiku (spēles laiks ir apturēts, tāpēc unscaled)
        if (posms == TestaPosms.Raditajs)
        {
            raditajaTaimeris = raditajaTaimeris - Time.unscaledDeltaTime;
            if (raditajaTaimeris <= 0f)
            {
                if (spele.saskarne.IzvelneAtverta())
                {
                    posms = TestaPosms.Ievade;
                    spele.saskarne.AtjaunotIzvelni();
                }
                else
                {
                    posms = TestaPosms.Nav;       // spēlētājs aizvēra izvēlni, pārbaudījums atcelts
                }
            }
        }

        // 2. Aprupes parbaudes (tikai kad spele notiek)
        if (spele.irSuns == false || spele.saskarne.IrBloketa())
        {
            return;
        }

        parbaudesTaimeris = parbaudesTaimeris + Time.deltaTime;
        if (parbaudesTaimeris < ParbaudesIntervals)
        {
            return;
        }
        parbaudesTaimeris = 0f;
        VeiktAprupesParbaudi();
    }

    // Labklajibas centrs parbauda, vai suns tiek labi aprupets.
    // Nogurums vien nekad netiek sodits.
    void VeiktAprupesParbaudi()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        bool nolaidiba = suns.bariba < 15f || suns.veseliba < 30f || suns.garastavoklis < 10f;

        if (nolaidiba)
        {
            bridinajumi = bridinajumi + 1;
            if (bridinajumi == 1)
            {
                spele.saskarne.Pazinojums("Labklājības centra brīdinājums: " + suns.vards + " netiek pienācīgi aprūpēts! Uzlabo stāvokli.");
                spele.skana.Atskanot(Skana.Slikti);
            }
            else
            {
                int sods = Mathf.Min(spele.ekonomika.monetas, 30);
                spele.ekonomika.monetas = spele.ekonomika.monetas - sods;
                spele.saskarne.Pazinojums("Labklājības centrs piemēroja naudas sodu (" + sods + " monētas) par nolaidību.");
                spele.skana.Atskanot(Skana.Slikti);
            }
        }
        else if (bridinajumi > 0)
        {
            bridinajumi = bridinajumi - 1;
        }
    }
}

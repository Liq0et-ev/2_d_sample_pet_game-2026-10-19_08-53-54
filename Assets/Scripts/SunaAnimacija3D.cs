using UnityEngine;

// SunaAnimacija3D: suna kustibas 3D pasaule (tas pats princips kā SunaAnimacija 2D versijā).
// Skripts katru kadru pagriež un pakustina suna kājas, asti, ausis, galvu un ermeni.
// Pozu saraksts (SunaStavoklis) ir tas pats, kas 2D versijā.
public class SunaAnimacija3D : MonoBehaviour
{
    // Suna dalas
    Transform izskats;
    Transform ermenis;
    Transform galvaGrupa;
    Transform acis;
    Transform ausisK;
    Transform ausisL;
    Transform aste;
    Transform kajaPK;      // priekšējā kreisā
    Transform kajaPL;      // priekšējā labā
    Transform kajaAK;      // aizmugures kreisā
    Transform kajaAL;      // aizmugures labā
    bool sagatavots = false;

    // Pasreizejais stavoklis un taimeri
    SunaStavoklis stavoklis = SunaStavoklis.Stav;
    float stavesanasLaiks = 0f;
    float ediensTaimeris = 0f;
    float lecienaTaimeris = 0f;
    float gaitasFaze = 0f;
    float laiks = 0f;

    const float LecienaIlgums = 0.9f;

    // Pasreizejas vertibas (lēnām tuvojas mērķa vērtībām)
    float ermenaPitch = 0f;         // ķermeņa sasvēršana (negatīvs = priekša uz augšu)
    float ermenaY = 0f;
    float ermenaZ = 0f;
    float galvasPitch = 0f;         // galvas sasvēršana (pozitīvs = deguns uz leju)
    float galvasY = 0f;
    float galvasZ = 0f;
    float priekskajuLeknis = 0f;    // negatīvs = kājas uz priekšu
    float aizmugKajuLeknis = 0f;
    float ausuLenkis = 0f;
    float acuMerogs = 1f;
    float astesPitch = 35f;

    // Merka vertibas
    float merkaErmenaPitch = 0f;
    float merkaErmenaY = 0f;
    float merkaErmenaZ = 0f;
    float merkaGalvasPitch = 0f;
    float merkaGalvasY = 0f;
    float merkaGalvasZ = 0f;
    float merkaPriekskajuLeknis = 0f;
    float merkaAizmugKajuLeknis = 0f;
    float merkaAusuLenkis = 0f;
    float merkaAcuMerogs = 1f;
    float merkaAstesPitch = 35f;

    // Sakuma pozicijas (lai pievienotu nobides pie tām)
    Vector3 ermenaBaze;
    Vector3 galvasBaze;

    // Citi skripti izsauc šīs funkcijas
    public void Ediens()
    {
        ediensTaimeris = 1.4f;
    }

    public void Priecasanas()
    {
        lecienaTaimeris = LecienaIlgums;
    }

    void Sagatavot()
    {
        izskats = transform.Find("Izskats");
        ermenis = transform.Find("Izskats/Ermenis");
        galvaGrupa = transform.Find("Izskats/GalvaGrupa");
        acis = transform.Find("Izskats/GalvaGrupa/Acis");
        ausisK = transform.Find("Izskats/GalvaGrupa/AusisK");
        ausisL = transform.Find("Izskats/GalvaGrupa/AusisL");
        aste = transform.Find("Izskats/Aste");
        kajaPK = transform.Find("Izskats/KajaPK");
        kajaPL = transform.Find("Izskats/KajaPL");
        kajaAK = transform.Find("Izskats/KajaAK");
        kajaAL = transform.Find("Izskats/KajaAL");
        ermenaBaze = ermenis.localPosition;
        galvasBaze = galvaGrupa.localPosition;
        sagatavots = true;
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele == null || spele.irSuns == false)
        {
            return;
        }
        if (sagatavots == false)
        {
            Sagatavot();
        }

        float dt = Time.deltaTime;
        laiks = laiks + dt;

        NoteiktStavokli(spele, dt);
        UzstaditMerkus(spele);
        TuvinatVertibas(dt);
        PielietotUzDalam(spele, dt);
    }

    // ---------------------------------------------------------------
    // 1. Kuru pozu suns šobrīd izmanto?
    // ---------------------------------------------------------------
    void NoteiktStavokli(GameManager spele, float dt)
    {
        if (ediensTaimeris > 0f)
        {
            ediensTaimeris = ediensTaimeris - dt;
            stavoklis = SunaStavoklis.Ed;
            return;
        }
        if (lecienaTaimeris > 0f)
        {
            lecienaTaimeris = lecienaTaimeris - dt;
            stavoklis = SunaStavoklis.Lec;
            return;
        }

        if (spele.SunaKustas())
        {
            stavesanasLaiks = 0f;
            stavoklis = SunaStavoklis.Iet;
            return;
        }

        stavesanasLaiks = stavesanasLaiks + dt;
        bool nogurisVaiSlims = spele.suns.energija < 20f || spele.suns.slims;

        if (nogurisVaiSlims && stavesanasLaiks > 3f)
        {
            stavoklis = SunaStavoklis.Guli;
        }
        else if (stavesanasLaiks > 5f)
        {
            stavoklis = SunaStavoklis.Sed;
        }
        else
        {
            stavoklis = SunaStavoklis.Stav;
        }
    }

    // ---------------------------------------------------------------
    // 2. Kādas vērtības vajag katrai pozai?
    // ---------------------------------------------------------------
    void UzstaditMerkus(GameManager spele)
    {
        // Sākums: parasta stāvēšana
        merkaErmenaPitch = 0f;
        merkaErmenaY = 0f;
        merkaErmenaZ = 0f;
        merkaGalvasPitch = 0f;
        merkaGalvasY = 0f;
        merkaGalvasZ = 0f;
        merkaPriekskajuLeknis = 0f;
        merkaAizmugKajuLeknis = 0f;
        merkaAusuLenkis = 0f;
        merkaAcuMerogs = 1f;
        merkaAstesPitch = 35f;

        if (stavoklis == SunaStavoklis.Sed)
        {
            // Aizmugure zemāk, priekšpuse paceļas, aizmugures kājas ir uz priekšu
            merkaErmenaPitch = -25f;
            merkaErmenaY = -0.2f;
            merkaErmenaZ = -0.12f;
            merkaGalvasPitch = 12f;
            merkaGalvasY = 0.1f;
            merkaGalvasZ = -0.12f;
            merkaAizmugKajuLeknis = -85f;
            merkaAstesPitch = 5f;
        }
        else if (stavoklis == SunaStavoklis.Guli)
        {
            // Suns guļ: viss zemu, kājas saliektas, ausis nolaistas, acis aizvērtas
            merkaErmenaY = -0.3f;
            merkaGalvasY = -0.4f;
            merkaGalvasZ = 0.1f;
            merkaGalvasPitch = 15f;
            merkaPriekskajuLeknis = -80f;
            merkaAizmugKajuLeknis = -80f;
            merkaAusuLenkis = 35f;
            merkaAcuMerogs = 0.1f;
            merkaAstesPitch = 0f;
        }
        else if (stavoklis == SunaStavoklis.Ed)
        {
            // Galva zemu pie bļodas
            merkaErmenaPitch = 6f;
            merkaGalvasPitch = 40f;
            merkaGalvasY = -0.42f;
            merkaGalvasZ = 0.2f;
        }
        else if (stavoklis == SunaStavoklis.Lec)
        {
            merkaAusuLenkis = -25f;
        }

        // Slims suns noliec galvu un ausis
        bool slimsTikaiStav = stavoklis == SunaStavoklis.Stav || stavoklis == SunaStavoklis.Iet || stavoklis == SunaStavoklis.Sed;
        if (spele.suns.slims && slimsTikaiStav)
        {
            merkaGalvasPitch = merkaGalvasPitch + 20f;
            merkaGalvasY = merkaGalvasY - 0.15f;
            merkaAusuLenkis = 25f;
            merkaAstesPitch = merkaAstesPitch - 25f;
        }
    }

    // ---------------------------------------------------------------
    // 3. Pamazām virzām pašreizējās vērtības uz mērķa vērtībām
    // ---------------------------------------------------------------
    void TuvinatVertibas(float dt)
    {
        float k = 1f - Mathf.Exp(-10f * dt);

        ermenaPitch = Mathf.Lerp(ermenaPitch, merkaErmenaPitch, k);
        ermenaY = Mathf.Lerp(ermenaY, merkaErmenaY, k);
        ermenaZ = Mathf.Lerp(ermenaZ, merkaErmenaZ, k);
        galvasPitch = Mathf.Lerp(galvasPitch, merkaGalvasPitch, k);
        galvasY = Mathf.Lerp(galvasY, merkaGalvasY, k);
        galvasZ = Mathf.Lerp(galvasZ, merkaGalvasZ, k);
        priekskajuLeknis = Mathf.Lerp(priekskajuLeknis, merkaPriekskajuLeknis, k);
        aizmugKajuLeknis = Mathf.Lerp(aizmugKajuLeknis, merkaAizmugKajuLeknis, k);
        ausuLenkis = Mathf.Lerp(ausuLenkis, merkaAusuLenkis, k);
        acuMerogs = Mathf.Lerp(acuMerogs, merkaAcuMerogs, k);
        astesPitch = Mathf.Lerp(astesPitch, merkaAstesPitch, k);
    }

    // ---------------------------------------------------------------
    // 4. Uzliekam vērtības pašām suņa daļām
    // ---------------------------------------------------------------
    void PielietotUzDalam(GameManager spele, float dt)
    {
        // Gaitas fāze: skrienot ātri mainās, stāvot - lēnām
        if (stavoklis == SunaStavoklis.Iet)
        {
            gaitasFaze = gaitasFaze + dt * 16f;
        }
        else
        {
            gaitasFaze = gaitasFaze + dt * 3f;
        }

        float visasY = 0f;          // viss suns uz augšu (lēciens, lēkāšana skrienot)
        float kajuSvarsts = 0f;     // kāju švīkstēšana uz priekšu/atpakaļ (grādi)
        float galvasPapildusPitch = 0f;
        float ausuLidojums = 0f;

        // Astes luncinasana: laimigs suns luncina vairak
        float luncinasStiprums = 12f;
        float luncinasAtrums = 6f;
        if (spele.suns.garastavoklis > 50f)
        {
            luncinasStiprums = 40f;
        }
        if (stavoklis == SunaStavoklis.Lec)
        {
            luncinasStiprums = 55f;
            luncinasAtrums = 20f;
        }
        if (stavoklis == SunaStavoklis.Guli || spele.suns.slims)
        {
            luncinasStiprums = 6f;
        }
        float astesLuncisana = Mathf.Sin(laiks * luncinasAtrums) * luncinasStiprums;

        // ---- Skriešana ----
        if (stavoklis == SunaStavoklis.Iet)
        {
            visasY = Mathf.Abs(Mathf.Sin(gaitasFaze)) * 0.06f;
            kajuSvarsts = Mathf.Sin(gaitasFaze) * 40f;
            ausuLidojums = Mathf.Sin(gaitasFaze * 2f) * 10f;
            galvasPapildusPitch = Mathf.Sin(gaitasFaze * 2f) * 3f;
        }

        // ---- Elpošana, kad nestaigā ----
        float elpa = 1f;
        if (stavoklis != SunaStavoklis.Iet)
        {
            elpa = 1f + Mathf.Sin(laiks * 2.2f) * 0.03f;
        }

        // ---- Ēšana: galva kustas uz augšu un leju ----
        if (stavoklis == SunaStavoklis.Ed)
        {
            galvasPapildusPitch = Mathf.Sin(laiks * 14f) * 8f;
        }

        // ---- Lēkšana: divi lēcieni pēc kārtas ----
        if (stavoklis == SunaStavoklis.Lec)
        {
            float progress = 1f - lecienaTaimeris / LecienaIlgums;
            visasY = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 2f)) * 0.7f;
            kajuSvarsts = Mathf.Sin(laiks * 20f) * 15f;
        }

        // ---- Visa suņa pacelšana ----
        izskats.localPosition = new Vector3(0f, visasY, 0f);

        // ---- Ermenis ----
        ermenis.localPosition = ermenaBaze + new Vector3(0f, ermenaY, ermenaZ);
        ermenis.localRotation = Quaternion.Euler(ermenaPitch, 0f, 0f);
        ermenis.localScale = new Vector3(0.62f * elpa, 0.6f * elpa, 1.15f);

        // ---- Galva (kopā ar degunu, ausīm un acīm) ----
        galvaGrupa.localPosition = galvasBaze + new Vector3(0f, galvasY, galvasZ);
        galvaGrupa.localRotation = Quaternion.Euler(galvasPitch + galvasPapildusPitch, 0f, 0f);
        acis.localScale = new Vector3(1f, acuMerogs, 1f);

        // ---- Ausis (griežas ap Z asi uz āru) ----
        ausisK.localRotation = Quaternion.Euler(0f, 0f, -ausuLenkis - ausuLidojums);
        ausisL.localRotation = Quaternion.Euler(0f, 0f, ausuLenkis + ausuLidojums);

        // ---- Kājas: pa diagonāli pretējās kustas vienlaikus ----
        kajaPK.localRotation = Quaternion.Euler(priekskajuLeknis + kajuSvarsts, 0f, 0f);
        kajaAL.localRotation = Quaternion.Euler(aizmugKajuLeknis + kajuSvarsts, 0f, 0f);
        kajaPL.localRotation = Quaternion.Euler(priekskajuLeknis - kajuSvarsts, 0f, 0f);
        kajaAK.localRotation = Quaternion.Euler(aizmugKajuLeknis - kajuSvarsts, 0f, 0f);

        // ---- Aste: pitch paceļ/nolaiž, luncinasana ir pagriezieni sāņus ----
        aste.localRotation = Quaternion.Euler(astesPitch, astesLuncisana, 0f);
    }
}

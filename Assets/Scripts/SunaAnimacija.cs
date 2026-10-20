using UnityEngine;

// Suna stavokli (pozas)
public enum SunaStavoklis
{
    Stav,       // stav uz vietas
    Iet,        // skrien pakal speletajam
    Sed,        // sēž (ja ilgi nekas nenotiek)
    Guli,       // guļ (noguris vai slims suns)
    Ed,         // ēd
    Lec         // priecīgi lēkā
}

// SunaAnimacija: suna kustibas, kas tiek zimetas ar kodu (nav vajadzigi animaciju faili).
// Skripts katru kadru pakustina suna dalas: kajas, ausis, asti, galvu un ermeni.
public class SunaAnimacija : MonoBehaviour
{
    // Maza klase, kas atceras, kur suna dala atradas sakuma
    class DalasBaze
    {
        public Transform dala;
        public Vector3 pozicija;
        public Vector3 merogs;
        public float rotacija;

        public DalasBaze(Transform jaunaDala)
        {
            dala = jaunaDala;
            pozicija = jaunaDala.localPosition;
            merogs = jaunaDala.localScale;
            rotacija = jaunaDala.localEulerAngles.z;
        }
    }

    // Suna dalas
    DalasBaze ermenis;
    DalasBaze galva;
    DalasBaze ausisA;
    DalasBaze ausisB;
    DalasBaze purns;
    DalasBaze deguns;
    DalasBaze acs;
    DalasBaze kajaA;       // pakaļkāja (kreisā)
    DalasBaze kajaB;       // priekškāja (labā)
    DalasBaze aste;
    bool sagatavots = false;

    // Pasreizejais stavoklis un taimeri
    SunaStavoklis stavoklis = SunaStavoklis.Stav;
    float stavesanasLaiks = 0f;      // cik sekundes suns jau stav uz vietas
    float ediensTaimeris = 0f;       // cik vel ilgi ed
    float lecienaTaimeris = 0f;      // cik vel ilgi leka
    float gaitasFaze = 0f;           // skaitlis, kas nosaka, kura kaja ir uz priekšu
    float laiks = 0f;                // vispārējs laiks mazajām kustībām

    const float LecienaIlgums = 0.9f;

    // Pasreizejas vertibas (tās lēnām tuvojas mērķa vērtībām, tāpēc pāreja starp pozām ir mīksta)
    float ermenaRotacija = 0f;
    Vector2 ermenaNobide = Vector2.zero;
    Vector2 galvasNobide = Vector2.zero;
    float kajuMerogs = 1f;
    float ausuRotacija = 0f;
    float acuMerogs = 1f;
    float astesRotacija = 35f;
    Vector2 astesNobide = Vector2.zero;

    // Merka vertibas (uz tam virzamies)
    float merkaErmenaRotacija = 0f;
    Vector2 merkaErmenaNobide = Vector2.zero;
    Vector2 merkaGalvasNobide = Vector2.zero;
    float merkaKajuMerogs = 1f;
    float merkaAusuRotacija = 0f;
    float merkaAcuMerogs = 1f;
    float merkaAstesRotacija = 35f;
    Vector2 merkaAstesNobide = Vector2.zero;

    // ---------------------------------------------------------------
    // Citi skripti var izsaukt šīs funkcijas, lai suns kaut ko izdara
    // ---------------------------------------------------------------

    // Suns paēd (nolaiž galvu pie bļodas)
    public void Ediens()
    {
        ediensTaimeris = 1.4f;
    }

    // Suns priecigi palēkā (piemēram, pēc veiksmīga treniņa)
    public void Priecasanas()
    {
        lecienaTaimeris = LecienaIlgums;
    }

    // ---------------------------------------------------------------
    // Sagatavosana: atrodam visas suna dalas pec nosaukuma
    // ---------------------------------------------------------------
    void Sagatavot()
    {
        ermenis = new DalasBaze(transform.Find("Ermenis"));
        galva = new DalasBaze(transform.Find("Galva"));
        ausisA = new DalasBaze(transform.Find("AusisA"));
        ausisB = new DalasBaze(transform.Find("AusisB"));
        purns = new DalasBaze(transform.Find("Purns"));
        deguns = new DalasBaze(transform.Find("Deguns"));
        acs = new DalasBaze(transform.Find("Acs"));
        kajaA = new DalasBaze(transform.Find("KajaA"));
        kajaB = new DalasBaze(transform.Find("KajaB"));
        aste = new DalasBaze(transform.Find("Aste"));
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
        // Ēšana un lēkāšana ir īslaicīgas un ir svarīgākas par visu citu
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

        // Ja suns skrien, tad tas iet
        if (spele.SunaKustas())
        {
            stavesanasLaiks = 0f;
            stavoklis = SunaStavoklis.Iet;
            return;
        }

        // Ja suns stav uz vietas, tad ar laiku apsēžas vai apguļas
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
        // Sakums: parasta stāvēšana
        merkaErmenaRotacija = 0f;
        merkaErmenaNobide = Vector2.zero;
        merkaGalvasNobide = Vector2.zero;
        merkaKajuMerogs = 1f;
        merkaAusuRotacija = 0f;
        merkaAcuMerogs = 1f;
        merkaAstesRotacija = 35f;
        merkaAstesNobide = Vector2.zero;

        if (stavoklis == SunaStavoklis.Iet)
        {
            // Galva nedaudz uz priekšu
            merkaGalvasNobide = new Vector2(0.05f, 0f);
        }
        else if (stavoklis == SunaStavoklis.Sed)
        {
            // Ķermeņa aizmugure iet zemāk, priekšpuse paceļas
            merkaErmenaRotacija = 22f;
            merkaErmenaNobide = new Vector2(-0.06f, -0.1f);
            merkaGalvasNobide = new Vector2(-0.1f, 0.12f);
            merkaKajuMerogs = 0.7f;
            merkaAstesRotacija = 75f;
            merkaAstesNobide = new Vector2(-0.05f, -0.25f);
        }
        else if (stavoklis == SunaStavoklis.Guli)
        {
            // Suns guļ: viss zemu, ausis nolaistas, acis aizvērtas
            merkaErmenaNobide = new Vector2(0f, -0.27f);
            merkaGalvasNobide = new Vector2(0.12f, -0.3f);
            merkaKajuMerogs = 0.25f;
            merkaAusuRotacija = 35f;
            merkaAcuMerogs = 0.15f;
            merkaAstesRotacija = 80f;
            merkaAstesNobide = new Vector2(0f, -0.28f);
        }
        else if (stavoklis == SunaStavoklis.Ed)
        {
            // Galva zemu pie bļodas
            merkaErmenaRotacija = -8f;
            merkaErmenaNobide = new Vector2(0.02f, -0.04f);
            merkaGalvasNobide = new Vector2(0.2f, -0.4f);
        }
        else if (stavoklis == SunaStavoklis.Lec)
        {
            // Ausis atliecas atpakaļ no prieka
            merkaAusuRotacija = -20f;
        }

        // Slims suns noliec galvu un ausis (ja nav jau citas pozas, kas to dara)
        bool slimsTikaiStav = stavoklis == SunaStavoklis.Stav || stavoklis == SunaStavoklis.Iet || stavoklis == SunaStavoklis.Sed;
        if (spele.suns.slims && slimsTikaiStav)
        {
            merkaGalvasNobide = merkaGalvasNobide + new Vector2(0.04f, -0.15f);
            merkaAusuRotacija = 25f;
            merkaAstesRotacija = merkaAstesRotacija + 30f;
        }
    }

    // ---------------------------------------------------------------
    // 3. Pamazām virzām pašreizējās vērtības uz mērķa vērtībām
    // ---------------------------------------------------------------
    void TuvinatVertibas(float dt)
    {
        // Jo lielāks skaitlis, jo ātrāk notiek pāreja
        float k = 1f - Mathf.Exp(-12f * dt);

        ermenaRotacija = Mathf.Lerp(ermenaRotacija, merkaErmenaRotacija, k);
        ermenaNobide = Vector2.Lerp(ermenaNobide, merkaErmenaNobide, k);
        galvasNobide = Vector2.Lerp(galvasNobide, merkaGalvasNobide, k);
        kajuMerogs = Mathf.Lerp(kajuMerogs, merkaKajuMerogs, k);
        ausuRotacija = Mathf.Lerp(ausuRotacija, merkaAusuRotacija, k);
        acuMerogs = Mathf.Lerp(acuMerogs, merkaAcuMerogs, k);
        astesRotacija = Mathf.Lerp(astesRotacija, merkaAstesRotacija, k);
        astesNobide = Vector2.Lerp(astesNobide, merkaAstesNobide, k);
    }

    // ---------------------------------------------------------------
    // 4. Uzliekam vērtības pašām suņa daļām
    // ---------------------------------------------------------------
    void PielietotUzDalam(GameManager spele, float dt)
    {
        // Gaitas fāze: skrienot ātri mainās, stāvot - lēnām (mazajām kustībām)
        if (stavoklis == SunaStavoklis.Iet)
        {
            gaitasFaze = gaitasFaze + dt * 16f;
        }
        else
        {
            gaitasFaze = gaitasFaze + dt * 3f;
        }

        // Kopēja nobide visām daļām (lēciens un skriešanas lēkāšana)
        float visasY = 0f;
        float galvasPapildusX = 0f;
        float galvasPapildusY = 0f;
        float kajuMaina = 0f;       // cik kājas ies uz priekšu/atpakaļ
        float ausuLidojums = 0f;    // ausu plīvošana
        float astesIzliekums = 0f;  // astes luncinasana

        // Astes luncinasanas stiprums: laimigs suns luncina vairak
        float luncinasStiprums = 8f;
        float luncinasAtrums = 6f;
        if (spele.suns.garastavoklis > 50f)
        {
            luncinasStiprums = 28f;
        }
        if (stavoklis == SunaStavoklis.Lec)
        {
            luncinasStiprums = 40f;
            luncinasAtrums = 20f;
        }
        if (stavoklis == SunaStavoklis.Guli || spele.suns.slims)
        {
            luncinasStiprums = 5f;
        }
        astesIzliekums = Mathf.Sin(laiks * luncinasAtrums) * luncinasStiprums;

        // ---- Skriešana ----
        if (stavoklis == SunaStavoklis.Iet)
        {
            visasY = Mathf.Abs(Mathf.Sin(gaitasFaze)) * 0.05f;
            kajuMaina = Mathf.Sin(gaitasFaze);
            ausuLidojums = Mathf.Sin(gaitasFaze * 2f) * 10f;
            galvasPapildusY = Mathf.Sin(gaitasFaze * 2f) * 0.02f;
        }

        // ---- Elpošana, kad stāv, sēž vai guļ ----
        float elpa = 1f;
        if (stavoklis != SunaStavoklis.Iet)
        {
            elpa = 1f + Mathf.Sin(laiks * 2.2f) * 0.025f;
        }

        // ---- Ausu kustināšana, kad stāv (reizēm pakustina ausi) ----
        if (stavoklis == SunaStavoklis.Stav || stavoklis == SunaStavoklis.Sed)
        {
            float raustisana = Mathf.Max(0f, Mathf.Sin(laiks * 0.9f) - 0.85f) * 80f;
            ausuLidojums = raustisana;
        }

        // ---- Ēšana: galva kustas uz augšu un leju ----
        if (stavoklis == SunaStavoklis.Ed)
        {
            galvasPapildusY = Mathf.Sin(laiks * 14f) * 0.05f;
            galvasPapildusX = Mathf.Sin(laiks * 7f) * 0.02f;
        }

        // ---- Lēkšana: divi lēcieni pēc kārtas ----
        if (stavoklis == SunaStavoklis.Lec)
        {
            float progress = 1f - lecienaTaimeris / LecienaIlgums;      // 0 līdz 1
            visasY = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 2f)) * 0.55f;
            kajuMaina = Mathf.Sin(laiks * 20f) * 0.3f;
        }

        Vector3 visasNobide = new Vector3(0f, visasY, 0f);
        Vector3 galvasKopa = new Vector3(galvasNobide.x + galvasPapildusX, galvasNobide.y + galvasPapildusY, 0f) + visasNobide;

        // ---- Ermenis ----
        ermenis.dala.localPosition = ermenis.pozicija + (Vector3)ermenaNobide + visasNobide;
        ermenis.dala.localRotation = Quaternion.Euler(0, 0, ermenis.rotacija + ermenaRotacija);
        ermenis.dala.localScale = new Vector3(ermenis.merogs.x, ermenis.merogs.y * elpa, 1f);

        // ---- Galva un sejas daļas pārvietojas kopā ----
        galva.dala.localPosition = galva.pozicija + galvasKopa;
        purns.dala.localPosition = purns.pozicija + galvasKopa;
        deguns.dala.localPosition = deguns.pozicija + galvasKopa;
        acs.dala.localPosition = acs.pozicija + galvasKopa;
        acs.dala.localScale = new Vector3(acs.merogs.x, acs.merogs.y * acuMerogs, 1f);

        // ---- Ausis ----
        ausisA.dala.localPosition = ausisA.pozicija + galvasKopa;
        ausisB.dala.localPosition = ausisB.pozicija + galvasKopa;
        ausisA.dala.localRotation = Quaternion.Euler(0, 0, ausisA.rotacija + ausuRotacija + ausuLidojums);
        ausisB.dala.localRotation = Quaternion.Euler(0, 0, ausisB.rotacija - ausuRotacija - ausuLidojums);

        // ---- Kājas ----
        // Katra kāja virzās uz priekšu un atpakaļ, un nedaudz pacelas (pretējā fāzē)
        float kajaAx = kajuMaina * 0.14f;
        float kajaAy = Mathf.Max(0f, Mathf.Cos(gaitasFaze)) * 0.07f * Mathf.Abs(kajuMaina);
        float kajaBx = -kajuMaina * 0.14f;
        float kajaBy = Mathf.Max(0f, -Mathf.Cos(gaitasFaze)) * 0.07f * Mathf.Abs(kajuMaina);

        // Kad kājas "saspiežas" (guļ/sēž), tās jānolaiž zemāk, lai paliek pie zemes
        float nolaisana = -(1f - kajuMerogs) * 0.15f;
        kajaA.dala.localPosition = kajaA.pozicija + new Vector3(kajaAx, kajaAy + nolaisana, 0f) + visasNobide * 0.5f;
        kajaB.dala.localPosition = kajaB.pozicija + new Vector3(kajaBx, kajaBy + nolaisana, 0f) + visasNobide * 0.5f;
        kajaA.dala.localScale = new Vector3(kajaA.merogs.x, kajaA.merogs.y * kajuMerogs, 1f);
        kajaB.dala.localScale = new Vector3(kajaB.merogs.x, kajaB.merogs.y * kajuMerogs, 1f);

        // ---- Aste ----
        aste.dala.localPosition = aste.pozicija + (Vector3)astesNobide + visasNobide;
        aste.dala.localRotation = Quaternion.Euler(0, 0, astesRotacija + astesIzliekums);
    }
}

using UnityEngine;

// Suna raksturs - ietekme, cik viegli vai gruti suns macas
public enum Raksturs
{
    Aktivs,
    Mierigs,
    Bailigs
}

// DogStatus: suna cetri raditaji - veseliba, izsalkums, energija un garastavoklis.
// Visi raditaji ir no 0 lidz 100.
public class DogStatus : MonoBehaviour
{
    public string vards = "Reksis";
    public Raksturs raksturs;
    public Color kruasa = new Color(0.8f, 0.55f, 0.3f);

    public float veseliba = 100f;
    public float bariba = 80f;          // cik suns ir paedis (100 = pilnigi paedis)
    public float energija = 100f;
    public float garastavoklis = 70f;
    public bool slims = false;

    // Taimeris, kas ik pec 60 sekundem pārbauda, vai suns saslimst
    float slimibasTaimeris = 0f;

    // Izsalkums ir pretejs barībai: ja paedis 100, tad izsalkums ir 0
    public float Izsalkums()
    {
        return 100f - bariba;
    }

    // Rakstura nosaukums latviski
    public static string RaksturaNosaukums(Raksturs r)
    {
        if (r == Raksturs.Aktivs) return "Aktīvs";
        if (r == Raksturs.Mierigs) return "Mierīgs";
        return "Bailīgs";
    }

    // Rakstura apraksts, ko paradam speletajam
    public string RaksturaApraksts()
    {
        if (raksturs == Raksturs.Aktivs)
        {
            return "Aktīvs: ātri nogurst, bet labāk veic kustību komandas.";
        }
        if (raksturs == Raksturs.Mierigs)
        {
            return "Mierīgs: labi koncentrējas, labāk apgūst sēdēšanu, gulēšanu un gaidīšanu.";
        }
        return "Bailīgs: vajag daudz pozitīvas pieredzes; sliktā garastāvoklī mācās slikti.";
    }

    // Sagatavo suni (kad speletajs sanem jaunu suni)
    public void Inicializet(string jaunsVards, Raksturs jaunsRaksturs, Color jaunaKrasa)
    {
        vards = jaunsVards;
        raksturs = jaunsRaksturs;
        kruasa = jaunaKrasa;

        veseliba = 100f;
        bariba = 75f;
        energija = 100f;
        if (jaunsRaksturs == Raksturs.Bailigs)
        {
            garastavoklis = 50f;
        }
        else
        {
            garastavoklis = 70f;
        }
        slims = false;
        slimibasTaimeris = 0f;

        SunaIzskats izskats = GetComponent<SunaIzskats>();
        if (izskats != null)
        {
            izskats.Nokrasot(jaunaKrasa);
        }
    }

    // Maina visus raditajus uzreiz. Pozitivs skaitlis = pieaug, negativs = samazinas.
    public void Mainit(float veselibasMaina, float baribasMaina, float energijasMaina, float garastavoklaMaina)
    {
        veseliba = Mathf.Clamp(veseliba + veselibasMaina, 1f, 100f);
        bariba = Mathf.Clamp(bariba + baribasMaina, 0f, 100f);
        energija = Mathf.Clamp(energija + energijasMaina, 0f, 100f);
        garastavoklis = Mathf.Clamp(garastavoklis + garastavoklaMaina, 0f, 100f);
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele == null || spele.irSuns == false)
        {
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        // Suns pamazam kluust izsalcis
        bariba = bariba - 0.30f * dt;
        if (bariba < 0f) bariba = 0f;

        // Energija samazinas lēnām; kad suns kustas, tad 3 reizes ātrāk
        float energijasZaudejums = 0.06f * dt;
        if (raksturs == Raksturs.Aktivs)
        {
            energijasZaudejums = energijasZaudejums * 1.5f;
        }
        if (spele.sunaSekosana != null && spele.sunaSekosana.kustas)
        {
            energijasZaudejums = energijasZaudejums * 3f;
        }
        energija = energija - energijasZaudejums;
        if (energija < 0f) energija = 0f;

        // Garastavoklis krit, ja suns ir izsalcis, noguris vai slims
        float garastavoklaMaina = -0.08f;
        if (bariba < 25f) garastavoklaMaina = garastavoklaMaina - 0.25f;
        if (energija < 15f) garastavoklaMaina = garastavoklaMaina - 0.10f;
        if (slims) garastavoklaMaina = garastavoklaMaina - 0.20f;
        garastavoklis = Mathf.Clamp(garastavoklis + garastavoklaMaina * dt, 0f, 100f);

        // Veselibas izmaiņas
        if (bariba <= 0f)
        {
            veseliba = Mathf.Max(1f, veseliba - 0.4f * dt);
        }
        else if (slims)
        {
            veseliba = Mathf.Max(5f, veseliba - 0.15f * dt);
        }
        else if (bariba > 50f && veseliba < 100f)
        {
            veseliba = Mathf.Min(100f, veseliba + 0.05f * dt);
        }

        ParbauditSlimibu(dt, spele);
        AtjaunotStatusaTekstu();
    }

    // Ik pec 60 sekundem ir neliela iespeja, ka suns saslimst
    void ParbauditSlimibu(float dt, GameManager spele)
    {
        slimibasTaimeris = slimibasTaimeris + dt;
        if (slimibasTaimeris <= 60f)
        {
            return;
        }
        slimibasTaimeris = 0f;

        float iespeja = 0.06f;
        if (bariba < 20f) iespeja = iespeja + 0.15f;
        if (garastavoklis < 20f) iespeja = iespeja + 0.10f;

        if (slims == false && Random.value < iespeja)
        {
            slims = true;
            spele.saskarne.Pazinojums(vards + " saslima! Nogādā to veterinārajā klīnikā.");
            spele.skana.Atskanot(Skana.Slikti);
        }
    }

    // Parada mazu simbolu virs suna galvas
    void AtjaunotStatusaTekstu()
    {
        SunaIzskats izskats = GetComponent<SunaIzskats>();
        if (izskats == null || izskats.statusaTeksts == null)
        {
            return;
        }

        string simbols = "";
        if (slims) simbols = "+";
        else if (bariba < 25f) simbols = "!";
        else if (energija < 15f) simbols = "zZ";
        else if (garastavoklis > 80f) simbols = "<3";
        izskats.statusaTeksts.text = simbols;
    }
}

using UnityEngine;

// EconomyManager: monetas, inventars, uzlabojumi un pasivie ienakumi.
public class EconomyManager : MonoBehaviour
{
    public int monetas = 50;

    // Inventars
    public int ediens = 3;              // suna barība
    public int gardumi = 2;
    public bool irRotallieta = false;
    public bool irPavada = false;
    public bool irKonusi = false;

    // Uzlabojumu limeni
    public int majasLimenis = 0;
    public int laukumaLimenis = 0;
    public const int MaksimalaisUzlabojums = 2;

    // Pasivie ienakumi (nauda, kas nak pati, ja sunim klajas labi)
    public int pedejaPasivaPelna = 0;
    const float PasivaIntervals = 12f;      // ik pec 12 sekundem
    float pasivaisTaimeris = 0f;

    // Pieskaita monetas. Ja "klusi" ir true, tad skana netiek atskanota.
    public void Nopelnit(int daudzums, bool klusi = false)
    {
        monetas = monetas + daudzums;
        if (klusi == false)
        {
            GameManager.Galvenais.skana.Atskanot(Skana.Monetas);
        }
    }

    // Mēģina iztērēt monetas. Atgriez true, ja pietika naudas.
    public bool IztereMonetas(int daudzums)
    {
        if (monetas < daudzums)
        {
            GameManager.Galvenais.saskarne.Pazinojums("Nepietiek monētu (vajag " + daudzums + ").");
            GameManager.Galvenais.skana.Atskanot(Skana.Slikti);
            return false;
        }
        monetas = monetas - daudzums;
        return true;
    }

    // Suna kopeja labsajutas vertiba no 0 lidz 1 (cetru raditaju videjais)
    public static float Labsajutas(DogStatus suns)
    {
        float summa = suns.veseliba + (100f - suns.Izsalkums()) + suns.energija + suns.garastavoklis;
        return summa / 400f;
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele == null || spele.irSuns == false || spele.saskarne.IrBloketa())
        {
            return;
        }

        pasivaisTaimeris = pasivaisTaimeris + Time.deltaTime;
        if (pasivaisTaimeris < PasivaIntervals)
        {
            return;
        }
        pasivaisTaimeris = 0f;

        // Ja suns ir slims vai jutas slikti, pasivo ienakumu nav
        float labsajutas = Labsajutas(spele.suns);
        if (spele.suns.slims || labsajutas <= 0.5f)
        {
            pedejaPasivaPelna = 0;
            return;
        }

        float pamats = 1f + (labsajutas - 0.5f) * 6f;
        float majasPiemaksa = 1f + 0.25f * majasLimenis;
        pedejaPasivaPelna = Mathf.Max(1, Mathf.RoundToInt(pamats * majasPiemaksa));
        Nopelnit(pedejaPasivaPelna, true);
    }
}

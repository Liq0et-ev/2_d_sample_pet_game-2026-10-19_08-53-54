using UnityEngine;

// Cetras komandas, ko suns var apgut
public enum Komanda
{
    Sedi,
    Guli,
    PieManis,
    Gaidi
}

// Palidzfunkcija: dod komandas nosaukumu latviski
public static class KomandasNosaukumi
{
    public static string Dabut(Komanda komanda)
    {
        if (komanda == Komanda.Sedi) return "Sēdi";
        if (komanda == Komanda.Guli) return "Guli";
        if (komanda == Komanda.PieManis) return "Pie manis";
        return "Gaidi";
    }
}

// GameManager: viena vieta, kur glabajas atsauces uz visam speles sistemam.
// Citi skripti raksta GameManager.Galvenais, lai dabutu piekluvi visam.
public class GameManager : MonoBehaviour
{
    public static GameManager Galvenais;

    // Cik adopciju vajag, lai uzvaretu
    public const int UzvarasAdopcijas = 10;

    // Atsauces uz objektiem un sistemam
    public Transform speletajs;
    public GameObject sunaObjekts;
    public DogStatus suns;
    public DogProgress sunaProgress;
    public DogFollow sunaSekosana;
    public SunaAnimacija sunaAnimacija;
    public DogFollow3D sunaSekosana3D;          // 3D versijas suna sekošana
    public SunaAnimacijaBaze sunaAnimacija3D;     // 3D versijas suna animācija
    public PlayerProgress speletajaProgress;
    public EconomyManager ekonomika;
    public CareSystem aprupe;
    public TrainingSystem trenins;
    public ShopSystem veikals;
    public NujasSpele nujasSpele;
    public WelfareAndAdoption labklajiba;
    public UIManager saskarne;
    public AudioManager skana;

    // Speles stavoklis
    public bool irTrisDimensiju = false;   // true, ja spēle darbojas 3D versijā
    public bool irSuns;          // vai speletajam pasreiz ir suns
    public bool irPabarots;      // vai suns jau reizi ir pabarots (pamacibai)
    public int adopcijas;        // cik suni jau adoptēti
    public ZonasInfo pasreizejaZona;

    void Awake()
    {
        Galvenais = this;
    }

    // Atgriež objekta vietu kartes koordinātēs (2D: X un Y, 3D: X un Z)
    public Vector2 KartesVieta(Transform objekts)
    {
        if (irTrisDimensiju)
        {
            return new Vector2(objekts.position.x, objekts.position.z);
        }
        return new Vector2(objekts.position.x, objekts.position.y);
    }

    // Vai suns pasreiz skrien?
    public bool SunaKustas()
    {
        if (irTrisDimensiju)
        {
            return sunaSekosana3D != null && sunaSekosana3D.kustas;
        }
        return sunaSekosana != null && sunaSekosana.kustas;
    }

    // Uzreiz noliek suni blakus speletajam
    public void PieliktSuniPieSpeletaja()
    {
        if (irTrisDimensiju)
        {
            sunaSekosana3D.Pielekt();
        }
        else
        {
            sunaSekosana.Pielekt();
        }
    }

    // Suns ēd (animacija)
    public void SunaEd()
    {
        if (irTrisDimensiju)
        {
            sunaAnimacija3D.Ediens();
        }
        else
        {
            sunaAnimacija.Ediens();
        }
    }

    // Suns priecājas (animacija)
    public void SunaPriecajas()
    {
        if (irTrisDimensiju)
        {
            sunaAnimacija3D.Priecasanas();
        }
        else
        {
            sunaAnimacija.Priecasanas();
        }
    }

    // Atver izvelni atkariba no ekas, pie kuras speletajs stav
    public void AtvertZonu(ZonasVeids veids)
    {
        if (veids == ZonasVeids.Labklajiba) labklajiba.AtvertIzvelni();
        if (veids == ZonasVeids.Trenins) trenins.AtvertIzvelni();
        if (veids == ZonasVeids.Parks) aprupe.AtvertParkaIzvelni();
        if (veids == ZonasVeids.Maja) aprupe.AtvertMajasIzvelni();
        if (veids == ZonasVeids.Veikals) veikals.AtvertIzvelni();
        if (veids == ZonasVeids.Klinika) aprupe.AtvertKlinikasIzvelni();
    }

    // Atgriez tekstu, kas parada, ko speletajam darit tālāk
    public string DabutUzdevumu()
    {
        if (adopcijas >= UzvarasAdopcijas)
        {
            return "Mērķis sasniegts! Turpini brīvajā režīmā un palīdzi vēl vairāk suņiem.";
        }

        if (irSuns == false)
        {
            if (adopcijas == 0)
            {
                return "Dodies uz Labklājības centru (ZR) un nospied E, lai saņemtu pirmo suni.";
            }
            return "Saņem nākamo suni Labklājības centrā. Adopcijas: " + adopcijas + "/" + UzvarasAdopcijas;
        }

        if (irPabarots == false)
        {
            return "Aizved suni uz Māju (DR) un pabaro to (E).";
        }

        int apgustas = sunaProgress.ApgustoKomanduSkaits();
        if (apgustas == 0)
        {
            return "Apmeklē Treniņu laukumu (augšā vidū) un iemāci pirmo komandu.";
        }
        if (apgustas < 4)
        {
            return "Iemāci visas 4 komandas (" + apgustas + "/4). Seko suņa vajadzībām!";
        }
        if (labklajiba.VaiGatavsAdopcijai())
        {
            return "Suns ir gatavs! Dodies uz Labklājības centru adopcijas pārbaudījumam.";
        }
        return "Parūpējies par suni (veselība, barība, garastāvoklis), tad dodies uz adopciju.";
    }
}

using UnityEngine;

// Iestatijumi: spēles iestatījumi (skaņa, skaļums, pilnekrāns).
// Tos saglabā PlayerPrefs, tāpēc iestatījumi paliek arī pēc spēles aizvēršanas
// un tie ir vienādi galvenajā izvēlnē un pašā spēlē.
public static class Iestatijumi
{
    const string SkalumaAtslega = "iest_skalums";
    const string SkanasAtslega = "iest_skana";
    const string PilnekranaAtslega = "iest_pilnekrans";

    // ---------------- Skaļums (0 līdz 1) ----------------
    public static float DabutSkalumu()
    {
        return PlayerPrefs.GetFloat(SkalumaAtslega, 0.8f);
    }

    public static void IestatitSkalumu(float vertiba)
    {
        vertiba = Mathf.Clamp01(vertiba);
        PlayerPrefs.SetFloat(SkalumaAtslega, vertiba);
        PlayerPrefs.Save();
        Pielietot();
    }

    // ---------------- Skaņa ieslēgta / izslēgta ----------------
    public static bool SkanaIeslegta()
    {
        return PlayerPrefs.GetInt(SkanasAtslega, 1) == 1;
    }

    public static void IestatitSkanu(bool ieslegta)
    {
        int vertiba = 0;
        if (ieslegta)
        {
            vertiba = 1;
        }
        PlayerPrefs.SetInt(SkanasAtslega, vertiba);
        PlayerPrefs.Save();
    }

    // ---------------- Pilnekrāna režīms ----------------
    public static bool DabutPilnekranu()
    {
        int sakumaVertiba = 0;
        if (Screen.fullScreen)
        {
            sakumaVertiba = 1;
        }
        return PlayerPrefs.GetInt(PilnekranaAtslega, sakumaVertiba) == 1;
    }

    public static void IestatitPilnekranu(bool ieslegts)
    {
        int vertiba = 0;
        if (ieslegts)
        {
            vertiba = 1;
        }
        PlayerPrefs.SetInt(PilnekranaAtslega, vertiba);
        PlayerPrefs.Save();
        Pielietot();
    }

    // Uzliek saglabātos iestatījumus (izsauc, kad sākas aina)
    public static void Pielietot()
    {
        AudioListener.volume = DabutSkalumu();
        if (PlayerPrefs.HasKey(PilnekranaAtslega))
        {
            Screen.fullScreen = DabutPilnekranu();
        }
    }
}

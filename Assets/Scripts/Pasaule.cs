using System.Collections.Generic;
using UnityEngine;

// Seši vietas (zonas) speles karte
public enum ZonasVeids
{
    Labklajiba,
    Trenins,
    Parks,
    Maja,
    Veikals,
    Klinika
}

// Vienas zonas dati
public class ZonasInfo
{
    public ZonasVeids veids;
    public string nosaukums;
    public string burts;                  // burts minikartei
    public Vector2 centrs;                // zonas viduspunkts
    public Vector2 mijiedarbibasPunkts;   // vieta, kur spiez E
    public Color krasa;
}

// Pasaule: kartes izmeri, zonas un sadursmes (kur speletajs nedrikst staigat)
public static class Pasaule
{
    public static Rect Robezas = new Rect(-31f, -19f, 62f, 38f);
    public static Vector2 ZonasPusIzmers = new Vector2(9.5f, 7f);

    // Visas "sienas" - ekas, koki, dīķis. Speletajs tam cauri netiek.
    public static List<Rect> Barjeras = new List<Rect>();

    public static ZonasInfo[] Zonas = IzveidotZonas();

    static ZonasInfo IzveidotZonu(ZonasVeids veids, string nosaukums, string burts,
        float centraX, float centraY, float punktaY, Color krasa)
    {
        ZonasInfo zona = new ZonasInfo();
        zona.veids = veids;
        zona.nosaukums = nosaukums;
        zona.burts = burts;
        zona.centrs = new Vector2(centraX, centraY);
        zona.mijiedarbibasPunkts = new Vector2(centraX, punktaY);
        zona.krasa = krasa;
        return zona;
    }

    static ZonasInfo[] IzveidotZonas()
    {
        ZonasInfo[] saraksts = new ZonasInfo[6];
        saraksts[0] = IzveidotZonu(ZonasVeids.Labklajiba, "Labklājības centrs", "L", -20, 11, 10.2f, new Color(0.42f, 0.68f, 0.40f));
        saraksts[1] = IzveidotZonu(ZonasVeids.Trenins, "Treniņu laukums", "T", 0, 11, 10.2f, new Color(0.50f, 0.70f, 0.36f));
        saraksts[2] = IzveidotZonu(ZonasVeids.Parks, "Parks", "P", 20, 11, 10.2f, new Color(0.34f, 0.66f, 0.38f));
        saraksts[3] = IzveidotZonu(ZonasVeids.Maja, "Māja", "M", -20, -11, -10.2f, new Color(0.44f, 0.66f, 0.42f));
        saraksts[4] = IzveidotZonu(ZonasVeids.Veikals, "Veikals", "V", 0, -11, -10.2f, new Color(0.46f, 0.64f, 0.40f));
        saraksts[5] = IzveidotZonu(ZonasVeids.Klinika, "Veterinārā klīnika", "K", 20, -11, -10.2f, new Color(0.40f, 0.66f, 0.44f));
        return saraksts;
    }

    // Atrod zonu pec veida
    public static ZonasInfo AtrastZonu(ZonasVeids veids)
    {
        for (int i = 0; i < Zonas.Length; i++)
        {
            if (Zonas[i].veids == veids)
            {
                return Zonas[i];
            }
        }
        return null;
    }

    // Parbauda, vai taisnstūris (ar centru "pozicija") saskaras ar kadu barjeru
    static bool VaiSaduras(Vector2 pozicija, Vector2 pusIzmers)
    {
        Rect telaRamis = new Rect(pozicija.x - pusIzmers.x, pozicija.y - pusIzmers.y, pusIzmers.x * 2f, pusIzmers.y * 2f);
        for (int i = 0; i < Barjeras.Count; i++)
        {
            if (Barjeras[i].Overlaps(telaRamis))
            {
                return true;
            }
        }
        return false;
    }

    // Parvieto teli par "starpibu", ja nav sienas. Kustamies atsevisķi pa X un Y,
    // tapec tels var slidet gar sienu.
    public static Vector2 Parvietot(Vector2 pozicija, Vector2 starpiba, Vector2 pusIzmers)
    {
        Vector2 jaunaPozicija = pozicija;

        // Kustiba pa X asi
        float jaunsX = jaunaPozicija.x + starpiba.x;
        jaunsX = Mathf.Clamp(jaunsX, Robezas.xMin + pusIzmers.x, Robezas.xMax - pusIzmers.x);
        Vector2 variantsX = new Vector2(jaunsX, jaunaPozicija.y);
        if (VaiSaduras(variantsX, pusIzmers) == false)
        {
            jaunaPozicija = variantsX;
        }

        // Kustiba pa Y asi
        float jaunsY = jaunaPozicija.y + starpiba.y;
        jaunsY = Mathf.Clamp(jaunsY, Robezas.yMin + pusIzmers.y, Robezas.yMax - pusIzmers.y);
        Vector2 variantsY = new Vector2(jaunaPozicija.x, jaunsY);
        if (VaiSaduras(variantsY, pusIzmers) == false)
        {
            jaunaPozicija = variantsY;
        }

        return jaunaPozicija;
    }

    // Atrod tuvako zonu, ar kuru var mijiedarboties (ja ir pietiekami tuvu)
    public static ZonasInfo TuvakaZona(Vector2 pozicija, float radiuss)
    {
        ZonasInfo tuvaka = null;
        float labakaisAttalums = radiuss;
        for (int i = 0; i < Zonas.Length; i++)
        {
            float attalums = Vector2.Distance(pozicija, Zonas[i].mijiedarbibasPunkts);
            if (attalums <= labakaisAttalums)
            {
                labakaisAttalums = attalums;
                tuvaka = Zonas[i];
            }
        }
        return tuvaka;
    }

    // Atrod, kura zona atrodas zem dota punkta (vai null, ja esam uz celiem)
    public static ZonasInfo ZonaPunkta(Vector2 pozicija)
    {
        for (int i = 0; i < Zonas.Length; i++)
        {
            float starpibaX = Mathf.Abs(pozicija.x - Zonas[i].centrs.x);
            float starpibaY = Mathf.Abs(pozicija.y - Zonas[i].centrs.y);
            if (starpibaX <= ZonasPusIzmers.x && starpibaY <= ZonasPusIzmers.y)
            {
                return Zonas[i];
            }
        }
        return null;
    }
}

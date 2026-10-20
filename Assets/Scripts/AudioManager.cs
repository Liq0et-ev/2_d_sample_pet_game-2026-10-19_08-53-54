using UnityEngine;

// Skanu efektu veidi
public enum Skana
{
    Klikskis,
    Labi,
    Slikti,
    Monetas,
    Riet,
    Limenis
}

// AudioManager: visas skanas tiek izveidotas ar kodu (nevajag audio failus).
// Ir divi veidi: isi skanu efekti un garas fona skanas katrai zonai.
public class AudioManager : MonoBehaviour
{
    const int NolasesBiezums = 22050;     // cik paraugu sekunde
    const float CiklaIlgums = 8f;         // fona skanas garums sekundes

    AudioSource efektuAvots;
    AudioSource[] fonaAvoti = new AudioSource[7];     // 6 zonas + 1 laukums (centrs)
    AudioClip[] efekti = new AudioClip[6];
    public bool izslegts = false;

    // ---------------------------------------------------------------
    // Sakuma izveide
    // ---------------------------------------------------------------
    public void Izveidot()
    {
        efektuAvots = gameObject.AddComponent<AudioSource>();
        efektuAvots.playOnAwake = false;

        efekti[(int)Skana.Klikskis] = IzveidotKlipu("klikskis", IzveidotKlikskaDatus());
        efekti[(int)Skana.Labi] = IzveidotKlipu("labi", IzveidotLabiDatus());
        efekti[(int)Skana.Slikti] = IzveidotKlipu("slikti", IzveidotSliktiDatus());
        efekti[(int)Skana.Monetas] = IzveidotKlipu("monetas", IzveidotMonetuDatus());
        efekti[(int)Skana.Riet] = IzveidotKlipu("riet", IzveidotRietDatus());
        efekti[(int)Skana.Limenis] = IzveidotKlipu("limenis", IzveidotLimenaDatus());

        // Katrai zonai sava fona skana (indekss = zonas numurs)
        fonaAvoti[(int)ZonasVeids.Maja] = IzveidotFonaAvotu("maja", IzveidotMajasFonu());
        fonaAvoti[(int)ZonasVeids.Parks] = IzveidotFonaAvotu("parks", IzveidotParkaFonu());
        fonaAvoti[(int)ZonasVeids.Trenins] = IzveidotFonaAvotu("trenins", IzveidotTreninaFonu());
        fonaAvoti[(int)ZonasVeids.Klinika] = IzveidotFonaAvotu("klinika", IzveidotKlinikasFonu());
        fonaAvoti[(int)ZonasVeids.Veikals] = IzveidotFonaAvotu("veikals", IzveidotVeikalaFonu());
        fonaAvoti[(int)ZonasVeids.Labklajiba] = IzveidotFonaAvotu("labklajiba", IzveidotLabklajibasFonu());
        fonaAvoti[6] = IzveidotFonaAvotu("laukums", IzveidotLaukumaFonu());

        // Visi fona avoti tiek ieslēgti, bet ar skaļumu 0. Vēlāk palielinām tikai vajadzīgo.
        for (int i = 0; i < fonaAvoti.Length; i++)
        {
            fonaAvoti[i].volume = 0f;
            fonaAvoti[i].Play();
        }
    }

    // Atskano vienu efektu
    public void Atskanot(Skana veids)
    {
        if (izslegts || efektuAvots == null)
        {
            return;
        }
        efektuAvots.PlayOneShot(efekti[(int)veids], 0.8f);
    }

    // Katru kadru pieskaņojam fona skanu skaļumu: skan tikai tas, kuras zonas iekšā esam
    void Update()
    {
        GameManager spele = GameManager.Galvenais;

        int aktivaisIndekss = 6;     // 6 = laukums, ja neesam nevienas zonas iekšā
        if (spele.pasreizejaZona != null)
        {
            aktivaisIndekss = (int)spele.pasreizejaZona.veids;
        }

        for (int i = 0; i < fonaAvoti.Length; i++)
        {
            if (fonaAvoti[i] == null)
            {
                continue;
            }

            float merkaSkalums = 0f;
            if (izslegts == false && i == aktivaisIndekss)
            {
                merkaSkalums = 0.28f;
            }
            fonaAvoti[i].volume = Mathf.MoveTowards(fonaAvoti[i].volume, merkaSkalums, Time.unscaledDeltaTime * 0.4f);
        }
    }

    // ---------------------------------------------------------------
    // Palīgfunkcijas skanas veidošanai
    // ---------------------------------------------------------------

    // Tīra sinusa skana
    float Sinuss(float frekvence, float laiks)
    {
        return Mathf.Sin(2f * Mathf.PI * frekvence * laiks);
    }

    // Zāģa skana (skan skarbaka)
    float Zaga(float frekvence, float laiks)
    {
        float dala = (laiks * frekvence) % 1f;
        return dala * 2f - 1f;
    }

    // Skaņas skalums laika gaitā: sāk spēcīgi un pakāpeniski izdziest
    float Apskalojums(float laiks, float ilgums)
    {
        float izdzisana = Mathf.Clamp01(1f - laiks / ilgums);
        float sakums = Mathf.Clamp01(laiks * 200f);
        return izdzisana * sakums;
    }

    // Tonis, kas labi atkārtojas ciklā: frekvence tiek noapaļota,
    // lai 8 sekundēs ietilptu vesels skaits vilnu (nav "klikšķa" cikla beigās)
    float CiklaTonis(float frekvence, float laiks)
    {
        float noapalota = Mathf.Round(frekvence * CiklaIlgums) / CiklaIlgums;
        return Mathf.Sin(2f * Mathf.PI * noapalota * laiks);
    }

    // Trīs toņu akords
    float Akords(float laiks, float a, float b, float c)
    {
        float summa = CiklaTonis(a, laiks) + CiklaTonis(b, laiks) * 0.8f + CiklaTonis(c, laiks) * 0.6f;
        return summa * 0.3f;
    }

    // Spēlē vairākas notis pēc kārtas (arpedžo)
    float Arpedzo(float laiks, float[] notis, float notesIlgums)
    {
        int indekss = (int)(laiks / notesIlgums);
        if (indekss > notis.Length - 1)
        {
            indekss = notis.Length - 1;
        }
        float notesLaiks = laiks - indekss * notesIlgums;
        float skalums = Mathf.Exp(-notesLaiks * 6f) * Mathf.Clamp01(notesLaiks * 300f);
        return Sinuss(notis[indekss], laiks) * skalums;
    }

    // Izveido tukšu skaņas datu masīvu dotajam sekunžu skaitam
    float[] JaunsMasivs(float sekundes)
    {
        int garums = Mathf.CeilToInt(NolasesBiezums * sekundes);
        return new float[garums];
    }

    // Skaņas vērtībām jābūt starp -1 un 1
    float Ierobezot(float vertiba)
    {
        return Mathf.Clamp(vertiba, -1f, 1f);
    }

    AudioClip IzveidotKlipu(string nosaukums, float[] dati)
    {
        AudioClip klips = AudioClip.Create(nosaukums, dati.Length, 1, NolasesBiezums, false);
        klips.SetData(dati, 0);
        return klips;
    }

    AudioSource IzveidotFonaAvotu(string nosaukums, float[] dati)
    {
        GameObject objekts = new GameObject("Fons_" + nosaukums);
        objekts.transform.SetParent(transform, false);
        AudioSource avots = objekts.AddComponent<AudioSource>();
        avots.clip = IzveidotKlipu(nosaukums, dati);
        avots.loop = true;
        avots.playOnAwake = false;
        return avots;
    }

    // ---------------------------------------------------------------
    // Skanu efekti (katrs ir atsevišķa funkcija ar vienkāršu ciklu)
    // ---------------------------------------------------------------
    float[] IzveidotKlikskaDatus()
    {
        float[] dati = JaunsMasivs(0.06f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            dati[i] = Ierobezot(Sinuss(880f, t) * Apskalojums(t, 0.06f) * 0.4f);
        }
        return dati;
    }

    float[] IzveidotLabiDatus()
    {
        float[] notis = { 523.25f, 659.25f, 783.99f };
        float[] dati = JaunsMasivs(0.36f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            dati[i] = Ierobezot(Arpedzo(t, notis, 0.12f) * 0.4f);
        }
        return dati;
    }

    float[] IzveidotSliktiDatus()
    {
        float[] dati = JaunsMasivs(0.3f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float frekvence = Mathf.Lerp(220f, 150f, t / 0.3f);     // skana krit zemāk
            dati[i] = Ierobezot(Zaga(frekvence, t) * Apskalojums(t, 0.3f) * 0.3f);
        }
        return dati;
    }

    float[] IzveidotMonetuDatus()
    {
        float[] dati = JaunsMasivs(0.2f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float frekvence = 1760f;
            if (t < 0.07f)
            {
                frekvence = 1319f;      // pirmā īsā nots ir zemāka
            }
            dati[i] = Ierobezot(Sinuss(frekvence, t) * Apskalojums(t, 0.2f) * 0.35f);
        }
        return dati;
    }

    float[] IzveidotLimenaDatus()
    {
        float[] notis = { 523.25f, 659.25f, 783.99f, 1046.5f };
        float[] dati = JaunsMasivs(0.6f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            dati[i] = Ierobezot(Arpedzo(t, notis, 0.15f) * 0.4f);
        }
        return dati;
    }

    // Suņa riešana: divi īsi "vau"
    float[] IzveidotRietDatus()
    {
        float[] dati = JaunsMasivs(0.4f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            if (t > 0.35f)
            {
                dati[i] = 0f;
                continue;
            }
            float laiksVauIeksa = t % 0.2f;
            float skana = Mathf.Sin(laiksVauIeksa * 2f * Mathf.PI * 260f) + Mathf.Sin(laiksVauIeksa * 2f * Mathf.PI * 390f) * 0.5f;
            dati[i] = Ierobezot(skana * Mathf.Exp(-laiksVauIeksa * 18f) * 0.3f);
        }
        return dati;
    }

    // ---------------------------------------------------------------
    // Fona skanas (katra ir 8 sekunžu cikls, kas atkārtojas)
    // ---------------------------------------------------------------

    // Vējš: nejaušs troksnis, kas izlīdzināts ar vienkāršu filtru
    float[] IzveidotVejaTroksni(int sekla, float filtrs, float skalums)
    {
        System.Random nejausais = new System.Random(sekla);
        float[] dati = JaunsMasivs(CiklaIlgums);
        float izlidzinats = 0f;
        for (int i = 0; i < dati.Length; i++)
        {
            float troksnis = (float)(nejausais.NextDouble() * 2.0 - 1.0);
            izlidzinats = izlidzinats + (troksnis - izlidzinats) * filtrs;
            dati[i] = izlidzinats * skalums;
        }
        return dati;
    }

    float[] IzveidotMajasFonu()
    {
        float[] dati = JaunsMasivs(CiklaIlgums);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float vilnojums = 0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 0.25f);
            dati[i] = Ierobezot(Akords(t, 130.81f, 164.81f, 196f) * vilnojums * 0.5f);
        }
        return dati;
    }

    float[] IzveidotParkaFonu()
    {
        // Pamats ir vējš, kam pieliekam putnu čivināšanu
        float[] dati = IzveidotVejaTroksni(3, 0.02f, 1.4f);
        float[] putnuLaiki = { 1f, 3.2f, 5.5f, 6.1f };

        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            dati[i] = dati[i] * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 0.125f));

            for (int p = 0; p < putnuLaiki.Length; p++)
            {
                float laiksNoSakuma = t - putnuLaiki[p];
                if (laiksNoSakuma >= 0f && laiksNoSakuma < 0.18f)
                {
                    float frekvence = Mathf.Lerp(2500f, 3600f, laiksNoSakuma / 0.18f);
                    float apjoms = Mathf.Sin(laiksNoSakuma / 0.18f * Mathf.PI) * 0.18f;
                    dati[i] = dati[i] + Sinuss(frekvence, t) * apjoms;
                }
            }
            dati[i] = Ierobezot(dati[i]);
        }
        return dati;
    }

    float[] IzveidotTreninaFonu()
    {
        float[] notis = { 220f, 261.63f, 329.63f, 261.63f };
        float[] dati = JaunsMasivs(CiklaIlgums);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float ritms = Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 2f));
            int notesNumurs = (int)(t * 4f) % 4;
            float skana = CiklaTonis(110f, t) * ritms * 0.35f + CiklaTonis(notis[notesNumurs], t) * 0.25f;
            dati[i] = Ierobezot(skana * 0.7f);
        }
        return dati;
    }

    float[] IzveidotKlinikasFonu()
    {
        float[] dati = JaunsMasivs(CiklaIlgums);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float vilnis = 0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 0.125f);
            float skana = CiklaTonis(261.63f, t) + CiklaTonis(392f, t) * 0.7f;
            dati[i] = Ierobezot(skana * vilnis * 0.3f);
        }
        return dati;
    }

    float[] IzveidotVeikalaFonu()
    {
        float[] notis = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 659.25f, 587.33f, 783.99f };
        float[] dati = JaunsMasivs(CiklaIlgums);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            int notesNumurs = (int)t;                 // jauna nots katru sekundi
            float laiksNotes = t - notesNumurs;
            float zvans = CiklaTonis(notis[notesNumurs % 8], t) * Mathf.Exp(-laiksNotes * 4f) * 0.35f;
            float basis = CiklaTonis(130.81f, t) * 0.12f;
            dati[i] = Ierobezot(zvans + basis);
        }
        return dati;
    }

    float[] IzveidotLabklajibasFonu()
    {
        float[] dati = JaunsMasivs(CiklaIlgums);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            float laiksCikla = t % 4f;
            float zvanins = 0f;
            if (laiksCikla < 1.2f)
            {
                zvanins = CiklaTonis(1046.5f, t) * Mathf.Exp(-laiksCikla * 3f) * 0.25f;
            }
            dati[i] = Ierobezot(Akords(t, 196f, 246.94f, 293.66f) * 0.45f + zvanins);
        }
        return dati;
    }

    float[] IzveidotLaukumaFonu()
    {
        float[] dati = IzveidotVejaTroksni(11, 0.012f, 1.2f);
        for (int i = 0; i < dati.Length; i++)
        {
            float t = i / (float)NolasesBiezums;
            dati[i] = Ierobezot(dati[i] + CiklaTonis(174.6f, t) * 0.05f);
        }
        return dati;
    }
}

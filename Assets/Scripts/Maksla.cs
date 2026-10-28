using System.Collections.Generic;
using UnityEngine;

// Maksla: visa speles grafika tiek uzzimeta ar kodu.
// Mums nevajag attelu failus - visu veidojam no apla, kvadrata un gredzena.
public static class Maksla
{
    // Sprites tiek izveidoti tikai vienu reizi un pec tam atkartoti izmantoti
    static Sprite kvadrataSprite;
    static Sprite apalaSprite;
    static Sprite gredzenaSprite;
    static Font burtuFonts;

    // ---------------------------------------------------------------
    // Sprite iegusana (ja vel nav izveidots, tad izveido)
    // ---------------------------------------------------------------
    public static Sprite Kvadrats()
    {
        if (kvadrataSprite == null)
        {
            kvadrataSprite = IzveidotKvadratuSprite();
        }
        return kvadrataSprite;
    }

    public static Sprite Aplis()
    {
        if (apalaSprite == null)
        {
            apalaSprite = IzveidotAplaSprite();
        }
        return apalaSprite;
    }

    public static Sprite Gredzens()
    {
        if (gredzenaSprite == null)
        {
            gredzenaSprite = IzveidotGredzenaSprite();
        }
        return gredzenaSprite;
    }

    public static Font Fonts()
    {
        if (burtuFonts == null)
        {
            burtuFonts = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        if (burtuFonts == null)
        {
            burtuFonts = Font.CreateDynamicFontFromOSFont("Arial", 16);
        }
        return burtuFonts;
    }

    // ---------------------------------------------------------------
    // Spritu veidosana
    // ---------------------------------------------------------------
    static Sprite IzveidotKvadratuSprite()
    {
        int izmers = 4;
        Texture2D tekstura = new Texture2D(izmers, izmers, TextureFormat.RGBA32, false);
        tekstura.filterMode = FilterMode.Point;
        tekstura.wrapMode = TextureWrapMode.Clamp;

        // Visi pikseli ir balti (krasu veidojam vel pec tam ar SpriteRenderer.color)
        Color[] pikseli = new Color[izmers * izmers];
        for (int i = 0; i < pikseli.Length; i++)
        {
            pikseli[i] = Color.white;
        }
        tekstura.SetPixels(pikseli);
        tekstura.Apply();

        // Pedejais skaitlis ir "pikseli uz vienibu" - tapec sprits ir tiesi 1x1 vienibas liels
        return Sprite.Create(tekstura, new Rect(0, 0, izmers, izmers), new Vector2(0.5f, 0.5f), izmers);
    }

    static Sprite IzveidotAplaSprite()
    {
        int izmers = 64;
        Texture2D tekstura = new Texture2D(izmers, izmers, TextureFormat.RGBA32, false);
        tekstura.filterMode = FilterMode.Bilinear;
        tekstura.wrapMode = TextureWrapMode.Clamp;

        Color[] pikseli = new Color[izmers * izmers];
        for (int y = 0; y < izmers; y++)
        {
            for (int x = 0; x < izmers; x++)
            {
                // Pikselja attalums no centra (0 = centrs, 1 = mala)
                float dx = (x + 0.5f) / izmers * 2f - 1f;
                float dy = (y + 0.5f) / izmers * 2f - 1f;
                float attalums = Mathf.Sqrt(dx * dx + dy * dy);

                // Caurspidiba: ara no apla ir 0, iekspusee 1 (ar mazu mikstu malu)
                float caurspidiba = Mathf.Clamp01((1f - attalums) * izmers * 0.5f);
                pikseli[y * izmers + x] = new Color(1, 1, 1, caurspidiba);
            }
        }
        tekstura.SetPixels(pikseli);
        tekstura.Apply();
        return Sprite.Create(tekstura, new Rect(0, 0, izmers, izmers), new Vector2(0.5f, 0.5f), izmers);
    }

    static Sprite IzveidotGredzenaSprite()
    {
        int izmers = 256;
        Texture2D tekstura = new Texture2D(izmers, izmers, TextureFormat.RGBA32, false);
        tekstura.filterMode = FilterMode.Bilinear;
        tekstura.wrapMode = TextureWrapMode.Clamp;

        Color[] pikseli = new Color[izmers * izmers];
        for (int y = 0; y < izmers; y++)
        {
            for (int x = 0; x < izmers; x++)
            {
                float dx = (x + 0.5f) / izmers * 2f - 1f;
                float dy = (y + 0.5f) / izmers * 2f - 1f;
                float attalums = Mathf.Sqrt(dx * dx + dy * dy);

                // Gredzens ir redzams tikai starp 0.93 un 1.0 no centra
                float arejaMala = Mathf.Clamp01((1f - attalums) * izmers * 0.5f);
                float ieksejaMala = Mathf.Clamp01((attalums - 0.93f) * izmers * 0.5f);
                pikseli[y * izmers + x] = new Color(1, 1, 1, arejaMala * ieksejaMala);
            }
        }
        tekstura.SetPixels(pikseli);
        tekstura.Apply();
        return Sprite.Create(tekstura, new Rect(0, 0, izmers, izmers), new Vector2(0.5f, 0.5f), 100f);
    }

    // ---------------------------------------------------------------
    // Formu un teksta izveide
    // ---------------------------------------------------------------

    // Izveido vienu krasainu formu (kvadratu vai apli).
    // kartiba = zimesanas secibas numurs (lielaks skaitlis = zimejas augstak)
    public static SpriteRenderer IzveidotFormu(string nosaukums, Transform vecaks, bool irAplis,
        Vector2 pozicija, Vector2 izmers, Color krasa, int kartiba, float rotacija = 0f)
    {
        GameObject objekts = new GameObject(nosaukums);
        objekts.transform.SetParent(vecaks, false);
        objekts.transform.localPosition = pozicija;
        objekts.transform.localScale = new Vector3(izmers.x, izmers.y, 1f);
        objekts.transform.localRotation = Quaternion.Euler(0, 0, rotacija);

        SpriteRenderer zimetajs = objekts.AddComponent<SpriteRenderer>();
        if (irAplis)
        {
            zimetajs.sprite = Aplis();
        }
        else
        {
            zimetajs.sprite = Kvadrats();
        }
        zimetajs.color = krasa;
        zimetajs.sortingOrder = kartiba;
        Apgaismojums2D.PielietotGaismu(zimetajs, irAplis);       // 2D gaisma (ja pieejama)
        return zimetajs;
    }

    // Izveido tekstu speles pasaule (piemeram, ekas nosaukumu)
    public static TextMesh IzveidotTekstu(string teksts, Transform vecaks, Vector2 pozicija,
        float augstums, Color krasa, int kartiba)
    {
        GameObject objekts = new GameObject("Teksts");
        objekts.transform.SetParent(vecaks, false);
        objekts.transform.localPosition = pozicija;

        TextMesh tekstaKomponente = objekts.AddComponent<TextMesh>();
        tekstaKomponente.font = Fonts();
        tekstaKomponente.text = teksts;
        tekstaKomponente.fontSize = 64;
        tekstaKomponente.characterSize = augstums / 6.4f;
        tekstaKomponente.anchor = TextAnchor.MiddleCenter;
        tekstaKomponente.alignment = TextAlignment.Center;
        tekstaKomponente.color = krasa;

        MeshRenderer zimetajs = objekts.GetComponent<MeshRenderer>();
        zimetajs.sharedMaterial = Fonts().material;
        zimetajs.sortingOrder = kartiba;
        return tekstaKomponente;
    }

    // Teksts ar tumsu ēnu, lai to var labi lasit uz zales
    public static void IzveidotTekstuArVaru(string teksts, Transform vecaks, Vector2 pozicija,
        float augstums, Color krasa, int kartiba)
    {
        Color varasKrasa = new Color(0, 0, 0, 0.7f);
        IzveidotTekstu(teksts, vecaks, pozicija + new Vector2(0.06f, -0.06f), augstums, varasKrasa, kartiba);
        IzveidotTekstu(teksts, vecaks, pozicija, augstums, krasa, kartiba + 1);
    }

    // ---------------------------------------------------------------
    // Tēli (objekta pozicija ir tēla pēdu vieta)
    // ---------------------------------------------------------------
    public static GameObject IzveidotSpeletaju(Transform vecaks)
    {
        GameObject sakne = new GameObject("Speletajs");
        sakne.transform.SetParent(vecaks, false);
        Transform t = sakne.transform;

        Color biksesKrasa = new Color(0.2f, 0.22f, 0.4f);
        Color jakasKrasa = new Color(0.25f, 0.55f, 0.85f);
        Color rokuKrasa = new Color(0.22f, 0.5f, 0.8f);
        Color adasKrasa = new Color(1f, 0.82f, 0.68f);
        Color matuKrasa = new Color(0.35f, 0.2f, 0.1f);

        IzveidotFormu("Vara", t, true, new Vector2(0, 0.05f), new Vector2(0.95f, 0.35f), new Color(0, 0, 0, 0.3f), 0);
        IzveidotFormu("KajaKreisa", t, false, new Vector2(-0.14f, 0.2f), new Vector2(0.18f, 0.4f), biksesKrasa, 1);
        IzveidotFormu("KajaLabaa", t, false, new Vector2(0.14f, 0.2f), new Vector2(0.18f, 0.4f), biksesKrasa, 1);
        IzveidotFormu("Ermenis", t, false, new Vector2(0, 0.7f), new Vector2(0.52f, 0.55f), jakasKrasa, 2);
        IzveidotFormu("RokaKreisa", t, false, new Vector2(-0.34f, 0.7f), new Vector2(0.14f, 0.45f), rokuKrasa, 2);
        IzveidotFormu("RokaLabaa", t, false, new Vector2(0.34f, 0.7f), new Vector2(0.14f, 0.45f), rokuKrasa, 2);
        IzveidotFormu("Galva", t, true, new Vector2(0, 1.2f), new Vector2(0.5f, 0.5f), adasKrasa, 3);
        IzveidotFormu("Mati", t, true, new Vector2(0, 1.33f), new Vector2(0.52f, 0.34f), matuKrasa, 4);
        IzveidotFormu("AcsKreisa", t, true, new Vector2(-0.1f, 1.2f), new Vector2(0.07f, 0.07f), Color.black, 5);
        IzveidotFormu("AcsLabaa", t, true, new Vector2(0.1f, 1.2f), new Vector2(0.07f, 0.07f), Color.black, 5);
        return sakne;
    }

    public static GameObject IzveidotSunu(Transform vecaks)
    {
        GameObject sakne = new GameObject("Suns");
        sakne.transform.SetParent(vecaks, false);
        SunaIzskats izskats = sakne.AddComponent<SunaIzskats>();
        Transform t = sakne.transform;

        // Dalas, kuras krasosim ar suna kreklu (kruasaDalas) vai tumsaku toni (tumsasDalas)
        IzveidotFormu("Vara", t, true, new Vector2(0, 0.04f), new Vector2(1.2f, 0.35f), new Color(0, 0, 0, 0.3f), 0);
        izskats.tumsasDalas.Add(IzveidotFormu("KajaA", t, false, new Vector2(-0.3f, 0.15f), new Vector2(0.14f, 0.3f), Color.white, 1));
        izskats.tumsasDalas.Add(IzveidotFormu("KajaB", t, false, new Vector2(0.28f, 0.15f), new Vector2(0.14f, 0.3f), Color.white, 1));

        SpriteRenderer aste = IzveidotFormu("Aste", t, false, new Vector2(-0.55f, 0.55f), new Vector2(0.14f, 0.45f), Color.white, 1, 35f);
        izskats.tumsasDalas.Add(aste);
        izskats.aste = aste.transform;

        izskats.kruasaDalas.Add(IzveidotFormu("Ermenis", t, true, new Vector2(0, 0.45f), new Vector2(1.15f, 0.62f), Color.white, 2));
        izskats.kruasaDalas.Add(IzveidotFormu("Galva", t, true, new Vector2(0.55f, 0.72f), new Vector2(0.52f, 0.5f), Color.white, 3));
        izskats.tumsasDalas.Add(IzveidotFormu("AusisA", t, true, new Vector2(0.42f, 0.98f), new Vector2(0.17f, 0.3f), Color.white, 2, 15f));
        izskats.tumsasDalas.Add(IzveidotFormu("AusisB", t, true, new Vector2(0.7f, 0.98f), new Vector2(0.17f, 0.3f), Color.white, 2, -15f));
        IzveidotFormu("Purns", t, true, new Vector2(0.78f, 0.66f), new Vector2(0.22f, 0.18f), new Color(0.95f, 0.88f, 0.8f), 4);
        IzveidotFormu("Deguns", t, true, new Vector2(0.88f, 0.69f), new Vector2(0.09f, 0.08f), Color.black, 5);
        IzveidotFormu("Acs", t, true, new Vector2(0.64f, 0.8f), new Vector2(0.08f, 0.08f), Color.black, 5);

        // Mazs teksts virs suna galvas (piemeram "!" ja ir izsalcis)
        izskats.statusaTeksts = IzveidotTekstu("", t, new Vector2(0.2f, 1.6f), 0.5f, Color.white, 5000);

        sakne.AddComponent<YKartosana>();
        return sakne;
    }
}

// SunaIzskats: glaba suna daalas, lai varetu nomainit krasu un kustinat asti
public class SunaIzskats : MonoBehaviour
{
    public List<SpriteRenderer> kruasaDalas = new List<SpriteRenderer>();
    public List<SpriteRenderer> tumsasDalas = new List<SpriteRenderer>();
    public Transform aste;
    public TextMesh statusaTeksts;

    // 3D versijas dalas (tās ir Renderer, nevis SpriteRenderer)
    public List<Renderer> kruasaDalas3D = new List<Renderer>();
    public List<Renderer> tumsasDalas3D = new List<Renderer>();

    // Pasreizejas suna krasas (lai pec mirgošanas varētu atjaunot)
    Color pamataKruasa = Color.white;
    Color pamataTumsa = Color.white;

    // Mirgošana (kad notiek kaut kas svarigs)
    Color mirgosanasKrasa = Color.white;
    float mirgosanasLaiks = 0f;
    float mirgosanasIlgums = 0f;

    // Nokraso suni ar izveleto kruasu
    public void Nokrasot(Color krasa)
    {
        pamataKruasa = krasa;
        pamataTumsa = new Color(krasa.r * 0.65f, krasa.g * 0.65f, krasa.b * 0.65f, 1f);
        UzliktKrasas(0f);

        // 3D dalas: katrai jauns materials ar dotu krasu
        for (int i = 0; i < kruasaDalas3D.Count; i++)
        {
            Maksla3D.Nokrasot(kruasaDalas3D[i], krasa);
        }
        for (int i = 0; i < tumsasDalas3D.Count; i++)
        {
            Maksla3D.Nokrasot(tumsasDalas3D[i], pamataTumsa);
        }
    }

    // Uz brīdi nokrāso suni mirgošanas krāsā (piemēram, sarkana = slikti, zaļa = labi)
    public void Uzplaiksnit(Color krasa, float ilgums)
    {
        mirgosanasKrasa = krasa;
        mirgosanasIlgums = ilgums;
        mirgosanasLaiks = ilgums;
    }

    // Uzliek 2D dalu krasas; "mirgosana" ir no 0 (nav) lidz 1 (pilna mirgošanas krāsa)
    void UzliktKrasas(float mirgosana)
    {
        Color kruasa = Color.Lerp(pamataKruasa, mirgosanasKrasa, mirgosana);
        Color tumsa = Color.Lerp(pamataTumsa, mirgosanasKrasa, mirgosana * 0.7f);
        for (int i = 0; i < kruasaDalas.Count; i++)
        {
            kruasaDalas[i].color = kruasa;
        }
        for (int i = 0; i < tumsasDalas.Count; i++)
        {
            tumsasDalas[i].color = tumsa;
        }
    }

    void Update()
    {
        if (mirgosanasLaiks <= 0f)
        {
            return;
        }
        mirgosanasLaiks = mirgosanasLaiks - Time.unscaledDeltaTime;
        if (mirgosanasLaiks <= 0f)
        {
            UzliktKrasas(0f);       // mirgošana beigusies, atjaunojam parasto krasu
            return;
        }
        UzliktKrasas(mirgosanasLaiks / mirgosanasIlgums);
    }
}

// YKartosana: ja tels ir zemak uz ekrana, tas tiek zimets priekspusee.
// Tapec speletajs var staigat gan prieksa, gan aiz ekam un kokiem.
public class YKartosana : MonoBehaviour
{
    SpriteRenderer[] zimetaji;
    int[] sakumaKartiba;

    void Start()
    {
        zimetaji = GetComponentsInChildren<SpriteRenderer>(true);
        sakumaKartiba = new int[zimetaji.Length];
        for (int i = 0; i < zimetaji.Length; i++)
        {
            sakumaKartiba[i] = zimetaji[i].sortingOrder;
        }
    }

    void LateUpdate()
    {
        if (zimetaji == null)
        {
            return;
        }
        int pamats = Mathf.RoundToInt(-transform.position.y * 20f);
        for (int i = 0; i < zimetaji.Length; i++)
        {
            zimetaji[i].sortingOrder = pamats + sakumaKartiba[i];
        }
    }
}

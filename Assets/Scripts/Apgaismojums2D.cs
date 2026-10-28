using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Apgaismojums2D: 2D gaisma 2D versijai (URP 2D Renderer).
//   - globala gaisma, kas lēnām mainās: diena -> vakars -> nakts -> diena
//   - laterna pie spēlētāja un lampas pie ēkām, kas iedegas tumsā
//   - normālkarte apaļajiem objektiem (koku lapotnēm, sunim, strūklakai), lai tie izskatās apjomīgi
// Ja šis skripts nevar strādāt (nav 2D renderētāja vai šeidera), spēle vienkārši paliek bez gaismas.
public class Apgaismojums2D : MonoBehaviour
{
    const string SpritaSheideris = "Universal Render Pipeline/2D/Sprite-Lit-Default";
    const float DienasIlgums = 360f;       // cik sekundes ilgst viena pilna diena

    static Material spritaMaterials;
    static Texture2D apalaNormalKarte;
    static bool irParbaudits = false;
    static bool irPieejams = false;

    Light2D globalaGaisma;
    Light2D laterna;
    List<Light2D> ekuGaismas = new List<Light2D>();
    float laiks = 0f;

    // ---------------------------------------------------------------
    // Palīgfunkcijas (tās izmanto Maksla, veidojot sprite)
    // ---------------------------------------------------------------

    // Vai 2D gaisma vispār ir pieejama?
    public static bool IrPieejams()
    {
        if (irParbaudits == false)
        {
            irParbaudits = true;
            irPieejams = Shader.Find(SpritaSheideris) != null && GraphicsSettings.currentRenderPipeline != null;
        }
        return irPieejams;
    }

    // Materials, kas reaģē uz 2D gaismu (viens materials visiem sprite)
    static Material SpritaMaterials()
    {
        if (spritaMaterials == null)
        {
            spritaMaterials = new Material(Shader.Find(SpritaSheideris));
        }
        return spritaMaterials;
    }

    // Normālkarte aplim: izskatās kā puslode (centrs izliekts pret skatītāju)
    static Texture2D ApalaNormalKarte()
    {
        if (apalaNormalKarte == null)
        {
            int izmers = 64;
            apalaNormalKarte = new Texture2D(izmers, izmers, TextureFormat.RGBA32, false, true);
            apalaNormalKarte.wrapMode = TextureWrapMode.Clamp;
            Color[] pikseli = new Color[izmers * izmers];
            for (int y = 0; y < izmers; y++)
            {
                for (int x = 0; x < izmers; x++)
                {
                    float dx = (x + 0.5f) / izmers * 2f - 1f;
                    float dy = (y + 0.5f) / izmers * 2f - 1f;
                    float attalumsKvadrata = dx * dx + dy * dy;

                    Vector3 normala = new Vector3(0f, 0f, 1f);      // plakana virsma
                    if (attalumsKvadrata < 1f)
                    {
                        float z = Mathf.Sqrt(1f - attalumsKvadrata);
                        normala = new Vector3(dx * 0.8f, dy * 0.8f, z).normalized;
                    }
                    // Normāles vērtības no (-1..1) uz krāsu (0..1)
                    pikseli[y * izmers + x] = new Color(normala.x * 0.5f + 0.5f, normala.y * 0.5f + 0.5f, normala.z * 0.5f + 0.5f, 1f);
                }
            }
            apalaNormalKarte.SetPixels(pikseli);
            apalaNormalKarte.Apply();
        }
        return apalaNormalKarte;
    }

    // Padara sprite jutīgu pret 2D gaismu un (aplim) pievieno normālkarti
    public static void PielietotGaismu(SpriteRenderer zimetajs, bool irAplis)
    {
        if (IrPieejams() == false)
        {
            return;
        }
        zimetajs.sharedMaterial = SpritaMaterials();
        if (irAplis)
        {
            MaterialPropertyBlock bloks = new MaterialPropertyBlock();
            zimetajs.GetPropertyBlock(bloks);
            bloks.SetTexture("_NormalMap", ApalaNormalKarte());
            zimetajs.SetPropertyBlock(bloks);
        }
    }

    // ---------------------------------------------------------------
    // Gaismu izveide
    // ---------------------------------------------------------------
    Light2D IzveidotGaismu(string nosaukums, Transform vecaks, Light2D.LightType veids)
    {
        GameObject objekts = new GameObject(nosaukums);
        objekts.transform.SetParent(vecaks, false);
        Light2D gaisma = objekts.AddComponent<Light2D>();
        gaisma.lightType = veids;
        return gaisma;
    }

    void Start()
    {
        if (IrPieejams() == false)
        {
            return;
        }
        GameManager spele = GameManager.Galvenais;

        // Globala gaisma (apgaismo visu vienādi un nosaka dienas / nakts toni)
        globalaGaisma = IzveidotGaismu("GlobalaGaisma", transform, Light2D.LightType.Global);
        globalaGaisma.intensity = 1f;
        globalaGaisma.color = Color.white;

        // Laterna pie spēlētāja
        laterna = IzveidotGaismu("Laterna", spele.speletajs, Light2D.LightType.Point);
        laterna.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        laterna.pointLightOuterRadius = 8f;
        laterna.pointLightInnerRadius = 1f;
        laterna.color = new Color(1f, 0.92f, 0.75f);
        laterna.intensity = 0.4f;

        // Lampa pie katras zonas ieejas
        for (int i = 0; i < Pasaule.Zonas.Length; i++)
        {
            ZonasInfo zona = Pasaule.Zonas[i];
            float puse = 1f;
            if (zona.centrs.y > 0)
            {
                puse = 1f;      // augšējām zonām lampa ir mazliet augstāk (aiz mijiedarbības vietas)
            }
            else
            {
                puse = -1f;
            }
            Light2D lampa = IzveidotGaismu("Lampa_" + zona.veids, transform, Light2D.LightType.Point);
            lampa.transform.position = new Vector3(zona.mijiedarbibasPunkts.x + 2.2f, zona.mijiedarbibasPunkts.y + puse * 1.4f, 0f);
            lampa.pointLightOuterRadius = 6.5f;
            lampa.pointLightInnerRadius = 0.5f;
            lampa.color = new Color(1f, 0.85f, 0.55f);
            lampa.intensity = 0f;
            ekuGaismas.Add(lampa);
        }

        // Strūklakas gaisma kartes vidū
        Light2D struklaka = IzveidotGaismu("StruklakasGaisma", transform, Light2D.LightType.Point);
        struklaka.transform.position = new Vector3(0f, -1.2f, 0f);
        struklaka.pointLightOuterRadius = 7f;
        struklaka.pointLightInnerRadius = 0.5f;
        struklaka.color = new Color(0.7f, 0.85f, 1f);
        struklaka.intensity = 0f;
        ekuGaismas.Add(struklaka);
    }

    // ---------------------------------------------------------------
    // Diena un nakts
    // ---------------------------------------------------------------
    void Update()
    {
        if (globalaGaisma == null)
        {
            return;
        }

        laiks = laiks + Time.deltaTime;
        float faze = (laiks / DienasIlgums) * 2f * Mathf.PI;
        float diena = (Mathf.Cos(faze) + 1f) * 0.5f;        // 1 = pusdienlaiks, 0 = pusnakts
        float tumsums = 1f - diena;

        // Globālā gaisma: dienā balta un spilgta, naktī zila un tumšāka (bet ne pilnīgi melna)
        globalaGaisma.intensity = Mathf.Lerp(0.5f, 1f, diena);
        globalaGaisma.color = Color.Lerp(new Color(0.55f, 0.62f, 0.95f), Color.white, diena);

        // Lampas deg, kad kļūst tumšs
        for (int i = 0; i < ekuGaismas.Count; i++)
        {
            ekuGaismas[i].intensity = tumsums * 1.2f;
        }

        // Laterna ir vienmēr, bet naktī spilgtāka
        laterna.intensity = 0.4f + tumsums * 0.6f;
    }
}

using UnityEngine;

// Maksla3D: 3D grafika. Visu veidojam no Unity gatavajam formam (kubs, sfera, cilindrs, kapsula).
// Atkal nevajag nekadus modelu failus.
public static class Maksla3D
{
    static Shader sheideris;

    // Atrod sheideri, ar kuru zimet krasainus objektus
    static Shader DabutSheideri()
    {
        if (sheideris == null)
        {
            sheideris = Shader.Find("Universal Render Pipeline/Lit");
        }
        if (sheideris == null)
        {
            sheideris = Shader.Find("Standard");
        }
        if (sheideris == null)
        {
            sheideris = Shader.Find("Sprites/Default");
        }
        return sheideris;
    }

    // Uzliek objektam jaunu materialu ar dotu krasu
    public static void Nokrasot(Renderer zimetajs, Color krasa)
    {
        Material materials = new Material(DabutSheideri());
        materials.color = krasa;
        if (materials.HasProperty("_Smoothness"))
        {
            materials.SetFloat("_Smoothness", 0.15f);       // lai nespid pārāk spoži
        }
        zimetajs.sharedMaterial = materials;
    }

    // Izveido vienu gatavo formu (kubu, sferu...) ar izmēru un krāsu
    static GameObject IzveidotFormu(PrimitiveType veids, string nosaukums, Transform vecaks,
        Vector3 pozicija, Vector3 izmers, Color krasa)
    {
        GameObject objekts = GameObject.CreatePrimitive(veids);
        objekts.name = nosaukums;

        // Sadursmes mums nav vajadzīgas (tās risina klase Pasaule), tāpēc dzēšam Collider
        Collider sadursme = objekts.GetComponent<Collider>();
        if (sadursme != null)
        {
            Object.Destroy(sadursme);
        }

        objekts.transform.SetParent(vecaks, false);
        objekts.transform.localPosition = pozicija;
        objekts.transform.localScale = izmers;
        Nokrasot(objekts.GetComponent<Renderer>(), krasa);
        return objekts;
    }

    public static GameObject Kubs(string nosaukums, Transform vecaks, Vector3 pozicija, Vector3 izmers, Color krasa)
    {
        return IzveidotFormu(PrimitiveType.Cube, nosaukums, vecaks, pozicija, izmers, krasa);
    }

    public static GameObject Sfera(string nosaukums, Transform vecaks, Vector3 pozicija, Vector3 izmers, Color krasa)
    {
        return IzveidotFormu(PrimitiveType.Sphere, nosaukums, vecaks, pozicija, izmers, krasa);
    }

    // Uzmanību: cilindra augstums ir 2 × izmers.y, bet platums ir 1 × izmers.x
    public static GameObject Cilindrs(string nosaukums, Transform vecaks, Vector3 pozicija, Vector3 izmers, Color krasa)
    {
        return IzveidotFormu(PrimitiveType.Cylinder, nosaukums, vecaks, pozicija, izmers, krasa);
    }

    // Tukss objekts (piemeram, locitavas punkts, ap kuru griezt kaju)
    public static Transform TuksaisPunkts(string nosaukums, Transform vecaks, Vector3 pozicija)
    {
        GameObject objekts = new GameObject(nosaukums);
        objekts.transform.SetParent(vecaks, false);
        objekts.transform.localPosition = pozicija;
        return objekts.transform;
    }

    // Teksts 3D pasaule, kas vienmēr ir pagriezts pret kameru
    public static TextMesh Teksts(string teksts, Transform vecaks, Vector3 pozicija, float augstums, Color krasa)
    {
        GameObject objekts = new GameObject("Teksts");
        objekts.transform.SetParent(vecaks, false);
        objekts.transform.localPosition = pozicija;
        objekts.AddComponent<Billboards>();

        TextMesh tekstaKomponente = objekts.AddComponent<TextMesh>();
        tekstaKomponente.font = Maksla.Fonts();
        tekstaKomponente.text = teksts;
        tekstaKomponente.fontSize = 64;
        tekstaKomponente.characterSize = augstums / 6.4f;
        tekstaKomponente.anchor = TextAnchor.MiddleCenter;
        tekstaKomponente.alignment = TextAlignment.Center;
        tekstaKomponente.color = krasa;
        objekts.GetComponent<MeshRenderer>().sharedMaterial = Maksla.Fonts().material;
        return tekstaKomponente;
    }

    // Teksts ar tumšu ēnu (divi teksti viens otram virsū)
    public static void TekstsArVaru(string teksts, Transform vecaks, Vector3 pozicija, float augstums, Color krasa)
    {
        Teksts(teksts, vecaks, pozicija + new Vector3(0.05f, -0.05f, 0.02f), augstums, new Color(0, 0, 0, 0.8f));
        Teksts(teksts, vecaks, pozicija, augstums, krasa);
    }

    // ---------------------------------------------------------------
    // Spēlētājs (skatās uz +Z virzienu)
    // ---------------------------------------------------------------
    public static GameObject IzveidotSpeletaju(Transform vecaks)
    {
        GameObject sakne = new GameObject("Speletajs");
        sakne.transform.SetParent(vecaks, false);
        Transform izskats = TuksaisPunkts("Izskats", sakne.transform, Vector3.zero);

        Color biksesKrasa = new Color(0.2f, 0.22f, 0.4f);
        Color jakasKrasa = new Color(0.25f, 0.55f, 0.85f);
        Color adasKrasa = new Color(1f, 0.82f, 0.68f);
        Color matuKrasa = new Color(0.35f, 0.2f, 0.1f);

        // Kājas: katrai ir "gurna punkts" (pivots), ap kuru tā griežas
        Transform kajaK = TuksaisPunkts("KajaK", izskats, new Vector3(-0.13f, 0.75f, 0f));
        Kubs("Kaja", kajaK, new Vector3(0f, -0.375f, 0f), new Vector3(0.2f, 0.75f, 0.22f), biksesKrasa);
        Transform kajaL = TuksaisPunkts("KajaL", izskats, new Vector3(0.13f, 0.75f, 0f));
        Kubs("Kaja", kajaL, new Vector3(0f, -0.375f, 0f), new Vector3(0.2f, 0.75f, 0.22f), biksesKrasa);

        // Ķermenis un rokas
        Kubs("Ermenis", izskats, new Vector3(0f, 1.08f, 0f), new Vector3(0.52f, 0.66f, 0.3f), jakasKrasa);
        Transform rokaK = TuksaisPunkts("RokaK", izskats, new Vector3(-0.35f, 1.37f, 0f));
        Kubs("Roka", rokaK, new Vector3(0f, -0.3f, 0f), new Vector3(0.14f, 0.6f, 0.16f), jakasKrasa);
        Transform rokaL = TuksaisPunkts("RokaL", izskats, new Vector3(0.35f, 1.37f, 0f));
        Kubs("Roka", rokaL, new Vector3(0f, -0.3f, 0f), new Vector3(0.14f, 0.6f, 0.16f), jakasKrasa);

        // Galva, mati un acis
        Sfera("Galva", izskats, new Vector3(0f, 1.62f, 0f), new Vector3(0.52f, 0.52f, 0.52f), adasKrasa);
        Sfera("Mati", izskats, new Vector3(0f, 1.72f, -0.04f), new Vector3(0.56f, 0.4f, 0.56f), matuKrasa);
        Sfera("AcsK", izskats, new Vector3(-0.1f, 1.63f, 0.23f), new Vector3(0.07f, 0.07f, 0.07f), Color.black);
        Sfera("AcsL", izskats, new Vector3(0.1f, 1.63f, 0.23f), new Vector3(0.07f, 0.07f, 0.07f), Color.black);
        return sakne;
    }

    // ---------------------------------------------------------------
    // Suns (skatās uz +Z virzienu)
    // ---------------------------------------------------------------
    public static GameObject IzveidotSunu(Transform vecaks)
    {
        GameObject sakne = new GameObject("Suns");
        sakne.transform.SetParent(vecaks, false);
        SunaIzskats izskats = sakne.AddComponent<SunaIzskats>();

        // "Izskats" satur visas suņa daļas; to var pacelt (lēciens), neaiztiekot pašu suni
        Transform t = TuksaisPunkts("Izskats", sakne.transform, Vector3.zero);

        // Ermenis (izstiepta sfera)
        GameObject ermenis = Sfera("Ermenis", t, new Vector3(0f, 0.6f, 0f), new Vector3(0.62f, 0.6f, 1.15f), Color.white);
        izskats.kruasaDalas3D.Add(ermenis.GetComponent<Renderer>());

        // Galva ar visam sejas dalam (viss ir viena grupa, lai kustetu kopa)
        Transform galvaGrupa = TuksaisPunkts("GalvaGrupa", t, new Vector3(0f, 0.85f, 0.6f));
        GameObject galva = Sfera("Galva", galvaGrupa, Vector3.zero, new Vector3(0.5f, 0.48f, 0.5f), Color.white);
        izskats.kruasaDalas3D.Add(galva.GetComponent<Renderer>());
        Sfera("Purns", galvaGrupa, new Vector3(0f, -0.06f, 0.27f), new Vector3(0.26f, 0.2f, 0.3f), new Color(0.95f, 0.88f, 0.8f));
        Sfera("Deguns", galvaGrupa, new Vector3(0f, -0.02f, 0.42f), new Vector3(0.1f, 0.08f, 0.08f), Color.black);

        // Acis (grupa, lai varētu aizvērt, samazinot augstumu)
        Transform acis = TuksaisPunkts("Acis", galvaGrupa, new Vector3(0f, 0.08f, 0.2f));
        Sfera("AcsK", acis, new Vector3(-0.13f, 0f, 0.1f), new Vector3(0.07f, 0.07f, 0.07f), Color.black);
        Sfera("AcsL", acis, new Vector3(0.13f, 0f, 0.1f), new Vector3(0.07f, 0.07f, 0.07f), Color.black);

        // Ausis (katrai savs pivots galvas augšā, lai tās var liekt)
        Transform ausisK = TuksaisPunkts("AusisK", galvaGrupa, new Vector3(-0.18f, 0.18f, -0.02f));
        GameObject ausisKForma = Sfera("Auss", ausisK, new Vector3(-0.03f, -0.1f, 0f), new Vector3(0.1f, 0.3f, 0.2f), Color.white);
        izskats.tumsasDalas3D.Add(ausisKForma.GetComponent<Renderer>());
        Transform ausisL = TuksaisPunkts("AusisL", galvaGrupa, new Vector3(0.18f, 0.18f, -0.02f));
        GameObject ausisLForma = Sfera("Auss", ausisL, new Vector3(0.03f, -0.1f, 0f), new Vector3(0.1f, 0.3f, 0.2f), Color.white);
        izskats.tumsasDalas3D.Add(ausisLForma.GetComponent<Renderer>());

        // Četras kājas: pivots gurnos, kāja karājas uz leju
        IzveidotSunaKaju("KajaPK", t, new Vector3(-0.2f, 0.4f, 0.35f), izskats);
        IzveidotSunaKaju("KajaPL", t, new Vector3(0.2f, 0.4f, 0.35f), izskats);
        IzveidotSunaKaju("KajaAK", t, new Vector3(-0.2f, 0.4f, -0.35f), izskats);
        IzveidotSunaKaju("KajaAL", t, new Vector3(0.2f, 0.4f, -0.35f), izskats);

        // Aste (pivots ermeņa aizmugurē)
        Transform aste = TuksaisPunkts("Aste", t, new Vector3(0f, 0.7f, -0.55f));
        GameObject asteForma = Cilindrs("AsteForma", aste, new Vector3(0f, 0f, -0.2f), new Vector3(0.1f, 0.22f, 0.1f), Color.white);
        asteForma.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        izskats.tumsasDalas3D.Add(asteForma.GetComponent<Renderer>());
        izskats.aste = aste;

        // Teksts virs galvas ("!", "zZ", "<3")
        izskats.statusaTeksts = Teksts("", t, new Vector3(0f, 1.8f, 0.3f), 0.5f, Color.white);
        return sakne;
    }

    static void IzveidotSunaKaju(string nosaukums, Transform vecaks, Vector3 gurnaVieta, SunaIzskats izskats)
    {
        Transform kaja = TuksaisPunkts(nosaukums, vecaks, gurnaVieta);
        GameObject forma = Cilindrs("Kaja", kaja, new Vector3(0f, -0.2f, 0f), new Vector3(0.14f, 0.2f, 0.14f), Color.white);
        izskats.tumsasDalas3D.Add(forma.GetComponent<Renderer>());
    }
}

// Billboards: teksts vienmēr skatās uz kameru
public class Billboards : MonoBehaviour
{
    void LateUpdate()
    {
        Camera kamera = Camera.main;
        if (kamera != null)
        {
            transform.rotation = kamera.transform.rotation;
        }
    }
}

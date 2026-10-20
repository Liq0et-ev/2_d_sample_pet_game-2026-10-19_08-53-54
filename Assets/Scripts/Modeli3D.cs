using System.Collections.Generic;
using UnityEngine;

// Modeli3D: gatavie 3D modeli (FBX faili mapē Assets/Resources/Modeli).
// Visi modeli ir bezmaksas (CC0): Kenney.nl un Quaternius. Autoru saraksts ir README failā.
//
// Ja kāds modelis netiek atrasts, funkcijas atgriež null, un spēle izmanto vienkāršās
// formas no klases Maksla3D (tā spēle vienmēr darbojas).
public static class Modeli3D
{
    // Ceļi uz modeļiem (bez .fbx) mapē Resources
    public const string SunaModelis = "Modeli/Suns/Dog";
    public const string SpeletajaModelis = "Modeli/Speletajs/character-a";
    public const string DabasMape = "Modeli/Daba/";
    public const string EkuMape = "Modeli/Ekas/";

    // Cik augsti ir tēli spēles vienībās
    const float SunaAugstums = 1.05f;
    const float SpeletajaAugstums = 1.8f;

    // Ja spēlētāja modelis ir pagriezts uz nepareizo pusi, izmaini šo skaitli (grādos)
    public const float SpeletajaPagrieziens = 0f;

    // ---------------------------------------------------------------
    // Palīgfunkcijas
    // ---------------------------------------------------------------

    // Atrod modela krāsu (dažādiem šejderiem krāsas īpašība saucas citādi)
    static Color DabutKrasu(Material materials)
    {
        if (materials.HasProperty("_BaseColor"))
        {
            return materials.GetColor("_BaseColor");
        }
        if (materials.HasProperty("_Color"))
        {
            return materials.GetColor("_Color");
        }
        return Color.white;
    }

    // Atrod modela tekstūru (vai null)
    static Texture DabutTeksturu(Material materials)
    {
        if (materials.HasProperty("_BaseMap"))
        {
            return materials.GetTexture("_BaseMap");
        }
        if (materials.HasProperty("_MainTex"))
        {
            return materials.GetTexture("_MainTex");
        }
        return null;
    }

    // FBX ieimportētie materiāli var izskatīties rozā (nepareizs šejderis).
    // Tāpēc katram modelim uzliekam savus materiālus ar to pašu krāsu un tekstūru.
    public static void PielagotMaterialus(GameObject modelis)
    {
        Renderer[] zimetaji = modelis.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < zimetaji.Length; i++)
        {
            Material[] vecie = zimetaji[i].sharedMaterials;
            Material[] jaunie = new Material[vecie.Length];
            for (int j = 0; j < vecie.Length; j++)
            {
                Color krasa = Color.white;
                Texture tekstura = null;
                if (vecie[j] != null)
                {
                    krasa = DabutKrasu(vecie[j]);
                    tekstura = DabutTeksturu(vecie[j]);
                }
                jaunie[j] = Maksla3D.JaunsMaterials(krasa, tekstura);
            }
            zimetaji[i].sharedMaterials = jaunie;
        }
    }

    // Visa modela robežas pasaules koordinātēs (kaste, kas apņem visas daļas)
    public static Bounds DabutRobezas(GameObject modelis)
    {
        Renderer[] zimetaji = modelis.GetComponentsInChildren<Renderer>();
        Bounds robezas = new Bounds(modelis.transform.position, Vector3.zero);
        bool pirmais = true;
        for (int i = 0; i < zimetaji.Length; i++)
        {
            if (pirmais)
            {
                robezas = zimetaji[i].bounds;
                pirmais = false;
            }
            else
            {
                robezas.Encapsulate(zimetaji[i].bounds);
            }
        }
        return robezas;
    }

    // Atrod pēcnācēju ar doto nosaukumu (meklē visā hierarhijā)
    static Transform AtrastPecnaceji(Transform vecaks, string nosaukums)
    {
        for (int i = 0; i < vecaks.childCount; i++)
        {
            Transform berns = vecaks.GetChild(i);
            if (berns.name == nosaukums)
            {
                return berns;
            }
            Transform atrasts = AtrastPecnaceji(berns, nosaukums);
            if (atrasts != null)
            {
                return atrasts;
            }
        }
        return null;
    }

    // Noskaidro, uz kuru pusi suns skatās. Suņa modelim ir kaulu "mērķi" kājām
    // (IKFrontLeft, IKFrontRight, IKBackLeft, IKBackRight). Virziens no aizmugures kājām
    // uz priekšējām ir suņa skatiena virziens.
    // Atgriež grādus, par cik jāpagriež modelis, lai tas skatītos uz +Z (pa 90 grādiem).
    static float AtrastSunaPagriezienu(GameObject modelis)
    {
        Transform priekssK = AtrastPecnaceji(modelis.transform, "IKFrontLeft");
        Transform priekssL = AtrastPecnaceji(modelis.transform, "IKFrontRight");
        Transform aizmugureK = AtrastPecnaceji(modelis.transform, "IKBackLeft");
        Transform aizmugureL = AtrastPecnaceji(modelis.transform, "IKBackRight");
        if (priekssK == null || priekssL == null || aizmugureK == null || aizmugureL == null)
        {
            return 0f;      // nevarējām noskaidrot, atstājam kā ir
        }

        Vector3 prieksa = (priekssK.position + priekssL.position) * 0.5f;
        Vector3 aizmugure = (aizmugureK.position + aizmugureL.position) * 0.5f;
        Vector3 virziens = prieksa - aizmugure;         // uz kuru pusi skatās suns
        float lenkis = Mathf.Atan2(virziens.x, virziens.z) * Mathf.Rad2Deg;      // leņķis no +Z uz +X
        return Mathf.Round(-lenkis / 90f) * 90f;
    }

    // ---------------------------------------------------------------
    // Galvenā funkcija: izveido modeli ar pareizu izmēru un vietu
    // ---------------------------------------------------------------
    //   cels             - ceļš Resources mapē (piemēram "Modeli/Daba/tree_oak")
    //   pozicija         - kur novietot (modela apakša un centrs būs šajā punktā)
    //   merka            - izmērs, uz kuru pielāgot
    //   merogotPecAugstuma - true: "merka" ir augstums; false: "merka" ir platums (X)
    //   pagrieziens      - pagriešana grādos ap Y asi
    //   automatiskiPagriezt - true: pagriež modeli pats (priekš suņa)
    //   izmeri           - gatavā modeļa izmēri
    // Atgriež apvalku (tukšu objektu) vai null, ja modelis netika atrasts.
    public static GameObject Izveidot(string cels, Transform vecaks, Vector3 pozicija, float merka,
        bool merogotPecAugstuma, float pagrieziens, bool automatiskiPagriezt, out Vector3 izmeri)
    {
        izmeri = Vector3.zero;
        GameObject sagatave = Resources.Load<GameObject>(cels);
        if (sagatave == null)
        {
            return null;
        }

        GameObject apvalks = new GameObject("Modelis_" + sagatave.name);
        apvalks.transform.SetParent(vecaks, false);
        apvalks.transform.localPosition = pozicija;

        GameObject modelis = Object.Instantiate(sagatave, apvalks.transform);
        modelis.name = sagatave.name;
        modelis.transform.localPosition = Vector3.zero;
        modelis.transform.localRotation = Quaternion.identity;
        modelis.transform.localScale = Vector3.one;
        PielagotMaterialus(modelis);

        // Pagriešana
        if (automatiskiPagriezt)
        {
            pagrieziens = AtrastSunaPagriezienu(modelis);
        }
        modelis.transform.localRotation = Quaternion.Euler(0f, pagrieziens, 0f);

        // Izmērs
        Bounds robezas = DabutRobezas(modelis);
        float merogs = 1f;
        if (merogotPecAugstuma && robezas.size.y > 0.0001f)
        {
            merogs = merka / robezas.size.y;
        }
        else if (merogotPecAugstuma == false && robezas.size.x > 0.0001f)
        {
            merogs = merka / robezas.size.x;
        }
        modelis.transform.localScale = Vector3.one * merogs;

        // Novietojam tā, lai apakša ir uz zemes un centrs uz apvalka punkta
        Bounds jaunas = DabutRobezas(modelis);
        Vector3 nobide = apvalks.transform.position - new Vector3(jaunas.center.x, jaunas.min.y, jaunas.center.z);
        modelis.transform.position = modelis.transform.position + nobide;

        izmeri = jaunas.size;
        return apvalks;
    }

    // Vienkāršāks variants: modelis ar zināmu augstumu, bez pagriešanas
    public static GameObject IzveidotArAugstumu(string cels, Transform vecaks, Vector3 pozicija, float augstums, float pagrieziens)
    {
        Vector3 izmeri;
        return Izveidot(cels, vecaks, pozicija, augstums, true, pagrieziens, false, out izmeri);
    }

    // ---------------------------------------------------------------
    // Spēlētājs
    // ---------------------------------------------------------------
    public static GameObject IzveidotSpeletaju(Transform vecaks)
    {
        Vector3 izmeri;
        GameObject sakne = new GameObject("Speletajs");
        sakne.transform.SetParent(vecaks, false);

        GameObject apvalks = Izveidot(SpeletajaModelis, sakne.transform, Vector3.zero, SpeletajaAugstums,
            true, SpeletajaPagrieziens, false, out izmeri);
        if (apvalks == null)
        {
            Object.Destroy(sakne);
            return null;
        }

        // Animācijas: stāv / iet
        GameObject modelis = apvalks.transform.GetChild(0).gameObject;
        ModelaAnimacijasAtskanotajs atskanotajs = modelis.AddComponent<ModelaAnimacijasAtskanotajs>();
        atskanotajs.Sagatavot(SpeletajaModelis);
        sakne.AddComponent<SpeletajaModelaAnimacija>().atskanotajs = atskanotajs;
        return sakne;
    }

    // ---------------------------------------------------------------
    // Suns
    // ---------------------------------------------------------------
    public static GameObject IzveidotSunu(Transform vecaks)
    {
        Vector3 izmeri;
        GameObject sakne = new GameObject("Suns");
        sakne.transform.SetParent(vecaks, false);
        SunaIzskats izskats = sakne.AddComponent<SunaIzskats>();

        // "Izskats" ir objekts, ko pagriežam un paceļam sēdēšanai, guļēšanai un lēkšanai
        Transform izskatsObjekts = Maksla3D.TuksaisPunkts("Izskats", sakne.transform, Vector3.zero);

        GameObject apvalks = Izveidot(SunaModelis, izskatsObjekts, Vector3.zero, SunaAugstums, true, 0f, true, out izmeri);
        if (apvalks == null)
        {
            Object.Destroy(sakne);
            return null;
        }

        // Visas modela daļas var nokrāsot suņa kažoka krāsā
        Renderer[] zimetaji = apvalks.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < zimetaji.Length; i++)
        {
            izskats.kruasaDalas3D.Add(zimetaji[i]);
        }

        // Animācijas: stāv / iet
        GameObject modelis = apvalks.transform.GetChild(0).gameObject;
        ModelaAnimacijasAtskanotajs atskanotajs = modelis.AddComponent<ModelaAnimacijasAtskanotajs>();
        atskanotajs.Sagatavot(SunaModelis);

        // Teksts virs galvas ("!", "zZ", "<3")
        izskats.statusaTeksts = Maksla3D.Teksts("", sakne.transform, new Vector3(0f, izmeri.y + 0.5f, 0f), 0.5f, Color.white);
        return sakne;
    }
}

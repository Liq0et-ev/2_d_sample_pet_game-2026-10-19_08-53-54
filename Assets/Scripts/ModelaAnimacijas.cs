using UnityEngine;

// SunaAnimacijaBaze: kopīga "forma" abām 3D suņa animāciju versijām
// (viena strādā ar vienkāršajām formām, otra - ar gatavo modeli).
// Pārējie skripti izsauc šīs funkcijas, nezinot, kura versija ir izmantota.
public abstract class SunaAnimacijaBaze : MonoBehaviour
{
    public abstract void Ediens();
    public abstract void Priecasanas();
}

// ModelaAnimacijasAtskanotajs: atskano gatavā modela animācijas (piemēram "idle", "walk").
// Izmanto vecāko Animation komponenti, jo tā ir vienkāršākā: nosaukums -> atskaņot.
public class ModelaAnimacijasAtskanotajs : MonoBehaviour
{
    Animation animacija;

    // Ielādē visas animācijas no FBX faila (ceļš Resources mapē)
    public void Sagatavot(string resursaCels)
    {
        // Parasto Animator izslēdzam, lai tas netraucē
        Animator animators = GetComponent<Animator>();
        if (animators != null)
        {
            animators.enabled = false;
        }

        animacija = gameObject.GetComponent<Animation>();
        if (animacija == null)
        {
            animacija = gameObject.AddComponent<Animation>();
        }
        animacija.playAutomatically = false;

        AnimationClip[] klipi = Resources.LoadAll<AnimationClip>(resursaCels);
        for (int i = 0; i < klipi.Length; i++)
        {
            // Klipi ar "__" sākumā ir Unity iekšējie priekšskatījumi
            if (klipi[i].name.StartsWith("__"))
            {
                continue;
            }
            klipi[i].legacy = true;
            klipi[i].wrapMode = WrapMode.Loop;
            animacija.AddClip(klipi[i], IsaisNosaukums(klipi[i].name));
        }
    }

    // "DogArmature|Walking" -> "walking" (mazie burti, bez priedēkļa)
    static string IsaisNosaukums(string pilnsNosaukums)
    {
        string nosaukums = pilnsNosaukums;
        int atdalitajs = nosaukums.LastIndexOf('|');
        if (atdalitajs >= 0)
        {
            nosaukums = nosaukums.Substring(atdalitajs + 1);
        }
        return nosaukums.ToLower();
    }

    // Vai ir animācija ar šādu nosaukumu?
    public bool IrAnimacija(string nosaukums)
    {
        return animacija != null && animacija.GetClip(nosaukums) != null;
    }

    // Pāriet uz dotu animāciju (mīksta pāreja)
    public void Atskanot(string nosaukums, float atrums = 1f)
    {
        if (IrAnimacija(nosaukums) == false)
        {
            return;
        }
        animacija[nosaukums].speed = atrums;
        animacija.CrossFade(nosaukums, 0.2f);
    }
}

// SpeletajaModelaAnimacija: spēlētāja modelis stāv vai iet atkarībā no kustības.
public class SpeletajaModelaAnimacija : MonoBehaviour
{
    public ModelaAnimacijasAtskanotajs atskanotajs;
    PlayerMovement3D kustiba;
    string pasreizeja = "";

    void Update()
    {
        if (atskanotajs == null)
        {
            return;
        }
        if (kustiba == null)
        {
            kustiba = GetComponent<PlayerMovement3D>();
            if (kustiba == null)
            {
                return;
            }
        }

        string vajadziga = "idle";
        if (kustiba.kustas)
        {
            vajadziga = "sprint";
            if (atskanotajs.IrAnimacija(vajadziga) == false)
            {
                vajadziga = "walk";
            }
        }

        if (vajadziga != pasreizeja)
        {
            pasreizeja = vajadziga;
            atskanotajs.Atskanot(vajadziga);
        }
    }
}

// SunaModelaAnimacija: gatavā suņa modeļa animācijas.
// Modelim ir tikai divas gatavas animācijas (stāv un iet). Pārējās pozas
// (sēž, guļ, ēd, lec) veidojam, pagriežot un saspiežot visu suni.
public class SunaModelaAnimacija : SunaAnimacijaBaze
{
    Transform izskats;                  // objekts, ko pagriežam, paceļam un saspiežam
    ModelaAnimacijasAtskanotajs atskanotajs;
    bool sagatavots = false;

    SunaStavoklis stavoklis = SunaStavoklis.Stav;
    float stavesanasLaiks = 0f;
    float ediensTaimeris = 0f;
    float lecienaTaimeris = 0f;
    string pasreizejaAnimacija = "";

    const float LecienaIlgums = 0.9f;

    // Pasreizejas vertibas (lēnām tuvojas mērķa vērtībām)
    float pitch = 0f;
    float augstumaMerogs = 1f;
    float garumaMerogs = 1f;

    public override void Ediens()
    {
        ediensTaimeris = 1.4f;
    }

    public override void Priecasanas()
    {
        lecienaTaimeris = LecienaIlgums;
    }

    void Sagatavot()
    {
        izskats = transform.Find("Izskats");
        atskanotajs = GetComponentInChildren<ModelaAnimacijasAtskanotajs>();
        sagatavots = true;
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele == null || spele.irSuns == false)
        {
            return;
        }
        if (sagatavots == false)
        {
            Sagatavot();
        }
        if (izskats == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        NoteiktStavokli(spele, dt);

        // Mērķa vērtības katrai pozai
        float merkaPitch = 0f;
        float merkaAugstums = 1f;
        float merkaGarums = 1f;
        float visasY = 0f;

        if (stavoklis == SunaStavoklis.Sed)
        {
            merkaPitch = -22f;          // priekša uz augšu, aizmugure uz leju
            merkaAugstums = 0.85f;
        }
        else if (stavoklis == SunaStavoklis.Guli)
        {
            merkaAugstums = 0.6f;       // suns "nolaižas" zemē
            merkaGarums = 1.05f;
        }
        else if (stavoklis == SunaStavoklis.Ed)
        {
            merkaPitch = 14f + Mathf.Sin(Time.time * 14f) * 5f;       // deguns uz leju un "kasās"
        }
        else if (stavoklis == SunaStavoklis.Lec)
        {
            float progress = 1f - lecienaTaimeris / LecienaIlgums;
            visasY = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 2f)) * 0.7f;
        }

        // Slims suns nedaudz nosvērts uz priekšu
        if (spele.suns.slims && stavoklis != SunaStavoklis.Guli)
        {
            merkaPitch = merkaPitch + 8f;
        }

        // Mīksta pāreja uz mērķa vērtībām
        float k = 1f - Mathf.Exp(-10f * dt);
        pitch = Mathf.Lerp(pitch, merkaPitch, k);
        augstumaMerogs = Mathf.Lerp(augstumaMerogs, merkaAugstums, k);
        garumaMerogs = Mathf.Lerp(garumaMerogs, merkaGarums, k);

        izskats.localPosition = new Vector3(0f, visasY, 0f);
        izskats.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        izskats.localScale = new Vector3(1f, augstumaMerogs, garumaMerogs);

        // Gatavā animācija: iet vai stāv
        if (atskanotajs != null)
        {
            string vajadziga = "idle";
            if (stavoklis == SunaStavoklis.Iet || stavoklis == SunaStavoklis.Lec)
            {
                vajadziga = "walking";
            }
            if (vajadziga != pasreizejaAnimacija)
            {
                pasreizejaAnimacija = vajadziga;
                float atrums = 1f;
                if (vajadziga == "walking")
                {
                    atrums = 1.6f;          // skrienot animācija ir ātrāka
                }
                atskanotajs.Atskanot(vajadziga, atrums);
            }
        }
    }

    void NoteiktStavokli(GameManager spele, float dt)
    {
        if (ediensTaimeris > 0f)
        {
            ediensTaimeris = ediensTaimeris - dt;
            stavoklis = SunaStavoklis.Ed;
            return;
        }
        if (lecienaTaimeris > 0f)
        {
            lecienaTaimeris = lecienaTaimeris - dt;
            stavoklis = SunaStavoklis.Lec;
            return;
        }
        if (spele.SunaKustas())
        {
            stavesanasLaiks = 0f;
            stavoklis = SunaStavoklis.Iet;
            return;
        }

        stavesanasLaiks = stavesanasLaiks + dt;
        bool nogurisVaiSlims = spele.suns.energija < 20f || spele.suns.slims;
        if (nogurisVaiSlims && stavesanasLaiks > 3f)
        {
            stavoklis = SunaStavoklis.Guli;
        }
        else if (stavesanasLaiks > 5f)
        {
            stavoklis = SunaStavoklis.Sed;
        }
        else
        {
            stavoklis = SunaStavoklis.Stav;
        }
    }
}

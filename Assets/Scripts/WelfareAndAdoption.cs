using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Welfare centre: gives dogs, runs the adoption test and periodic care inspections.
public class WelfareAndAdoption : MonoBehaviour
{
    GameManager gm { get { return GameManager.I; } }

    static readonly string[] Names = { "Reksis", "Bella", "Maksis", "Luna", "Čaks", "Mila", "Brūnis", "Ziba", "Toms", "Pūka", "Rūfus", "Dūka" };
    static readonly Color[] Coats =
    {
        new Color(0.85f, 0.58f, 0.30f), new Color(0.55f, 0.38f, 0.25f), new Color(0.92f, 0.88f, 0.78f),
        new Color(0.35f, 0.33f, 0.35f), new Color(0.80f, 0.70f, 0.40f), new Color(0.75f, 0.45f, 0.30f),
    };

    // adoption test state
    enum Phase { None, Show, Input, Result }
    Phase phase;
    List<Command> sequence = new List<Command>();
    int idx;
    string resultText = "";
    bool resultOk;

    // inspections
    float inspectTimer;
    int strikes;
    const float InspectInterval = 90f;

    public void OpenMenu()
    {
        phase = Phase.None;
        gm.ui.OpenMenu(Build);
    }

    // ---- readiness ----
    public List<string> Missing()
    {
        var list = new List<string>();
        var d = gm.dog;
        if (gm.dogProg.LearnedCount < 4) list.Add("Apgūtas komandas: " + gm.dogProg.LearnedCount + "/4");
        if (d.health < 70f) list.Add("Veselība ≥ 70 (tagad " + Mathf.RoundToInt(d.health) + ")");
        if (d.sick) list.Add("Suns ir slims");
        if (d.mood < 50f) list.Add("Garastāvoklis ≥ 50 (tagad " + Mathf.RoundToInt(d.mood) + ")");
        if (d.food < 40f) list.Add("Suns nedrīkst būt izsalcis (paēdis ≥ 40)");
        if (d.energy < 20f) list.Add("Enerģija ≥ 20 (tagad " + Mathf.RoundToInt(d.energy) + ")");
        return list;
    }

    public bool IsReady() { return gm.hasDog && Missing().Count == 0; }

    // ---- menu ----
    MenuDef Build()
    {
        var m = new MenuDef { title = "Labklājības centrs" };

        if (phase == Phase.Show || phase == Phase.Input) return BuildTest();
        if (phase == Phase.Result)
        {
            m.title = resultOk ? "Pārbaudījums nokārtots!" : "Pārbaudījums neizdevās";
            m.body = resultText;
            m.closeLabel = "Labi";
            return m;
        }

        if (!gm.hasDog)
        {
            if (gm.adoptions >= GameManager.WinAdoptions)
                m.body = "Darbinieks: Apsveicam! Tu esi sasniedzis lielāko atzinību – " + gm.adoptions + " veiksmīgas adopcijas. Vari turpināt brīvajā režīmā.";
            else if (gm.adoptions == 0)
                m.body = "Darbinieks: Sveiks! Mēs palīdzam suņiem atrast jaunas ģimenes. Uzticēsim tev pirmo suni – parūpējies par to, iemāci 4 komandas un sagatavo adopcijai.";
            else
                m.body = "Darbinieks: Paldies par darbu! Adopcijas: " + gm.adoptions + "/" + GameManager.WinAdoptions + ". Esam gatavi uzticēt nākamo suni.";
            m.items.Add(new MenuItem("Saņemt suni", () => { GiveDog(); gm.ui.CloseMenu(); }));
            return m;
        }

        var d = gm.dog;
        var missing = Missing();
        m.body = "Darbinieks: " + d.dogName + " – " + DogStatus.TemperamentName(d.temperament) + ", " + gm.dogProg.level + ". līmenis.\n" +
                 (missing.Count == 0
                    ? "Visi nosacījumi izpildīti – var sākt adopcijas pārbaudījumu!"
                    : "Vēl jāizpilda:\n  • " + string.Join("\n  • ", missing));
        m.items.Add(new MenuItem("Sākt adopcijas pārbaudījumu", StartTest, missing.Count == 0));
        return m;
    }

    void GiveDog()
    {
        var d = gm.dog;
        var t = (Temperament)Random.Range(0, 3);
        d.Init(Names[Random.Range(0, Names.Length)], t, Coats[Random.Range(0, Coats.Length)]);
        gm.dogProg.ResetProgress();
        gm.hasDog = true;
        gm.fedOnce = gm.adoptions > 0;       // tutorial hint only for the first dog
        gm.dogObj.SetActive(true);
        gm.dogFollow.Snap();
        gm.audioMgr.Play(Sfx.Bark);
        gm.ui.Toast("Saņēmi suni " + d.dogName + "! " + d.TemperamentInfo());
        strikes = 0;
    }

    // ---- adoption test: memorise and repeat the command sequence ----
    void StartTest()
    {
        sequence.Clear();
        int len = Mathf.Min(7, 4 + gm.adoptions / 2);
        for (int i = 0; i < len; i++) sequence.Add((Command)Random.Range(0, 4));
        idx = 0;
        phase = Phase.Show;
        StartCoroutine(ShowThenInput(1.2f * len + 1.5f));
    }

    IEnumerator ShowThenInput(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (phase == Phase.Show && gm.ui.MenuOpen)
        {
            phase = Phase.Input;
            gm.ui.RefreshMenu();
        }
        else if (phase == Phase.Show) phase = Phase.None;
    }

    MenuDef BuildTest()
    {
        var m = new MenuDef { title = "Adopcijas pārbaudījums", showClose = false };
        if (phase == Phase.Show)
        {
            var names = new List<string>();
            foreach (var c in sequence) names.Add(Commands.Name(c));
            m.body = "Iegaumē komandu secību, kuru suns jāizpilda:\n\n" + string.Join("  →  ", names) + "\n\nPēc brīža tā pazudīs...";
            return m;
        }
        m.body = "Atkārto komandu secību! Izpildītas komandas: " + idx + "/" + sequence.Count;
        for (int i = 0; i < 4; i++)
        {
            var c = (Command)i;
            m.items.Add(new MenuItem(Commands.Name(c), () => Answer(c)));
        }
        return m;
    }

    void Answer(Command c)
    {
        if (c != sequence[idx])
        {
            Finish(false, "Darbinieks: Nepareiza komanda secībā. " + gm.dog.dogName + " vēl nav gatavs. Pamēģini vēlreiz, kad būsi gatavs.");
            return;
        }
        gm.audioMgr.Play(Sfx.Good);
        idx++;
        if (idx >= sequence.Count) Finish(true, "");
    }

    void Finish(bool ok, string text)
    {
        var d = gm.dog;
        resultOk = ok;
        phase = Phase.Result;
        if (!ok)
        {
            d.Mod(0, 0, -5, -3);
            gm.audioMgr.Play(Sfx.Bad);
            resultText = text;
            return;
        }

        int level = gm.dogProg.level;
        int reward = 60 + 15 * level + 5 * gm.adoptions;
        gm.adoptions++;
        gm.economy.Earn(reward);
        gm.playerProg.AddXp(40 + 10 * level);
        string name = d.dogName;
        gm.hasDog = false;
        gm.dogObj.SetActive(false);
        gm.audioMgr.Play(Sfx.Level);
        resultText = name + " tika veiksmīgi adoptēts un ieguva jaunu ģimeni!\nAtlīdzība: " + reward + " monētas.\nAdopcijas: " + gm.adoptions + "/" + GameManager.WinAdoptions;
        if (gm.adoptions == GameManager.WinAdoptions)
            resultText += "\n\nTu sasniedzi galveno mērķi – labklājības centrs piešķir tev īpašu atzinību! Vari turpināt brīvajā režīmā.";
    }

    // ---- periodic welfare inspections ----
    void Update()
    {
        if (!gm.hasDog || gm.ui.Blocking) return;
        inspectTimer += Time.deltaTime;
        if (inspectTimer < InspectInterval) return;
        inspectTimer = 0f;

        var d = gm.dog;
        // Tiredness alone is never penalised.
        bool neglected = d.food < 15f || d.health < 30f || d.mood < 10f;
        if (neglected)
        {
            strikes++;
            if (strikes == 1)
            {
                gm.ui.Toast("Labklājības centra brīdinājums: " + d.dogName + " netiek pienācīgi aprūpēts! Uzlabo stāvokli.");
                gm.audioMgr.Play(Sfx.Bad);
            }
            else
            {
                int fine = Mathf.Min(gm.economy.coins, 30);
                gm.economy.coins -= fine;
                gm.ui.Toast("Labklājības centrs piemēroja naudas sodu (" + fine + " monētas) par nolaidību.");
                gm.audioMgr.Play(Sfx.Bad);
            }
        }
        else if (strikes > 0) strikes--;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Command training + the "Aportēšana" timing minigame.
public class TrainingSystem : MonoBehaviour
{
    GameManager gm { get { return GameManager.I; } }
    bool useTreat;

    public void OpenMenu() { gm.ui.OpenMenu(Build); }

    string CannotTrain()
    {
        var d = gm.dog;
        if (d.sick) return d.dogName + " ir slims – vispirms uz klīniku.";
        if (d.energy < 15f) return d.dogName + " ir pārāk noguris – ļauj atpūsties.";
        if (d.food < 15f) return d.dogName + " ir izsalcis – pabaro to.";
        return null;
    }

    MenuDef Build()
    {
        if (!gm.hasDog) return new MenuDef { title = "Treniņu laukums", body = "Tev nav suņa. Saņem to Labklājības centrā." };
        var d = gm.dog; var p = gm.dogProg; var e = gm.economy;
        var m = new MenuDef { title = "Treniņu laukums" };
        m.body = d.dogName + " – " + p.level + ". līmenis (" + p.xp + "/" + p.XpToNext + " pieredze)\n" +
                 "Enerģija " + Mathf.RoundToInt(d.energy) + "  |  Garastāvoklis " + Mathf.RoundToInt(d.mood) + "  |  Gardumi: " + e.treats +
                 "\nLaukuma līmenis: " + e.fieldLevel + "/" + EconomyManager.MaxUpgrade;
        string block = CannotTrain();
        if (block != null) m.body += "\n" + block;

        for (int i = 0; i < 4; i++)
        {
            var c = (Command)i;
            string status = p.Learned(c) ? "apgūta" : Mathf.RoundToInt(p.skill[i]) + "%";
            m.items.Add(new MenuItem("Trenēt: " + Commands.Name(c) + "  (" + status + ")", () => Train(c), block == null));
        }
        m.items.Add(new MenuItem("Izmantot gardumu treniņā: " + (useTreat ? "JĀ" : "NĒ"), () => useTreat = !useTreat, true));
        bool canFetch = block == null && e.toy && d.energy >= 20f;
        m.items.Add(new MenuItem(e.toy ? "Minispēle: Aportēšana" : "Minispēle: Aportēšana (vajag rotaļlietu)", StartFetch, canFetch));

        if (e.fieldLevel < EconomyManager.MaxUpgrade)
        {
            int target = e.fieldLevel + 1;
            int cost = 100 * target;
            int req = target + 2;
            bool ok = gm.playerProg.level >= req;
            m.items.Add(new MenuItem("Uzlabot laukumu → " + target + ". līm. (" + cost + " monētas, spēlētāja līm. " + req + ")",
                () => { if (ok && e.Spend(cost)) { e.fieldLevel++; gm.ui.Toast("Treniņu laukums uzlabots!"); gm.audioMgr.Play(Sfx.Good); } }, ok));
        }
        return m;
    }

    float SuccessChance(Command c)
    {
        var d = gm.dog; var e = gm.economy;
        float p = 0.45f + 0.20f * (d.mood / 100f) + 0.15f * (d.energy / 100f) + 0.03f * gm.dogProg.level;
        p -= 0.05f * (int)c;                          // later commands are harder
        p += 0.04f * e.fieldLevel + (e.cones ? 0.08f : 0f);
        if (useTreat && e.treats > 0) p += 0.20f;
        switch (d.temperament)
        {
            case Temperament.Active: if (c == Command.Come) p += 0.10f; break;
            case Temperament.Calm: if (c != Command.Come) p += 0.08f; break;
            case Temperament.Timid: if (d.mood < 55f) p -= 0.15f; break;
        }
        return Mathf.Clamp(p, 0.1f, 0.95f);
    }

    void Train(Command c)
    {
        string block = CannotTrain();
        if (block != null) { gm.ui.Toast(block); return; }
        var d = gm.dog; var p = gm.dogProg; var e = gm.economy;
        bool treated = false;
        if (useTreat && e.treats > 0) { e.treats--; treated = true; }

        bool success = Random.value < SuccessChance(c);
        float eCost = 8f * (d.temperament == Temperament.Active ? 1.4f : d.temperament == Temperament.Calm ? 0.8f : 1f);
        d.Mod(0, -4, -eCost, treated ? 4 : 0);

        if (success)
        {
            bool wasLearned = p.Learned(c);
            p.skill[(int)c] = Mathf.Min(100f, p.skill[(int)c] + Random.Range(20f, 30f));
            p.AddXp(15);
            gm.playerProg.AddXp(5);
            e.Earn(3);
            gm.audioMgr.Play(Sfx.Good);
            if (!wasLearned && p.Learned(c))
            {
                e.Earn(10, true);
                gm.ui.Toast(d.dogName + " apguva komandu \"" + Commands.Name(c) + "\"! (+10 monētas)");
                gm.audioMgr.Play(Sfx.Bark);
            }
            else gm.ui.Toast("Veiksmīgi! \"" + Commands.Name(c) + "\" – " + Mathf.RoundToInt(p.skill[(int)c]) + "%");
        }
        else
        {
            p.skill[(int)c] = Mathf.Min(100f, p.skill[(int)c] + 4f);
            d.Mod(0, 0, 0, -3);
            gm.audioMgr.Play(Sfx.Bad);
            gm.ui.Toast(d.dogName + " nesaprata. Mēģini vēlreiz (gardums un laba noskaņa palīdz).");
        }
    }

    // ---------------- Fetch minigame ----------------
    GameObject fetchRoot;
    RectTransform marker, zone, track;
    Text info;
    bool fetching, done;
    int round, hits;
    float zoneCenter, zoneWidth, clock, speed, finishTimer;
    const float TrackW = 640f;

    void StartFetch()
    {
        gm.ui.CloseMenu();
        RectTransform panel;
        fetchRoot = gm.ui.CreateModal(new Vector2(760, 360), out panel);
        gm.ui.onModalCancel = EndFetch;
        var title = gm.ui.NewText(panel, "Aportēšana", 40, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(700, 50));
        title.fontStyle = FontStyle.Bold;
        info = gm.ui.NewText(panel, "", 26, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -76), new Vector2(700, 70));
        track = gm.ui.NewImage("Track", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(TrackW, 44), new Color(0, 0, 0, 0.6f)).rectTransform;
        zone = gm.ui.NewImage("Zone", track, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 44), new Color(0.3f, 0.8f, 0.35f, 0.9f)).rectTransform;
        marker = gm.ui.NewImage("Marker", track, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10, 64), Color.white).rectTransform;
        gm.ui.NewButton(panel, "MEST!  (atstarpe)", Throw, new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(360, 64));
        round = 0; hits = 0; done = false; finishTimer = 0f; clock = 0f;
        fetching = true;
        NextRound();
    }

    void NextRound()
    {
        round++;
        speed = 0.7f + 0.25f * round - (gm.dog.energy < 40f ? 0.1f : 0f);
        zoneWidth = Mathf.Clamp(0.22f - 0.03f * round + 0.03f * gm.dogProg.level + (gm.dog.temperament == Temperament.Active ? 0.02f : 0f), 0.1f, 0.3f);
        zoneCenter = Random.Range(0.2f, 0.8f);
        zone.sizeDelta = new Vector2(zoneWidth * TrackW, 44);
        zone.anchoredPosition = new Vector2(zoneCenter * TrackW, 0);
        clock = 0f;
        info.text = "Raunds " + round + "/3  –  nospied MEST!, kad atzīme ir zaļajā zonā.   Trāpījumi: " + hits;
    }

    void Throw()
    {
        if (!fetching || done) return;
        float t = Mathf.PingPong(clock * speed, 1f);
        bool hit = Mathf.Abs(t - zoneCenter) <= zoneWidth * 0.5f;
        if (hit) { hits++; gm.audioMgr.Play(Sfx.Good); }
        else gm.audioMgr.Play(Sfx.Bad);
        if (round >= 3)
        {
            done = true;
            finishTimer = 1.4f;
            info.text = "Trāpījumi: " + hits + "/3" + (hits >= 2 ? "  –  lieliski!" : "  –  nākamreiz labāk!");
        }
        else NextRound();
    }

    void EndFetch()
    {
        if (!fetching) return;
        fetching = false;
        gm.ui.CloseModal(fetchRoot);
        fetchRoot = null;
        if (round == 0) return;
        var d = gm.dog;
        d.Mod(0, -4, -15, 4 * hits - 2);
        if (hits > 0)
        {
            gm.economy.Earn(4 * hits);
            gm.dogProg.AddXp(12 * hits);
            gm.playerProg.AddXp(5 * hits);
            gm.audioMgr.Play(Sfx.Bark);
        }
        gm.ui.Toast("Aportēšana: " + hits + "/3 trāpījumi" + (hits > 0 ? "  (+" + 4 * hits + " monētas)" : "."));
    }

    void Update()
    {
        if (!fetching) return;
        clock += Time.unscaledDeltaTime;
        if (done)
        {
            finishTimer -= Time.unscaledDeltaTime;
            if (finishTimer <= 0f) EndFetch();
            return;
        }
        float t = Mathf.PingPong(clock * speed, 1f);
        marker.anchoredPosition = new Vector2(t * TrackW, 0);
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame) Throw();
    }
}

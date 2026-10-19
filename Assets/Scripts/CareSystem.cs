using UnityEngine;

// Feeding, resting, walking, playing and treatment (Home, Park, Clinic menus).
public class CareSystem : MonoBehaviour
{
    GameManager gm { get { return GameManager.I; } }

    MenuDef NoDog(string title)
    {
        return new MenuDef { title = title, body = "Tev vēl nav suņa. Dodies uz Labklājības centru, lai saņemtu savu pirmo suni." };
    }

    // ---------------- HOME ----------------
    public void OpenHomeMenu() { gm.ui.OpenMenu(HomeMenu); }

    MenuDef HomeMenu()
    {
        if (!gm.hasDog) return NoDog("Māja");
        var d = gm.dog; var e = gm.economy;
        var m = new MenuDef { title = "Māja" };
        m.body = d.dogName + " – " + DogStatus.TemperamentName(d.temperament) + ". " + d.TemperamentInfo() +
                 "\nBarība inventārā: " + e.food + "   |   Mājas līmenis: " + e.houseLevel + "/" + EconomyManager.MaxUpgrade;
        m.items.Add(new MenuItem("Pabarot suni (barība: " + e.food + ")", Feed, e.food > 0 && d.food < 95f));
        m.items.Add(new MenuItem("Likt atpūsties guļvietā", Rest, d.energy < 95f));

        if (e.houseLevel < EconomyManager.MaxUpgrade)
        {
            int target = e.houseLevel + 1;
            int cost = 80 * target;
            int req = target + 1;
            bool ok = gm.playerProg.level >= req;
            m.items.Add(new MenuItem("Uzlabot māju → " + target + ". līm. (" + cost + " monētas, spēlētāja līm. " + req + ")",
                () => { if (ok && e.Spend(cost)) { e.houseLevel++; gm.ui.Toast("Māja uzlabota! Labāka atpūta un lielāka pasīvā peļņa."); gm.audioMgr.Play(Sfx.Good); } },
                ok));
        }
        return m;
    }

    void Feed()
    {
        var d = gm.dog; var e = gm.economy;
        if (e.food <= 0) { gm.ui.Toast("Nav barības – nopērc to veikalā."); return; }
        e.food--;
        d.Mod(0, 40, 0, 2);
        gm.fedOnce = true;
        gm.audioMgr.Play(Sfx.Bark);
        gm.ui.Toast(d.dogName + " paēda.");
    }

    void Rest()
    {
        var d = gm.dog; var e = gm.economy;
        gm.ui.Fade();
        d.Mod(3, -8, 50 + 15 * e.houseLevel, 3);
        gm.audioMgr.Play(Sfx.Good);
        gm.ui.Toast(d.dogName + " izgulējās un atguva spēkus.");
    }

    // ---------------- PARK ----------------
    public void OpenParkMenu() { gm.ui.OpenMenu(ParkMenu); }

    MenuDef ParkMenu()
    {
        if (!gm.hasDog) return NoDog("Parks");
        var d = gm.dog; var e = gm.economy;
        var m = new MenuDef { title = "Parks" };
        m.body = "Pastaigas patērē enerģiju, bet uzlabo garastāvokli." + (e.leash ? " Ar pavadu pastaiga ir patīkamāka." : "");
        bool canWalk = d.energy >= 15f && !d.sick;
        m.items.Add(new MenuItem(canWalk ? "Doties pastaigā" : "Doties pastaigā (suns ir noguris vai slims)", Walk, canWalk));
        bool canPlay = e.toy && d.energy >= 10f && !d.sick;
        m.items.Add(new MenuItem(e.toy ? "Spēlēties ar rotaļlietu" : "Spēlēties (vajag rotaļlietu no veikala)", Play, canPlay));
        return m;
    }

    void Walk()
    {
        var d = gm.dog; var e = gm.economy;
        gm.ui.Fade();
        d.Mod(0, -5, -12, e.leash ? 18 : 10);
        e.Earn(1);
        gm.dogProg.AddXp(3);
        gm.ui.Toast(d.dogName + " izbaudīja pastaigu.");
    }

    void Play()
    {
        var d = gm.dog;
        d.Mod(0, -3, -10, 20);
        gm.audioMgr.Play(Sfx.Bark);
        gm.dogProg.AddXp(4);
        gm.ui.Toast(d.dogName + " priecīgi spēlējas!");
    }

    // ---------------- CLINIC ----------------
    public void OpenClinicMenu() { gm.ui.OpenMenu(ClinicMenu); }

    MenuDef ClinicMenu()
    {
        if (!gm.hasDog) return NoDog("Veterinārā klīnika");
        var d = gm.dog;
        var m = new MenuDef { title = "Veterinārā klīnika" };
        m.body = "Veterinārārsts: " + (d.sick ? d.dogName + " ir slims, vajag ārstēšanu." : d.health < 100f ? "Var veikt profilaktisko apskati." : d.dogName + " ir pilnīgi vesels.") +
                 "\nVeselība: " + Mathf.RoundToInt(d.health);
        int cost = d.sick ? 40 : 15;
        m.items.Add(new MenuItem((d.sick ? "Ārstēt slimību" : "Profilaktiskā apskate") + " (" + cost + " monētas)", () =>
        {
            if (!gm.economy.Spend(cost)) return;
            d.sick = false;
            d.Mod(100, 0, 0, 5);
            gm.audioMgr.Play(Sfx.Good);
            gm.ui.Toast(d.dogName + " ir vesels!");
        }, d.sick || d.health < 99f));
        return m;
    }
}

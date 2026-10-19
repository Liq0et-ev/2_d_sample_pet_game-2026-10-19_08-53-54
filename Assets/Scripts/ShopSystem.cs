using UnityEngine;

// Shop: food, treats, toy, leash and training cones.
public class ShopSystem : MonoBehaviour
{
    GameManager gm { get { return GameManager.I; } }

    public void OpenMenu() { gm.ui.OpenMenu(Build); }

    MenuDef Build()
    {
        var e = gm.economy;
        var m = new MenuDef { title = "Veikals" };
        m.body = "Pārdevējs: Laipni lūgti! Monētas: " + e.coins +
                 "\nInventārs – barība: " + e.food + ", gardumi: " + e.treats +
                 ", rotaļlieta: " + (e.toy ? "ir" : "nav") + ", pavada: " + (e.leash ? "ir" : "nav") + ", konusi: " + (e.cones ? "ir" : "nav");

        m.items.Add(Buy("Barība (+1)", 10, () => e.food++));
        m.items.Add(Buy("Gardums (+1) – motivē treniņos", 15, () => e.treats++));
        m.items.Add(e.toy ? new MenuItem("Rotaļlieta – nopirkta", null, false) : Buy("Rotaļlieta – spēlēm un aportēšanai", 25, () => e.toy = true));
        m.items.Add(e.leash ? new MenuItem("Pavada – nopirkta", null, false) : Buy("Pavada – labākas pastaigas", 30, () => e.leash = true));
        m.items.Add(e.cones ? new MenuItem("Konusi – nopirkti", null, false) : Buy("Treniņu konusi – labāka mācīšanās", 45, () => e.cones = true));
        return m;
    }

    MenuItem Buy(string label, int price, System.Action give)
    {
        return new MenuItem(label + "  –  " + price + " monētas", () =>
        {
            if (!gm.economy.Spend(price)) return;
            give();
            gm.audioMgr.Play(Sfx.Coin);
            gm.ui.Toast("Nopirkts!");
        }, true);
    }
}

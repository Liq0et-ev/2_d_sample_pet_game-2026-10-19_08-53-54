using UnityEngine;

// ShopSystem: veikals, kur var nopirkt barību, gardumus, rotaļlietu, pavadu un konusus.
public class ShopSystem : MonoBehaviour
{
    // Preču numuri (lai pogas zinātu, ko pirkt)
    const int PreceEdiens = 0;
    const int PreceGardums = 1;
    const int PreceRotallieta = 2;
    const int PrecePavada = 3;
    const int PreceKonusi = 4;

    public void AtvertIzvelni()
    {
        GameManager.Galvenais.saskarne.AtvertIzvelni(VeikalaIzvelne);
    }

    // Preces cena pec numura
    int PrecesCena(int preceNumurs)
    {
        if (preceNumurs == PreceEdiens) return 10;
        if (preceNumurs == PreceGardums) return 15;
        if (preceNumurs == PreceRotallieta) return 25;
        if (preceNumurs == PrecePavada) return 30;
        return 45;      // konusi
    }

    // Teksts "ir" vai "nav" inventara paradīšanai
    string IrVaiNav(bool irPrece)
    {
        if (irPrece)
        {
            return "ir";
        }
        return "nav";
    }

    IzvelnesApraksts VeikalaIzvelne()
    {
        EconomyManager ekonomika = GameManager.Galvenais.ekonomika;

        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Veikals";
        izvelne.teksts = "Pārdevējs: Laipni lūgti! Monētas: " + ekonomika.monetas +
                         "\nInventārs – barība: " + ekonomika.ediens +
                         ", gardumi: " + ekonomika.gardumi +
                         ", rotaļlieta: " + IrVaiNav(ekonomika.irRotallieta) +
                         ", pavada: " + IrVaiNav(ekonomika.irPavada) +
                         ", konusi: " + IrVaiNav(ekonomika.irKonusi);

        // Barība un gardumi var pirkt vairakkart
        izvelne.pogas.Add(new IzvelnesPoga("Barība (+1)  –  " + PrecesCena(PreceEdiens) + " monētas", Iegadaties, PreceEdiens));
        izvelne.pogas.Add(new IzvelnesPoga("Gardums (+1) – motivē treniņos  –  " + PrecesCena(PreceGardums) + " monētas", Iegadaties, PreceGardums));

        // Pārējās preces pērk tikai vienu reizi
        if (ekonomika.irRotallieta)
        {
            izvelne.pogas.Add(new IzvelnesPoga("Rotaļlieta – nopirkta", null, false));
        }
        else
        {
            izvelne.pogas.Add(new IzvelnesPoga("Rotaļlieta – spēlēm un aportēšanai  –  " + PrecesCena(PreceRotallieta) + " monētas", Iegadaties, PreceRotallieta));
        }

        if (ekonomika.irPavada)
        {
            izvelne.pogas.Add(new IzvelnesPoga("Pavada – nopirkta", null, false));
        }
        else
        {
            izvelne.pogas.Add(new IzvelnesPoga("Pavada – labākas pastaigas  –  " + PrecesCena(PrecePavada) + " monētas", Iegadaties, PrecePavada));
        }

        if (ekonomika.irKonusi)
        {
            izvelne.pogas.Add(new IzvelnesPoga("Konusi – nopirkti", null, false));
        }
        else
        {
            izvelne.pogas.Add(new IzvelnesPoga("Treniņu konusi – labāka mācīšanās  –  " + PrecesCena(PreceKonusi) + " monētas", Iegadaties, PreceKonusi));
        }
        return izvelne;
    }

    // Nopērk preci ar doto numuru
    void Iegadaties(int preceNumurs)
    {
        GameManager spele = GameManager.Galvenais;
        EconomyManager ekonomika = spele.ekonomika;

        // Ja nepietiek naudas, IztereMonetas pati paradīs ziņu
        if (ekonomika.IztereMonetas(PrecesCena(preceNumurs)) == false)
        {
            return;
        }

        if (preceNumurs == PreceEdiens) ekonomika.ediens = ekonomika.ediens + 1;
        if (preceNumurs == PreceGardums) ekonomika.gardumi = ekonomika.gardumi + 1;
        if (preceNumurs == PreceRotallieta) ekonomika.irRotallieta = true;
        if (preceNumurs == PrecePavada) ekonomika.irPavada = true;
        if (preceNumurs == PreceKonusi) ekonomika.irKonusi = true;

        spele.skana.Atskanot(Skana.Monetas);
        spele.saskarne.Pazinojums("Nopirkts!");
    }
}

using UnityEngine;

// CareSystem: suna aprupe. Šeit ir trīs izvelnes:
//   Maja     - barot un likt atpūsties
//   Parks    - pastaigas un rotaļas
//   Klinika  - slimibu arstēšana
public class CareSystem : MonoBehaviour
{
    // Izvelne, ko rādām, ja speletajam vēl nav suņa
    IzvelnesApraksts IzvelneBezSuna(string virsraksts)
    {
        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = virsraksts;
        izvelne.teksts = "Tev vēl nav suņa. Dodies uz Labklājības centru, lai saņemtu savu pirmo suni.";
        return izvelne;
    }

    // =================================================================
    // MAJA
    // =================================================================
    public void AtvertMajasIzvelni()
    {
        GameManager.Galvenais.saskarne.AtvertIzvelni(MajasIzvelne);
    }

    IzvelnesApraksts MajasIzvelne()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele.irSuns == false)
        {
            return IzvelneBezSuna("Māja");
        }

        DogStatus suns = spele.suns;
        EconomyManager ekonomika = spele.ekonomika;

        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Māja";
        izvelne.teksts = suns.vards + " – " + DogStatus.RaksturaNosaukums(suns.raksturs) + ". " + suns.RaksturaApraksts() +
                         "\nBarība inventārā: " + ekonomika.ediens +
                         "   |   Mājas līmenis: " + ekonomika.majasLimenis + "/" + EconomyManager.MaksimalaisUzlabojums;

        // Poga "Pabarot" ir iespējota, ja ir barība un suns nav pilns
        bool varPabarot = ekonomika.ediens > 0 && suns.bariba < 95f;
        izvelne.pogas.Add(new IzvelnesPoga("Pabarot suni (barība: " + ekonomika.ediens + ")", Pabarot, varPabarot));

        // Poga "Atpūsties" ir iespējota, ja suns nav pilnīgi spēka pilns
        bool varAtpusties = suns.energija < 95f;
        izvelne.pogas.Add(new IzvelnesPoga("Likt atpūsties guļvietā", Atpusties, varAtpusties));

        // Mājas uzlabojums (ja vēl nav sasniegts maksimums)
        if (ekonomika.majasLimenis < EconomyManager.MaksimalaisUzlabojums)
        {
            int jaunaisLimenis = ekonomika.majasLimenis + 1;
            int cena = 80 * jaunaisLimenis;
            int vajadzigsLimenis = jaunaisLimenis + 1;
            bool varUzlabot = spele.speletajaProgress.limenis >= vajadzigsLimenis;
            string teksts = "Uzlabot māju → " + jaunaisLimenis + ". līm. (" + cena + " monētas, spēlētāja līm. " + vajadzigsLimenis + ")";
            izvelne.pogas.Add(new IzvelnesPoga(teksts, UzlabotMaju, varUzlabot));
        }
        return izvelne;
    }

    void Pabarot()
    {
        GameManager spele = GameManager.Galvenais;
        EconomyManager ekonomika = spele.ekonomika;
        DogStatus suns = spele.suns;

        if (ekonomika.ediens <= 0)
        {
            spele.saskarne.Pazinojums("Nav barības – nopērc to veikalā.");
            return;
        }

        ekonomika.ediens = ekonomika.ediens - 1;
        suns.Mainit(0, 40, 0, 2);       // +40 barība, +2 garastāvoklis
        spele.irPabarots = true;
        spele.skana.Atskanot(Skana.Riet);
        spele.SunaEd();
        spele.saskarne.Pazinojums(suns.vards + " paēda.");
    }

    void Atpusties()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        // Jo labāka māja, jo vairāk enerģijas atgūst
        float energijasPieaugums = 50f + 15f * spele.ekonomika.majasLimenis;

        spele.saskarne.Aptumsot();
        suns.Mainit(3, -8, energijasPieaugums, 3);
        spele.skana.Atskanot(Skana.Labi);
        spele.saskarne.Pazinojums(suns.vards + " izgulējās un atguva spēkus.");
    }

    void UzlabotMaju()
    {
        GameManager spele = GameManager.Galvenais;
        int jaunaisLimenis = spele.ekonomika.majasLimenis + 1;
        int cena = 80 * jaunaisLimenis;

        if (spele.ekonomika.IztereMonetas(cena))
        {
            spele.ekonomika.majasLimenis = jaunaisLimenis;
            spele.saskarne.Pazinojums("Māja uzlabota! Labāka atpūta un lielāka pasīvā peļņa.");
            spele.skana.Atskanot(Skana.Labi);
        }
    }

    // =================================================================
    // PARKS
    // =================================================================
    public void AtvertParkaIzvelni()
    {
        GameManager.Galvenais.saskarne.AtvertIzvelni(ParkaIzvelne);
    }

    IzvelnesApraksts ParkaIzvelne()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele.irSuns == false)
        {
            return IzvelneBezSuna("Parks");
        }

        DogStatus suns = spele.suns;
        EconomyManager ekonomika = spele.ekonomika;

        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Parks";
        izvelne.teksts = "Pastaigas patērē enerģiju, bet uzlabo garastāvokli.";
        if (ekonomika.irPavada)
        {
            izvelne.teksts = izvelne.teksts + " Ar pavadu pastaiga ir patīkamāka.";
        }

        // Pastaiga
        bool varDotiesPastaiga = suns.energija >= 15f && suns.slims == false;
        string pastaigasTeksts = "Doties pastaigā";
        if (varDotiesPastaiga == false)
        {
            pastaigasTeksts = "Doties pastaigā (suns ir noguris vai slims)";
        }
        izvelne.pogas.Add(new IzvelnesPoga(pastaigasTeksts, Pastaiga, varDotiesPastaiga));

        // Nujas mesana (minispele)
        bool varMestNuju = suns.energija >= 10f && suns.slims == false;
        string nujasTeksts = "Mest nūju (5 mēģinājumi)";
        if (varMestNuju == false)
        {
            nujasTeksts = "Mest nūju (suns ir noguris vai slims)";
        }
        izvelne.pogas.Add(new IzvelnesPoga(nujasTeksts, SaktNujasSpeli, varMestNuju));

        // Spēlēšanās ar rotaļlietu
        bool varSpeleties = ekonomika.irRotallieta && suns.energija >= 10f && suns.slims == false;
        string speluTeksts = "Spēlēties ar rotaļlietu";
        if (ekonomika.irRotallieta == false)
        {
            speluTeksts = "Spēlēties (vajag rotaļlietu no veikala)";
        }
        izvelne.pogas.Add(new IzvelnesPoga(speluTeksts, Speleties, varSpeleties));
        return izvelne;
    }

    void SaktNujasSpeli()
    {
        GameManager.Galvenais.nujasSpele.Sakt();
    }

    void Pastaiga()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        // Ar pavadu garastāvoklis pieaug vairāk
        float garastavoklaPieaugums = 10f;
        if (spele.ekonomika.irPavada)
        {
            garastavoklaPieaugums = 18f;
        }

        spele.saskarne.Aptumsot();
        suns.Mainit(0, -5, -12, garastavoklaPieaugums);
        spele.ekonomika.Nopelnit(1);
        spele.sunaProgress.PieliktPieredzi(3);
        spele.SunaPriecajas();
        spele.saskarne.Pazinojums(suns.vards + " izbaudīja pastaigu.");
    }

    void Speleties()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        suns.Mainit(0, -3, -10, 20);
        spele.skana.Atskanot(Skana.Riet);
        spele.SunaPriecajas();
        spele.sunaProgress.PieliktPieredzi(4);
        spele.saskarne.Pazinojums(suns.vards + " priecīgi spēlējas!");
    }

    // =================================================================
    // KLINIKA
    // =================================================================
    public void AtvertKlinikasIzvelni()
    {
        GameManager.Galvenais.saskarne.AtvertIzvelni(KlinikasIzvelne);
    }

    // Arstesanas cena: slimu sunu arstet ir dargak nekā parasta apskate
    int ArstesanasCena()
    {
        if (GameManager.Galvenais.suns.slims)
        {
            return 40;
        }
        return 15;
    }

    IzvelnesApraksts KlinikasIzvelne()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele.irSuns == false)
        {
            return IzvelneBezSuna("Veterinārā klīnika");
        }

        DogStatus suns = spele.suns;

        IzvelnesApraksts izvelne = new IzvelnesApraksts();
        izvelne.virsraksts = "Veterinārā klīnika";

        string arstaTeksts;
        if (suns.slims)
        {
            arstaTeksts = suns.vards + " ir slims, vajag ārstēšanu.";
        }
        else if (suns.veseliba < 100f)
        {
            arstaTeksts = "Var veikt profilaktisko apskati.";
        }
        else
        {
            arstaTeksts = suns.vards + " ir pilnīgi vesels.";
        }
        izvelne.teksts = "Veterinārārsts: " + arstaTeksts + "\nVeselība: " + Mathf.RoundToInt(suns.veseliba);

        int cena = ArstesanasCena();
        string pogasTeksts = "Profilaktiskā apskate";
        if (suns.slims)
        {
            pogasTeksts = "Ārstēt slimību";
        }
        bool varArstet = suns.slims || suns.veseliba < 99f;
        izvelne.pogas.Add(new IzvelnesPoga(pogasTeksts + " (" + cena + " monētas)", Arstet, varArstet));
        return izvelne;
    }

    void Arstet()
    {
        GameManager spele = GameManager.Galvenais;
        DogStatus suns = spele.suns;

        if (spele.ekonomika.IztereMonetas(ArstesanasCena()) == false)
        {
            return;
        }

        suns.slims = false;
        suns.Mainit(100, 0, 0, 5);
        spele.skana.Atskanot(Skana.Labi);
        spele.saskarne.Pazinojums(suns.vards + " ir vesels!");
    }
}

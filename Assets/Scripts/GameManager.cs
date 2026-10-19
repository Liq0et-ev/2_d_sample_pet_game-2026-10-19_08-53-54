using UnityEngine;

public enum Command { Sit, Down, Come, Wait }

public static class Commands
{
    public static string Name(Command c)
    {
        switch (c)
        {
            case Command.Sit: return "Sēdi";
            case Command.Down: return "Guli";
            case Command.Come: return "Pie manis";
            default: return "Gaidi";
        }
    }
}

// Central access point to all game systems.
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public const int WinAdoptions = 10;

    public Transform player;
    public GameObject dogObj;
    public DogStatus dog;
    public DogProgress dogProg;
    public DogFollow dogFollow;
    public PlayerProgress playerProg;
    public EconomyManager economy;
    public CareSystem care;
    public TrainingSystem training;
    public ShopSystem shop;
    public WelfareAndAdoption welfare;
    public UIManager ui;
    public AudioManager audioMgr;

    public bool hasDog;
    public bool fedOnce;
    public int adoptions;
    public ZoneInfo currentZone;

    void Awake() { I = this; }

    public void OpenZone(ZoneType t)
    {
        switch (t)
        {
            case ZoneType.Welfare: welfare.OpenMenu(); break;
            case ZoneType.Training: training.OpenMenu(); break;
            case ZoneType.Park: care.OpenParkMenu(); break;
            case ZoneType.Home: care.OpenHomeMenu(); break;
            case ZoneType.Shop: shop.OpenMenu(); break;
            case ZoneType.Clinic: care.OpenClinicMenu(); break;
        }
    }

    public string GetQuest()
    {
        if (adoptions >= WinAdoptions) return "Mērķis sasniegts! Turpini brīvajā režīmā un palīdzi vēl vairāk suņiem.";
        if (!hasDog)
            return adoptions == 0
                ? "Dodies uz Labklājības centru (ZR) un nospied E, lai saņemtu pirmo suni."
                : "Saņem nākamo suni Labklājības centrā. Adopcijas: " + adoptions + "/" + WinAdoptions;
        if (!fedOnce) return "Aizved suni uz Māju (DR) un pabaro to (E).";
        int n = dogProg.LearnedCount;
        if (n == 0) return "Apmeklē Treniņu laukumu (augšā vidū) un iemāci pirmo komandu.";
        if (n < 4) return "Iemāci visas 4 komandas (" + n + "/4). Seko suņa vajadzībām!";
        if (welfare.IsReady()) return "Suns ir gatavs! Dodies uz Labklājības centru adopcijas pārbaudījumam.";
        return "Parūpējies par suni (veselība, barība, garastāvoklis), tad dodies uz adopciju.";
    }
}

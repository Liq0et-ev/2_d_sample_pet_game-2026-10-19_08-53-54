using UnityEngine;
using UnityEngine.InputSystem;

// PlayerMovement3D: speletaja kustiba 3D pasaule ar WASD vai bultinam.
// Karte ir "plakana": kartes X ir pasaules X, kartes Y ir pasaules Z.
public class PlayerMovement3D : MonoBehaviour
{
    public float atrums = 6f;
    public bool kustas = false;

    Vector2 sadursmesPusIzmers = new Vector2(0.35f, 0.35f);
    float soluFaze = 0f;

    Transform izskats;
    Transform kajaK;
    Transform kajaL;
    Transform rokaK;
    Transform rokaL;

    void Start()
    {
        izskats = transform.Find("Izskats");
        kajaK = transform.Find("Izskats/KajaK");
        kajaL = transform.Find("Izskats/KajaL");
        rokaK = transform.Find("Izskats/RokaK");
        rokaL = transform.Find("Izskats/RokaL");
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        Keyboard tastatura = Keyboard.current;

        // 1. Kuras pogas ir nospiestas? (W = uz priekšu pa Z asi)
        Vector2 virziens = Vector2.zero;
        if (tastatura != null && spele.saskarne.IrBloketa() == false)
        {
            if (tastatura.wKey.isPressed || tastatura.upArrowKey.isPressed) virziens.y = virziens.y + 1;
            if (tastatura.sKey.isPressed || tastatura.downArrowKey.isPressed) virziens.y = virziens.y - 1;
            if (tastatura.aKey.isPressed || tastatura.leftArrowKey.isPressed) virziens.x = virziens.x - 1;
            if (tastatura.dKey.isPressed || tastatura.rightArrowKey.isPressed) virziens.x = virziens.x + 1;
        }

        kustas = virziens.sqrMagnitude > 0.01f;

        // 2. Kustamies un pagriežamies kustības virzienā
        if (kustas)
        {
            virziens.Normalize();

            Vector2 pasreizeja = new Vector2(transform.position.x, transform.position.z);
            Vector2 solis = virziens * atrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(pasreizeja, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, 0f, jaunaVieta.y);

            Quaternion merkaPagriezens = Quaternion.LookRotation(new Vector3(virziens.x, 0f, virziens.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, merkaPagriezens, 12f * Time.deltaTime);

            soluFaze = soluFaze + Time.deltaTime * 12f;
        }

        AnimetStaigasanu();
    }

    // Kājas un rokas švīkst pretējos virzienos staigājot
    void AnimetStaigasanu()
    {
        float kajuLeknis = 0f;
        float ermenaLekasana = 0f;
        if (kustas)
        {
            kajuLeknis = Mathf.Sin(soluFaze) * 40f;
            ermenaLekasana = Mathf.Abs(Mathf.Sin(soluFaze)) * 0.06f;
        }
        else
        {
            // Pamazām atgriežam kājas sākuma pozā
            soluFaze = 0f;
        }

        if (kajaK != null) kajaK.localRotation = Quaternion.Euler(kajuLeknis, 0f, 0f);
        if (kajaL != null) kajaL.localRotation = Quaternion.Euler(-kajuLeknis, 0f, 0f);
        if (rokaK != null) rokaK.localRotation = Quaternion.Euler(-kajuLeknis * 0.8f, 0f, 0f);
        if (rokaL != null) rokaL.localRotation = Quaternion.Euler(kajuLeknis * 0.8f, 0f, 0f);
        if (izskats != null) izskats.localPosition = new Vector3(0f, ermenaLekasana, 0f);
    }
}

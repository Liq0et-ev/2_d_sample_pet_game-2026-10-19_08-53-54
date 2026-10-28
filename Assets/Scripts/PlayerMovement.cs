using UnityEngine;
using UnityEngine.InputSystem;

// PlayerMovement: speletaja kustiba ar WASD vai bultinam.
// Kustiba ir plūdena: tēls pakāpeniski uzņem ātrumu un pakāpeniski apstājas
// (nav pēkšņu apstāšanos).
public class PlayerMovement : MonoBehaviour
{
    public float atrums = 6f;               // maksimalais atrums
    public float paatrinajums = 45f;        // cik ātri tēls uzņem ātrumu
    public float bremzesana = 60f;          // cik ātri tēls apstājas, kad nekas nav nospiests
    public bool kustas = false;

    // Pasreizejais ātrums (virziens un lielums)
    Vector2 pasreizejaisAtrums = Vector2.zero;

    // Speletaja "kajas" izmers sadursmem (mazs taisnstūris pie pēdām).
    // Sadursmju robeža ir atdalīta no izskata: tā ir mazāka par zīmēto tēlu.
    Vector2 sadursmesPusIzmers = new Vector2(0.35f, 0.2f);

    // Skaitlis, ar kuru liekam ermenim lēkāt staigajot
    float lekasanasLaiks = 0f;

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        Keyboard tastatura = Keyboard.current;

        // 1. Noskaidrojam, kuras pogas ir nospiestas
        Vector2 virziens = Vector2.zero;
        if (tastatura != null && spele.saskarne.IrBloketa() == false)
        {
            if (tastatura.wKey.isPressed || tastatura.upArrowKey.isPressed) virziens.y = virziens.y + 1;
            if (tastatura.sKey.isPressed || tastatura.downArrowKey.isPressed) virziens.y = virziens.y - 1;
            if (tastatura.aKey.isPressed || tastatura.leftArrowKey.isPressed) virziens.x = virziens.x - 1;
            if (tastatura.dKey.isPressed || tastatura.rightArrowKey.isPressed) virziens.x = virziens.x + 1;
        }

        // 2. Merka atrums: ja kaut kas nospiests, tad pilns atrums, citādi 0
        Vector2 merkaAtrums = Vector2.zero;
        float izmainaSekunde = bremzesana;
        if (virziens.sqrMagnitude > 0.01f)
        {
            virziens.Normalize();           // lai pa diagonali nebutu atraks
            merkaAtrums = virziens * atrums;
            izmainaSekunde = paatrinajums;
        }

        // 3. Pakāpeniski mainām pasreizejo atrumu uz merka atrumu
        pasreizejaisAtrums = Vector2.MoveTowards(pasreizejaisAtrums, merkaAtrums, izmainaSekunde * Time.deltaTime);
        kustas = pasreizejaisAtrums.magnitude > 0.1f;

        // 4. Kustamies
        if (kustas)
        {
            Vector2 pasreizeja = transform.position;
            Vector2 solis = pasreizejaisAtrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(pasreizeja, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, jaunaVieta.y, 0f);

            lekasanasLaiks = lekasanasLaiks + Time.deltaTime * 14f;

            // Pagriežam speletaju uz kreiso vai labo pusi
            if (pasreizejaisAtrums.x > 0.5f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (pasreizejaisAtrums.x < -0.5f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
        else
        {
            lekasanasLaiks = 0f;
        }

        // 5. Neliela lekasana staigajot (ermenis kustas augsup un lejup)
        Transform ermenis = transform.Find("Ermenis");
        if (ermenis != null)
        {
            float lekasana = Mathf.Abs(Mathf.Sin(lekasanasLaiks)) * 0.06f;
            ermenis.localPosition = new Vector3(0, 0.7f + lekasana, 0);
        }
    }
}

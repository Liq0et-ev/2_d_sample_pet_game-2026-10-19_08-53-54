using UnityEngine;
using UnityEngine.InputSystem;

// PlayerMovement: speletaja kustiba ar WASD vai bultinam.
public class PlayerMovement : MonoBehaviour
{
    public float atrums = 6f;
    public bool kustas = false;

    // Speletaja "kajas" izmers sadursmem (mazs taisnstūris pie pēdām)
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

        // 2. Ja kaut kas ir nospiests, tad kustamies
        kustas = virziens.sqrMagnitude > 0.01f;
        if (kustas)
        {
            virziens.Normalize();     // lai pa diagonali nebutu atraks

            Vector2 pasreizeja = transform.position;
            Vector2 solis = virziens * atrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(pasreizeja, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, jaunaVieta.y, 0f);

            lekasanasLaiks = lekasanasLaiks + Time.deltaTime * 14f;

            // Pagriežam speletaju uz kreiso vai labo pusi
            if (virziens.x > 0.1f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (virziens.x < -0.1f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
        else
        {
            lekasanasLaiks = 0f;
        }

        // 3. Neliela lekasana staigajot (ermenis kustas augsup un lejup)
        Transform ermenis = transform.Find("Ermenis");
        if (ermenis != null)
        {
            float lekasana = Mathf.Abs(Mathf.Sin(lekasanasLaiks)) * 0.06f;
            ermenis.localPosition = new Vector3(0, 0.7f + lekasana, 0);
        }
    }
}

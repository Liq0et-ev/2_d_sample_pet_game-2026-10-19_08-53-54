using UnityEngine;
using UnityEngine.InputSystem;

// InteractionController: E mijiedarbiba ar ekam un vietam.
// Šeit arī noskaidrojam, kura zona ir speletajam apkārt (skanai).
public class InteractionController : MonoBehaviour
{
    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        Vector2 speletajaVieta = spele.KartesVieta(spele.speletajs);

        // Pasaka, kura zona ir speletajam apkārt (vai null, ja uz cela)
        spele.pasreizejaZona = Pasaule.ZonaPunkta(speletajaVieta);

        // Kamēr ir atverta izvelne, mijiedarbibas nav
        if (spele.saskarne.IrBloketa())
        {
            spele.saskarne.IestatitUzaicinajumu("");
            return;
        }

        // Meklējam tuvako vietu, ar kuru var mijiedarboties
        ZonasInfo tuvaka = Pasaule.TuvakaZona(speletajaVieta, 3.2f);
        if (tuvaka == null)
        {
            spele.saskarne.IestatitUzaicinajumu("");
        }
        else
        {
            spele.saskarne.IestatitUzaicinajumu("[E]  " + tuvaka.nosaukums);
        }

        // Ja nospiests E, atveram izvelni
        Keyboard tastatura = Keyboard.current;
        if (tuvaka != null && tastatura != null && tastatura.eKey.wasPressedThisFrame)
        {
            spele.skana.Atskanot(Skana.Klikskis);
            spele.AtvertZonu(tuvaka.veids);
        }
    }
}

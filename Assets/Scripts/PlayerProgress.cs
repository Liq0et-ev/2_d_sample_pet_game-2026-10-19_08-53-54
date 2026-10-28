using UnityEngine;

// PlayerProgress: speletaja limenis un pieredze.
// Speletaja limenis paliek arī tad, kad suns tiek adoptēts.
public class PlayerProgress : MonoBehaviour
{
    public int limenis = 1;
    public int pieredze = 0;

    // Cik pieredzes vajag lidz nakamajam limenim
    public int PieredzesVajag()
    {
        return 40 + 35 * limenis;
    }

    public void PieliktPieredzi(int daudzums)
    {
        pieredze = pieredze + daudzums;
        while (pieredze >= PieredzesVajag())
        {
            pieredze = pieredze - PieredzesVajag();
            limenis = limenis + 1;

            GameManager spele = GameManager.Galvenais;
            spele.saskarne.Pazinojums("Spēlētāja līmenis: " + limenis + "! Pārbaudi uzlabojumus mājā un laukumā.");
            spele.skana.Atskanot(Skana.Limenis);
            spele.TricetKameru(0.1f, 0.2f);
        }
    }
}

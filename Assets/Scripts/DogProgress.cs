using UnityEngine;

// DogProgress: katra suna limenis, pieredze un komandu prasmes.
public class DogProgress : MonoBehaviour
{
    public int limenis = 1;
    public int pieredze = 0;

    // Prasme katrai komandai (0 lidz 100). 100 nozime, ka komanda ir apgūta.
    public float[] prasmes = new float[4];

    // Cik pieredzes vajag lidz nakamajam limenim
    public int PieredzesVajag()
    {
        return 30 + 25 * limenis;
    }

    // Vai suns ir apguvis so komandu?
    public bool VaiApgusta(Komanda komanda)
    {
        return prasmes[(int)komanda] >= 100f;
    }

    // Cik komandas suns jau ir apguvis (0 lidz 4)
    public int ApgustoKomanduSkaits()
    {
        int skaits = 0;
        for (int i = 0; i < 4; i++)
        {
            if (prasmes[i] >= 100f)
            {
                skaits = skaits + 1;
            }
        }
        return skaits;
    }

    // Atiestata visu (kad nak jauns suns)
    public void Atiestatit()
    {
        limenis = 1;
        pieredze = 0;
        for (int i = 0; i < prasmes.Length; i++)
        {
            prasmes[i] = 0f;
        }
    }

    // Pieskaita pieredzi un paaugstina limeni, ja vajag
    public void PieliktPieredzi(int daudzums)
    {
        pieredze = pieredze + daudzums;
        while (pieredze >= PieredzesVajag())
        {
            pieredze = pieredze - PieredzesVajag();
            limenis = limenis + 1;

            GameManager spele = GameManager.Galvenais;
            spele.saskarne.Pazinojums(spele.suns.vards + " sasniedza " + limenis + ". līmeni!");
            spele.skana.Atskanot(Skana.Limenis);
        }
    }
}

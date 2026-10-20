using UnityEngine;

// DogFollow: aktivais suns seko speletajam.
public class DogFollow : MonoBehaviour
{
    public Transform speletajs;
    public bool kustas = false;

    Vector2 sadursmesPusIzmers = new Vector2(0.4f, 0.15f);
    float animacijasLaiks = 0f;

    // Uzreiz noliek suni blakus speletajam
    public void Pielekt()
    {
        Vector2 vieta = Pasaule.Parvietot(speletajs.position, new Vector2(-1.4f, 0), sadursmesPusIzmers);
        transform.position = new Vector3(vieta.x, vieta.y, 0);
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele.irSuns == false || speletajs == null)
        {
            return;
        }

        // 1. Cik talu suns ir no speletaja?
        Vector2 sunaVieta = transform.position;
        Vector2 uzSpeletaju = (Vector2)speletajs.position - sunaVieta;
        float attalums = uzSpeletaju.magnitude;

        // 2. Ja suns ir talu (vairak neka 2 vienibas), tas skrien tuvak
        float atrums = 0f;
        if (attalums > 2f)
        {
            atrums = Mathf.Min(8.5f, 3f + (attalums - 2f) * 2.5f);
        }

        // Noguris vai slims suns iet lenak
        if (spele.suns.energija < 15f || spele.suns.slims)
        {
            atrums = atrums * 0.6f;
        }

        kustas = atrums > 0.1f;

        // 3. Parvietojam suni
        if (kustas)
        {
            Vector2 solis = uzSpeletaju.normalized * atrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(sunaVieta, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, jaunaVieta.y, 0);

            // Pagriežam suni uz speletaja pusi
            if (uzSpeletaju.x > 0.3f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (uzSpeletaju.x < -0.3f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        // 4. Animacijas
        if (kustas)
        {
            animacijasLaiks = animacijasLaiks + Time.deltaTime * 16f;
        }
        else
        {
            animacijasLaiks = animacijasLaiks + Time.deltaTime * 6f;
        }

        SunaIzskats izskats = GetComponent<SunaIzskats>();
        if (izskats != null)
        {
            // Statusa tekstu nedrikst atspoguļot, kad suns ir pagriezts uz kreiso pusi
            if (izskats.statusaTeksts != null)
            {
                izskats.statusaTeksts.transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
            }

            // Aste luncinas: laimigs suns luncina stipraki
            if (izskats.aste != null)
            {
                float luncinasStiprums = 8f;
                if (spele.suns.garastavoklis > 50f)
                {
                    luncinasStiprums = 28f;
                }
                float leciens = Mathf.Sin(animacijasLaiks) * luncinasStiprums;
                izskats.aste.localRotation = Quaternion.Euler(0, 0, 35f + leciens);
            }
        }

        // Ermenis lekā, kad suns skrien
        Transform ermenis = transform.Find("Ermenis");
        if (ermenis != null && kustas)
        {
            float lekasana = Mathf.Abs(Mathf.Sin(animacijasLaiks)) * 0.05f;
            ermenis.localPosition = new Vector3(0, 0.45f + lekasana, 0);
        }
    }
}

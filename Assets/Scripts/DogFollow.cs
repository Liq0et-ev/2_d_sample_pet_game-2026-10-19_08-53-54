using UnityEngine;

// DogFollow: aktivais suns seko speletajam.
// (Suna kustibu animacijas ir atseviskaja skriptaa SunaAnimacija.)
public class DogFollow : MonoBehaviour
{
    public Transform speletajs;
    public bool kustas = false;

    Vector2 sadursmesPusIzmers = new Vector2(0.4f, 0.15f);

    // Suns paātrinās un bremzē plūdeni (nav pēkšņu apstāšanos)
    Vector2 pasreizejaisAtrums = Vector2.zero;
    const float Paatrinajums = 30f;

    // Uzreiz noliek suni blakus speletajam
    public void Pielekt()
    {
        Vector2 vieta = Pasaule.Parvietot(speletajs.position, new Vector2(-1.4f, 0), sadursmesPusIzmers);
        transform.position = new Vector3(vieta.x, vieta.y, 0);
        pasreizejaisAtrums = Vector2.zero;
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

        // Merka atrums ir virziens uz speletaju; suns pakāpeniski tuvojas šim atrumam
        Vector2 merkaAtrums = Vector2.zero;
        if (atrums > 0.1f)
        {
            merkaAtrums = uzSpeletaju.normalized * atrums;
        }
        pasreizejaisAtrums = Vector2.MoveTowards(pasreizejaisAtrums, merkaAtrums, Paatrinajums * Time.deltaTime);
        kustas = pasreizejaisAtrums.magnitude > 0.2f;

        // 3. Parvietojam suni
        if (kustas)
        {
            Vector2 solis = pasreizejaisAtrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(sunaVieta, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, jaunaVieta.y, 0);

            // Pagriežam suni kustības virzienā
            if (pasreizejaisAtrums.x > 0.3f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (pasreizejaisAtrums.x < -0.3f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        // 4. Statusa tekstu nedrikst atspoguļot, kad suns ir pagriezts uz kreiso pusi
        SunaIzskats izskats = GetComponent<SunaIzskats>();
        if (izskats != null && izskats.statusaTeksts != null)
        {
            izskats.statusaTeksts.transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        }
    }
}

using UnityEngine;

// DogFollow3D: aktivais suns seko speletajam 3D pasaule.
// (Suna kustibu animacijas ir skriptaa SunaAnimacija3D.)
public class DogFollow3D : MonoBehaviour
{
    public Transform speletajs;
    public bool kustas = false;

    Vector2 sadursmesPusIzmers = new Vector2(0.4f, 0.4f);

    // Uzreiz noliek suni aiz speletaja
    public void Pielekt()
    {
        Vector2 speletajaVieta = new Vector2(speletajs.position.x, speletajs.position.z);
        Vector2 vieta = Pasaule.Parvietot(speletajaVieta, new Vector2(-1.4f, 0f), sadursmesPusIzmers);
        transform.position = new Vector3(vieta.x, 0f, vieta.y);
        transform.rotation = speletajs.rotation;
    }

    void Update()
    {
        GameManager spele = GameManager.Galvenais;
        if (spele.irSuns == false || speletajs == null)
        {
            return;
        }

        // 1. Cik talu suns ir no speletaja? (Vector2: X un Z)
        Vector2 sunaVieta = new Vector2(transform.position.x, transform.position.z);
        Vector2 speletajaVieta = new Vector2(speletajs.position.x, speletajs.position.z);
        Vector2 uzSpeletaju = speletajaVieta - sunaVieta;
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

        // 3. Parvietojam un pagriežam suni
        if (kustas)
        {
            Vector2 virziens = uzSpeletaju.normalized;
            Vector2 solis = virziens * atrums * Time.deltaTime;
            Vector2 jaunaVieta = Pasaule.Parvietot(sunaVieta, solis, sadursmesPusIzmers);
            transform.position = new Vector3(jaunaVieta.x, 0f, jaunaVieta.y);

            Quaternion merkaPagriezens = Quaternion.LookRotation(new Vector3(virziens.x, 0f, virziens.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, merkaPagriezens, 10f * Time.deltaTime);
        }
    }
}

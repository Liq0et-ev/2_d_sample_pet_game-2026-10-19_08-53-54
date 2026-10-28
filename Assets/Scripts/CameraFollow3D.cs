using UnityEngine;

// CameraFollow3D: kamera seko speletajam no augšas un nedaudz no aizmugures.
public class CameraFollow3D : MonoBehaviour
{
    public Transform merkis;                                       // ko kamera seko (speletajs)
    public Vector3 nobide = new Vector3(0f, 11f, -9f);             // kur kamera atrodas attiecībā pret spēlētāju
    public float gludums = 6f;

    // Trīcēšana
    float tricesStiprums = 0f;
    float tricesIlgums = 0f;
    float tricesLaiks = 0f;
    Vector3 tricesNobide = Vector3.zero;

    // Sāk kameras trīcēšanu (stiprums ~ 0.1 maza, 0.3 liela; ilgums sekundēs)
    public void Tricet(float stiprums, float ilgums)
    {
        tricesStiprums = stiprums;
        tricesIlgums = ilgums;
        tricesLaiks = ilgums;
    }

    void LateUpdate()
    {
        if (merkis == null)
        {
            return;
        }

        // Kur kamerai jābūt
        Vector3 vajadzigaVieta = merkis.position + nobide;

        // Maigi pārvietojam kameru un pagriežam to uz speletaju
        float koeficients = 1f - Mathf.Exp(-gludums * Time.unscaledDeltaTime);
        Vector3 mierigaVieta = Vector3.Lerp(transform.position - tricesNobide, vajadzigaVieta, koeficients);
        tricesNobide = Vector3.zero;
        if (tricesLaiks > 0f)
        {
            tricesLaiks = tricesLaiks - Time.unscaledDeltaTime;
            float atlicis = Mathf.Clamp01(tricesLaiks / tricesIlgums);
            tricesNobide = Random.insideUnitSphere * tricesStiprums * atlicis;
        }
        transform.position = mierigaVieta + tricesNobide;

        Vector3 skatamais = merkis.position + new Vector3(0f, 1f, 1.5f);
        Quaternion merkaPagriezens = Quaternion.LookRotation(skatamais - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, merkaPagriezens, koeficients);
    }

    // Uzreiz noliek kameru vietā (spēles sākumā)
    public void Pielekt()
    {
        if (merkis == null)
        {
            return;
        }
        transform.position = merkis.position + nobide;
        Vector3 skatamais = merkis.position + new Vector3(0f, 1f, 1.5f);
        transform.rotation = Quaternion.LookRotation(skatamais - transform.position);
    }
}

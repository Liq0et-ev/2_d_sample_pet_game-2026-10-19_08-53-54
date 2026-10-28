using UnityEngine;

// CameraFollow: kamera seko speletajam, bet neiziet ārpus kartes.
// Kamerai ir arī "trīcēšana" (screen shake) spēcīgiem notikumiem.
public class CameraFollow : MonoBehaviour
{
    public Transform merkis;        // ko kamera seko (speletajs)
    public float gludums = 8f;

    // Trīcēšana
    float tricesStiprums = 0f;
    float tricesIlgums = 0f;
    float tricesLaiks = 0f;

    // Sāk kameras trīcēšanu (stiprums ~ 0.1 maza, 0.3 liela; ilgums sekundēs)
    public void Tricet(float stiprums, float ilgums)
    {
        // Ja jau trīc stiprāk, neaizvietojam ar vājāku
        if (tricesLaiks > 0f && stiprums < tricesStiprums * (tricesLaiks / tricesIlgums))
        {
            return;
        }
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

        Camera kamera = GetComponent<Camera>();
        float pusAugstums = kamera.orthographicSize;
        float pusPlatums = pusAugstums * kamera.aspect;

        // Vieta, uz kuru kamera grib tikt (nedaudz virs speletaja)
        Vector3 vieta = merkis.position + new Vector3(0, 0.8f, 0);

        // Neļaujam kamerai redzet ārpus kartes malam
        float minX = Pasaule.Robezas.xMin + pusPlatums;
        float maxX = Mathf.Max(minX, Pasaule.Robezas.xMax - pusPlatums);
        float minY = Pasaule.Robezas.yMin + pusAugstums;
        float maxY = Mathf.Max(minY, Pasaule.Robezas.yMax - pusAugstums);
        vieta.x = Mathf.Clamp(vieta.x, minX, maxX);
        vieta.y = Mathf.Clamp(vieta.y, minY, maxY);
        vieta.z = -10f;

        // Maigi pārvietojam kameru uz jauno vietu
        float koeficients = 1f - Mathf.Exp(-gludums * Time.unscaledDeltaTime);
        Vector3 mierigaVieta = Vector3.Lerp(transform.position - sakumaNobide, vieta, koeficients);
        sakumaNobide = Vector3.zero;

        // Trīcēšana: nejausa nobide, kas pakāpeniski izdziest
        if (tricesLaiks > 0f)
        {
            tricesLaiks = tricesLaiks - Time.unscaledDeltaTime;
            float atlicis = Mathf.Clamp01(tricesLaiks / tricesIlgums);
            Vector2 nejausa = Random.insideUnitCircle * tricesStiprums * atlicis;
            sakumaNobide = new Vector3(nejausa.x, nejausa.y, 0f);
        }
        transform.position = mierigaVieta + sakumaNobide;
    }

    // Trīcēšanas nobide, kas tika pielikta iepriekšējā kadrā (lai kamera nedriftē)
    Vector3 sakumaNobide = Vector3.zero;

    // Uzreiz noliek kameru pie speletaja (spēles sākumā)
    public void Pielekt()
    {
        if (merkis == null)
        {
            return;
        }
        Vector3 vieta = merkis.position;
        vieta.z = -10f;
        transform.position = vieta;
    }
}

using UnityEngine;

// CameraFollow: kamera seko speletajam, bet neiziet ārpus kartes.
public class CameraFollow : MonoBehaviour
{
    public Transform merkis;        // ko kamera seko (speletajs)
    public float gludums = 8f;

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
        transform.position = Vector3.Lerp(transform.position, vieta, koeficients);
    }

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

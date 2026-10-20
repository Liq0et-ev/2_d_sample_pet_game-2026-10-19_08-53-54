using UnityEngine;

// CameraFollow3D: kamera seko speletajam no augšas un nedaudz no aizmugures.
public class CameraFollow3D : MonoBehaviour
{
    public Transform merkis;                                       // ko kamera seko (speletajs)
    public Vector3 nobide = new Vector3(0f, 11f, -9f);             // kur kamera atrodas attiecībā pret spēlētāju
    public float gludums = 6f;

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
        transform.position = Vector3.Lerp(transform.position, vajadzigaVieta, koeficients);

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

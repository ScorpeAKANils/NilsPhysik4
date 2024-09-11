using System.Collections.Generic;
using UnityEngine;

public class Achse : MonoBehaviour
{
    [SerializeField]
    private FederDaempfer _federDaempfer;
    [SerializeField]
    public AchsenTyp m_AchsenTyp;
    [SerializeField]
    private float m_fahrwerksHoehe;
    public float FahrwerksHoehe { get { return m_fahrwerksHoehe; } }
    public GameObject Reifen; 
    private Vector3 ruheLage;
    [SerializeField]
    List<Transform> m_reifen = new List<Transform>();
    float time;
    public enum AchsenTyp
    {
        Starr,
        Lenkbar
    }

    private void Start()
    {
        ruheLage = transform.position;
    }
    public int GetWheelCount() 
    {
        return m_reifen.Count; 
    }
    public void AddNewWheel() 
    {
        var temp = Instantiate(m_reifen[0].gameObject, m_reifen[0].position + m_reifen[0].right, Quaternion.identity);
        m_reifen.Add(temp.transform); 

    }

    public void RemoveWheele() 
    {
        var objToDelete = m_reifen[GetWheelCount() - 1];
        m_reifen.Remove(objToDelete);
        Destroy(objToDelete.gameObject); 
    }
    public Vector3 BerechneDaempfungVektor(float daempfer, Vector3 geschwindigkeit)
    {
        // Berechne die Dämpfung für jede Achse proportional zur Geschwindigkeit
        Vector3 daempfung = geschwindigkeit * daempfer;
        return daempfung;
    }

    public void RotateWheels(float input, float drehgeschwindigkeit)
    {
        float radstand = 2.5f;
        float spurweite = 1.5f; //Werte sind nur ausgedachte test werte 

        foreach (var x in m_reifen)
        {
            if (input != 0)
            {
                bool istLinkesRad = x.name == "ReifenLV";
                bool istRechtesRad = x.name == "ReifenRV";


                float innerRadius = radstand / Mathf.Tan(input);
                float outerRadius = innerRadius + spurweite;

                float innerAngle = Mathf.Rad2Deg * Mathf.Atan(radstand / innerRadius);
                float outerAngle = Mathf.Rad2Deg * Mathf.Atan(radstand / outerRadius);

                float targetAngle = 0f;
                if (istLinkesRad)
                {
                    targetAngle = (input > 0) ? innerAngle : outerAngle;
                }
                else if (istRechtesRad)
                {
                    targetAngle = (input > 0) ? outerAngle : innerAngle;
                }
                targetAngle = Mathf.Clamp(targetAngle, -35, 35f);

                float newRotationY = Mathf.LerpAngle(x.transform.localEulerAngles.y, targetAngle, Time.deltaTime * drehgeschwindigkeit);
                x.localRotation = Quaternion.Euler(0, newRotationY, 90);

            }
            else
            {
                float resetRot = Mathf.LerpAngle(x.localEulerAngles.y, 0, Time.deltaTime * drehgeschwindigkeit);
                x.localRotation = Quaternion.Euler(0, resetRot, 90);
            }
        }
    }




    public float UpdateAchse(Rigidbody rb, Transform reifen)
    {
        float auslenkung = BerechneAuslenkung();
        float relativeGeschwindigkeit = rb.GetRelativePointVelocity(reifen.position).magnitude;
        return BerechneFederkraft(auslenkung) + BerechneDaempfung(relativeGeschwindigkeit);
    }

    public float BerechneFederkraft(float auslenkung)
    {
        return _federDaempfer.Feder * auslenkung;
    }

    public void SetFahrwerksHoehe(float val)
    {
        m_fahrwerksHoehe = val;
    }

    public float BerechneDaempfung(float relativeGeschwindigkeit)
    {
        return _federDaempfer.Daempfer * relativeGeschwindigkeit;
    }

    private float BerechneAuslenkung()
    {
        return (ruheLage - transform.position).magnitude;
    }

    public static float BerechneFederkraft(float feder, float auslenkung)
    {
        return feder * auslenkung;
    }

    public static float BerechneDaempfung(float daempfer, float relativeGeschwindigkeit)
    {
        return daempfer * relativeGeschwindigkeit;
    }
}

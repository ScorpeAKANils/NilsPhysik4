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
    [SerializeField]
    Transform Carpos;
    float time;
    public List<Transform> Selectables;
    public Fahrwerk fahrwerk; 
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
    public void AddNewWheel(Vector3 SpawnPos)
    {
        if(SpawnPos == new Vector3(-999, -999, -999)) 
        {
            Debug.LogError("Invalid Vector"); 
            return;
        }
        var temp = Instantiate(m_reifen[0].gameObject, SpawnPos, m_reifen[0].rotation);
        temp.transform.localScale = m_reifen[0].lossyScale; 
        temp.transform.parent = this.transform;
        m_reifen.Add(temp.transform);
    }

    private Vector3 CalculateWheelSpawnPosition()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 directionToCamera = (cameraPosition - transform.position).normalized;
        float distanceFromObject = 1.0f;
        Vector3 spawnPosition = transform.position + directionToCamera * distanceFromObject;

        return spawnPosition;
    }

    Vector3 CalculateSpawnPos() 
    {
        return transform.position; 
    }
    public void RemoveWheele() 
    {
        var objToDelete = m_reifen[GetWheelCount() - 1];
        m_reifen.Remove(objToDelete);
        Debug.Log(objToDelete.gameObject.name); 
        Destroy(objToDelete.gameObject); 
    }
    public Vector3 BerechneDaempfungVektor(float daempfer, Vector3 geschwindigkeit)
    {
        // Berechne die D�mpfung f�r jede Achse proportional zur Geschwindigkeit
        Vector3 daempfung = geschwindigkeit * daempfer;
        return daempfung;
    }

    public void RotateWheels(float input, float drehgeschwindigkeit)
    {
        float radstand = (fahrwerk._achsen[0].transform.position - fahrwerk._achsen[1].transform.position).magnitude;
        float spurweite = (m_reifen[0].position - m_reifen[1].position).magnitude;
        foreach (var x in m_reifen)
        {
            if (Mathf.Abs(input) > 0.1f)
            {
                bool istLinkesRad = x.name == "ReifenLV";
                bool istRechtesRad = x.name == "ReifenRV";

                float innerRadius = radstand / Mathf.Tan(input);
                float outerRadius = Mathf.Abs(innerRadius) + spurweite * Mathf.Sign(innerRadius);

                float innerAngle = Mathf.Sign(input) * Mathf.Rad2Deg * Mathf.Atan(radstand / Mathf.Abs(innerRadius));
                float outerAngle = Mathf.Sign(input) * Mathf.Rad2Deg * Mathf.Atan(radstand / Mathf.Abs(outerRadius));
                float targetAngle = 0f;
                if (istLinkesRad)
                {
                    targetAngle = (input > 0) ? innerAngle : outerAngle;
                }
                else if (istRechtesRad)
                {
                    targetAngle = (input > 0) ? outerAngle : innerAngle;
                }
                targetAngle = Mathf.Clamp(targetAngle, -35f, 35f);
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

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
    public List<Transform> ReifenList { get { return m_reifen; } }
    [SerializeField]
    Transform Carpos;
    float time;
    public List<Transform> Selectables;
    public Fahrwerk fahrwerk;
    [SerializeField]
    private Rigidbody rb;

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
        // Berechne die Dämpfung für jede Achse proportional zur Geschwindigkeit
        Vector3 daempfung = geschwindigkeit * daempfer;
        return daempfung;
    }

    public void RotateWheels(float input, float moveDirInput, float drehgeschwindigkeit)
    {
        float radstand = (fahrwerk._achsen[0].transform.position - fahrwerk._achsen[1].transform.position).magnitude;
        float spurweite = (m_reifen[0].position - m_reifen[1].position).magnitude;

        foreach (var x in m_reifen)
        {
            if (Mathf.Abs(input) > 0.1f)
            {
                // Berechne den Lenkwinkel basierend auf dem Input
                float targetAngle = input * 35f; // Maximaler Lenkwinkel 35 Grad

                // Verwende Mathf.Lerp, um die Lenkung zu glätten
                float newRotationY = Mathf.LerpAngle(x.transform.localEulerAngles.y, targetAngle, Time.deltaTime * drehgeschwindigkeit);
                x.localRotation = Quaternion.Euler(0, newRotationY, 90);
            }
            else
            {
                // Wenn kein Input mehr gegeben wird, zurück zur Neutralstellung
                float resetRot = Mathf.LerpAngle(x.localEulerAngles.y, 0, Time.deltaTime * drehgeschwindigkeit);
                x.localRotation = Quaternion.Euler(0, resetRot, 90);
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateAchse(); 
    }

    public void UpdateAchse()
    {
       //foreach(var reifen in m_reifen) 
       //{
       //    float auslenkung = BerechneAuslenkung();
       //    float relativeGeschwindigkeit = rb.GetRelativePointVelocity(reifen.position).magnitude;
       //     rb.AddForceAtPosition(fahrwerk.ClampShit(transform.up*(BerechneFederkraft(auslenkung) + BerechneDaempfung(relativeGeschwindigkeit))* Time.fixedDeltaTime), reifen.transform.position, ForceMode.VelocityChange);
       //}

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

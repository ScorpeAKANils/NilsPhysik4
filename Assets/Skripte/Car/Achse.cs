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

    public void RotateWheels(float input, float drehgeschwindigkeit)
    {
        foreach (var x in m_reifen)
        {
            if (input != 0)//spieler lenkt das fahrzeug
            {
                var rot = input * drehgeschwindigkeit * Time.deltaTime;
                float currentRotationY = x.localEulerAngles.y;
                if (currentRotationY > 180)
                    currentRotationY -= 360;
                float newRotationY = Mathf.Clamp(currentRotationY + rot, -90f, 90f);
                x.localRotation = Quaternion.Euler(0, newRotationY, 0);
            } else //spieler gibt kein Input => Lenkrad geht zurück in die ausgangsposition
            {
                if(transform.rotation.y != 0) 
                {
                    float resetRot = Mathf.Lerp(x.rotation.y, 0, time);
                    time += Time.fixedDeltaTime;
                    time = Mathf.Clamp01(time);
                    x.localRotation = Quaternion.Euler(0, resetRot, 0);
                } else 
                {
                    time = 0; 
                }
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

using UnityEngine;

public class Achse : MonoBehaviour
{
    [SerializeField]
    private FederDaempfer _federDaempfer; 
    [SerializeField]
    private AchsenTyp achsenTyp;
    private Vector3 ruheLage;

    public enum AchsenTyp //=> gehört in die klasse
    {
        Starr,
        Lenkbar
    }
    private void Start()
    {
        ruheLage = transform.position; 
    }

    public float UpdateAchse(Rigidbody rb) 
    {
        float auslenkung = BerechneAuslenkung(); 
        float relativeGeschwindigkeit = rb.GetRelativePointVelocity(this.transform.position).magnitude;
        return BerechneFederkraft(auslenkung) + BerechneDaempfung(relativeGeschwindigkeit);
    }

    public float BerechneFederkraft(float auslenkung)
    {
        return _federDaempfer.Feder * auslenkung;
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

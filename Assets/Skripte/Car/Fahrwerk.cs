using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class Fahrwerk : MonoBehaviour
{
    [SerializeField]
    private int AnzahlAchsen;
    [SerializeField]
    private List<Achse> _achsen = new List<Achse>();
    [SerializeField]
    private FahrwerksTyp _fahrwerksTyp;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float _motorStärke = 12000f;
    [SerializeField]
    private float reibungsKoeffizient = 0.8f;
    [SerializeField]
    List<FederDaempfer> m_reifen = new List<FederDaempfer>();
    private float maxVel = 5    ;
    private float inputVertical;
    private float inputHorizontal;
    private Vector3 bewegung;
    public LayerMask ignore;
    public AnimationCurve leistungsKurve; 

    public enum FahrwerksTyp
    {
        Radfahrwerk,
        Kettenfahrwerk
    }

    private void Update()
    {
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        UpdateFahrwerk();
    }

    void UpdateFahrwerk()
    {
        switch (_fahrwerksTyp)
        {
            case FahrwerksTyp.Radfahrwerk:
                HandleRadKFZ(); 
                break;

            case FahrwerksTyp.Kettenfahrwerk:
                HansBringSePanzerOhneFaustMagGoetheEhNichtSo(); 
                break;

            default:
                Debug.LogError("Unbekannter Fahrwerkstyp");
                break;
        }
    }

    void HandleRadKFZ() 
    {
        foreach (var r in m_reifen)
        {
            if (inputVertical != 0)
                Acceloration(r.transform, r.Achse);

            if (r.Achse.m_AchsenTyp == Achse.AchsenTyp.Lenkbar)
            {
               r.Achse.RotateWheels(inputHorizontal, 5);
            //this may be bullshit or i dont get it right: 
                rb.AddTorque(Vector3.up * inputHorizontal * Time.deltaTime, ForceMode.VelocityChange);
                Lenkung(r.Achse, r.transform);
            }
            //rb.AddForceAtPosition(r.transform.forward*bewegung.magnitude * inputVertical * _motorStärke * Time.fixedDeltaTime, r.transform.position);
        }
        if (rb.velocity.magnitude > maxVel)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.normalized * maxVel, Time.fixedDeltaTime * 2f);
        }
    }

    void HansBringSePanzerOhneFaustMagGoetheEhNichtSo() 
    {
        Vector3 kraft = BewegungKettenfahrwerk();
        Vector3 kraftLinks = kraft;
        Vector3 kraftRechts = kraft;
        if (inputHorizontal > 0)
        {
            kraftRechts *= -1;
        }
        if (inputHorizontal < 0)
        {
            kraftLinks *= -1;
        }

        rb.AddForceAtPosition(Time.fixedDeltaTime * kraftLinks, _achsen[0].transform.position);
        rb.AddForceAtPosition(Time.fixedDeltaTime * kraftRechts, _achsen[1].transform.position);
    }

    void Acceloration(Transform reifen, Achse achse) 
    {
            Vector3 dir = reifen.forward;

            float geschwindigkeit = Vector3.Dot(this.transform.position, rb.velocity);
            float velNormalized = Mathf.Clamp01(Mathf.Abs(geschwindigkeit) / maxVel);
            float avaibleTorque = leistungsKurve.Evaluate(velNormalized) * inputVertical * _motorStärke;
            rb.AddForceAtPosition(dir * avaibleTorque, reifen.position);
    }
    void Lenkung(Achse a, Transform reifen)
    {
        bool hitSomething = Physics.Raycast(reifen.position, Vector3.down, a.FahrwerksHoehe, ~ignore);
        if (hitSomething) 
        {
             float grip = 0.05f;
             Vector2 lenkInput = new Vector2(inputHorizontal, inputVertical);
            Vector3 lenkDir = new Vector3(lenkInput.x, 0, lenkInput.y).normalized; 
             Vector3 reifenGeschwindigkeit = rb.GetPointVelocity(reifen.position);

             float newVel = Vector3.Dot(lenkDir, reifenGeschwindigkeit);
             float idealeBeschleunigung = -newVel * grip;

             float beschleunigung = idealeBeschleunigung / Time.fixedDeltaTime;
             float mass = reifen.GetComponent<FederDaempfer>().Mass;
             //Debug.Log(lenkInput * mass * beschleunigung); 
             rb.AddForceAtPosition(lenkDir  * mass *  beschleunigung * Time.fixedDeltaTime, reifen.position);
        }
    }

    Vector3 BewegungKettenfahrwerk()
    {
        Vector3 forceDirLeft = (transform.forward * inputVertical);
        float normalKraft = rb.mass * Physics.gravity.magnitude;
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;
        return forceDirLeft * traktionskraft * 3;
    }

    public void SetFahrwerk(Fahrwerk.FahrwerksTyp f) 
    {
        _fahrwerksTyp = f; 
    }
}

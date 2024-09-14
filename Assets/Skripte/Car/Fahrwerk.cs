using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Fahrwerk : MonoBehaviour
{
    [SerializeField]
    private int AnzahlAchsen;
    [SerializeField]
    public List<Achse> _achsen = new List<Achse>();
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
    [SerializeField]
    float drehGeschwindigkeit = 0.75f;  
    [SerializeField]
    private float maxVel = 5f;
    private float inputVertical;
    private float inputHorizontal;
    public LayerMask ignore;

    [SerializeField]
    private float lenkkraft = 10000f;

    [SerializeField]
    private float bremskraft = 15000f;

    public enum FahrwerksTyp
    {
        Radfahrwerk,
        Kettenfahrwerk
    }
    private void Awake()
    {
        Time.fixedDeltaTime = 0.01f;
    }
    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        var gdtool = EnableGameDesignerTool.instance; 
        if (gdtool != null && gdtool.ToolEnabled == true) 
        {
            Debug.Log("Ok Cool"); 
            return; 
        }
        UpdateFahrwerk();
    }

    private void HandleInput()
    {
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    void UpdateFahrwerk()
    {
        switch (_fahrwerksTyp)
        {
            case FahrwerksTyp.Radfahrwerk:
                HandleRadKFZ();
                break;
            case FahrwerksTyp.Kettenfahrwerk:
                HandleKettenKFZ();
                break;
            default:
                Debug.LogError("Unbekannter Fahrwerkstyp");
                break;
        }
    }

    private void HandleRadKFZ()
    {
        foreach (var r in m_reifen)
        {
            if (inputVertical != 0)
            {
                Acceleration(r.transform, r.Achse);
            }

            if (rb.velocity.magnitude > 0.1f && Input.GetKey(KeyCode.Space))
            {
                ApplyBrakes(r.transform);
            }

            if (r.Achse.m_AchsenTyp == Achse.AchsenTyp.Lenkbar)
            {
                // Angepasste Lenkung mit Reifen-Schlupf
                r.Achse.RotateWheels(inputHorizontal, inputVertical, drehGeschwindigkeit);
                if (Mathf.Abs(rb.velocity.magnitude) > 0.1f)
                {
                    AdjustVehicleRotation();
                }
            }
            Lenkung(r.Achse, r.transform);
        }

        // Geschwindigkeit begrenzen
        if (rb.velocity.magnitude > maxVel)
        {
            rb.velocity = rb.velocity.normalized * maxVel;
            Debug.Log("Velocity nach clamp: " + rb.velocity.magnitude); 
        }
        IgnoreXAndZRotation(); // das locken dieser scheint ja egal zu sein lmao 
    }
    void IgnoreXAndZRotation() 
    {
        var x = transform.rotation;
        x.z = 0;
        x.x = 0;
        transform.rotation = x;
    }
    void HandleKettenKFZ()
    {
        if (Mathf.Abs(inputVertical) >= 0.1f)
        {
            Vector3 kraft = BewegungKettenfahrwerk();
            Vector3 kraftLinks = kraft;
            Vector3 kraftRechts = kraft;

            if (Mathf.Abs(inputHorizontal) > 0.01f)
            {
                float drehVerstärkung = 4f;
                if (inputHorizontal >= 0.01)
                {
                    kraftRechts *= -1 * drehVerstärkung;
                }
                else if (inputHorizontal <= -0.01)
                {
                    kraftLinks *= -1 * drehVerstärkung;
                }
            }
            var reifenRechts = _achsen[1].ReifenList;
            var reifenLinks = _achsen[0].ReifenList; 
            for (int i = 0; i < reifenRechts.Count; i++) 
            {
                rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftLinks), reifenRechts[i].position, ForceMode.Acceleration);
                rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftLinks), reifenLinks[i].position, ForceMode.Acceleration);
            }
            Debug.Log("kraft linkss: " + kraftLinks + " kraft rechts: " + kraftRechts);
        }

    }


    private void Acceleration(Transform reifen, Achse achse)
    {
        Vector3 dir = reifen.forward;
        //float geschwindigkeit = Vector3.Dot(rb.velocity, transform.forward);
        float availableTorque = inputVertical * _motorStärke;
        Debug.Log("Torque: " + availableTorque);                                                               
        if (rb.velocity.magnitude < maxVel)
        {
            Debug.Log("Angewendete beschleunigung: " + dir * (availableTorque * Time.fixedDeltaTime)); 
            rb.AddForceAtPosition(dir * (availableTorque * Time.fixedDeltaTime), reifen.position, ForceMode.Acceleration);
            Debug.Log("Geschwindigkeit vor clamp: " + rb.velocity.magnitude); 

        }
    }

    private void ApplyBrakes(Transform reifen)
    {
        Vector3 bremseKraft = -rb.velocity.normalized * bremskraft;
        rb.AddForceAtPosition(bremseKraft * Time.fixedDeltaTime, reifen.position, ForceMode.VelocityChange);
        rb.angularDrag = Mathf.Lerp(rb.angularDrag, 5f, Time.fixedDeltaTime * 5);
        Debug.Log("mache shit langsamer"); 
    }

    Vector3 ClampShit(Vector3 value) 
    {
        if(value.magnitude > maxVel) 
        {
            return value.normalized * maxVel; 
        }
        return value; 
    }
    private void Lenkung(Achse a, Transform reifen)
    {
        if (Physics.Raycast(reifen.position, Vector3.down, out RaycastHit hit, a.FahrwerksHoehe, ~ignore))
        {
            float geschwindigkeitsFaktor = Mathf.Clamp(rb.velocity.magnitude, 0.5f, 2f);
            Vector3 lenkDir = transform.right * inputHorizontal * geschwindigkeitsFaktor; // changed from transform.right to reifen.right

            Vector3 reifenGeschwindigkeit = rb.GetPointVelocity(hit.point);
            Vector3 seitlicheGeschwindigkeit = Vector3.Project(reifenGeschwindigkeit, transform.right);
            Vector3 korrekturKraft = -seitlicheGeschwindigkeit * lenkkraft * geschwindigkeitsFaktor;
            rb.AddForceAtPosition(lenkDir * lenkkraft * Time.fixedDeltaTime, reifen.position, ForceMode.Force);
            rb.AddForceAtPosition(korrekturKraft * Time.fixedDeltaTime, reifen.position, ForceMode.Force);  
        }
    }

    private void AdjustVehicleRotation()
    {
        if (Mathf.Abs(inputVertical) > 0.01f)
        {
            Vector3 intendedDirection = rb.velocity.normalized;
            Vector3 projectedVelocity = Vector3.Project(rb.velocity, transform.forward);
            if (projectedVelocity.magnitude > 0.1f)
            {
                intendedDirection = projectedVelocity.normalized;
            }
            Quaternion targetRotation = Quaternion.LookRotation(intendedDirection, Vector3.up);
            float rotationSpeed = 1.5f;
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }


    Vector3 BewegungKettenfahrwerk()
    {
        Vector3 forceDir = transform.forward * Mathf.Sign(inputVertical);
        float normalKraft = rb.mass * Physics.gravity.magnitude;
        float traktionskraft = _motorStärke * Mathf.Abs(inputVertical);
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        Vector3 gesKraft = (forceDir * traktionskraft) - (transform.forward * reibungsKraft);
        Debug.Log(gesKraft.magnitude + " " + gesKraft);
        return gesKraft;
    }

    public void SetFahrwerk(Fahrwerk.FahrwerksTyp f)
    {
        _fahrwerksTyp = f;
    }
}

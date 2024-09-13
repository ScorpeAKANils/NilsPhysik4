using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public AnimationCurve leistungsKurve;

    [SerializeField]
    private float lenkkraft = 10000f;

    [SerializeField]
    private float bremskraft = 15000f;

    public enum FahrwerksTyp
    {
        Radfahrwerk,
        Kettenfahrwerk
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
                
                r.Achse.RotateWheels(inputHorizontal, drehGeschwindigkeit);
                if (inputHorizontal != 0f)
                {
                    AdjustVehicleRotation();
                }
                //Lenkung(r.Achse, r.transform);
            }
        }
        if (rb.velocity.magnitude > maxVel)
        {
            rb.velocity = rb.velocity.normalized * maxVel;
        }
    }

    void HandleKettenKFZ()
    {
        Vector3 kraft = BewegungKettenfahrwerk();
        Vector3 kraftLinks = kraft;
        Vector3 kraftRechts = kraft;

        if (Mathf.Abs(inputHorizontal) > 0.01f)
        {
            if (inputHorizontal > 0)
            {
                kraftRechts *= (1 - Mathf.Abs(inputHorizontal));
            }
            else if (inputHorizontal < 0)
            {
                kraftLinks *= (1 - Mathf.Abs(inputHorizontal));
            }
        }

        rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftLinks), _achsen[0].transform.position, ForceMode.Acceleration);
        rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftRechts), _achsen[1].transform.position, ForceMode.Acceleration);
    }


    private void Acceleration(Transform reifen, Achse achse)
    {
        Vector3 dir = reifen.forward;
        if(reifen.name == "ReifenLV")
            Debug.Log(reifen.forward);
        float geschwindigkeit = Vector3.Dot(rb.velocity, transform.forward);
        float velNormalized = Mathf.Clamp01(Mathf.Abs(geschwindigkeit) / maxVel);
        float availableTorque = leistungsKurve.Evaluate(velNormalized) * inputVertical * _motorStärke;
        Debug.Log("Speeing up lan: " + dir * availableTorque * Time.fixedDeltaTime); 
        rb.AddForceAtPosition(dir * availableTorque * Time.fixedDeltaTime, reifen.position, ForceMode.Acceleration);
    }

    private void ApplyBrakes(Transform reifen)
    {
        Vector3 bremseKraft = -rb.velocity.normalized * bremskraft;
        rb.AddForceAtPosition(bremseKraft * Time.fixedDeltaTime, reifen.position, ForceMode.Impulse);
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
            Vector3 lenkDir = transform.right * inputHorizontal * geschwindigkeitsFaktor;

            Vector3 reifenGeschwindigkeit = rb.GetPointVelocity(hit.point);
            Vector3 seitlicheGeschwindigkeit = Vector3.Project(reifenGeschwindigkeit, transform.right);
            Vector3 korrekturKraft = -seitlicheGeschwindigkeit * lenkkraft * geschwindigkeitsFaktor;
            //rb.AddForceAtPosition(lenkDir * lenkkraft * Time.fixedDeltaTime, reifen.position, ForceMode.Impulse);
            //rb.AddForceAtPosition(korrekturKraft * Time.fixedDeltaTime, reifen.position, ForceMode.Impulse);
        }
    }


    private void AdjustVehicleRotation()
    {
        if (Mathf.Abs(inputVertical) > 0.01f)
        {
            Vector3 intendedDirection = transform.forward * Mathf.Sign(inputVertical);
            Vector3 currentVelocity = rb.velocity;
            Vector3 projectedVelocity = Vector3.Project(currentVelocity, intendedDirection);
            Vector3 desiredDirection = (projectedVelocity.magnitude > 1f) ? projectedVelocity.normalized : intendedDirection;
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, transform.up);
            float rotationSpeed = 2.5f;

            this.transform.localRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    Vector3 BewegungKettenfahrwerk()
    {
        Vector3 forceDir = transform.forward * inputVertical;
        float normalKraft = rb.mass * Physics.gravity.magnitude;
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;
        return forceDir * traktionskraft * 3;
    }

    public void SetFahrwerk(Fahrwerk.FahrwerksTyp f)
    {
        _fahrwerksTyp = f;
    }
}

using System.Collections;
using System.Collections.Generic;
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
        if (gdtool != null && gdtool.ToolEnabled == false) 
        {
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
            
            if(rb.velocity.magnitude > 0.1f && Input.GetKey(KeyCode.Space))
            {
                // Bremsen, wenn kein Gas gegeben wird
                ApplyBrakes(r.transform);
            }

            if (r.Achse.m_AchsenTyp == Achse.AchsenTyp.Lenkbar)                                                                                                      
            {
                r.Achse.RotateWheels(inputHorizontal, drehGeschwindigkeit);
             
                if(inputHorizontal > 0f) 
                {
                    AdjustVehicleRotation();
                }
                Lenkung(r.Achse, r.transform);
            }
        }

        // Begrenzung der Geschwindigkeit
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

        if (inputHorizontal > 0)
        {
            kraftRechts *= (1 - Mathf.Abs(inputHorizontal));
        }
        else if (inputHorizontal < 0)
        {
            kraftLinks *= (1 - Mathf.Abs(inputHorizontal));
        }

        rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftLinks), _achsen[0].transform.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(ClampShit(Time.fixedDeltaTime * kraftRechts), _achsen[1].transform.position, ForceMode.VelocityChange);
    }


    private void Acceleration(Transform reifen, Achse achse)
    {
        Vector3 dir = reifen.forward; // Verwenden Sie die Vorwärtsrichtung des Fahrzeugs
        float geschwindigkeit = Vector3.Dot(rb.velocity, transform.forward);
        float velNormalized = Mathf.Clamp01(Mathf.Abs(geschwindigkeit) / maxVel);
        float availableTorque = leistungsKurve.Evaluate(velNormalized) * inputVertical * _motorStärke;

        rb.AddForceAtPosition(dir * availableTorque * Time.fixedDeltaTime, reifen.position, ForceMode.Acceleration);
    }

    private void ApplyBrakes(Transform reifen)
    {
        Vector3 bremseKraft = -rb.velocity.normalized * bremskraft;
        rb.AddForceAtPosition(bremseKraft * Time.fixedDeltaTime, reifen.position, ForceMode.Acceleration);
    }
    Vector3 ClampShit(Vector3 value) 
    {
        //=> clampe shit um die größe der kräfte besser zu kontrollieren um somit ein stabileres verhalten zuerreichen
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
            Vector3 lenkDir = transform.right * inputHorizontal;
            Vector3 reifenGeschwindigkeit = rb.GetPointVelocity(hit.point);
            Vector3 seitlicheGeschwindigkeit = Vector3.Project(reifenGeschwindigkeit, transform.right);
            Vector3 korrekturKraft = -seitlicheGeschwindigkeit * lenkkraft;
            rb.AddForceAtPosition(lenkDir * lenkkraft * Time.fixedDeltaTime, reifen.position, ForceMode.Force);
            rb.AddForceAtPosition(korrekturKraft * Time.fixedDeltaTime, reifen.position, ForceMode.Force);
        }
    }
    private void AdjustVehicleRotation()
    {
        if (Mathf.Abs(inputVertical) > 0.1f) // Nur anpassen, wenn der Spieler Gas gibt oder bremst
        {
            Vector3 intendedDirection = transform.forward * Mathf.Sign(inputVertical);
            Vector3 currentVelocity = rb.velocity;

            // Projizieren Sie die aktuelle Geschwindigkeit auf die beabsichtigte Richtung
            Vector3 projectedVelocity = Vector3.Project(currentVelocity, intendedDirection);

            // Berechnen Sie die gewünschte Richtung basierend auf der projizierten Geschwindigkeit
            Vector3 desiredDirection = (projectedVelocity.magnitude > 1f) ? projectedVelocity.normalized : intendedDirection;

            // Berechnen Sie die Zielrotation
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, transform.up);

            // Wenden Sie eine sanfte Rotation an
            float rotationSpeed = 2.5f; // Anpassen Sie diesen Wert nach Bedarf
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

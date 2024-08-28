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
    private float maxVel = 10f;
    private float drehgeschwindigkeit = 1;
    private float drehMomentKetteRechts = 0;
    private float drehMomentKetteLinks = 0;

    private enum FahrwerksTyp
    {
        Radfahrwerk,
        Kettenfahrwerk
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
                Vector3 bewegung = BewegungRadfahrwerk();
                rb.AddForce(bewegung * Time.fixedDeltaTime);

                if (rb.velocity.magnitude > maxVel)
                {
                    rb.velocity = rb.velocity.normalized * maxVel;
                }

                if (rb.velocity.magnitude > 4)
                {
                    float drehInput = Input.GetAxis("Horizontal");
                    rb.AddTorque(Vector3.up * drehInput * drehgeschwindigkeit * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                break;

            case FahrwerksTyp.Kettenfahrwerk:
                float forceDirForward = Input.GetAxis("Vertical");
                float sidewardMovementDir = Input.GetAxis("Horizontal");
                Vector3 kraft = BewegungKettenfahrwerk(forceDirForward); // => annahme: Die kraft entwicklung ist gleich groß an beiden ketten
                Vector3 kraftLinks = kraft; 
                Vector3 kraftRechts = kraft;
                if (sidewardMovementDir > 0) 
                {
                    kraftRechts *= -1; 
                } 
                if(sidewardMovementDir < 0) 
                {
                    kraftLinks *= -1;
                }

                rb.AddForceAtPosition(Time.fixedDeltaTime * kraftLinks, _achsen[0].transform.position);
                rb.AddForceAtPosition(Time.fixedDeltaTime * kraftRechts, _achsen[1].transform.position);
                break;

            default:
                Debug.LogError("Unbekannter Fahrwerkstyp");
                break;
        }
    }

    Vector3 GetMovementVector()
    {
        float forceDirForward = Input.GetAxis("Vertical");

        return (transform.forward * forceDirForward).normalized;
    }

    Vector3 BewegungRadfahrwerk()
    {
        float force = 0f;
        foreach (Achse a in _achsen)
        {
            force += a.UpdateAchse(rb);
        }
        return GetMovementVector() * force;
    }

    Vector3 BewegungKettenfahrwerk(float forceDirForward)
    {   
        Vector3 forceDirLeft = (transform.forward * forceDirForward);
        float normalKraft = rb.mass * Physics.gravity.magnitude;
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;
        return forceDirLeft * traktionskraft*3;
    }
}
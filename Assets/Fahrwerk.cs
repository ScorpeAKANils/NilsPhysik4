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
                float forceDirSideward = Input.GetAxis("Horizontal");
                Vector3 kraftLinks = BewegungKettenfahrwerkLinks(forceDirForward, forceDirSideward);
                Vector3 kraftRechts = BewegungKettenfahrwerkRechts(forceDirForward, forceDirSideward);

                rb.AddForceAtPosition(kraftLinks * Time.fixedDeltaTime, _achsen[0].transform.position);
                rb.AddForceAtPosition(kraftRechts * Time.fixedDeltaTime, _achsen[1].transform.position);
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

    Vector3 BewegungKettenfahrwerkLinks(float forceDirForward, float forceDirSideward)
    {
        if (forceDirSideward < 0)
        {
            forceDirForward *= -1f;
        }

        Vector3 forceDirLeft = (transform.forward * forceDirForward);

        float normalKraft = rb.mass * Physics.gravity.magnitude;
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;

       // drehMomentKetteLinks += 1 * Time.deltaTime;
        //drehMomentKetteLinks = Mathf.Clamp(drehMomentKetteLinks, 0, 5);
        return forceDirLeft * traktionskraft*3;// * drehMomentKetteLinks;
    }

    Vector3 BewegungKettenfahrwerkRechts(float forceDirForward, float forceDirSideward)
    {
        if (forceDirSideward > 0)
        {
            forceDirForward *= -1f;
        }

        Vector3 forceDirRight = (transform.forward * forceDirForward);

        float normalKraft = rb.mass * Physics.gravity.magnitude;                    
        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;

        //drehMomentKetteRechts = Mathf.Clamp(drehMomentKetteRechts, 0, 5);

        return forceDirRight * traktionskraft*3;
    }
}



    public enum FahrwerksTyp
    {
        Radfahrwerk,
        Kettenfahrwerk
    }

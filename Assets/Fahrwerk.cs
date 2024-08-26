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
    private float maxVel = 25f;
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
                Vector3 kraftLinks = BewegungKettenfahrwerkLinks();
                Vector3 kraftRechts = BewegungKettenfahrwerkRechts();

                Vector3 gesamtKraft = (kraftLinks + kraftRechts) * 0.5f;
                rb.AddForce(gesamtKraft * Time.fixedDeltaTime);

                Vector3 drehmoment = Vector3.Cross(kraftLinks, kraftRechts);
                rb.AddTorque(drehmoment * Time.fixedDeltaTime*0.5f);
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

    Vector3 BewegungKettenfahrwerkLinks()
    {
        //Fragen: 
        //  => Welche schritte sind notwendig um dann die entgültige kraft als Vector für das fahrzeug zu berechnen? 
        /* Antwort: 
        * Kettenkräfte:
        *   Jede seite der Kette wird unabhängige Kräfte erzeugen 
        *Drehmoment: 
        *   Kettenfahrzeuge drehen sich, in dem sie unterschiedliche Drehmomente auf die Kettenseiten anwenden
        *       =>  z.B. in dem die eine Kette vorwärts dreht und die andere rückwärts
        *Schlupf und Reibung: 
        *   Kettenfahrzeuge gehen anders mit Reibung und Schlupf um
        *       =>  Wiederstand und Fluss sollten möglicher weise Berechnet werden
        *       Fragen:
        *           1.  was ist mit Seite der Kette gemeint?
        *               =>  Die beiden Kettenstränge, üblicher weiße gibt es 2 davon
        *                   => diese sind einzeln steuerbar 
        *           2.  Was ist mit Schlupf gemeint?
        *               =>  Unterschied zwischen der theoretischen Geschwindigkeit und der tatsächlichen Geschwindigkeit
        *                   => Theoretische Geschwindigkeit = Geschwindigkeit die das Fahrzeug aufgrund der Radumdrehung eigentlich haben sollte 
        *                   => Tritt auf, wenn die Ketten durch drehen 
        *           3.  Wie berechnet man die Reibung für eine Kette?
        *               =>  F_Reibung = μ * F_Normal; 
        *                   μ = Reibungskoeffizient 
        *                   F_Normal = Normalkraft die das Gewicht des Fahrzeuges auf die Ketten ausübt 
        *                       => F_Normal = m*g?
        *               3.1 Wie berechnet man den Schlupf für die Ketten?
        *                   Der Schlupf ist die diffrenz zwischen der linearen Geschwindigkeit der Kette und der tatsächlichen Geschwindigkeit des Fahrzeuges 
        *                   =>  Schlupf = (V_Kette-V_Fahrzeug)/V_Kette; 
        *                       V_Kette = Geschwindigkeit der Kette
        *                       V_Fahrzeug = Geschwindigkeit des Fahrzeuges 
        *                   
        *                   3.1.1  In welchen Fällen sollte man das tun? 
        *                           =>  Offroad Fahrzeuge
        *                           =>  Präzisere Steuerung 
        *                           =>  Kurvenfahrten 
        *                               =>  leider komm ich da wohl nicht drumherum 
        *                   
        *       
            
         */
        float forceDirLeftForward = Input.GetAxis("Vertical");
        float forceDirLeftSideward = Input.GetAxis("Horizontal");

        Vector3 forceDirLeft = (transform.forward * forceDirLeftForward) + (transform.right * forceDirLeftSideward);

        float normalKraft = GetComponent<Rigidbody>().mass * Physics.gravity.magnitude;

        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;

        if (forceDirLeft.magnitude > 0)
        {
            drehMomentKetteLinks += 1 * Time.deltaTime;
            drehMomentKetteLinks = Mathf.Clamp(drehMomentKetteLinks, 0, 5);
        }
        else
        {
            if(drehMomentKetteLinks > 0)
            {
                drehMomentKetteLinks -= 1 * Time.deltaTime;
                drehMomentKetteLinks = Mathf.Clamp(drehMomentKetteLinks, 0, 5);
            }
        }
        return forceDirLeft * traktionskraft * drehMomentKetteLinks; 
    }
    Vector3 BewegungKettenfahrwerkRechts()
    {
        //Fragen: 
        //  => Welche schritte sind notwendig um dann die entgültige kraft als Vector für das fahrzeug zu berechnen? 
        /* Antwort: 
        * Kettenkräfte:
        *   Jede seite der Kette wird unabhängige Kräfte erzeugen 
        *Drehmoment: 
        *   Kettenfahrzeuge drehen sich, in dem sie unterschiedliche Drehmomente auf die Kettenseiten anwenden
        *       =>  z.B. in dem die eine Kette vorwärts dreht und die andere rückwärts
        *Schlupf und Reibung: 
        *   Kettenfahrzeuge gehen anders mit Reibung und Schlupf um
        *       =>  Wiederstand und Fluss sollten möglicher weise Berechnet werden
        *       Fragen:
        *           1.  was ist mit Seite der Kette gemeint?
        *               =>  Die beiden Kettenstränge, üblicher weiße gibt es 2 davon
        *                   => diese sind einzeln steuerbar 
        *           2.  Was ist mit Schlupf gemeint?
        *               =>  Unterschied zwischen der theoretischen Geschwindigkeit und der tatsächlichen Geschwindigkeit
        *                   => Theoretische Geschwindigkeit = Geschwindigkeit die das Fahrzeug aufgrund der Radumdrehung eigentlich haben sollte 
        *                   => Tritt auf, wenn die Ketten durch drehen 
        *           3.  Wie berechnet man die Reibung für eine Kette?
        *               =>  F_Reibung = μ * F_Normal; 
        *                   μ = Reibungskoeffizient 
        *                   F_Normal = Normalkraft die das Gewicht des Fahrzeuges auf die Ketten ausübt 
        *                       => F_Normal = m*g?
        *               3.1 Wie berechnet man den Schlupf für die Ketten?
        *                   Der Schlupf ist die diffrenz zwischen der linearen Geschwindigkeit der Kette und der tatsächlichen Geschwindigkeit des Fahrzeuges 
        *                   =>  Schlupf = (V_Kette-V_Fahrzeug)/V_Kette; 
        *                       V_Kette = Geschwindigkeit der Kette
        *                       V_Fahrzeug = Geschwindigkeit des Fahrzeuges 
        *                   
        *                   3.1.1  In welchen Fällen sollte man das tun? 
        *                           =>  Offroad Fahrzeuge
        *                           =>  Präzisere Steuerung 
        *                           =>  Kurvenfahrten 
        *                               =>  leider komm ich da wohl nicht drumherum 
        *                   
        *       

         */

        float forceDirRightForward  =   Input.GetAxis("RightVertical");
        float forceDirRightSideward =   Input.GetAxis("RightHorizontal");
        Vector3 forceDirRight = (transform.forward * forceDirRightForward) + (transform.right * forceDirRightSideward);

        float normalKraft = GetComponent<Rigidbody>().mass * Physics.gravity.magnitude;

        float reibungsKraft = reibungsKoeffizient * normalKraft;
        float traktionskraft = _motorStärke - reibungsKraft;

        if (forceDirRight.magnitude > 0)
        {
            drehMomentKetteRechts += 1 * Time.deltaTime;
            drehMomentKetteRechts = Mathf.Clamp(drehMomentKetteRechts, 0, 5);
        }
        else
        {
            if (drehMomentKetteRechts > 0)
            {
                drehMomentKetteRechts -= 1 * Time.deltaTime;
                drehMomentKetteRechts = Mathf.Clamp(drehMomentKetteRechts, 0, 5);
            }
        }

        return forceDirRight * traktionskraft*drehMomentKetteRechts;
    }
}

public enum FahrwerksTyp
{
    Radfahrwerk,
    Kettenfahrwerk
}

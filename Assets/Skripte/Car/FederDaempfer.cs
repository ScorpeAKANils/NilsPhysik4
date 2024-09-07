using UnityEngine;

public class FederDaempfer : MonoBehaviour
{
    private float m_zielHoehe;
    [SerializeField]
    private float m_daempfer; 
    public float Daempfer { get { return m_daempfer; } }
    [SerializeField]
    private float m_feder;
    public float Feder { get { return m_feder; } } 
    [SerializeField]
    private Rigidbody m_rb;
    [SerializeField]
    private Achse m_achse;
    public float Mass = 2f; 
    public Achse Achse { get { return m_achse; } }
    [SerializeField]
    private LayerMask ignore;

    private void Start()
    {
        m_zielHoehe = m_achse.FahrwerksHoehe; 
    }
    Vector3 CalculateForcePerTimeStamp()
    {
        float hoehe = transform.position.y;
        float auslenkung = m_zielHoehe - hoehe;
        float federStaerke = Achse.BerechneFederkraft(m_feder, auslenkung);
        float geschwindigkeit = m_rb.GetPointVelocity(transform.position).y;

        float daempferStaerke = Achse.BerechneDaempfung(m_daempfer, geschwindigkeit);
        float gesamtKraft = federStaerke - daempferStaerke;
        return new Vector3(0, gesamtKraft, 0);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, Vector3.down, out hit, m_zielHoehe, ~ignore);
        if(hitSomething) 
        {
            Debug.Log(hit.collider.gameObject); 
            Vector3 force = CalculateForcePerTimeStamp();
            m_rb.AddForceAtPosition(force*Time.deltaTime, transform.position, ForceMode.Force);
        }
    }
}

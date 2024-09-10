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
            float federkraft = Achse.BerechneFederkraft(m_feder, auslenkung);
            float daempfungskraft = Achse.BerechneDaempfung(m_daempfer, m_rb.GetPointVelocity(this.transform.position).magnitude);
            Vector3 resultierendeKraft = new Vector3(0, federkraft - daempfungskraft, 0);
            return resultierendeKraft;
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, m_zielHoehe, ~ignore);
        if (hitSomething)
        {
            Vector3 force = CalculateForcePerTimeStamp();
            m_rb.AddForceAtPosition(force * Time.fixedDeltaTime, transform.position, ForceMode.VelocityChange);
        }
    }
}

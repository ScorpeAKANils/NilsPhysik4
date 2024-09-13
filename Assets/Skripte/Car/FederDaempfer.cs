using UnityEngine;

public class FederDaempfer : MonoBehaviour
{
    private float m_zielHoehe;
    [SerializeField] private float m_daempfer;
    public float Daempfer { get { return m_daempfer; } }
    [SerializeField] private float m_feder;
    public float Feder { get { return m_feder; } }
    [SerializeField] private Rigidbody m_rb;
    [SerializeField] private Achse m_achse;
    public float Mass = 2f;
    public Achse Achse { get { return m_achse; } }
    [SerializeField] private LayerMask ignore;

    [SerializeField] private float maxSchlupf = 0.2f; // Maximaler Reifen-Schlupf
    [SerializeField] private float seitenschub = 5000f; // Seitenschubkraft des Reifens
    [SerializeField] private float laengsschub = 5000f; // Längsschubkraft des Reifens

    private void Start()
    {
        m_zielHoehe = m_achse.FahrwerksHoehe;
    }

    // Berechnet die resultierende Kraft (Feder, Dämpfung und Schlupf)
    Vector3 CalculateForcePerTimeStamp()
    {
        float hoehe = transform.position.y;
        float auslenkung = m_zielHoehe - hoehe;
        float federkraft = Achse.BerechneFederkraft(m_feder, auslenkung);
        float daempfungskraft = Achse.BerechneDaempfung(m_daempfer, m_rb.GetPointVelocity(this.transform.position).magnitude);

        // Berechnung des Längsschlupfs (Vereinfachung)
        float geschwindigkeit = Vector3.Dot(m_rb.velocity, transform.forward);
        float schlupfKraft = Mathf.Clamp(geschwindigkeit * laengsschub, -maxSchlupf, maxSchlupf);

        // Seitenschub (Abhängig vom Schlupf und der Geschwindigkeit)
        float seitlicheGeschwindigkeit = Vector3.Dot(m_rb.velocity, transform.right);
        float seitenschubKraft = Mathf.Clamp(-seitlicheGeschwindigkeit * seitenschub, -maxSchlupf, maxSchlupf);

        Vector3 resultierendeKraft = new Vector3(seitenschubKraft, federkraft - daempfungskraft, schlupfKraft);
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

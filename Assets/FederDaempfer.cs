using UnityEngine;

public class FederDaempfer : MonoBehaviour
{
    [SerializeField]
    private float _zielHoehe;
    public float ZielHoehe { get { return _zielHoehe; } } 
    [SerializeField]
    private float _daempfer; 
    public float Daempfer { get { return _daempfer; } }
    [SerializeField]
    private float _feder;
    public float Feder { get { return _feder; } } 
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private Achse m_achse;
    public float Mass = 2f; 
    public Achse Achse { get { return m_achse; } }
    [SerializeField]
    LayerMask ignore; 
    Vector3 CalculateForcePerTimeStamp()
    {
        float hoehe = transform.position .y;
        float auslenkung = _zielHoehe - hoehe;
        float federStaerke = Achse.BerechneFederkraft(_feder, auslenkung);
        float geschwindigkeit = _rb.GetPointVelocity(transform.position).y;

        float daempferStaerke = Achse.BerechneDaempfung(_daempfer, geschwindigkeit);
        float gesamtKraft = federStaerke - daempferStaerke;
        return new Vector3(0, gesamtKraft, 0);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, Vector3.down, out hit, _zielHoehe, ~ignore);
        if(hitSomething) 
        {
            Debug.Log(hit.collider.gameObject); 
            Vector3 force = CalculateForcePerTimeStamp();
            _rb.AddForceAtPosition(force, transform.position, ForceMode.Force);


        }
    }
}

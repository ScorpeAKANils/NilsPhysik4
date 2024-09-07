using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("R�der Settings")]
    public int anzahlR�der = 4;  // Anzahl der R�der
    public Transform[] radPositionen;  // Positionen der R�der

    void OnValidate()
    {
        // Sicherstellen, dass das Array immer die korrekte Gr��e hat
        if (radPositionen == null || radPositionen.Length != anzahlR�der)
        {
            radPositionen = new Transform[anzahlR�der];
        }
    }

    void OnDrawGizmos()
    {
        // R�der-Positionen im Editor anzeigen
        Gizmos.color = Color.red;
        foreach (var position in radPositionen)
        {
            Gizmos.DrawSphere(position.position, 0.2f);  // Zeichnet Kugeln an den Radpositionen
        }
    }
}

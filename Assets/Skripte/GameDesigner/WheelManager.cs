using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Räder Settings")]
    public int anzahlRäder = 4;  // Anzahl der Räder
    public Transform[] radPositionen;  // Positionen der Räder

    void OnValidate()
    {
        // Sicherstellen, dass das Array immer die korrekte Größe hat
        if (radPositionen == null || radPositionen.Length != anzahlRäder)
        {
            radPositionen = new Transform[anzahlRäder];
        }
    }

    void OnDrawGizmos()
    {
        // Räder-Positionen im Editor anzeigen
        Gizmos.color = Color.red;
        foreach (var position in radPositionen)
        {
            Gizmos.DrawSphere(position.position, 0.2f);  // Zeichnet Kugeln an den Radpositionen
        }
    }
}

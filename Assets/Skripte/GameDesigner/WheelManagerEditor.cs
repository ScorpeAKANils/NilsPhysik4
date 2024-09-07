using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WheelManager))]
public class WheelManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        // Referenz auf das WheelManager-Objekt
        WheelManager manager = (WheelManager)target;

        // Schleife über alle Rad-Positionen
        for (int i = 0; i < manager.radPositionen.Length; i++)
        {
            // Zeichne ein verschiebbares Handle für jede Radposition
            var oldPosition = manager.radPositionen[i];
            var newPosition = Handles.PositionHandle(manager.transform.position + oldPosition.position, Quaternion.identity);

            if (oldPosition.position != newPosition)
            {
                Undo.RecordObject(manager, "Move Wheel Position");
                manager.radPositionen[i].position = newPosition;
                EditorUtility.SetDirty(manager);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        // Zeige den Standard-Inspector mit allen Feldern an
        base.OnInspectorGUI();

        // Optional: Zusätzliche Layout-Elemente für eine bessere Benutzerfreundlichkeit
        WheelManager manager = (WheelManager)target;

        // Automatische Aktualisierung der Scene View, wenn sich etwas ändert
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }
}

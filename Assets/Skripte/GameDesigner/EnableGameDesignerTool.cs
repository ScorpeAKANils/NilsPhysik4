using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnableGameDesignerTool : MonoBehaviour
{
    public static EnableGameDesignerTool instance; 
    public bool ToolEnabled = true;
    public List<float> rectTransform = new List<float>();
    bool mhallesdoof = true;
    private string inputString = "0"; // Hier speicherst du die Eingabe als String
    private int inputValue = 0;
    public Fahrwerk Fahrwerk;
    public Achse selectedAchse; 


    private void Awake()
    {
        if(instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(this); 
        }
        else 
        {
            Destroy(gameObject); 
        }
    }
    private void OnGUI()
    {
        Rect temp = new Rect(10, 10, 100, 30);
        mhallesdoof = GUI.Toggle(temp, mhallesdoof, "WheelTool");

        if (mhallesdoof)
        {
            GUI.Label(new Rect(10, 50, 200, 30), "WheelTool enabled!");
            ToolEnabled = true;
        }
        else
        {
            GUI.Label(new Rect(10, 50, 200, 30), "WheelTool disabled!");
            ToolEnabled = false;
        }

        if (ToolEnabled) 
        {
            GUI.Label(new Rect(10, 90, 200, 30), "Enter an integer value:");
            inputString = GUI.TextField(new Rect(10, 120, 200, 30), inputString);

            if (GUI.Button(new Rect(10, 160, 100, 30), "Submit"))
            {
                if (int.TryParse(inputString, out inputValue))
                {
                    Debug.Log("Valid int entered: " + inputValue);

                    if(selectedAchse == null) 
                    {
                        Debug.LogError("Keine Achse ausgewählt"); 
                        return; 
                    }
                    if(inputValue == 0) 
                    {
                        Debug.LogError("Fahrzeug muss mindestens 1 Reifen haben");
                        return; 
                    }
                    int wheelCount = selectedAchse.GetWheelCount(); 
                    int dif = inputValue - wheelCount;
                    if(dif > 0) 
                    {
                        for(int i = 0; i <= dif; i++ )
                            selectedAchse.AddNewWheel(); 
                    }
                    else if (dif > 0)
                    {
                        for (int i = 0; i <= Mathf.Abs(dif); i++)
                            selectedAchse.RemoveWheele();
                    }
                }
                else
                {
                    Debug.LogError("Invalid input! Please enter a valid integer.");
                }
            }
            GUI.Label(new Rect(10, 200, 200, 30), "Current value: " + inputValue);
        }
    }
}

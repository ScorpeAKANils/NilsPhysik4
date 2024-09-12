using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Windows;

public class MoveableObjectManager : MonoBehaviour
{
    private static MoveableObjectManager _instance;
    private Vector3 latestMousePos;
    private void Awake()
    {
        latestMousePos = UnityEngine.Input.mousePosition; 
    }
    public static MoveableObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MoveableObjectManager>();
                if (_instance == null)
                {
                    GameObject managerObject = new GameObject("MoveableObjectManager");
                    _instance = managerObject.AddComponent<MoveableObjectManager>();
                }
            }
            return _instance;
        }
    }

    private List<Selectable> moveableObjects = new List<Selectable>();
    private Selectable selectedObject;
    private Vector3 m_input;

    public void DeselectObject() 
    {
        selectedObject.isSelected = false;
        selectedObject = null; 
    }
    private void Update()
    {
        if (selectedObject == null)
        {
            Debug.LogError("Deine Mutter"); 
            return;
        }
            float objectZ = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
        m_input = Camera.main.ScreenToWorldPoint(new Vector3(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, objectZ));

        if (UnityEngine.Input.GetButton("Fire1"))
        {
            Vector3 mouseDir = (m_input-latestMousePos).normalized;
            Debug.Log("MouseDir: " + mouseDir);
            mouseDir.x = 0;
            selectedObject.transform.position += mouseDir * Time.deltaTime;
          }
    }

    private void LateUpdate()
    {
        if (selectedObject == null) 
        {
            return; 
        }
        float objectZ = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
        latestMousePos = Camera.main.ScreenToWorldPoint(new Vector3(UnityEngine.Input.mousePosition.x,   UnityEngine.Input.mousePosition.y, objectZ));
    }

    public void RegisterObject(Selectable obj)
    {
        if (!moveableObjects.Contains(obj))
        {
            moveableObjects.Add(obj);
        }
    }

    public void UnregisterObject(Selectable obj)
    {
        moveableObjects.Remove(obj);
        if (selectedObject == obj)
        {
            selectedObject = null;
        }
    }

    public void SelectObject(Selectable obj)
    {
        Debug.Log("Selected object: " + obj); 
        if (selectedObject != null)
        {
            selectedObject.isSelected = false;
        }
        selectedObject = obj;
        selectedObject.isSelected = true;
    }

    public Selectable GetSelectedObject()
    {
        return selectedObject;
    }
}
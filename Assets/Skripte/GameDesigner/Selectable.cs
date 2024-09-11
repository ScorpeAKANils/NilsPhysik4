using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic; 

public class Selectable : MonoBehaviour
{
    [HideInInspector]
    public bool isSelected = false;
    public Material IsSelected;
    public Material IsDeselected;
    public Achse m_Achse; 
    private MeshRenderer m_meshRenderer;
    [SerializeField]
    private LayerMask layer; 

    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        MoveableObjectManager.Instance.RegisterObject(this);
    }

    private void Update()
    {
        if(!IsSelected && m_meshRenderer.material != IsDeselected) 
        {
            m_meshRenderer.material = IsDeselected;  
        }
    }
    private void OnDestroy()
    {
        MoveableObjectManager.Instance.UnregisterObject(this);
    }
    void OnMouseDown()
    {
        Debug.Log("MouseDown"); 
        if (EnableGameDesignerTool.instance.ToolEnabled == false) 
        {
            return; 
        }
        Debug.Log("ToolEnabled");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer) && hit.transform == this.transform && !isSelected) 
        {
            Debug.Log(hit.transform.name); 
            if(m_Achse == null) 
            {
                 MoveableObjectManager.Instance.SelectObject(this);
                 m_meshRenderer.material = IsSelected;
            } else 
            {
                EnableGameDesignerTool.instance.selectedAchse = m_Achse;
                m_meshRenderer.material = IsSelected;
            }
        } 
    }

    private void OnMouseUp()
    {
        Debug.Log("Mouse Up"); 
        if (isSelected)
        {
            MoveableObjectManager.Instance.DeselectObject();
            m_meshRenderer.material = IsDeselected;
        }
    }
}

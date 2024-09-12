using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic; 

public class Selectable : MonoBehaviour
{
    [HideInInspector]
    public bool isSelected = false;
    public Material Selected;
    public Material Deselected;
    public Achse m_Achse; 
    private MeshRenderer m_meshRenderer;
    [SerializeField]
    private LayerMask layer;
    [SerializeField]
    private SelectableType type; 
    public enum SelectableType 
    {
        Reifen, 
        Achse
    }
    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        MoveableObjectManager.Instance.RegisterObject(this);
    }

    private void Update()
    {
        if(!isSelected && m_meshRenderer.material != Deselected) 
        {
            m_meshRenderer.material = Deselected;  
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
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer) && hit.transform == this.transform) 
        {
            Debug.Log(hit.transform.name);
            if (type == Selectable.SelectableType.Reifen && !isSelected) 
            {
                 MoveableObjectManager.Instance.SelectObject(this);
                Debug.Log("Achse gleich true"); 
                 m_meshRenderer.material = Selected;
            } else 
            {
                Debug.Log("Cool af ");
                if (!isSelected) 
                {
                    Debug.Log("Aktiviere shit my man bro diggah waz up");
                    isSelected = true;
                    EnableGameDesignerTool.instance.selectedAchse = m_Achse;
                    EnableGameDesignerTool.instance.SpawnPos = this.gameObject.transform.position; 
                    m_meshRenderer.material = Selected;
                    return; 
                } 
                if(isSelected)
                {
                    Debug.Log("Deaktiviere shit my man bro diggah waz up"); 
                    isSelected = false;
                    EnableGameDesignerTool.instance.selectedAchse = null;
                    EnableGameDesignerTool.instance.SpawnPos = new Vector3(-999, -999, -999);
                    m_meshRenderer.material = Deselected;
                }

            }
        } 
    }

    private void OnMouseUp()
    {
        Debug.Log("Mouse Up"); 
        if (isSelected && type == Selectable.SelectableType.Reifen)
        {
            MoveableObjectManager.Instance.DeselectObject();
            m_meshRenderer.material = Deselected;
        }
    }
}

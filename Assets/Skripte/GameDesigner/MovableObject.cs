using Unity.VisualScripting;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    [HideInInspector]
    public bool isSelected = false;

    private void Start()
    {
        MoveableObjectManager.Instance.RegisterObject(this);
    }

    private void OnDestroy()
    {
        MoveableObjectManager.Instance.UnregisterObject(this);
    }
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.transform == this.transform)
                MoveableObjectManager.Instance.SelectObject(this);
    }
} 
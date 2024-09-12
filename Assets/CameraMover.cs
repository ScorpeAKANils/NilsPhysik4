using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMover : MonoBehaviour
{
    //init transform data in local space 
    Quaternion InitRot;
    Vector3 InitPos;
    [SerializeField]
    Quaternion TargetRotation;
    [SerializeField]
    Vector3 TargetPos;
    public static CameraMover instance;

    private void Awake()
    {
        instance = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        InitRot = transform.localRotation; 
        InitPos = transform.localPosition;
    }
    public void MoveCamToEditorPos() 
    {
        transform.localRotation = TargetRotation;
        transform.localPosition = TargetPos; 
    }

    public void MoveCamToGamePos()
    {
        transform.localRotation = InitRot;
        transform.localPosition = InitPos;
    }
}

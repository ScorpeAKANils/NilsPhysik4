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
    public bool inGame = true; 

    private void Awake()
    {
        instance = this;
        InitRot = transform.localRotation;
        InitPos = transform.localPosition;
    }
    public void MoveCamToEditorPos() 
    {
        if(inGame == true) 
        {
            inGame = false; 
            transform.localRotation = TargetRotation;
            transform.localPosition = TargetPos; 
        }
    }

    public void MoveCamToGamePos()
    {
        if (inGame == false) 
        {
            inGame = true; 
            transform.localRotation = InitRot;
            transform.localPosition = InitPos;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lockypos : MonoBehaviour
{
    Vector3 localLocPos; 
    // Start is called before the first frame update
    void Start()
    {
        localLocPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = localLocPos; 
    }
}

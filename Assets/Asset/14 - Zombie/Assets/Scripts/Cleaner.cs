using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : MonoBehaviour
{
    Action onClean;

    private void Start()
    {
        onClean += CleaningRoomA;
        onClean += CleaningRoomB;
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            onClean();
        }
    }

    void CleaningRoomA()
    {
        Debug.Log("A规 没家");
    }

    void CleaningRoomB()
    {
        Debug.Log("B规 没家");
    }
}

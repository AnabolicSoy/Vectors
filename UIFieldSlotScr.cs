using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIFieldSlotScr : MonoBehaviour
{
    int X;
    int Y;
    public void SetUp(int x , int y)
    {
        X = x; Y = y;   
    }

    private bool GetXYByMousePos(Vector3 MousePos, out int X, out int Y)
    {
        X = 0; Y = 0;


        return false;
    }

    bool PointerIsDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerIsDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (PointerIsDown)
        {
            PointerIsDown = false;



 


        }
    }
}

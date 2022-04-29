using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("constructor 1: ");
        Grid<int> grid1 = new Grid<int>(4, 4);
        foreach (int i in grid1)
        {
            Debug.Log(i);
        }



    }
}

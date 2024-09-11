using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public HexagonStack HexagonStack { get; private set; }
    public bool IsOccupied 
    {
        get => HexagonStack != null;
        private set { }
    }

    public void AssignStack(HexagonStack stack)
    {
        HexagonStack = stack;
    }

}

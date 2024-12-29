using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotArea : MonoBehaviour
{
    [SerializeField] private LayerMask _slingshotAreaMask;
    
    public bool IsWithinSlingshotArea()
    {
        Vector2 worldposition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (Physics2D.OverlapPoint(worldposition))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    void Start()
    {
        if (Mouse.current != null)
            Mouse.current.enabled = false;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class DisableMouse : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject); // ëSÉVÅ[ÉìÇ≈ê∂ë∂
        if (Mouse.current != null)
            InputSystem.DisableDevice(Mouse.current);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}

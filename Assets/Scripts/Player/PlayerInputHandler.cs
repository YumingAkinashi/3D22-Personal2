using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Tooltip("靈敏度")]
    public float LookSensitivity = 1f;

    [Tooltip("給WebGL靈敏度的調整項")]
    public float WebglLookSensitivityMultiplier = 0.25f;

    private void Start()
    {


    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0)
            return;

        GetFireInputDown();
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public bool GetFireInputDown()
    {
        if (CanProcessInput())
        {
            return Mouse.current.leftButton.isPressed;
        }

        return false;
    }

    public bool GetReloadButtonDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(GameConstants.k_ButtonReload);
        }

        return false;
    }

    float GetMouseLookAxis(string mouseInputName)
    {
        float i = Input.GetAxisRaw(mouseInputName);
        i *= (LookSensitivity * 0.01f);

#if UNITY_WEBGL
        
        // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
        i *= WebglLookSensitivityMultiplier;

#endif

        return i;
    }

}

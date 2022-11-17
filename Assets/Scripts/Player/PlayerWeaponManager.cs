using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class PlayerWeaponManager : MonoBehaviour
{

    [Header("References")]
    [Tooltip("主要武器")]
    public WeaponController weapon;

    [Tooltip("主要攝影機")]
    public CinemachineVirtualCamera vCamera;

    [Tooltip("武器總父物件")]
    public Transform WeaponParentSocket;

    //

    [Header("武器晃動")]
    [Tooltip("晃動頻率")]
    public float BobFrequency = 10f;

    [Tooltip("晃動速度")]
    public float BobSharpness = 10f;

    [Tooltip("晃動距離")]
    public float DefaultBobAmount = 0.05f;

    [Tooltip("預設Field of view")]
    public float DefaultFov = 50f;

    PlayerInputHandler m_InputHandler;
    public FirstPersonController m_PlayerCharacterController;
    float m_WeaponBobFactor;
    Vector3 m_LastCharacterPosition;
    Vector3 m_WeaponMainLocalPosition;
    Vector3 m_WeaponBobLocalPosition;

    private void Start()
    {
        m_InputHandler = GetComponent<PlayerInputHandler>();


        SetFov(DefaultFov);
    }

    private void Update()
    {

        if (Time.timeScale == 0)
            return;

        // handle reload
        if(weapon.m_LastTimeShot + weapon.AmmoReloadDelay < Time.time)
        {
            weapon.TryReload(m_InputHandler.GetReloadButtonDown());
        }
        
        // automatic reload
        if(weapon.m_CurrentAmmo == 0)
        {
            weapon.TryReload(true);
        }

        // handle shooting
        if (m_InputHandler.GetFireInputDown())
        {
            if (weapon.IsReloading && weapon.m_CurrentAmmo > 0)
            {
                weapon.m_ReloadAnimator.ResetTrigger("Reload");
                weapon.IsReloading = false;
                weapon.IsReloadingInterrupted = true;

                weapon.HandleShootInput(true);
            }
            else
            {
                weapon.HandleShootInput(true);
            }
        }

    }

    void LateUpdate()
    {
        UpdateWeaponBob();

        // Set final weapon socket position based on all the combined animation influences
        WeaponParentSocket.localPosition =
            m_WeaponMainLocalPosition + m_WeaponBobLocalPosition;
    }

    public void SetFov(float fov)
    {
        vCamera.m_Lens.FieldOfView = fov;
    }

    public void UpdateWeaponBob()
    {
        if (Time.deltaTime > 0f)
        {
            Vector3 playerCharacterVelocity =
                (m_PlayerCharacterController.transform.position - m_LastCharacterPosition) / Time.deltaTime;

            // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
            float characterMovementFactor = 0f;
            if (m_PlayerCharacterController.Grounded)
            {
                characterMovementFactor =
                    Mathf.Clamp01(playerCharacterVelocity.magnitude /
                                  (m_PlayerCharacterController.MoveSpeed * 
                                  (m_PlayerCharacterController.SprintSpeed / m_PlayerCharacterController.MoveSpeed)));
            }

            m_WeaponBobFactor =
                Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

            // Calculate vertical and horizontal weapon bob values based on a sine function
            float bobAmount = DefaultBobAmount;
            float frequency = BobFrequency;
            float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
            float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
                              m_WeaponBobFactor;

            // Apply weapon bob
            m_WeaponBobLocalPosition.x = hBobValue;
            m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            m_LastCharacterPosition = m_PlayerCharacterController.transform.position;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("武器名稱，UI需要可用")]
    public string WeaponName;

    [Tooltip("武器圖標，UI需要可用")]
    public Sprite WeaponIcon;

    [Header("CrossHair")]
    [Tooltip("準心預設樣式")]
    public CrosshairData CrosshairDataDefault;

    [Header("Internal References")]
    [Tooltip("武器的root object，如果武器要收起來之類的可以disable它")]
    public GameObject WeaponRoot;

    [Tooltip("射出子彈的位置")]
    public Transform WeaponMuzzle;
    [Tooltip("不受影響的射出子彈的位置")]
    public Transform DefaultMuzzle;

    [Tooltip("槍口閃爍特效")]
    public GameObject MuzzleFlashPrefab;

    [Header("Shoot Parameters")]
    [Tooltip("子彈Prefab")] 
    public ProjectileBase ProjectilePrefab;

    [Tooltip("最小射擊冷卻時間")]
    public float DelayBetweenShots = 1f;

    [Header("Ammo Parameters")]
    [Tooltip("玩家是否需要手動裝彈")]
    public bool AutomaticReload = true;

    [Tooltip("一個彈夾的子彈數")]
    public int ClipSize = 5;

    [Tooltip("每秒裝彈數")]
    public float AmmoReloadRate = 1f;

    [Tooltip("最後一次射擊到可以裝彈的冷卻時間")]
    public float AmmoReloadDelay = 1f;

    [Tooltip("一把槍最多攜帶總子彈數")]
    public int MaxAmmo = 20;

    [Header("Audio & Visual")]
    [Tooltip("射擊時的武器特效")]
    public Animator WeaponAnimator;

    [Tooltip("Unparent槍口特效")]
    public bool UnparentMuzzleFlash;

    [Tooltip("sound played when shooting")]
    public AudioClip ShootSfx;

    public UnityAction OnShoot;
    public event Action OnShootProcessed;

    public float m_CurrentAmmo;
    public float m_LastTimeShot = Mathf.NegativeInfinity;

    public GameObject Owner;

    AudioSource m_ShootAudioSource;

    public Animator m_ReloadAnimator;

    public float ReloadCount;
    public bool IsReloading;
    public bool IsReloadingInterrupted;


    private void Start()
    {
        m_CurrentAmmo = ClipSize;
        m_ReloadAnimator = GetComponent<Animator>();
    }

    public bool TryReload(bool reloadInput)
    {

        if(reloadInput && MaxAmmo > 0 && m_CurrentAmmo < ClipSize && !IsReloading)
        {
            if(ClipSize - m_CurrentAmmo >= MaxAmmo)
            {
                ReloadCount = MaxAmmo;
            }
            else
            {
                ReloadCount = ClipSize - m_CurrentAmmo;
            }

            IsReloading = true;
            IsReloadingInterrupted = false;
            InvokeRepeating("StartReloadAnimation", 0f, 1.1f);

            return true;
        }

        return false;
    }

    public void StartReloadAnimation()
    {
        if(IsReloading && !IsReloadingInterrupted)
        {
            m_ReloadAnimator.SetTrigger("Reload");
            ReloadCount--;
        }
        else
        {
            CancelInvoke("StartReloadAnimation");
            return;
        }

        if (ReloadCount <= 0)
        {
            IsReloading = false;
            CancelInvoke("StartReloadAnimation");
        }

    }

    public bool HandleShootInput(bool inputDown)
    {

        if (inputDown)
        {
            return TryShoot();
        }

        return false;
    }

    bool TryShoot()
    {
        if(m_CurrentAmmo >= 1f
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1f;

            return true;
        }

        return false;
    }

    void HandleShoot()
    {
        ProjectileBase newProjectile = Instantiate(ProjectilePrefab, DefaultMuzzle.position, Quaternion.LookRotation(DefaultMuzzle.forward));
        newProjectile.Shoot(this);

        // muzzle flash
        if (MuzzleFlashPrefab != null)
        {
            GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
                Quaternion.Euler(0f, -90f, 0f), WeaponMuzzle.transform);

            // Unparent the muzzleFlashInstance
            if (UnparentMuzzleFlash)
            {
                muzzleFlashInstance.transform.SetParent(null);
            }

            Destroy(muzzleFlashInstance, 2f);
        }

        // fire animation
        m_ReloadAnimator.SetTrigger("Fire");

        m_LastTimeShot = Time.time;

        OnShoot?.Invoke();
        OnShootProcessed?.Invoke();
    }

}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("準心圖片")]
    public Sprite CrosshairSprite;

    [Tooltip("準心大小")]
    public int CrosshairSize;

    [Tooltip("準心顏色")]
    public Color CrosshairColor;
}

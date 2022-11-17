using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("�Z���W�١AUI�ݭn�i��")]
    public string WeaponName;

    [Tooltip("�Z���ϼСAUI�ݭn�i��")]
    public Sprite WeaponIcon;

    [Header("CrossHair")]
    [Tooltip("�Ǥ߹w�]�˦�")]
    public CrosshairData CrosshairDataDefault;

    [Header("Internal References")]
    [Tooltip("�Z����root object�A�p�G�Z���n���_�Ӥ������i�Hdisable��")]
    public GameObject WeaponRoot;

    [Tooltip("�g�X�l�u����m")]
    public Transform WeaponMuzzle;
    [Tooltip("�����v�T���g�X�l�u����m")]
    public Transform DefaultMuzzle;

    [Tooltip("�j�f�{�{�S��")]
    public GameObject MuzzleFlashPrefab;

    [Header("Shoot Parameters")]
    [Tooltip("�l�uPrefab")] 
    public ProjectileBase ProjectilePrefab;

    [Tooltip("�̤p�g���N�o�ɶ�")]
    public float DelayBetweenShots = 1f;

    [Header("Ammo Parameters")]
    [Tooltip("���a�O�_�ݭn��ʸ˼u")]
    public bool AutomaticReload = true;

    [Tooltip("�@�Ӽu�����l�u��")]
    public int ClipSize = 5;

    [Tooltip("�C��˼u��")]
    public float AmmoReloadRate = 1f;

    [Tooltip("�̫�@���g����i�H�˼u���N�o�ɶ�")]
    public float AmmoReloadDelay = 1f;

    [Tooltip("�@��j�̦h��a�`�l�u��")]
    public int MaxAmmo = 20;

    [Header("Audio & Visual")]
    [Tooltip("�g���ɪ��Z���S��")]
    public Animator WeaponAnimator;

    [Tooltip("Unparent�j�f�S��")]
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
    [Tooltip("�Ǥ߹Ϥ�")]
    public Sprite CrosshairSprite;

    [Tooltip("�Ǥߤj�p")]
    public int CrosshairSize;

    [Tooltip("�Ǥ��C��")]
    public Color CrosshairColor;
}

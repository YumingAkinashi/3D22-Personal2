using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmoCounter : MonoBehaviour
{

    PlayerWeaponManager m_PlayerWeaponManager;
    WeaponController m_Weapon;

    public TextMeshProUGUI AmmoCounterText;

    // ­pºâ¤l¼u¡Gclip size, max ammo, m_currAmmo

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerWeaponManager = FindObjectOfType<PlayerWeaponManager>();
        m_Weapon = m_PlayerWeaponManager.weapon;
    }

    private void Update()
    {
        UpdateAmmo();
    }

    public void UpdateAmmo()
    {
        AmmoCounterText.text = m_Weapon.m_CurrentAmmo.ToString() + " / " + m_Weapon.MaxAmmo.ToString();
    }
}

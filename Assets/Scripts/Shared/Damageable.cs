using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the received damage")]
    public float DamageMultiplier = 1f;

    [Range(0, 1)]
    [Tooltip("Multiplier to apply to self damage")]
    public float SensibilityToSelfdamage = 0f;

    public Health Health { get; private set; }

    void Awake()
    {
        // find the health component either at the same level, or higher in the hierarchy
        Health = GetComponent<Health>();
        if (!Health)
        {
            Health = GetComponentInParent<Health>();
        }
    }

    public void InflictDamage(float damage, GameObject damageSource)
    {
        if (Health)
        {
            var totalDamage = damage;

            // potentially reduce damages if inflicted by self
            if (Health.gameObject == damageSource)
            {
                totalDamage *= SensibilityToSelfdamage;
            }

            // apply the damages
            Health.TakeDamage(totalDamage, damageSource);
        }
    }
}

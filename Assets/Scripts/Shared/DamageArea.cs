using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class DamageArea : MonoBehaviour
{

    [Tooltip("傷害最大範圍距離")]
    public float AreaOfEffectDistance = 8f;

    [Tooltip("傷害距離乘數")]
    public AnimationCurve DamageRatioOverDistance;

    [Tooltip("傷害推力")]
    public float PushForce = 20f;

    [Tooltip("推力向上乘數")]
    public float UpwardModifier = 3f;

    [Tooltip("敵人推力向上乘數")]
    public float EnemyUpwardModifier = 6f;

    [Tooltip("推力距離乘數")]
    public AnimationCurve PushForceRatioOverDistance;

    [Header("Debug")]
    [Tooltip("傷害範圍Gizmos顏色")]
    public Color AreaOfEffectColor = Color.red * 0.5f;

    public void InflictDamageInArea(float damage, Vector3 center, Vector3 normal, LayerMask layers,
        QueryTriggerInteraction interaction, GameObject owner)
    {
        Dictionary<Health, Damageable> uniqueDamagedHealths = new Dictionary<Health, Damageable>();

        // Create a collection of unique health components that would be damaged in the area of effect (in order to avoid damaging a same entity multiple times)
        Collider[] affectedColliders = Physics.OverlapSphere(center, AreaOfEffectDistance, layers, interaction);
        foreach (var coll in affectedColliders)
        {
            Damageable damageable = coll.GetComponent<Damageable>();
            if (damageable)
            {
                Health health = damageable.GetComponentInParent<Health>();
                if (health && !uniqueDamagedHealths.ContainsKey(health))
                {
                    uniqueDamagedHealths.Add(health, damageable);
                }
            }
        }

        // Apply damages with distance falloff
        foreach (Damageable uniqueDamageable in uniqueDamagedHealths.Values)
        {
            float distance = Vector3.Distance(uniqueDamageable.transform.position, transform.position);
            float distanceOnPlane = Vector3.Distance(
                new Vector3(uniqueDamageable.transform.position.x, 0f, uniqueDamageable.transform.position.z),
                new Vector3(center.x, 0f, center.z));
            uniqueDamageable.InflictDamage(
                damage * DamageRatioOverDistance.Evaluate(distance / AreaOfEffectDistance), owner);

            // push each damageable
            if (uniqueDamageable.CompareTag("Enemy"))
            {
                uniqueDamageable.gameObject.GetComponent<EnemyHitCounter>().BeingHit();
                uniqueDamageable.gameObject.GetComponentInChildren<Rigidbody>().AddExplosionForce(PushForce, center, AreaOfEffectDistance, EnemyUpwardModifier, ForceMode.Impulse);
            }

            if (uniqueDamageable.CompareTag("Player"))
            {
                Debug.Log("Hit Player");
                Vector3 PushDirection = (uniqueDamageable.transform.position - center).normalized;
                Vector3 PushVector = ((PushDirection * PushForce) + (Vector3.up * UpwardModifier))
                    * PushForceRatioOverDistance.Evaluate(distanceOnPlane / AreaOfEffectDistance);
                uniqueDamageable.gameObject.GetComponent<FirstPersonController>().ExplosionKnockBack(PushVector);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = AreaOfEffectColor;
        Gizmos.DrawSphere(transform.position, AreaOfEffectDistance);
    }

}

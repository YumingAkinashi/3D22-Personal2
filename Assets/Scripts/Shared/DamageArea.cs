using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class DamageArea : MonoBehaviour
{

    [Tooltip("�ˮ`�̤j�d��Z��")]
    public float AreaOfEffectDistance = 8f;

    [Tooltip("�ˮ`�Z������")]
    public AnimationCurve DamageRatioOverDistance;

    [Tooltip("�ˮ`���O")]
    public float PushForce = 20f;

    [Tooltip("���O�V�W����")]
    public float UpwardModifier = 3f;

    [Tooltip("�ĤH���O�V�W����")]
    public float EnemyUpwardModifier = 6f;

    [Tooltip("���O�Z������")]
    public AnimationCurve PushForceRatioOverDistance;

    [Header("Debug")]
    [Tooltip("�ˮ`�d��Gizmos�C��")]
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

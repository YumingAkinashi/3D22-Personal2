using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStandard : ProjectileBase
{

    [Header("General")]
    [Tooltip("�g���������P�w�d��")]
    public float Radius = 1f;

    [Tooltip("�g�������ڦ�m�]���F��ǧP�_�I���^")]
    public Transform Root;

    [Tooltip("�g�������y�ݦ�m�]���F��ǧP�_�I���^")]
    public Transform Tip;

    [Tooltip("�g�����̤j�s�b�ɶ�")]
    public float MaxLifeTime = 5f;

    [Tooltip("�����S��")]
    public GameObject ImpactVfx;

    [Tooltip("�����S�Ħs�b�ɶ�")]
    public float ImpactVfxLifetime = 5f;

    [Tooltip("�u�����������k�V�q���W�������q")]
    public float ImpactVfxSpawnOffset = 0.1f;

    [Tooltip("�g�����i�I���ؼ�Layers")]
    public LayerMask HittableLayers = -1;

    [Tooltip("�g�����i�v�T�ؼ�Layers")]
    public LayerMask AffectableLayers = -1;

    [Header("Movement")]
    [Tooltip("�g�����t��")]
    public float Speed = 35f;

    [Tooltip("�g�����_�B�t��")]
    public float InitialSpeed = 30f;

    [Tooltip("�_�B���q���[�t��")]
    public float AccelerateModifier = 0.05f;

    [Tooltip("�[�t���q���[�t��")]
    public float SpeedUpModifier = 0.1f;

    [Tooltip(
        "�y�D�ץ��Z���]���g�����g�X�᩹�g�X�ɪ��Ǥߤ��ߡ��۾����߾a��A�Y�p��0�N��L�ץ��^")]
    public float TrajectoryCorrectionDistance = 10f;

    [Header("Damage")]
    [Tooltip("�g�����ˮ`")]
    public float Damage = 40f;

    [Tooltip("�϶ˮ`�����d��ˮ`")]
    public DamageArea AreaOfDamage;

    [Header("Debug")]
    [Tooltip("�ˮ`�d��Gizmos�C��")]
    public Color RadiusColor = Color.cyan * 0.2f;

    ProjectileBase m_ProjectileBase; // ���ζˮ`�ӷ�
    Vector3 m_LastRootPosition; // �W�@�V�g�������ڦ�m
    Vector3 m_InitialVelocity; // �g�����_�B���q�ؼгt��
    Vector3 m_FinalVelocity; // �g�����׳t��
    Vector3 m_CurrVelocity; // �g�����ثe�t��
    Vector3 m_InitialVelocityDelta; // �g�����_�B���q�[�t��
    Vector3 m_VelocityDelta; // �g�����[�t���q�[�t��
    bool m_IsInAccelaratePart; // �g�����O�_�b�[�t���q
    bool m_HasTrajectoryOverride; // �O�_�ץ��g�����y�D
    float m_ShootTime; // �g���ɶ�
    Vector3 m_TrajectoryCorrectionVector; // �g�����y�D�ץ��V�q
    Vector3 m_ConsumedTrajectoryCorrectionVector; // �g�����w�ץ��V�q

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide; // �d�߮g�u�O�_�R��Ĳ�o��

    private void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        AreaOfDamage = GetComponent<DamageArea>();
        m_ProjectileBase.OnShoot += OnShoot;
        m_HasTrajectoryOverride = true;

        Destroy(gameObject, MaxLifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        // Move
        if (m_IsInAccelaratePart)
        {
            if(m_CurrVelocity.magnitude < Speed)
            {
                m_CurrVelocity = m_VelocityDelta;
                transform.position += m_CurrVelocity * Time.deltaTime;
                m_VelocityDelta = Vector3.Lerp(m_VelocityDelta, m_FinalVelocity, SpeedUpModifier);
            }
            else
            {
                m_CurrVelocity = transform.forward * Speed;
                transform.position += m_CurrVelocity * Time.deltaTime;
            }
        }

        // �Ʋ��g�����ܿù�����
        if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude <
            m_TrajectoryCorrectionVector.sqrMagnitude)
        {
            Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
            float distanceThisFrame = (Root.position - m_LastRootPosition).magnitude;
            Vector3 correctionThisFrame =
                (distanceThisFrame / TrajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
            m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

            // Detect end of correction
            if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                m_HasTrajectoryOverride = false;
            }

            transform.position += correctionThisFrame;
        }

        // Hit detection
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = Tip.position - m_LastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, Radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
                k_TriggerInteraction);

            foreach (var hit in hits)
            {
                if (hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // Handle case of casting while already inside a collider
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = Root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }
        }

        m_LastRootPosition = Root.position;
    }

    new void OnShoot()
    {
        m_ShootTime = Time.time;
        m_LastRootPosition = Root.position;
        m_CurrVelocity = Vector3.zero;
        m_InitialVelocityDelta = Vector3.zero;
        m_VelocityDelta = Vector3.zero;
        m_InitialVelocity = transform.forward * InitialSpeed * 0.3f;
        m_FinalVelocity = transform.forward * Speed;

        m_IsInAccelaratePart = false;
        StartCoroutine("ModifiedProjectileMovement");

        // Handle case of player shooting (make projectiles not go through walls, and remember center-of-screen trajectory)
        PlayerWeaponManager playerWeaponManager = m_ProjectileBase.Owner.GetComponent<PlayerWeaponManager>();
        if (playerWeaponManager)
        {
            m_HasTrajectoryOverride = true;

            Vector3 cameraToMuzzle = (m_ProjectileBase.InitialPosition -
                                      playerWeaponManager.vCamera.transform.position);

            m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                playerWeaponManager.vCamera.transform.forward);

            if (TrajectoryCorrectionDistance == 0)
            {
                transform.position += m_TrajectoryCorrectionVector;
                m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
            }
            else if (TrajectoryCorrectionDistance < 0)
            {
                m_HasTrajectoryOverride = false;
            }
        }
    }

    public void OnHit(Vector3 point, Vector3 normal, Collider collider)
    {

        // area damage
        AreaOfDamage.InflictDamageInArea(Damage, point, normal, AffectableLayers, k_TriggerInteraction,
            m_ProjectileBase.Owner);

        // impact vfx
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, point + (normal * ImpactVfxSpawnOffset),
                Quaternion.LookRotation(normal));

            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }

        Destroy(gameObject);
    }

    public IEnumerator ModifiedProjectileMovement()
    {
        float t = 0f;
        float initialStateDuration = .3f;
        
        while(t < initialStateDuration)
        {
            m_CurrVelocity = m_InitialVelocityDelta;
            transform.position += m_CurrVelocity * Time.deltaTime;
            m_InitialVelocityDelta = Vector3.Lerp(m_InitialVelocityDelta, m_InitialVelocity, AccelerateModifier);

            t += Time.deltaTime;

            yield return null;
        }

        m_CurrVelocity = m_InitialVelocity;
        m_IsInAccelaratePart = true;
    }
}

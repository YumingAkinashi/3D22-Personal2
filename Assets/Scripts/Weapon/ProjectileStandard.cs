using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStandard : ProjectileBase
{

    [Header("General")]
    [Tooltip("射擊物擊中判定範圍")]
    public float Radius = 1f;

    [Tooltip("射擊物的根位置（為了精準判斷碰撞）")]
    public Transform Root;

    [Tooltip("射擊物的尖端位置（為了精準判斷碰撞）")]
    public Transform Tip;

    [Tooltip("射擊物最大存在時間")]
    public float MaxLifeTime = 5f;

    [Tooltip("擊中特效")]
    public GameObject ImpactVfx;

    [Tooltip("擊中特效存在時間")]
    public float ImpactVfxLifetime = 5f;

    [Tooltip("沿著擊中平面法向量往上的偏移量")]
    public float ImpactVfxSpawnOffset = 0.1f;

    [Tooltip("射擊物可碰撞目標Layers")]
    public LayerMask HittableLayers = -1;

    [Tooltip("射擊物可影響目標Layers")]
    public LayerMask AffectableLayers = -1;

    [Header("Movement")]
    [Tooltip("射擊物速度")]
    public float Speed = 35f;

    [Tooltip("射擊物起步速度")]
    public float InitialSpeed = 30f;

    [Tooltip("起步階段的加速度")]
    public float AccelerateModifier = 0.05f;

    [Tooltip("加速階段的加速度")]
    public float SpeedUpModifier = 0.1f;

    [Tooltip(
        "軌道修正距離（讓射擊物射出後往射出時的準心中心／相機中心靠近，若小於0代表無修正）")]
    public float TrajectoryCorrectionDistance = 10f;

    [Header("Damage")]
    [Tooltip("射擊物傷害")]
    public float Damage = 40f;

    [Tooltip("使傷害成為範圍傷害")]
    public DamageArea AreaOfDamage;

    [Header("Debug")]
    [Tooltip("傷害範圍Gizmos顏色")]
    public Color RadiusColor = Color.cyan * 0.2f;

    ProjectileBase m_ProjectileBase; // 取用傷害來源
    Vector3 m_LastRootPosition; // 上一幀射擊物的根位置
    Vector3 m_InitialVelocity; // 射擊物起步階段目標速度
    Vector3 m_FinalVelocity; // 射擊物終速度
    Vector3 m_CurrVelocity; // 射擊物目前速度
    Vector3 m_InitialVelocityDelta; // 射擊物起步階段加速度
    Vector3 m_VelocityDelta; // 射擊物加速階段加速度
    bool m_IsInAccelaratePart; // 射擊物是否在加速階段
    bool m_HasTrajectoryOverride; // 是否修正射擊物軌道
    float m_ShootTime; // 射擊時間
    Vector3 m_TrajectoryCorrectionVector; // 射擊物軌道修正向量
    Vector3 m_ConsumedTrajectoryCorrectionVector; // 射擊物已修正向量

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide; // 查詢射線是否命中觸發器

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

        // 飄移射擊物至螢幕中心
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

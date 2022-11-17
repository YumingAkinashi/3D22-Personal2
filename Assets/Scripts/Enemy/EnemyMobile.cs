using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMobile : MonoBehaviour
{

    public float AgentSpeed = 3f;

    [Header("If Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Enemy Random Move")]
    [Tooltip("®Ì°Ê¶ZÂ÷")]
    public float DefaultMaxWalkAmount = 2f;

    public float RandomWalkCooldown = 2f;

    float minX = 0f;
    float minY = 0f;
    float maxX = 500f;
    float maxY = 500f;

    float RandomWalkDuration;
    float LastWalk;
    Vector3 _desiredDirection;
    bool _wasWalkingDone;
    bool _isWalking;
    bool _moveInvalid;

    Rigidbody rb;
    Animator anim;

    //[Header("Hit By Explosion")]

    public float PushResist = .3f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        DenyMovingOutOfBoarder();
        RandomDodgeAndMoving();
        transform.rotation = Quaternion.LookRotation(_desiredDirection);
        AnimationCheck();
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void AnimationCheck()
    {
        if (Grounded)
            anim.SetBool("Grounded", true);
        else
            anim.SetBool("Grounded", false);

        if (!_wasWalkingDone)
            anim.SetTrigger("Running");
        else
            anim.SetTrigger("Stop");

    }

    private void RandomDodgeAndMoving()
    {

        if (!Grounded)
        {
            LastWalk = Time.time;
            return;
        }

        if (!_isWalking)
        {
            float RandomXDistance = Random.Range(-DefaultMaxWalkAmount, DefaultMaxWalkAmount);
            float RandomZDistance = Random.Range(-DefaultMaxWalkAmount, DefaultMaxWalkAmount);
            RandomWalkDuration = Random.Range(1f, 2f);

            _desiredDirection = new Vector3(RandomXDistance, 0f, RandomZDistance);
            _desiredDirection = _desiredDirection.normalized;

            if (_desiredDirection != Vector3.zero)
            {
                _isWalking = true;
                _wasWalkingDone = false;
            }

        }
        else
        {
            if (!_wasWalkingDone)
            {

                if (RandomWalkDuration > 0)
                {
                    if(!_moveInvalid)
                        transform.position += _desiredDirection * AgentSpeed * Time.deltaTime;

                    RandomWalkDuration -= Time.deltaTime;
                }
                else
                {
                    LastWalk = Time.time;
                    _wasWalkingDone = true;
                }
            }
            else if (_wasWalkingDone)
            {
                if (Time.time - LastWalk > RandomWalkCooldown)
                {
                    _isWalking = false;
                }
                else
                    return;
            }
        }
    }

    void DenyMovingOutOfBoarder()
    {
        Vector3 nextMove = transform.position + transform.forward * AgentSpeed * Time.deltaTime;

        if (nextMove.x > maxX || nextMove.x < minX || nextMove.y > maxY || nextMove.y < minY)
            _moveInvalid = true;
        else
            _moveInvalid = false;
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);

    }
}

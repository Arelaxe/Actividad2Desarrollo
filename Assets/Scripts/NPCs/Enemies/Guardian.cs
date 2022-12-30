using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guardian : Enemy
{
    [SerializeField] private float distancing;
    [SerializeField] private float attackRate;
    private bool attacking;
    private float nextAttack;

    [Header("Chasing")]
    [SerializeField] private float maxChasingRange;
    [SerializeField] private float guardingPoint;
    private GameObject target;

    protected override void Start()
    {
        base.Start();
        guardingPoint = transform.position.x;
    }

    protected override void Update()
    {
        VisionCone();
        DetectTarget();
        RoutineMovement();
        ChaseTarget();
        CheckCollisions();
    }

    protected override void AfterTakeHit(GameObject source)
    {
        target = source;
        continueRoutine = false;
    }

    protected override void AfterDeath(){}

    protected override bool ShouldStop()
    {
        return target == null && continueRoutine == true;
    }

    protected void DetectTarget()
    {
        if (target == null)
        {
            RaycastHit2D player = CollisionUtils.FindFirst(visionHits, "Player");
            if (player.collider)
            {
                float x = GetDistancedWaypoint(player.collider.gameObject);
                if (IsWaypointInRange(x))
                {
                    target = player.collider.gameObject;
                    continueRoutine = false;
                }
            }
        }
    }

    protected void ChaseTarget()
    {
        if (target != null && !continueRoutine && IsAlive())
        {
            float x = GetDistancedWaypoint(target);

            if (IsWaypointInRange(x))
            {
                Vector2 targetPosition = new(x, target.transform.position.y);

                if (IsWaypointReached(targetPosition))
                {
                    Attack();
                }
                else
                {
                    StartCoroutine(Chase(targetPosition));
                }
            }
            else
            {
                target = null;
                continueRoutine = true;
            }
        }
    }

    protected void Attack()
    {
        Vector3 localScale = transform.localScale;
        if (transform.position.x < target.transform.position.x && Mathf.Sign(localScale.x) == -1
            || transform.position.x > target.transform.position.x && Mathf.Sign(localScale.x) == 1)
        {
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        animator.SetBool("Walking", false);

        if (Time.time > nextAttack)
        {
            nextAttack = Time.time + attackRate;

            animator.SetTrigger("Attack");
            mainAudioSource.PlayOneShot(attackSound);

            attacking = true;
        }
    }

    protected IEnumerator Chase(Vector2 targetPosition)
    {
        if (attacking)
        {
            yield return StartCoroutine(WaitForAnimCycle("Attack"));
            attacking = false;
        }
        else
        {
            MoveToWaypoint(targetPosition);
        }
    }

    // Auxiliar methods

    private float GetDistancedWaypoint(GameObject availableTarget)
    {
        float x = availableTarget.transform.position.x;
        if (transform.position.x > x)
        {
            x += distancing;
        }
        else if (transform.position.x < x)
        {
            x -= distancing;
        }

        return x;
    }

    private bool IsWaypointInRange(float waypointX)
    {
        return waypointX >= guardingPoint - maxChasingRange && waypointX <= guardingPoint + maxChasingRange;
    }

}

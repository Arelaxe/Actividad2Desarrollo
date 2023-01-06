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
    [SerializeField] private float chasingRangeLeft;
    [SerializeField] private float chasingRangeRight;
    [SerializeField] private float guardingPoint;
    private GameObject target;

    protected override void Update()
    {
        CastVisionCone();
        DetectTarget();
        RoutineMovement();
        ChaseTarget();
        CheckCollisions();
    }

    protected override void AfterTakeHit(GameObject source)
    {
        // Set target and chase
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
                // Set target only when attack position it's in range
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
                // Ignore player if it gets out of range
                target = null;
                continueRoutine = true;
            }
        }
    }

    protected void Attack()
    {
        CheckFlip(target.transform.position);

        animator.SetBool("Walking", false);

        // Only attack if it's available next attack
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
            // Disable attack state when animation ends
            yield return StartCoroutine(WaitForAnimCycle("Attack"));
            attacking = false;
        }
        else
        {
            MoveToWaypoint(targetPosition);
            // Idle animation when colliding with scenery while chasing
            if (CollisionUtils.Count(movementHits, "Ground") > 0)
            {
                animator.SetBool("Walking", false);
            }
        }
    }

    // Auxiliar methods

    /// <summary>
    /// Get waypoint to target keeping distance for attack
    /// </summary>
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

    /// <summary>
    /// Check if the waypoint isn't out of the chasing distance according to the guarding point
    /// </summary>
    private bool IsWaypointInRange(float waypointX)
    {
        return waypointX >= guardingPoint - chasingRangeLeft && waypointX <= guardingPoint + chasingRangeRight;
    }

}

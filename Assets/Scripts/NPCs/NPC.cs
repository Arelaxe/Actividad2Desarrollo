using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class NPC : MonoBehaviour
{
    [Header("General")]
    [SerializeField] protected float health;

    [Header("Routine")]
    [SerializeField] protected Vector2[] waypoints;
    protected int current = 0;
    protected float waypointRadius = 0.1f;
    protected bool continueRoutine = true;
    protected bool takingHit;

    [SerializeField] protected float speed;
    [SerializeField] protected float stopTime;
    [SerializeField] protected bool flip;

    [Header("Vision")]
    [SerializeField] private Vector2 visionConeOriginOffset;
    [SerializeField] private float visionConeAngle;
    [SerializeField] private float visionConeDistance;
    protected RaycastHit2D[] visionHits;

    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        RoutineMovement();
        VisionCone();
    }

    protected void RoutineMovement()
    {
        if (IsWaypointReached(waypoints[current]))
        {
            current++;
            if (current >= waypoints.Length)
            {
                current = 0;
            }

            if (ShouldStop())
            {
                StartCoroutine(Stop());
            }
        }

        if (continueRoutine)
        {
            MoveToWaypoint(waypoints[current]);
        }

    }

    protected abstract bool ShouldStop();

    protected bool IsWaypointReached(Vector2 waypoint)
    {
        return Vector2.Distance(waypoint, transform.position) < waypointRadius;
    }

    protected void MoveToWaypoint(Vector2 waypoint)
    {
        if (!takingHit)
        {
            if (flip)
            {
                CheckFlip(waypoint);
            }
            transform.position = Vector2.MoveTowards(transform.position, waypoint, Time.deltaTime * speed);

            animator.SetBool("Walking", true);
        }
    }

    protected void CheckFlip(Vector2 waypoint)
    {
        Vector3 localScale = transform.localScale;
        if ((transform.position.x > waypoint.x && Mathf.Sign(localScale.x) == 1) || (transform.position.x < waypoint.x && Mathf.Sign(localScale.x) == -1))
        {
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    protected IEnumerator Stop()
    {
        continueRoutine = false;
        animator.SetBool("Walking", false);
        yield return new WaitForSeconds(stopTime);

        if (health > 0)
        {
            continueRoutine = true;
        }
    }

    public IEnumerator TakeHit(GameObject source, float damage)
    {
        if (health > 0)
        {
            health -= damage;

            animator.SetTrigger("Take Hit");

            yield return StartCoroutine(WaitForAnimStart("TakeHit"));
            takingHit = true; 

            yield return StartCoroutine(WaitForAnimEnd());
            takingHit = false;

            if (health > 0)
            {
                AfterTakeHit(source);
            }
            else
            {
                animator.SetTrigger("Die");
                yield return StartCoroutine(WaitForAnimStart("Death"));
                GetComponent<Collider2D>().isTrigger = true;

                AfterDeath();
            }
        }
    }

    protected abstract void AfterTakeHit(GameObject source);
    protected abstract void AfterDeath();

    protected void VisionCone()
    {
        Vector2 direction = Mathf.Sign(transform.localScale.x) == -1 ? Vector2.left : Vector2.right;
        float originOffsetX = Mathf.Sign(transform.localScale.x) == -1 ? visionConeOriginOffset.x * -1 : visionConeOriginOffset.x;
        Vector3 origin = new(transform.position.x + originOffsetX, transform.position.y + visionConeOriginOffset.y, transform.position.z);

        visionHits = CollisionUtils.RaycastArc(10, visionConeAngle, origin, 0, direction, visionConeDistance, LayerMask.GetMask("Foreground"));
    }

    public IEnumerator WaitForAnimStart(string name)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + name))
        {
            yield return null;
        }
    }

    public IEnumerator WaitForAnimEnd()
    {
        float counter = 0;
        float waitTime = animator.GetCurrentAnimatorStateInfo(0).length;

        while (counter < (waitTime))
        {
            counter += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator WaitForAnimCycle(string name)
    {
        yield return StartCoroutine(WaitForAnimStart(name));
        yield return StartCoroutine(WaitForAnimEnd());
    }

}

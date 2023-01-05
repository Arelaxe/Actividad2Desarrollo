using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class NPC : MonoBehaviour
{
    [Header("General")]
    [SerializeField] protected float health;
    protected bool takingHit;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    protected BoxCollider2D bc;
    protected Rigidbody2D rb;
    protected AudioSource mainAudioSource;
    protected Animator animator;

    [Header("Vision")]
    [SerializeField] private Vector2 visionConeOriginOffset;
    [SerializeField] private float visionConeAngle;
    [SerializeField] private float visionConeDistance;
    protected RaycastHit2D[] visionHits;

    [Header("Routine")]
    [SerializeField] protected Vector2[] waypoints;
    protected int current = 0;
    [SerializeField] protected float waypointRadius = 1f;
    protected bool continueRoutine = true;
    [SerializeField] protected float speed;
    [SerializeField] protected float stopTime;
    [SerializeField] protected bool flip;
    [SerializeField] protected bool canFly;
    protected Vector2 movement;
    protected RaycastHit2D[] movementHits;
    protected bool reached;

    protected virtual void Start()
    {
        mainAudioSource = Camera.main.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        CastVisionCone();
        RoutineMovement();
        CheckCollisions();
    }

    // General

    public IEnumerator TakeHit(GameObject source, float damage)
    {
        if (IsAlive() && !takingHit)
        {
            health -= damage;

            mainAudioSource.PlayOneShot(hitSound);
            animator.SetTrigger("Take Hit");

            // Set taking hit state to avoid movement and take more hits while animation is playing
            yield return StartCoroutine(WaitForAnimStart("TakeHit"));
            takingHit = true;

            yield return StartCoroutine(WaitForAnimEnd());
            takingHit = false;

            if (IsAlive())
            {
                AfterTakeHit(source);
            }
            else
            {
                continueRoutine = false;
                animator.SetTrigger("Die");

                // Set layer to Player Ignore to avoid collisions
                gameObject.layer = LayerMask.NameToLayer("Player Ignore");

                yield return StartCoroutine(WaitForAnimStart("Death"));
                mainAudioSource.PlayOneShot(deathSound);

                AfterDeath();
            }
        }
    }

    protected abstract void AfterTakeHit(GameObject source);
    protected abstract void AfterDeath();

    // Vision

    /// <summary>
    /// Create group of RayCasts which detects Player
    /// </summary>
    protected void CastVisionCone()
    {
        Vector2 direction = Mathf.Sign(transform.localScale.x) == -1 ? Vector2.left : Vector2.right;
        float originOffsetX = Mathf.Sign(transform.localScale.x) == -1 ? visionConeOriginOffset.x * -1 : visionConeOriginOffset.x;
        Vector3 origin = new(transform.position.x + originOffsetX, transform.position.y + visionConeOriginOffset.y, transform.position.z);

        visionHits = CollisionUtils.RaycastArc(11, visionConeAngle, origin, direction, visionConeDistance, LayerMask.GetMask("Default", "Player"));
    }

    // Routine

    protected void RoutineMovement()
    {
        if (IsWaypointReached(waypoints[current]))
        {
            NextWaypoint();
        }

        if (continueRoutine)
        {
            MoveToWaypoint(waypoints[current]);
        }

    }

    protected abstract bool ShouldStop();

    protected bool IsWaypointReached(Vector2 waypoint)
    {
        // If NPC can't fly, ignore y coordinate of waypoint
        float waypointY = canFly ? waypoint.y : transform.position.y;
        return Vector2.Distance(new Vector2(waypoint.x, waypointY), transform.position) < waypointRadius;
    }

    protected void NextWaypoint()
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

    protected void MoveToWaypoint(Vector2 waypoint)
    {
        if (takingHit)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            if (flip)
            {
                CheckFlip(waypoint);
            }

            // If NPC can't fly, ignore y coordinate of waypoint
            float movementY = canFly ? (waypoint.y - transform.position.y) : 0;
            movement = new(waypoint.x - transform.position.x, movementY);
            rb.velocity = movement.normalized * speed;
            
            animator.SetBool("Walking", true);
        }
    }

    /// <summary>
    /// Rotate sprite according to waypoint if needed
    /// </summary>
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

        if (IsAlive())
        {
            continueRoutine = true;
        }
    }

    // Collisions
    protected void CheckCollisions()
    {
        // Create group of RayCasts according to movement
        // When colliding with scenery move to next waypoint
        movementHits = CollisionUtils.RaycastMovement(bc.bounds, movement, 10, 0.1f, LayerMask.GetMask("Default"));
        if (CollisionUtils.Count(movementHits, "Ground") > 0 && !reached)
        {
            reached = true;
            NextWaypoint();
        }
        else if (CollisionUtils.Count(movementHits, "Ground") == 0 && reached)
        {
            reached = false;
        }

    }

    // Auxiliar methods

    protected bool IsAlive()
    {
        return health > 0;
    }

    // Animation methods

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

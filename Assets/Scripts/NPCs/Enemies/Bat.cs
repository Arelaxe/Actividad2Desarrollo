using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    [Header("Stress")]
    [SerializeField] private float speedMultiplier;

    private GameObject target;

    protected override void Update()
    {
        base.Update();

        DetectTarget();
        // Follow target position if it doesn't flip while moving
        if (target != null && IsAlive() && !flip)
        {
            CheckFlip(target.transform.position);
        }
    }

    protected override void AfterTakeHit(GameObject source)
    {
        // Increase movement speed
        speed *= speedMultiplier;
    }

    protected override void AfterDeath() 
    {
        // Move to ground
        rb.velocity = Vector2.down * 5;
    }

    protected void DetectTarget()
    {
        if (target == null)
        {
            RaycastHit2D player = CollisionUtils.FindFirst(visionHits, "Player");
            if (player.collider)
            {
                target = player.collider.gameObject;
            }
        }
    }

    // Collisions

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "Player")
        {
            collision.gameObject.GetComponent<PlayerController>().Hit();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.name == "Player")
        {
            collision.gameObject.GetComponent<PlayerController>().Hit();
        }
    }

}

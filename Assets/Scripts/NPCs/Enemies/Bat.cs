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
        if (target != null && IsAlive())
        {
            CheckFlip(target.transform.position);
        }
    }

    protected override void AfterTakeHit(GameObject source)
    {
        speed *= speedMultiplier;
    }

    protected override void AfterDeath() 
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        rb.velocity = Vector2.down * 2;
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

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }
}

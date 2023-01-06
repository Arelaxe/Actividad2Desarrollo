using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (enemy.IsDamageFramePlaying())
        {
            if (collision.name == "Player")
            {
                collision.gameObject.GetComponent<PlayerController>().Hit();
            }
        }
    }

}

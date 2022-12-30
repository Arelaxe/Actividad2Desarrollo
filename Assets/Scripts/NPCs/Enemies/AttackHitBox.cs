using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    private Guardian enemy;

    void Start()
    {
        enemy = GetComponentInParent<Guardian>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (enemy.IsDamageFramePlaying())
        {
            if (collision.name == "Player")
            {
                Debug.Log("Player take hit function");
            }
        }
    }

}

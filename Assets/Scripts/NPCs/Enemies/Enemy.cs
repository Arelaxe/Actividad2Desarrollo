using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : NPC
{
    [Header("Combat")]
    [SerializeField] private float damage;
    [SerializeField] private bool damageFramePlaying;
    [SerializeField] protected AudioClip attackSound;

    protected override bool ShouldStop()
    {
        return true;
    }

    public float GetDamage()
    {
        return damage;
    }

    public bool IsDamageFramePlaying()
    {
        return damageFramePlaying;
    }

}

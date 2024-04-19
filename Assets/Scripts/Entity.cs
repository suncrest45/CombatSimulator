using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public float MaxHitPoints = 200;
    [HideInInspector]
    public float HitPoints = 200; // Current hit points

    public virtual void DoMovement() { }
    public virtual Entity FindTarget() { return null; }

    public virtual bool UseRandomAbility () { return false; }

    public virtual bool UseAbility(int abilityNumber) { return false; }

    public virtual void Initialize() { }

    public virtual bool TakeDamage(float damage) { return false; }  

}

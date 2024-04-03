using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------
    // Member Variables
    //------------------------------------------------------------------------------------------------------------------------------------

    // Name of the entity
    public string EntityName = string.Empty;
    // Max Health of the Entity
    public float MaxHitPoints = 200.0f;
    // How quickly the entity moves
    public float MoveSpeed = 0.1f;
    // The optimal distance the AI will try to maintain from targets
    public float OptimalRange = 5.0f;
    // Stun timer for an entity
    public float StunTimer = 0.0f;

    // Damage taken during a round
    private float DamageTaken = 0.0f;
    // Damage dealt during a round
    private float DamageDealt = 0.0f;

    // Current hit points
    private float HitPoints = 200;

    // Current Target
    private Entity Target;         

    // Reference to the healthbar
    private BarScaler HealthBar;

    // An array of abilities for this entity
    public Ability[] Abilities;


    // Moves the entity
    public virtual void DoMovement() { }

    // Initialises the entity
    public virtual void Initialise() { }

    // Finds the entities target
    public virtual Entity FindTarget() { return null; }

    // Use a randomly chosen ability
    public bool UseRandomAbility() { return UseAbility(Random.Range(0, Abilities.Length)); }

    // Use a specific ability
    public bool UseAbility(int abilityNumber)
    {
        if (abilityNumber < Abilities.Length)
        {
            if (Abilities[abilityNumber] != null)
            {
                return Abilities[abilityNumber].Use();
            }
        }

        return false;
    }
}

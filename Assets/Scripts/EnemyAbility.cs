/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component makes a game object an enemy ability. The game object then must
    be parented to the actual enemy game object in order to work. Should this really
    be a different class than the hero ability? It doesn duplicate some functionality,
    but often hero and enemy abilities end up subtly different, so this can be okay to do.
	
*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class EnemyAbility : MonoBehaviour
{
    // Properties that define the ability's cooldown time, damage done, range, etc.
    public float CooldownTime = 1.0f;
    public float DamageDone = 1.0f;
    public float MaximumRange = 10.0f;
    public float StunTimer = 0.0f;
    // This ability will steal the player's rizz
    public bool RizzSteal = false;
    // This ability will stun
    public bool Stuns = false;
    // This ability will summon cheap copies of enemies
    public bool summoner = false;
    // Make an ability inactive to temporarily or permanently not have it used.
    public bool Inactive = false;
    // The amount of rizz to steal
    public float RizzToSteal = 0.0f;
    // Catchy One Liner
    public string CatchyOneLiner = string.Empty;

    // How much of the cooldown time is actually left.
    [HideInInspector]
    public float CooldownLeft = 0.0f;

    // Reference to the cooldown timer bar, so we don't have to look it up all the time.
    [HideInInspector]
    public BarScaler CooldownBar;

    // Reference to the parent enemy, so we don't have to look it up all the time.
    [HideInInspector]
    public Enemy ParentEnemy; 

    // Start is called before the first frame update
    void Start()
    {
        // Get the parent.
        ParentEnemy = transform.parent.GetComponent<Enemy>();
        // Find the cooldown timer gameobject, which must be a child of this object.
        CooldownBar = transform.Find("Cooldown").GetComponent<BarScaler>();
    }

    //Update is called once per frame
    void Update()
    {
        // Don't let the cooldown amount left go below zero.
        CooldownLeft = Mathf.Clamp(CooldownLeft - SimControl.DT, 0.0f, CooldownTime);
        // Since cooldowns update every frame, no need to worry about interpolating over time.
        // Either doesn't have a cooldown or is inactive, so scale it to nothing.
        if (Inactive || CooldownTime == 0.0f) 
            CooldownBar.InterpolateToScale(0.0f, 0.0f);
        else
            CooldownBar.InterpolateToScale(CooldownLeft / CooldownTime, 0.0f);
    }

    // Don't let a cooldown affect the next fight
    public void ResetCooldown()
    {
        CooldownLeft = CooldownTime;
    }

    // Get the distance to the target along the X axis (1D not 2D).
    public float DistanceToTarget()
    {
        return Mathf.Abs(ParentEnemy.transform.position.x - ParentEnemy.Target.transform.position.x);
    }

    // Is an ability ready for use?
    public bool IsReady()
    {
        // It's inactive.
        if (Inactive)
            return false;
        // I'm dead.
        if (ParentEnemy.HitPoints == 0.0f)
            return false;
        // No target.
        if (ParentEnemy.Target == null)
            return false;
        // Dead target.
        if (ParentEnemy.Target.HitPoints == 0.0f)
            return false;
        // Target too far away.
        if (DistanceToTarget() > MaximumRange)
            return false;
        // Still on cooldown.
        if (CooldownLeft > 0.0f)
            return false;
        if (ParentEnemy.StunTimer > 0.0f)
            return false;
        // Ready to go.
        return true;
    }

    // Use the ability if it is ready.
    public bool Use()
    {
        // Is it ready?
        if (IsReady() == false)
            return false;

        // Apply the damage (or healing is the damage is negative).
        if (ParentEnemy.Target.TakeDamage(DamageDone) == true)
            ParentEnemy.Target = null; //If the target is dead, set it to null.

        // TODO: Add needed flags or other functionality for abilities that don't just do
        // damage or affect more than one target (AoE, heals, dodges, blocks, stuns, etc.)

        // Steals the player's rizz.
        if (RizzSteal)
        {
            ParentEnemy.Target.RizzStolen(RizzToSteal);
        }

        // Summons a cheap copy of an enemy
        if (summoner)
        {
            ParentEnemy.Target.Target = ParentEnemy;
            Instantiate(SimControl.EnemyTypePrefabs[Random.Range(6,12)], new Vector3(SimControl.StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
        }

        if (Stuns)
        {
            ParentEnemy.Target.StunTimer = StunTimer;
        }

        Text OneLiner = Object.Instantiate(SimControl.InfoTextPrefab, ParentEnemy.transform.position, Quaternion.identity, SimControl.Canvas.transform).GetComponent<Text>();
        OneLiner.text = CatchyOneLiner;

        // Put the ability on cooldown.
        CooldownLeft = CooldownTime;
        return true;
    }

}

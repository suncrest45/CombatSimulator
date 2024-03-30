/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component makes a game object a hero ability. The game object then must
    be parented to the actual hero game object in order to work. Should this really
    be a different class than the enemy ability? It doesn duplicate some functionality,
    but often hero and enemy abilities end up subtly different, so this can be okay to do.
	
*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class HeroAbility : MonoBehaviour
{
    //Properties that define the ability's cooldown time, damage done, power used, range, etc.
    public string AbilityName = string.Empty;
    public float CooldownTime = 1.0f;
    public float DamageDone = 1.0f;
    public float PowerUsed = 1.0f;
    public float MaximumRange = 10.0f;
    public bool Stuns = false;
    public float StunTimer = 0.0f;
    public bool Finisher = false;
    public bool AoE = false;
    public bool Inactive = false; //Make an ability inactive to temporarily or permanently not have it used.
    
  

    [HideInInspector]
    public float CooldownLeft = 0.0f; //How much of the cooldown time is actually left.

    [HideInInspector]
    public BarScaler CooldownBar; //Reference to the cooldown timer bar, so we don't have to look it up all the time.

    [HideInInspector]
    public Text AbilityNumber; //Reference to the ability number text, so we don't have to look it up all the time.

    [HideInInspector]
    public Hero ParentHero; //Reference to the parent hero, so we don't have to look it up all the time.

    //Start is called before the first frame update
    void Start()
    {
        //Get the parent.
        ParentHero = GameObject.Find("Hero").GetComponent<Hero>();
        //Find the cooldown timer gameobject, which must be a child of this object.
        CooldownBar = transform.Find("Cooldown").GetComponent<BarScaler>();
        //Find the ability number gameobject, which must be a child of this object.
        AbilityNumber = transform.Find("AbilityNumber").GetComponent<Text>();
    }

    //Update is called once per frame
    void Update()
    {
        //Don't let the cooldown amount left go below zero.
        CooldownLeft = Mathf.Clamp(CooldownLeft - SimControl.DT, 0.0f, CooldownTime);
        //Since cooldowns update every frame, no need to worry about interpolating over time.
        if (Inactive || CooldownTime == 0.0f) //Either doesn't have a cooldown or is inactive, so scale it to nothing.
            CooldownBar.InterpolateToScale(0.0f, 0.0f);
        else
            CooldownBar.InterpolateToScale(CooldownLeft / CooldownTime, 0.0f);
        if (IsReady())
            AbilityNumber.color = new Color(AbilityNumber.color.r, AbilityNumber.color.g, AbilityNumber.color.b, 1.0f);
        else
            AbilityNumber.color = new Color(AbilityNumber.color.r, AbilityNumber.color.g, AbilityNumber.color.b, 0.2f);
    }

    //Don't let a cooldown affect the next fight
    public void ResetCooldown()
    {
        CooldownLeft = 0.0f;
    }

    //Get the distance to the target along the X axis (1D not 2D).
    public float DistanceToTarget()
    {
        return Mathf.Abs(ParentHero.transform.position.x - ParentHero.Target.transform.position.x);
    }

    // Is an ability ready for use?
    public bool IsReady()
    {
        //It's inactive.
        if (Inactive)
            return false;
        //I'm dead.
        if (ParentHero.HitPoints == 0.0f)
            return false;
        //No target.
        if (ParentHero.Target == null)
            return false;
        //Dead target.
        if (ParentHero.Target.HitPoints == 0.0f)
            return false;
        //Target too far away.
        if (DistanceToTarget() > MaximumRange)
            return false;
        //Still on cooldown.
        if (CooldownLeft > 0.0f)
            return false;
        //Not enough power.
        if (PowerUsed > ParentHero.Power)
            return false;
        if (Finisher && (ParentHero.Target.HitPoints < (0.2 * ParentHero.Target.HitPoints)))
            return false;
        //Ready to go.
        return true;
    }

    //Use the ability if it is ready.
    public bool Use()
    {
        //Is it ready?
        if (IsReady() == false)
            return false;
        //Use the power.
        ParentHero.UsePower(PowerUsed);
        //Apply the damage (or healing is the damage is negative).
        if (ParentHero.Target.TakeDamage(DamageDone) == true)
            ParentHero.Target = ParentHero.FindTarget(); //If the target is dead, find a new one.

        //TODO: Add needed flags or other functionality for abilities that don't just do
        //damage or affect more than one target (AoE, heals, dodges, blocks, stuns, etc.)

        if (Stuns == true)
        {
            ParentHero.Target.StunTimer = StunTimer;
        }

        if (AoE == true)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");

            if (gos.Length == 0)
            {
                return false;
            }

            foreach (GameObject item in gos)
            {
                Enemy enemy = item.GetComponent<Enemy>();
                enemy.TakeDamage(DamageDone);
            }

            Text BoomerText = Object.Instantiate(SimControl.InfoTextPrefab, transform.position, Quaternion.identity, SimControl.Canvas.transform).GetComponent<Text>();
            BoomerText.text = "OK BOOMER!!!";
        }

        FightRecorder.SetAbilityUsage(AbilityName);

        // Put the ability on cooldown.
        CooldownLeft = CooldownTime;
        return true;
    }
}
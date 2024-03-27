/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component makes a game object a hero that can be controlled by the player.
    There is only a single hero that is already placed in the scene.
	
*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class Hero : MonoBehaviour
{
    //Properties for maximum hit points, movement speed, maximum power, and optimal range.
    public float MaxHitPoints = 200;
    public float MoveSpeed = 0.1f;
    public float MaxPower = 100;
    public float OptimalRange = 5.0f;

    [HideInInspector]
    public float HitPoints = 200; //Current hit points
    [HideInInspector]
    public float Power = 40; //Current power

    [HideInInspector]
    public Enemy Target; //Current target enemy.

    //References to the health and power UI bars, so we don't have to look them up all the time.
    [HideInInspector]
    public BarScaler HealthBar;
    [HideInInspector]
    public BarScaler PowerBar;

    //References to the abilities, so we don't have to look them up all the time.
    //These are set by hand in the inspector for the hero game object.
    public HeroAbility[] Abilities;

    //Start is called before the first frame update
    void Start()
    {
        //The static version of Find() on the GameObject class will just find the named object anywhere.
        //Use GetComponent so we don't have to use it later to access the functionality we want.
        HealthBar = GameObject.Find("HeroResources/HealthBar").GetComponent<BarScaler>(); 
        PowerBar = GameObject.Find("HeroResources/PowerBar").GetComponent<BarScaler>();
    }

    //Update is called once per frame
    void Update()
    {
        if (SimControl.RoundOver) //Don't update between rounds (or when the sim is over).
            return;
        if (Target == null) //If we don't have a target, the round must have just started.
            Initialize();
        //The fight is on, so move and use abilities.
        DoMovement();
        if (SimControl.AutoMode == true ||
            SimControl.FastMode == true) //Let an "AI" determine which abilities to use.
        {
            if (SimControl.CurrentAI == "Random")
                UseRandomAbility();
            else if (SimControl.CurrentAI == "Spam1")
                UseAbility(0);
            else if (SimControl.CurrentAI == "Spam2")
                UseAbility(1);
            else if (SimControl.CurrentAI == "Spam3")
                UseAbility(2);
            else if (SimControl.CurrentAI == "Spam4")
                UseAbility(3);
            else if (SimControl.CurrentAI == "Spam5")
                UseAbility(4);
            else if (SimControl.CurrentAI == "Smart")
                SmartAI();

        }
        else //Let the player select which abilities to use.
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) == true)
                UseAbility(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) == true)
                UseAbility(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) == true)
                UseAbility(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) == true)
                UseAbility(3);
            if (Input.GetKeyDown(KeyCode.Alpha5) == true)
                UseAbility(4);
        }

        Target = FindTarget();
    }

    //Try to stay close to optimal range. Note this is done even in Auto mode.
    public void DoMovement()
    {
        if (HitPoints <= 0.0f || Target == null) //If all enemies or the player is dead, no need to move.
            return;
        //Get our current X position.
        float newX = transform.position.x;

        if (SimControl.AutoMode == true ||
            SimControl.FastMode == true) //Let an "AI" determine how to move.
        {
            //Calculate distance to target along the X axis (1D not 2D).
            float distanceToTarget = transform.position.x - Target.transform.position.x;
            //If we are between 80% and 100% of optimal range, that's good enough.
            if (Mathf.Abs(distanceToTarget) <= OptimalRange && Mathf.Abs(distanceToTarget) >= OptimalRange * 0.8f)
                return;
            //If we are too close, flip the "distance" so we will move away instead of towards.
            if (Mathf.Abs(distanceToTarget) < OptimalRange * 0.8f)
                distanceToTarget = -distanceToTarget;
            if (distanceToTarget > 0) //Move to the left.
                newX -= MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
            else //Move to the right.
                newX += MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
        }
        else //Player is in control of movement.
        {
            if (Input.GetKey(KeyCode.LeftArrow) == true)
                newX -= MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
            if (Input.GetKey(KeyCode.RightArrow) == true)
                newX += MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
        }

        //Don't go past the edge of the arena.
        newX = Mathf.Clamp(newX, -SimControl.EdgeDistance, SimControl.EdgeDistance);
        //Update the transform.
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    //Find the best target for the hero.
    public Enemy FindTarget()
    {
        //Find all the enemies in the scene.
        var enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0) //No enemies means no target.
            return null;
        //There are enemies, now find the best one.
        Enemy target = null;
        if (Target != null && Target.HitPoints > 0.0f) //Start with our current target if it is still alive.
            target = Target;
        //Find the enemy with the lowest HP.
        float lowestHP = float.MaxValue;
        if (target) //Start with the current target so any ties don't cause target switching.
            lowestHP = target.HitPoints;
        //Loop through all the enemies to find the weakest enemy.
        foreach (Enemy enemy in enemies)
        {
            if (enemy.HitPoints > 0 && enemy.HitPoints < lowestHP)
            {
                target = enemy;
                lowestHP = enemy.HitPoints;
            }
        }
        return target;
    }

    //This is NOT a Start() function because we need to be able to call Initialize() whenever a new
    //round starts, not just when the object is created.
    public void Initialize()
    {
        //Set our X position to the correct starting position on the left side of the arena, while keeping the Y and Z the same.
        transform.position = new Vector3(-SimControl.StartingX, transform.position.y, transform.position.z);
        //Reset hit points.
        HitPoints = MaxHitPoints;
        //Reset power, but to 10% of MaxPower, not the full amount.
        Power = MaxPower * 0.1f;
        //Reset all the cooldowns.
        for (int i = 0; i < Abilities.Length; i++)
        {
            if (Abilities[i] != null) Abilities[i].ResetCooldown();
        }
        
        //Find a target.
        Target = FindTarget();
        //Make sure the health and power bars get reset.
        HealthBar.InterpolateImmediate(HitPoints / MaxHitPoints);
        PowerBar.InterpolateImmediate(Power / MaxPower);
    }
    
    //Try to use a random ability.
    public bool UseRandomAbility()
    {
        //Get a random number between 1 and 4. Yes, the integer version of this function is not
        //inclusive. This is wrong and Unity should feel bad for doing this.
        return UseAbility(Random.Range(0, Abilities.Length));
    }

    //Try to use a specific ability.
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

    // Smart AI
    public void SmartAI()
    {
        // Keep a record of target distance
        float distanceToTarget = transform.position.x - Target.transform.position.x;


        // Spam the tweet ability
        UseAbility(0);

        // If an enemy is very close to the player use the light skin stare
        if ((transform.position.x - Target.transform.position.x) < OptimalRange)
        {
            UseAbility(1);
        }

        // If the target is below 20% hit points, go in for the finisher.
        if (Target.HitPoints < (0.2 * Target.MaxHitPoints) && distanceToTarget > Abilities[3].MaximumRange)
        {
            // Get the player in range to use the finisher
            float newX = transform.position.x;
            newX += MoveSpeed * SimControl.DT;
            // Don't go past the edge of the arena.
            newX = Mathf.Clamp(newX, -SimControl.EdgeDistance, SimControl.EdgeDistance);
            // Update the transform.
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // When in range use the ability
            if (distanceToTarget <= Abilities[3].MaximumRange)
            {
                UseAbility(3);
            }
        }
    }

    //Use a given amount of power.
    public void UsePower(float power)
    {
        //Make sure power does not go negative (or above max, becaust the "power" could be negative).
        Power = Mathf.Clamp(Power - power, 0.0f, MaxPower);
        //Interpolate the power UI bar over half a second.
        PowerBar.InterpolateToScale(Power / MaxPower, 0.5f);
    }

    //Take damage from any source.
    public bool TakeDamage(float damage)
    {
        if (damage != 0.0f) //Don't bother if the damage is 0
        {
            //Make sure hit points do not go negative (or above max, because the "damage" could be negative, i.e., healing).
            HitPoints = Mathf.Clamp(HitPoints - damage, 0.0f, MaxHitPoints);
            //Interpolate the hit point UI bar over half a second.
            HealthBar.InterpolateToScale(HitPoints / MaxHitPoints, 0.5f);
            //Create a temporary InfoText object to show the damage using the static Instantiate() function.
            Text damageText = Object.Instantiate(SimControl.InfoTextPrefab, transform.position, Quaternion.identity, SimControl.Canvas.transform).GetComponent<Text>();
            //Set the damage text to just the integer amount of the damage done.
            //Uses the "empty string plus number" trick to make it a string.
            damageText.text = "" + Mathf.Floor(damage);
        }
        //Return true if dead.
        return (HitPoints <= 0.0f);
    }

    public void RizzStolen(float damage)
    {
        if (damage != 0.0f || Power != 0.0f)
        {
            // Make sure the player power does not go negative (or above max, because the "damage" could be negative, i.e., healing).
            Power = Mathf.Clamp(Power - damage, 0.0f, MaxPower);
            // Interpolate the power UI bar over half a second.
            PowerBar.InterpolateToScale(Power / MaxPower, 0.5f);
            //Create a temporary InfoText object to show the damage using the static Instantiate() function.
            Text damageText = Object.Instantiate(SimControl.InfoTextPrefab, transform.position, Quaternion.identity, SimControl.Canvas.transform).GetComponent<Text>();
            //Set the damage text to just the integer amount of the damage done.
            //Uses the "empty string plus number" trick to make it a string.
            damageText.text = "" + Mathf.Floor(damage);
        }
    }

}


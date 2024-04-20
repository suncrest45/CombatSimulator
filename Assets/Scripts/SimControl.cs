/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component controls the entire combat simulation. This component is added to
    a game object whose only purpose to contain this functionality, but using a
    ScriptedObject would potentially be a more advanced	way of doing this.
	
*******************************************************************************/

//Standard Unity component libraries
using JetBrains.Annotations;
using System;
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using System.IO; //Needed for writing telemetry data to a file.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class SimControl : MonoBehaviour
{
    public string testerName = string.Empty;
    //Does the simulation start in Auto mode?
    public static bool AutoMode = false;
    //Does the simulation start in Fast mode?
    public static bool FastMode = false;
    //Does the simulation start in Group mode?
    public static bool GroupMode = false;
    //Does the simulation start in Mixed mode?
    public static bool MixedMode = false;
    //Does the simulation start in Telemetry mode?
    public static bool TelemetryMode = false;
    //Does the simulation start in Telemetry mode?
    public static bool StandardMode = false;
    // Does the simulation start in Playtest mode?
    public static bool PlaytestMode = false;
    //This is the delta time the simulation uses,
    //which is artificially increased when in fast mode.
    public static float DT;

    // How many different AI types (and therefore how many "fights") do we want?
    public int Fights = 36;
    private int FightCount = 0;
    // Have all the fights been completed?
    public static bool SimOver = false;
    // What's the current type of AI for this fight?
    public static string CurrentAI = "Random"; 

    // How many rounds is each "fight"?
    public int Rounds = 10;
    private int RoundCount = 0;
    // Did the current round just end?
    public static bool RoundOver = false;
    // Is a new round just starting (make sure the player has time to find a target)?
    public static bool RoundStart = false; 
    // How long a delay between rounds?
    public float RoundDelay = 3.0f;
    private float RoundTimer = 3.0f;

    // How far from the center of the screen is the "edge" of the arena?
    public static float EdgeDistance = 8.0f;
    // How far from the center of the screen do combatants start?
    public static float StartingX = 5.0f;
 
    // Telemetry data for an individual fight.
    public static int Victories = 0;
    public static int Defeats = 0;
    public static float DamageDone = 0;
    public static float TotalFightTime = 0;
    // Stream used to write the data to a file.
    public static StreamWriter DataStream; 

    // Need a reference to the player, so we don't have to look it
    // up each time.
    public static Hero Player;

    // We will use the UI canvas a lot, so store a reference to it.
    public static GameObject Canvas;

	// References for text prefabs and enemy prefabs, so we don't
	// have to load them each time.
    public static GameObject InfoTextPrefab;
    public static GameObject StaticInfoTextPrefab;
    public static GameObject[] EnemyTypePrefabs = new GameObject[12];
    public HeroAbility[] Abilities = new HeroAbility[5];


    // Start is called before the first frame update
    void Start()
    {
        // Create a comma-separated value file to output telemetry data.
        // This can just then be directly opened in Excel.
        DataStream = new StreamWriter("FightData_" + testerName + ".csv", true);
        // Write some headers for our columns. You'll need more columns than this eventually.
        DataStream.WriteLine("AI TYPE,Enemy Type,Group,VICTORIES,DEFEATS,Win%,DPS,ROUND LENGTH,Tweet,Light-Skin Stare,Fact-Check,Cancel,OK BOOMER!!!,Tweet %,Light-Skin Stare %,Fact-Check %,Cancel %,OK BOOMER!!! %,Ratings,Ratings Difference,");

        // Get a reference to the canvas (used for UI objects).
        Canvas = GameObject.Find("Canvas");

        // Get a reference to the player's game object.
        // Note that we use GetComponent so we don't have to do that
        // every time we want to access the Hero class functionality.
        Player = GameObject.Find("Hero").GetComponent<Hero>();

        //Load all the prefabs we are going to use.
        InfoTextPrefab = Resources.Load("Prefabs/InfoText") as GameObject;
        StaticInfoTextPrefab = Resources.Load("Prefabs/StaticInfoText") as GameObject;

        FightRecorder.InitRecorder(Abilities, Rounds);
        InitialiseEnemies();

        UpdateTelemetryHealthValues();
    }

    // Update is called once per frame
    void Update()
    {
        // If the ESC key is pressed, exit the program.
        if (Input.GetKeyDown(KeyCode.Escape) == true)
            Application.Quit();

        // The simulation is over, so stop updating.
        if (FightCount >= Fights)
        {
            // Did the simulation just end?
            if (SimOver == false) 
            {
                SimOver = true;
                // Don't forget to close the stream.
                DataStream.Close(); 
                SpawnInfoText("SIMULATION OVER", true);
            }
            return;
        }

        // If the A key is pressed, toggle Auto mode on or off.
        if (Input.GetKeyDown(KeyCode.A) == true)
            AutoMode = !AutoMode;

        // If the F key is pressed, toggle Fast Auto mode on or off.
        if (Input.GetKeyDown(KeyCode.F) == true)
            FastMode = !FastMode;

        // If the G key is pressed, toggle Group mode on or off.
        if (Input.GetKeyDown(KeyCode.G) == true)
        {
            if (GroupMode == false)
            {
                GroupMode = !GroupMode;
                MixedMode = false;
                StandardMode = false;
                TelemetryMode = false;
                AutoMode = false;
                FastMode = false;
                Rounds = 6;
                Fights = 1;
                CurrentAI = "Player";
                NewFight();
            }
            else
            {
                GroupMode = !GroupMode;
                StandardMode = true;
                NewFight();
            }
        }
            

        // If the M key is pressed, toggle Mixed mode on or off.
        if (Input.GetKeyDown(KeyCode.M) == true)
        {
            if (MixedMode == false)
            {
                MixedMode = !MixedMode;
                GroupMode = false;
                StandardMode = false;
                TelemetryMode = false;
                AutoMode = false;
                FastMode = false;
                Rounds = 6;
                Fights = 1;
                CurrentAI = "Player";
                NewFight();
            }
            else
            {
                MixedMode = !MixedMode;
                StandardMode = true;
                NewFight();
            }
        }

        // If the T key is pressed, toggle Telemetry mode on or off.
        if (Input.GetKeyDown(KeyCode.T) == true)
        {
            if (TelemetryMode == false)
            {
                TelemetryMode = !TelemetryMode;
                GroupMode = false;
                StandardMode = false;
                MixedMode = false;
                AutoMode = false;
                FastMode = false;
                Rounds = 10;
                Fights = 36;
                NewFight();
            }
            else
            {
                TelemetryMode = !TelemetryMode;
                StandardMode = true;
                NewFight();
            }
        }
            

        // If the S key is pressed, toggle Standard mode on or off.
        if (Input.GetKeyDown(KeyCode.S) == true)
        {
            if (StandardMode == false)
            {
                StandardMode = !StandardMode;
                AutoMode = false;
                FastMode = false;
                MixedMode = false;
                GroupMode = false;
                TelemetryMode = false;
                Fights = 14;
                Rounds = 1;
                CurrentAI = "Player";
                NewFight();
            }
        }


        // If the R key is pressed, restart the simulation.
        if (Input.GetKeyDown(KeyCode.R) == true)
        {
            FightCount = 0;
            RoundCount = 0;
            RoundTimer = RoundDelay;
            RoundStart = false;
            RoundOver = false;
            SimOver = false;
            CurrentAI = "Random";
            Victories = 0;
            Defeats = 0;
            DamageDone = 0;
            TotalFightTime = 0;
        }

        // Get the actual delta time, but cap it at one-tenth of
        // a second. Except in fast mode, where we just make it
        // one-tenth of a second all the time. Note that if we make
        // this more than one-tenth of a second, we might get different
        // results in fast mode vs. normal mode by "jumping" over time
        // thresholds (cooldowns for example) that are in tenths of a second.
        if (FastMode)
            DT = 0.1f; // We could go even faster by not having visual feedback in this mode...
        else if (Time.deltaTime < 0.1f)
            DT = Time.deltaTime;
        else
            DT = 0.1f;

        // Standard Sim
        if (StandardMode)
        {
            StandardSim();
        }

        // Group Mode
        if (GroupMode)
        {
            GroupSim();
        }

        // Mixed Mode
        if (MixedMode)
        {
            MixedSim();
        }

        // Telemetry mode
        if (TelemetryMode)
        {
            TelemetryModeSim();
        }

        // Keep track of the lowest amount of player hit points
        FightRecorder.LowestPlayerHealth(Player.HitPoints);
        DetermineLowestTotalEnemyHealth();
    }

    // The round is over if either the player is dead or all enemies are.
    bool IsRoundOver()
    {
        // Player is dead.
        if (Player.HitPoints == 0.0f)
        {
            // Player just died.
            if (RoundOver == false) 
            {
                float totalMaxHP = 0.0f;
                SpawnInfoText("Cringe...");
                Defeats++;
                FightRecorder.LossesAcessor = Defeats;
                var enemies = FindObjectsOfType<Enemy>();
                foreach (Enemy item in enemies)
                {
                    totalMaxHP += item.MaxHitPoints;
                }

                FightRecorder.m_Ratings[RoundCount - 1] = -(FightRecorder.LowestEnemyHealthAccessor / totalMaxHP) * 100.0f;
            }
            return true;
        }
        // Enemies are dead.
        if (Player.Target == null)
        {
            // Make sure player has a chance to find a target at the start of a round.
            if (RoundStart == true) 
                return false;
            if (RoundOver == false) // Last enemy just died.
            {
                SpawnInfoText("VICTORY!!!");
                Victories++;
                FightRecorder.WinsAccessor = Victories;
                FightRecorder.m_Ratings[RoundCount - 1] = (FightRecorder.LowestPlayerHealthAccessor / Player.MaxHitPoints) * 100.0f;
            }
            return true;
        }
        // Round is not over.
        RoundStart = false;
        return false;
    }

	// Reset everything for the new round.
    void NewRound()
    {
        RoundCount++;
        // Clear out any remaining enemies.
        ClearEnemies();
        
        // The whole fight is over, so start a new one.
        if (RoundCount > Rounds)
        {
            NewFight();
            return;
        }

        // Spawn enemies by calling the Unity engine function Instantiate().
        // Pass in the appropriate prefab, its position, its rotation (90 degrees),
        // and its parent (none).
        // You'll really want these to be an array/dictionary of prefabs eventually.
        if (FightCount % 14 == 1)
        {
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = EnemyTypePrefabs[0].name;
            FightRecorder.GroupAccessor = "N/A";
        }   
        else if (FightCount % 14 == 2)
        {
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = EnemyTypePrefabs[1].name;
            FightRecorder.GroupAccessor = "N/A";
        }
            
        else if (FightCount % 14 == 3)
        { 
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = EnemyTypePrefabs[2].name;
            FightRecorder.GroupAccessor = "N/A";
        }
        else if (FightCount % 14 == 4)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "N/A";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[0].name;
        }
        else if (FightCount % 14 == 5)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "N/A";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[1].name;
        }
        else if (FightCount % 14 == 6)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "N/A";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[2].name;
        }
        else if (FightCount % 14 == 7)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "N/A";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[3].name + "2 x " + EnemyTypePrefabs[0].name;
        }
        else if (FightCount % 14 == 8)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[4].name;
        }
        else if (FightCount % 14 == 9)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[5].name;
        }
        else if (FightCount % 14 == 10)
        {
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[0].name + " 1 x " + EnemyTypePrefabs[1].name + " 1 x " + EnemyTypePrefabs[2].name;
        }
        else if (FightCount % 14 == 11)
        {
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "2 x " + EnemyTypePrefabs[1].name + " 1 x " + EnemyTypePrefabs[5].name;
        }
        else if (FightCount % 14 == 12)
        {
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[3].name + " 1 x " + EnemyTypePrefabs[2].name + " 1 x " + EnemyTypePrefabs[1].name;
        }
        else if (FightCount % 14 == 13)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[4].name + " 2 x " + EnemyTypePrefabs[2].name;
        }
        else if (FightCount % 14 == 0)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[5].name;
        }
        // Note that this just cycles through enemy types/groups, but you'll need more structure than this.
        // Each fight should be one AI type against one enemy type multiple times. And then each AI type
        // against a group of the same type multiple times. And then each AI type against a mixed group
        // multiple times. Group mode and mixed mode will require different logic, etc.

        // Call the Initialize() functions for the player.
        Player.Initialize();

        // Feedback is good...
        SpawnInfoText("ROUND " + RoundCount); //Look! A string concatenation operator!

        // Reset the round delay timer (and round start flag) for after this new round ends.
        RoundTimer = RoundDelay;
        RoundStart = true;
    }

    // Reset everything for the new fight.
    void NewFight()
    {
        FightCount++;
        RoundCount = 0;
        // Show a bit of telemetry data on screen.
        SpawnInfoText(Victories + "-" + Defeats + "\n" + DamageDone / TotalFightTime + " DPS");
        FightRecorder.AVGRoundTime += TotalFightTime;
        // Write all the telemetry data to the file.
        DataStream.WriteLine(CurrentAI
                             + "," + FightRecorder.NameAccessor
                             + "," + FightRecorder.GroupAccessor
                             + "," + FightRecorder.WinsAccessor
                             + "," + FightRecorder.LossesAcessor
                             + "," + FightRecorder.CalculateWinPercentage()
                             + "," + FightRecorder.DPSAccessor / Rounds
                             + "," + FightRecorder.CalculateAVGRoundTime()
                             + "," + FightRecorder.GetAbilityUsage("Tweet")
                             + "," + FightRecorder.GetAbilityUsage("Light-Skin Stare")
                             + "," + FightRecorder.GetAbilityUsage("Fact-Check")
                             + "," + FightRecorder.GetAbilityUsage("Cancel")
                             + "," + FightRecorder.GetAbilityUsage("OK BOOMER!!!")
                             + "," + FightRecorder.GetAbilityPercentage("Tweet")
                             + "," + FightRecorder.GetAbilityPercentage("Light-Skin Stare")
                             + "," + FightRecorder.GetAbilityPercentage("Fact-Check")
                             + "," + FightRecorder.GetAbilityPercentage("Cancel")
                             + "," + FightRecorder.GetAbilityPercentage("OK BOOMER!!!")
                             + "," + FightRecorder.m_Ratings[RoundCount]);
        // Reset the telemetry counters
        Victories = 0;
        Defeats = 0;
        DamageDone = 0;
        TotalFightTime = 0;

        // Reset the recorder for each fight
        FightRecorder.InitRecorder(Abilities, Rounds);
        UpdateTelemetryHealthValues();

        // After the first fight (which is random), just spam a single key for each fight.
        if (!TelemetryMode)
        {
            CurrentAI = "Spam" + FightCount;
        }
        else
        {
            if (FightCount < 18)
            {
                CurrentAI = "Random";
            }
            else
            {
                CurrentAI = "Smart";
            }

        }
    }

    //Destroy all the enemy game objects.
    void ClearEnemies()
    {
        // Find all the game objects that have an Enemy component.
        var enemies = FindObjectsOfType<Enemy>();

        // Didn't find any.
        if (enemies.Length == 0)
        {
            return;
        }

        // A foreach loop! Fancy...
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }  
    }

    // Spawn text at the center of the screen.
    // If set to static, that just means it doesn't move.
    void SpawnInfoText(string text, bool isStatic = false)
    {
        SpawnInfoText(new Vector3(0, 0, 0), text, isStatic);
    }

    // Spawn text wherever you want.
    // If set to static, that just means it doesn't move.
    void SpawnInfoText(Vector3 location, string text, bool isStatic = false)
    {
        // Throw up some text by calling the Unity engine function Instantiate().
        // Pass in the appropriate InfoText prefab, its position, its rotation (none in this case),
        // and its parent (the canvas because this is text). Then we get the
        // Text component from the new game object in order to set the text itself.
        Text infotext;
        if (isStatic)
            infotext = Instantiate(StaticInfoTextPrefab, location, Quaternion.identity, Canvas.transform).GetComponent<Text>();
        else
            infotext = Instantiate(InfoTextPrefab, location, Quaternion.identity, Canvas.transform).GetComponent<Text>();
        // Set the text.
        infotext.text = text;
    }

    void InitialiseEnemies()
    {
        EnemyTypePrefabs[0] = Resources.Load("Prefabs/MeleeEnemy") as GameObject;
        EnemyTypePrefabs[1] = Resources.Load("Prefabs/SniperEnemy") as GameObject;
        EnemyTypePrefabs[2] = Resources.Load("Prefabs/MeleeEliteEnemy") as GameObject;
        EnemyTypePrefabs[3] = Resources.Load("Prefabs/NFTBroEnemy") as GameObject;
        EnemyTypePrefabs[4] = Resources.Load("Prefabs/AIBroEnemy") as GameObject;
        EnemyTypePrefabs[5] = Resources.Load("Prefabs/RivalEnemy") as GameObject;
        EnemyTypePrefabs[6] = Resources.Load("Prefabs/WeakMeleeEnemy") as GameObject;
        EnemyTypePrefabs[7] = Resources.Load("Prefabs/WeakSniper") as GameObject;
        EnemyTypePrefabs[8] = Resources.Load("Prefabs/MeleeElite(Skipped)") as GameObject;
        EnemyTypePrefabs[9] = Resources.Load("Prefabs/FailedNFTBro") as GameObject;
        EnemyTypePrefabs[10] = Resources.Load("Prefabs/NULLReferenceError") as GameObject;
        EnemyTypePrefabs[11] = Resources.Load("Prefabs/Berserker") as GameObject;
    }

    void DetermineLowestTotalEnemyHealth()
    {
        float totalCurrentHP = 0.0f;
        var enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy item in enemies)
        {
            totalCurrentHP += item.HitPoints;
        }

        if (totalCurrentHP < FightRecorder.LowestEnemyHealthAccessor)
        {
            FightRecorder.LowestEnemyHealthAccessor = totalCurrentHP;
        }
    }

    void UpdateTelemetryHealthValues()
    {
        // Set the lowest player health.
        FightRecorder.LowestPlayerHealthAccessor = Player.MaxHitPoints;

        // Set the lowest enemy health
        float totalCurrentHP = 0.0f;
        var enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy item in enemies)
        {
            totalCurrentHP += item.MaxHitPoints;
        }
        FightRecorder.LowestEnemyHealthAccessor = totalCurrentHP;
    }

    void StandardSim()
    {
        // It's the start of a fight, so start a new round.
        if (RoundCount == 0)
            NewRound();

        RoundOver = IsRoundOver();
        if (RoundOver == false)     // The round isn't over, so run the simulation (all the logic is in the updates of other classes).
            TotalFightTime += DT;   // Accumulate the SIMULATED time for telemetry data.
        else if (RoundTimer > 0.0f) // The round is over, but this is the delay before a new round.
            RoundTimer -= DT;       // Update the round delay timer.
        else                        // Time for a new round.
            NewRound();
    }

    void GroupSim()
    {
        // It's the start of a fight, so start a new round.
        if (RoundCount == 0)
            RandomGroupRound();

        RoundOver = IsRoundOver();
        if (RoundOver == false)     // The round isn't over, so run the simulation (all the logic is in the updates of other classes).
            TotalFightTime += DT;   // Accumulate the SIMULATED time for telemetry data.
        else if (RoundTimer > 0.0f) // The round is over, but this is the delay before a new round.
            RoundTimer -= DT;       // Update the round delay timer.
        else                        // Time for a new round.
            RandomGroupRound();
    }

    void RandomGroupRound()
    {
        RoundCount++;

        // Clear out any remaining enemies.
        ClearEnemies();

        // The whole fight is over, so start a new one.
        if (RoundCount > 1)
        {
            GroupMode = false;
            StandardMode = true;
            NewFight();
            return;
        }

        // Three weak
        if (RoundCount % 6 == 1)
        {
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[0].name;
        }

        // Two ranged & 1 rival
        else if (RoundCount % 6 == 2)
        {
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[1].name;
        }

        // Brute and two ranged.
        else if (RoundCount % 6 == 3)
        {
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[2].name;
        }

        // Two brutes & one support.
        else if (RoundCount % 6 == 4)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[3].name;
        }

        else if (RoundCount % 6 == 5)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[4].name;
        }
        else if (RoundCount % 6 == 0)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[5].name;
        }

        // Call the Initialize() functions for the player.
        Player.Initialize();

        // Feedback is good...
        SpawnInfoText("ROUND " + RoundCount); // Look! A string concatenation operator!

        // Reset the round delay timer (and round start flag) for after this new round ends.
        RoundTimer = RoundDelay;
        RoundStart = true;
    }

    void MixedSim()
    {
        // It's the start of a fight, so start a new round.
        if (RoundCount == 0)
            MixedRounds();

        RoundOver = IsRoundOver();
        if (RoundOver == false)     // The round isn't over, so run the simulation (all the logic is in the updates of other classes).
            TotalFightTime += DT;   // Accumulate the SIMULATED time for telemetry data.
        else if (RoundTimer > 0.0f) // The round is over, but this is the delay before a new round.
            RoundTimer -= DT;       // Update the round delay timer.
        else                        // Time for a new round.
            MixedRounds();
    }

    void MixedRounds()
    {
        RoundCount++;
        // Clear out any remaining enemies.
        ClearEnemies();

        // The whole fight is over, so start a new one.
        if (RoundCount > 6)
        {
            MixedMode = false;
            StandardMode = true;
            NewFight();
            return;
        }

        // Three weak
        if (RoundCount % 6 == 1)
        {
            Instantiate(EnemyTypePrefabs[0], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[0].name + " 1 x " + EnemyTypePrefabs[1].name + " 1 x " + EnemyTypePrefabs[2].name;
        }
        
        // Two ranged & 1 rival
        else if (RoundCount % 6 == 2)
        {
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "2 x " + EnemyTypePrefabs[1].name + " 1 x " + EnemyTypePrefabs[5].name;
        }
        
        // Brute and two ranged.
        else if (RoundCount % 6 == 3)
        {
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[1], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[3].name + " 1 x " + EnemyTypePrefabs[2].name + " 1 x " + EnemyTypePrefabs[1].name;
        }
        
        // Two brutes & one support.
        else if (RoundCount % 6 == 4)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[4], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[4].name + " 2 x " + EnemyTypePrefabs[2].name;
        }

        else if (RoundCount % 6 == 5)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[3], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[2], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "1 x " + EnemyTypePrefabs[5].name + " 1 x " + EnemyTypePrefabs[3].name + " 1 x " + EnemyTypePrefabs[2].name;
        }
        else if (RoundCount % 6 == 0)
        {
            // Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[5], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = "3 x " + EnemyTypePrefabs[5].name;
        }

        // Call the Initialize() functions for the player.
        Player.Initialize();

        // Feedback is good...
        SpawnInfoText("ROUND " + RoundCount); // Look! A string concatenation operator!

        // Reset the round delay timer (and round start flag) for after this new round ends.
        RoundTimer = RoundDelay;
        RoundStart = true;
    }

    // Fight each enemy type 10 times
    void TelemetryModeSim()
    {
        FastMode = true;
        AutoMode = true;
        // It's the start of a fight, so start a new round.
        if (RoundCount == 0)
            TelemetryModeRounds();

        RoundOver = IsRoundOver();
        // The round isn't over, so run the simulation (all the logic is in the updates of other classes).
        if (RoundOver == false)
        {
            // Accumulate the SIMULATED time for telemetry data.
            TotalFightTime += DT; 
            FightRecorder.AVGRoundTime += TotalFightTime;
        }
        else if (RoundTimer > 0.0f) // The round is over, but this is the delay before a new round.
            RoundTimer -= DT;       // Update the round delay timer.
        else                        // Time for a new round.
            TelemetryModeRounds();
    }

    void TelemetryModeRounds()
    {
        RoundCount++;
        ClearEnemies();

        if (RoundCount > 10)
        {
            NewFight();
        }


        // Single Enemy Fights using Random AI
        if (FightCount <= 6)
        {
            Instantiate(EnemyTypePrefabs[FightCount - 1], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            FightRecorder.NameAccessor = EnemyTypePrefabs[FightCount - 1].name;
            FightRecorder.GroupAccessor = "N/A";
        }
        // Group Fights using Random AI
        else if (FightCount <= 12 )
        {
            Instantiate(EnemyTypePrefabs[FightCount - 7], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[FightCount - 7], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[FightCount - 7], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 X " + EnemyTypePrefabs[FightCount - 7].name;

        }
        // Mixed Preset Fights using Random AI
        else if(FightCount <= 18)
        {
            // An array for the mixed preset
            int[,] TelemetryMixedPresets = {
                {0, 1, 2 },
                {1, 5, 1 },
                {3, 2, 1 },
                {4, 2, 2 },
                {5, 3, 2 },
                {5, 5, 5 },
            };

            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 13, 0]], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 13, 1]], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 13, 2]], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = (FightCount - 13).ToString();
        }
        // Single Enemy Fights using Smart AI
        else if (FightCount <= 24)
        {
            Instantiate(EnemyTypePrefabs[FightCount - 19], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            FightRecorder.NameAccessor = EnemyTypePrefabs[FightCount - 19].name;
            FightRecorder.GroupAccessor = "N/A";
        }
        // Gorup Fights using Smart AI
        else if (FightCount <= 30)
        {
            Instantiate(EnemyTypePrefabs[FightCount - 25], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[FightCount - 25], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[FightCount - 25], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Group";
            FightRecorder.GroupAccessor = "3 X " + EnemyTypePrefabs[FightCount - 25].name;
        }
        // Mixed Preset Fights using Smart AI
        else
        {
            // An array for the mixed preset
            int[,] TelemetryMixedPresets = {
                {0, 1, 2 },
                {1, 5, 1 },
                {3, 2, 1 },
                {4, 2, 2 },
                {5, 3, 2 },
                {5, 5, 5 },
            };

            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 31, 0]], new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 31, 1]], new Vector3(StartingX + 1, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyTypePrefabs[TelemetryMixedPresets[FightCount - 31, 2]], new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);

            FightRecorder.NameAccessor = "Mixed";
            FightRecorder.GroupAccessor = (FightCount - 31).ToString();
        }

        // Call the Initialize() functions for the player.
        Player.Initialize();

        // Feedback is good...
        SpawnInfoText("ROUND " + RoundCount); // Look! A string concatenation operator!

        // Reset the round delay timer (and round start flag) for after this new round ends.
        RoundTimer = RoundDelay;
        RoundStart = true;
    }
}
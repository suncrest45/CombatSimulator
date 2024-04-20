using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FightRecorder
{

    private static string m_EnemyName;
    private static string m_GroupType;
    private static int m_Wins;
    private static int m_Losses;
    private static float m_WinPercentage;
    private static float m_AVGRoundTime;
    private static int m_TotalAbilityUse;
    private static Dictionary<string, int> m_AbilityUsage;
    private static Dictionary<string, float> m_AbilityPercentage;
    private static int m_Rounds;
    private static float m_DamageDone;
    public static float[] m_Ratings;
    private static float m_LowestPlayerHealth;
    private static float m_LowestEnemyHealth;
    private static float m_HealingDone;

    // Constructor for the fight recorder class
    static FightRecorder()
    {
        m_EnemyName = string.Empty;
        m_GroupType = string.Empty; 
        m_Wins = 0;
        m_Losses = 0;
        m_WinPercentage = 0.0f;
        m_AVGRoundTime = 0.0f;
        m_TotalAbilityUse = 1;
        m_AbilityUsage = new Dictionary<string, int>();
        m_AbilityPercentage = new Dictionary<string, float>();
        m_Rounds = 10;
        m_DamageDone = 0.0f;
        m_Ratings = new float[m_Rounds];
        m_HealingDone = 0.0f;
    }

    // Calculate The Win Percentage
    public static float CalculateWinPercentage() { return m_WinPercentage = (WinsAccessor / m_Rounds) * 100.0f; }

    // Calculate average round time
    public static float CalculateAVGRoundTime()
    {
        // Calculate average
        return (m_AVGRoundTime /= m_Rounds);
    }

    public static int WinsAccessor
    {
        get => m_Wins;
        set => m_Wins = value;
    }

    public static int LossesAcessor
    {
        get => m_Losses;
        set => m_Losses = value;
    }

    public static float HealingAccessor
    {
        get => m_HealingDone;
        set => m_HealingDone = value;
    }

    public static float DPSAccessor
    {
        get => m_DamageDone;
        set => m_DamageDone = value;
    }

    public static float AVGRoundTime
    {
        get => m_AVGRoundTime;
        set => m_AVGRoundTime = value;
    }

    public static string NameAccessor
    {
        get => m_EnemyName;
        set => m_EnemyName = value;
    }

    public static string GroupAccessor
    {
        get => m_GroupType;
        set => m_GroupType = value;
    }

    public static float LowestPlayerHealthAccessor
    {
        get => m_LowestPlayerHealth;
        set => m_LowestPlayerHealth = value;
    }

    public static float LowestEnemyHealthAccessor
    {
        get => m_LowestEnemyHealth;
        set => m_LowestEnemyHealth = value;
    }

    public static void LowestPlayerHealth(float playerHealth)
    {
        if (playerHealth < LowestPlayerHealthAccessor)
        {
            LowestPlayerHealthAccessor = playerHealth;
        }
    }

    public static void SetAbilityUsage(string AbilityName) 
    { 
        m_AbilityUsage[AbilityName] += 1; 
        m_TotalAbilityUse += 1;
    }

    public static int GetAbilityUsage(string AbilityName) {  return m_AbilityUsage[AbilityName]; }

    public static float GetAbilityPercentage(string AbilityName) { return (m_AbilityUsage[AbilityName] / m_TotalAbilityUse) * 100.0f; }

    // Resets the recorder for a new fight
    public static void InitRecorder(HeroAbility[] playerAbilities, int num_rounds)
    {
        m_EnemyName = string.Empty;
        m_GroupType = string.Empty;
        m_Wins = 0;
        m_Losses = 0;
        m_WinPercentage = 0.0f;
        m_AVGRoundTime = 0.0f;
        m_TotalAbilityUse = 1;
        m_AbilityUsage = new Dictionary<string, int>();
        m_AbilityPercentage = new Dictionary<string, float>();
        m_Rounds = num_rounds;
        m_DamageDone = 0.0f;
        m_Ratings = new float[num_rounds];
        m_LowestEnemyHealth = 0.0f;
        m_LowestPlayerHealth = 0.0f;
        HealingAccessor = 0.0f;

        // Initialise the dictionaries keeping track of the abilities.
        for (int i = 0; i < playerAbilities.Length; i++)
        {
            m_AbilityUsage.Add(playerAbilities[i].AbilityName, 0);
            m_AbilityPercentage.Add(playerAbilities[i].AbilityName, 0.0f);
        }
    }
}

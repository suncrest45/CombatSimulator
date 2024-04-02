using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // This is here so we don't have to type out longer names for UI components.

public class Ability : MonoBehaviour
{
    // Properties that define the ability's cooldown time, damage done, power used, range, etc.
    public string AbilityName = string.Empty;
    public float CooldownTime = 1.0f;
    public float DamageDone = 1.0f;
    public float PowerUsed = 1.0f;
    public float MaximumRange = 10.0f;
    public bool Inactive = false;

    // How much of the cooldown time is actually left.
    private float CooldownLeft = 0.0f;

    // Reference to the cooldown timer bar, so we don't have to look it up all the time.
    private BarScaler CooldownBar;

    // Reference to the ability number text, so we don't have to look it up all the time.
    private Text AbilityNumber;

    // Reference to the parent entity, so we don't have to look it up all the time.
    private Entity ParentEntity; 


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

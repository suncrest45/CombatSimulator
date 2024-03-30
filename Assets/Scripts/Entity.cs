using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------
    // Member Variables
    //------------------------------------------------------------------------------------------------------------------------------------
    // Max Health of the Entity
    public float MaxHitPoints = 200.0f;
    // Max Resource of the Entity
    public float MaxPower = 100.0f;
    // How quickly the entity moves
    public float MoveSpeed = 0.1f;
    // The optimal distance the AI will try to maintain from targets
    public float OptimalRange = 5.0f;

    [HideInInspector]
    private float HitPoints = 200; // Current hit points
    [HideInInspector]
    private float Power = 40;      // Current power
    [HideInInspector]
    private Entity Target;         // Current Target



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

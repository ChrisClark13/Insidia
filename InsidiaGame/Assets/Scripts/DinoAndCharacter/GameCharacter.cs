using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//General base-line functionality for all GameCharacters (Player Dinos, AI Dinos, AI Turrets, etc) use this script??
/// <summary>
/// A script that holds extremely important variables such as Health and Heat. It currently automatically cools its heat. Also serves as a way to tag a GameObject as something that takes action within the Scene.
/// The values in this script will also be sent over the network in order to keep networked games in sync.<para></para>
/// To use: Attach to any Dino (PC or NPC), Turret, etc that needs to make use any of these stats and is something that takes action of its own. (For destructibles in the environment, use another script).<para></para>
/// Any time you need to get or set Health, Heath, etc. Access this script. Some variables have event delegates that are called whenever they are changed. (Health, Heat, Overheated)<para></para>
/// There are also static and non-static event delegates to detect when a GameCharacter has died. (static: GameCharacter.CharacterDeath, non-static: Death)<para></para>
/// Non-monitored variables: maxHealth, heatCapacity, autoCooling, heatCoolingPerSecond, overheatCoolingThreshold. They won't send delegates when you mess with them, but changing them does do things.
/// More extremely important variables can and will be added to this script in the future in order to provide easy access and network seralization for them.<para></para>
/// Created by Christian Clark
/// </summary>
[RequireComponent(typeof(CharacterInput))]
public class GameCharacter : MonoBehaviour {
    [HideInInspector]
    [SerializeField]
    private CharacterInput _input;

    public CharacterInput Input { get { return _input; } }

    private void Awake()
    {
        _input = GetComponent<CharacterInput>();
    }

    ///////////
    // Health
    [Header("HEALTH")]
    [SerializeField]
    private float _health = 100f;
    public float Health {
        get { return _health; }
        set
        {
            value = Mathf.Clamp(value, 0f, maxHealth);
            if (value != _health)
            {
                _health = value;

                if (HealthChanged != null)
                    HealthChanged(this, _health);

                if (_health == 0)
                {
                    if (Death != null)
                        Death(this);
                    if (CharacterDeath != null)
                        CharacterDeath(this);
                }
            }
        }
    }
    public event Action<GameCharacter, float> HealthChanged;
    public event Action<GameCharacter> Death;
    public static event Action<GameCharacter> CharacterDeath;

    public float maxHealth = 100f;

    //////////
    // Heat
    [Header("HEAT")]
    [SerializeField]
    private float _heat = 0f;
    public float Heat
    {
        get { return _heat; }
        set
        {
            value = Mathf.Clamp(value, 0f, heatCapacity);
            if (value != _heat)
            {
                _heat = value;
                //Debug.Log("Heat " + _heat);

                if (HeatChanged != null)
                    HeatChanged(this, _heat);

                CheckOverheated();
            }
        }
    }
    public event Action<GameCharacter, float> HeatChanged;

    public float heatCapacity = 100f;
    /// <summary>
    /// Should be disabled when the local machine is not the one in control of the GameCharacter.
    /// </summary>
    public bool autoCooling = true;
    public float heatCoolingPerSecond = 10f;

    [SerializeField]
    [ReadOnly]
    private bool _overheated = false;
    /// <summary>
    /// Use this variable to check wether or not the GameCharacter is overheated or not. It cannot be set by other scripts. If you want to unoverheat a GameCharacter, lower their Heat.<para></para>
    /// You can subscribe to the "<see cref="OverheatedChanged"/>" delegate if you need to know whenever this value changes.
    /// </summary>
    public bool Overheated
    {
        get { return _overheated; }
        private set
        {
            if (value != _overheated)
            {
                _overheated = value;
                if (OverheatedChanged != null)
                    OverheatedChanged(this, _overheated);
            }
        }
    }
    public event Action<GameCharacter, bool> OverheatedChanged;

    /// <summary>
    /// The Heat of the GameCharater must go below this percentage of it's heatCapacity in order to not be overheated any more.
    /// </summary>
    [Range(0f, 1f)]
    [Tooltip("The Heat of the GameCharater must go below this percentage of it's heatCapacity in order to not be overheated any more.")]
    public float overheatCooldownThreshold = 0.5f;
    private const float COOLING_UPDATES_PER_SECOND = 50f; //It's cheap and if its run a lot the UI will be smoother for it.
    private Coroutine _coroutineCooling = null;

    //Other variables here?

    private void OnEnable()
    {
        //Start a coroutine that calls a function that does the heat cooling.
        _coroutineCooling = StartCoroutine(this.UpdateCoroutine(COOLING_UPDATES_PER_SECOND, CoolingUpdate));
    }

    private void OnDisable()
    {
        StopCoroutine(_coroutineCooling);
    }

    private void OnValidate()
    {
        //Keep values from dropping below zero via the editor.
        _health = Mathf.Max(0, _health);
        maxHealth = Mathf.Max(0, maxHealth);
        _heat = Mathf.Max(0, _heat);
        heatCapacity = Mathf.Max(0, heatCapacity);
        heatCoolingPerSecond = Mathf.Max(0, heatCoolingPerSecond);

        if (Application.isPlaying)
        {
            //If this was changed via the Editor during playtime, then do some tricks (making sure all the values are different) 
            //to make sure all the delegates get sent (because we just lost track of whatever they were at before).
            //This kind of weird solution is just fine because this code only runs in the editor

            float health = _health;
            _health--;
            Health = health;

            float heat = _heat;
            _heat--;
            Heat = heat;

            //Don't send one for Overheated since it can't be changed in the editor.
        }
    }

    // Lowers the heat by heatCoolingPerSecond every second if autoCooling is enabled.
    private void CoolingUpdate(float deltaTime)
    {
        if (autoCooling)
            Heat -= heatCoolingPerSecond * deltaTime;
    }

    //Check to see if we should start or stop being overheated and update the Overheated variable appropriately.
    private void CheckOverheated()
    {
        if (_heat >= heatCapacity)
        {
            Overheated = true;
        }
        else if (_heat <= (heatCapacity * overheatCooldownThreshold))
        {
            Overheated = false;
        }
    }

}

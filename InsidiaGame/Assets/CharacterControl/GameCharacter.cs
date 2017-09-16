using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//General base-line functionality for all GameCharacters (Player Dinos, AI Dinos, AI Turrets, etc) use this script??
/// <summary>
/// A script that caches references to other key scripts and may eventually do some calcuations of it's own. Should be placed on every Dino (Player or NPC).<para></para>
/// Created by Christian Clark
/// </summary>
[RequireComponent(typeof(CharacterInput))]
public class GameCharacter : MonoBehaviour {
    [SerializeField]
    private CharacterInput _input;
    /// <summary>
    /// Use this variable to check AND set the character's inputs.
    /// <para>You will need to subscribe to an input's delegate if you need to know when it is pressed/released as opposed to just needing whatever value the input happens to be at at the time.</para>
    /// How to get an input: "gameCharacter.Input.[input name];" (Allows for implict casting using some C# wizardry.)<para></para>
    /// How to get an input (alternate): "gameCharacter.Input.[input name].Value; (No implict casting.)<para></para>
    /// How to set an input: "gamecharacter.Input.[input name].Value = value;"<para></para>
    /// How to subscribe to an input delegate: "gameCharacter.Input.[input name].OnChange += [subscriber function];"<para></para>
    /// How to unsubscribe to an input delegate: "gameCharacter.Input.[input name].OnChange -= [subscriber function];"
    /// </summary>
    public CharacterInput Input { get { return _input; } }

    private void Awake()
    {
        _input = GetComponent<CharacterInput>();
    }

    //Health??

    //Heat??
}

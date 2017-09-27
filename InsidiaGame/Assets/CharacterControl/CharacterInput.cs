using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this script (referenced as the Input property inside the GameCharacter class) to check the character's inputs.
/// <para>You will need to subscribe to an input's delegate if you need to know when it is pressed/released as opposed to just needing whatever value the input happens to be at at the time.</para>
/// How to get an input: "gameCharacter.Input.[input name];" (Allows for implict conversion using some C# wizardry. Referencing the Value property itself is also possible.)<para></para>
/// How to set an input: "gamecharacter.Input.[input name].Value = value;"<para></para>
/// How to subscribe to an input delegate: "gameCharacter.Input.[input name].OnChange += [subscriber function];"<para></para>
/// How to unsubscribe to an input delegate: "gameCharacter.Input.[input name].OnChange -= [subscriber function];"<para></para>
/// Created by Christian Clark
/// </summary>
[DefaultExecutionOrder(-100)]
public class CharacterInput : MonoBehaviour
{
    [Serializable]
    public class Input<T>
    {
        private CharacterInput characterInput;
        [SerializeField]
        private T _value;
        /// <summary>
        /// The value of this input. Triggers the OnChange event when set.
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { if (!_value.Equals(value))
                {
                    _value = value;
                    if (OnChange != null)
                        OnChange(characterInput, _value);
                } }
        }

        public event Action<CharacterInput, T> OnChange;

        public Input(CharacterInput characterInput)
        {
            this.characterInput = characterInput;
        }
    }

    [SerializeField]
    private Input<Vector2> _move;
    public Input<Vector2> Move { get { return _move; } }

    [SerializeField]
    private Input<Vector3> _aimPoint;
    public Input<Vector3> AimPoint { get { return _aimPoint; } }

    [SerializeField]
    private Input<bool> _jump;
    public Input<bool> Jump { get {return _jump; } }

    [SerializeField]
    private Input<bool> _attack;
    public Input<bool> Attack { get { return _attack; } }

    [SerializeField]
    private Input<bool> _attackMode;
    public Input<bool> AttackMode { get { return _attackMode; } }

    [SerializeField]
    private Input<bool> _lockOn;
    public Input<bool> LockOn { get { return _lockOn; } }

    public void Awake()
    {
        _move = new Input<Vector2>(this);
        _aimPoint = new Input<Vector3>(this);
        _jump = new Input<bool>(this);
        _attack = new Input<bool>(this);
        _attackMode = new Input<bool>(this);
        _lockOn = new Input<bool>(this);
    }
}

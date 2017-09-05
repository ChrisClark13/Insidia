using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this class (referenced as the Input property inside the GameCharacter class) to check the character's inputs.
/// <para>You will need to subscribe to an input's delegate if you need to know when it is pressed/released as opposed to just needing whatever value the input happens to be at at the time.</para>
/// How to get an input: "gameCharacter.Input.[input name];" (Allows for implict conversion using some C# wizardry. Referencing the Value property itself is also possible.)<para></para>
/// How to set an input: "gamecharacter.Input.[input name].Value = value;"<para></para>
/// How to subscribe to an input delegate: "gameCharacter.Input.[input name].OnChange += [subscriber function];"<para></para>
/// How to unsubscribe to an input delegate: "gameCharacter.Input.[input name].OnChange -= [subscriber function];"
/// </summary>
[Serializable]
public class CharacterInputState
{
    [Serializable]
    public class Input<T>
    {
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
                        OnChange(_value);
                } }
        }

        public event Action<T> OnChange;

        public static implicit operator T(Input<T> input) {
            return input._value;
        }
    }

    public Input<Vector2> Move { get; private set; }
    public Input<Vector2> Aim { get; private set; }
    public Input<bool> Jump { get; private set; }
    public Input<bool> Sprint { get; private set; }
    public Input<bool> MeleeLight { get; private set; }
    public Input<bool> MeleeHeavy { get; private set; }
    public Input<bool> Special { get; private set; }

    public CharacterInputState()
    {
        Move = new Input<Vector2>();
        Aim = new Input<Vector2>();
        Jump = new Input<bool>();
        Sprint = new Input<bool>();
        MeleeLight = new Input<bool>();
        MeleeHeavy = new Input<bool>();
        Special = new Input<bool>();
    }
}

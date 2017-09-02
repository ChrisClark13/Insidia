using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterInputState
{
    /* Copy-paste this block for each seperate CharacterInput button/axis/etc.
    private Vector2 _move;
    public Vector2 Move { get { return _move; } set { if (value != _move && OnMoveChange != null) { _move = value; OnMoveChange(_move); } } }
    public Action<Vector2> OnMoveChange;
    */

    [SerializeField]
    private Vector2 _move;
    public Vector2 Move
    {
        get { return _move; }
        set
        {
            if (value != _move)
            {
                _move = value;
                if (OnMoveChange != null)
                    OnMoveChange(_move);
            }
        }
    }
    public Action<Vector2> OnMoveChange;

    [SerializeField]
    private Vector2 _aim;
    public Vector2 Aim
    {
        get { return _aim; }
        set
        {
            if (value != _aim)
            {
                _aim = value;
                if (OnAimChange != null)
                    OnAimChange(_aim);
            }
        }
    }
    public Action<Vector2> OnAimChange;

    [SerializeField]
    private bool _jump;
    public bool Jump
    {
        get { return _jump; }
        set
        {
            if (value != _jump)
            {
                _jump = value;
                if (OnJumpChange != null)
                    OnJumpChange(_jump);
            }
        }
    }
    public Action<bool> OnJumpChange;

    [SerializeField]
    private bool _meleeLight;
    public bool MeleeLight
    {
        get { return _meleeLight; }
        set
        {
            if (value != _meleeLight)
            {
                _meleeLight = value;
                if (OnMeleeLightChange != null)
                    OnMeleeLightChange(_meleeLight);
            }
        }
    }
    public Action<bool> OnMeleeLightChange;

    [SerializeField]
    private bool _meleeHeavy;
    public bool MeleeHeavy
    {
        get { return _meleeHeavy; }
        set
        {
            if (value != _meleeHeavy)
            {
                _meleeHeavy = value;
                if (OnMeleeHeavyChange != null)
                    OnMeleeHeavyChange(_meleeHeavy);
            }
        }
    }
    public Action<bool> OnMeleeHeavyChange;

    [SerializeField]
    private bool _fire;
    public bool Fire
    {
        get { return _fire; }
        set
        {
            if (value != _fire)
            {
                _fire = value;
                if (OnFireChange != null)
                    OnFireChange(_fire);
            }
        }
    }
    public Action<bool> OnFireChange;

    [SerializeField]
    private bool _special;
    public bool Special
    {
        get { return _special; }
        set
        {
            if (value != _special)
            {
                _special = value;
                if (OnSpecialChange != null)
                    OnSpecialChange(_special);
            }
        }
    }
    public Action<bool> OnSpecialChange;
}

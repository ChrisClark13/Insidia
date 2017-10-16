using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// This script syncs a Health and MaxHealth value back and forth between the server.
/// <para>To use: Place on anything that needs to keep track of it's health. Read the value from <see cref="Health"/> and use <see cref="ChangeHealth(float)"/> or <see cref="SetHealth(float)"/> depending on which one you need.</para>
/// <para>ChangeHealth changes adds that value passed in to the health, SetHealth directly sets the value of the health.</para>
/// <para><see cref="SetMaxHealth(float)"/> also exists for setting the <see cref="MaxHealth"/> value.</para>
/// <para>Created by Christian Clark</para>
/// </summary>
public class NetworkedHealth : NetworkBehaviour {

    public delegate void HealthChangeDelegate(NetworkedHealth sender, float newValue, float changeAmount);
    public HealthChangeDelegate OnHealthChanged;
    public static HealthChangeDelegate OnHealthChangedStatic;

    public delegate void MaxHealthChangeDelegate(NetworkedHealth sender, float newValue);
    public MaxHealthChangeDelegate OnMaxHealthChanged;
    public static MaxHealthChangeDelegate OnMaxHealthChangedStatic;

    [SerializeField]
    private float _health = 100f;
    public float Health { get { return _health; } }

    [SerializeField]
    [SyncVar(hook ="SyncMaxHealth")]
    private float _maxHealth = 100f;
    public float MaxHealth { get { return _maxHealth; } }

    public void ChangeHealth(float changeAmount)
    {
        //If we want to have instant feedback on the client that sent the action (which then gets overwritted/corrected by the server in a bit), this would be the place to do it.

        CmdChangeHealth(changeAmount);
    }

    public void SetHealth(float value)
    {
        CmdSetHealth(value);
    }

    [Command]
    private void CmdChangeHealth(float changeAmount)
    {
        _health = Mathf.Clamp(_health + changeAmount, 0, _maxHealth);
        RpcSyncHealth(_health, changeAmount);

        //Check to make sure we're not the host before we send the delegate, because it will have already be sent in the RpcSyncHealth call.
        if (!isClient)
            CallHealthDelegates(_health, changeAmount);
    }

    [Command]
    private void CmdSetHealth(float value)
    {
        value = Mathf.Clamp(value, 0, _maxHealth);
        float changeAmount = value - _health;
        _health = value;
        RpcSyncHealth(_health, changeAmount);

        //Check to make sure we're not the host before we send the delegate, because it will have already be sent in the RpcSyncHealth call.
        if (!isClient)
            CallHealthDelegates(_health, value);
    }

    [ClientRpc]
    private void RpcSyncHealth(float newValue, float changeAmount)
    {
        //If we're the host, then the value was already set so don't set it again.
        if (!isServer)
            _health = newValue;

        if (OnHealthChanged != null)
            OnHealthChanged(this, _health, changeAmount);
    }

    public void SetMaxHealth(float newValue)
    {
        CmdSetMaxHealth(newValue);
    }

    [Command]
    private void CmdSetMaxHealth(float newValue)
    {
        _maxHealth = Mathf.Max(_maxHealth, 0);

        //If we're the host, then the calls will be made in just a bit.
        if (!isClient)
            CallMaxHealthDelegates(_maxHealth);
    }

    private void SyncMaxHealth(float newValue)
    {
        //If we're the host, the value was already set.
        if (!isServer)
            _maxHealth = newValue;

        CallMaxHealthDelegates(newValue);
    }

    private void CallHealthDelegates(float newValue, float changeAmount)
    {
        if (OnHealthChanged != null)
            OnHealthChanged(this, newValue, changeAmount);

        if (OnHealthChangedStatic != null)
            OnHealthChangedStatic(this, newValue, changeAmount);
    }

    private void CallMaxHealthDelegates(float newValue)
    {
        if (OnMaxHealthChanged != null)
            OnMaxHealthChanged(this, newValue);

        if (OnMaxHealthChangedStatic != null)
            OnMaxHealthChangedStatic(this, newValue);
    }

    private void Start()
    {
        _health = _maxHealth;
        if (isServer)
            RpcSyncHealth(_health, 0f);
    }

    // For correcting the values when they get set in the editor.
    private void OnValidate()
    {
        _maxHealth = Mathf.Max(_maxHealth, 0);
        _health = Mathf.Clamp(_health, 0, _maxHealth);

        //If we mess with this in the editor while we're playing, send the change out to everyone!
        if (Application.isPlaying)
        {
            CmdSetHealth(_health);
            CmdSetMaxHealth(_maxHealth);
        }
    }
}

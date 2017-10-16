using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hit G to increase health, H to decrease it. For testing only.
/// </summary>
[RequireComponent(typeof(NetworkedHealth))]
public class HealthTester : MonoBehaviour {
    
    private NetworkedHealth _health;

    private void Awake()
    {
        _health = GetComponent<NetworkedHealth>();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.H))
            _health.ChangeHealth(-10f);
        if (Input.GetKeyDown(KeyCode.G))
            _health.ChangeHealth(10f);
    }

    private void OnGUI()
    {
        GUILayout.Label("Health: " + _health.Health);
    }

}

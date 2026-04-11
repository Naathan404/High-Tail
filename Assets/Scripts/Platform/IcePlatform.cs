using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class IcePlatform : MonoBehaviour
{
    [SerializeField] private float _newDeceleration = 20f;
    [SerializeField] private PlayerData _playerData;

    private float _jumpForce;
    private float _deceleration;
    private Trigger _trigger;

    private void Awake()
    {
        _trigger = GetComponentInChildren<Trigger>();
        // Get player data
        _jumpForce = _playerData.jumpForce;
        _deceleration = _playerData.deceleration;
    }

    private void OnEnable()
    {
        if (_trigger == null)
        {
            Debug.LogWarning("The ice platform's trigger is null");
            return;
        }
        _trigger.OnStepIn += InvokePlatform;
        _trigger.OnStepOut += DisablePlatform;
    }

    private void OnDisable()
    {
        if (_trigger == null)
        {
            Debug.LogWarning("The ice platform's trigger is null");
            return;
        }
        _trigger.OnStepIn -= InvokePlatform;
        _trigger.OnStepOut -= DisablePlatform;
    }

    private void InvokePlatform()
    {
        float newJumpForce = _playerData.jumpForce * 0.75f;
        float newDeceleration = _newDeceleration;
        // set new data
        _playerData.jumpForce = newJumpForce;
        _playerData.deceleration = newDeceleration;
    }

    private void DisablePlatform() // Temporary name (I cant find any better name right now)
    {
        // Return to the old data
        _playerData.jumpForce = _jumpForce;
        _playerData.deceleration = _deceleration;
    }
}

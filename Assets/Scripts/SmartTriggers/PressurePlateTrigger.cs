using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A specialized smart trigger that activates based on the combined weight/mass of objects on top of it.
/// Can be used to create pressure plates, weight-based puzzles, etc.
/// </summary>
public class PressurePlateTrigger : SmartTrigger
{
    [Tooltip("The minimum combined mass required to activate the trigger")]
    [SerializeField] private float requiredWeight = 10f;

    [Tooltip("The transform that will move down when pressed")]
    [SerializeField] private Transform plateTransform;

    [Tooltip("How far the plate moves down when fully pressed")]
    [SerializeField] private float pressedHeight = 0.1f;

    [Tooltip("How quickly the plate moves to its target position")]
    [SerializeField] private float moveSpeed = 2f;

    private float currentWeight;
    private Vector3 originalPosition;
    private bool wasPressed;
    private Dictionary<Collider, float> objectWeights = new Dictionary<Collider, float>();

    private void Start()
    {
        if (plateTransform != null)
        {
            originalPosition = plateTransform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTrigger(other)) return;

        if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            objectWeights[other] = rb.mass;
            UpdateTotalWeight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectWeights.ContainsKey(other))
        {
            objectWeights.Remove(other);
            UpdateTotalWeight();
        }
    }

    private void UpdateTotalWeight()
    {
        currentWeight = 0f;
        foreach (var weight in objectWeights.Values)
        {
            currentWeight += weight;
        }
    }

    private void Update()
    {
        if (plateTransform == null) return;

        // Calculate target position based on weight
        Vector3 targetPos = originalPosition;
        bool isPressed = currentWeight >= requiredWeight;

        if (isPressed)
        {
            targetPos.y = originalPosition.y - pressedHeight;
            
            // Trigger when first pressed
            if (!wasPressed)
            {
                wasPressed = true;
                base.ExecuteTriggerActions();
            }
        }
        else if (wasPressed)
        {
            // Handle unpress event
            wasPressed = false;
            if (base.hasAnyUntriggerActions)
            {
                base.ExecuteUntriggerActions();
            }
        }

        // Move plate to target position
        plateTransform.position = Vector3.Lerp(
            plateTransform.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );
    }

    /// <summary>
    /// Gets the current weight on the pressure plate
    /// </summary>
    public float CurrentWeight => currentWeight;

    /// <summary>
    /// Gets whether the pressure plate is currently pressed
    /// </summary>
    public bool IsPressed => wasPressed;
} 
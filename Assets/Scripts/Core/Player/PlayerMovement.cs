using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    private const float PARTICLE_STOP_THRESHOLD = 0.005f;

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dustCloud;
    
    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float turningRate;
    [SerializeField] private float particleEmissionValue = 10;

    private ParticleSystem.EmissionModule emissionModule;
    private Vector2 previousMovementInput;
    private Vector3 previousPos;

    private void Awake() {
        emissionModule = dustCloud.emission;
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) { return; }

        inputReader.MoveEvent += InputReader_MoveEvent;
    }

    public override void OnNetworkDespawn() {
        if (!IsOwner) { return; }

        inputReader.MoveEvent -= InputReader_MoveEvent;
    }

    private void Update() {
        if (!IsOwner) { return; }

        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void FixedUpdate() {
        if ((transform.position - previousPos).sqrMagnitude > PARTICLE_STOP_THRESHOLD) {
            emissionModule.rateOverTime = particleEmissionValue;
        } else {
            emissionModule.rateOverTime = 0;
        }

        previousPos = transform.position;

        if (!IsOwner) { return; }

        rb.velocity = (Vector2) bodyTransform.up * previousMovementInput.y * movementSpeed;
    }

    private void InputReader_MoveEvent(Vector2 movementInput) {
        previousMovementInput = movementInput;
    }

    
    
}

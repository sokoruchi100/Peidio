using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage;
    
    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId) { 
        this.ownerClientId = ownerClientId;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.attachedRigidbody == null) { return; } //Check if collided with tank

        if (collision.attachedRigidbody.TryGetComponent(out NetworkObject networkObject)) { //check if collided with self
            if (ownerClientId == networkObject.OwnerClientId) { return; }
        }

        if (collision.attachedRigidbody.TryGetComponent(out Health health)) {//Takes damage
            health.TakeDamage(damage);
        }
    }
}

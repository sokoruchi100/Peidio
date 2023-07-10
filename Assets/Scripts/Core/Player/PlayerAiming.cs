using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    private void LateUpdate() {
        if (!IsOwner) { return; }

        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        Vector2 pointDir = (cursorPos - (Vector2)turretTransform.position).normalized;
        turretTransform.up = pointDir;
    }
}
